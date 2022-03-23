using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using TravianTools.Data;
using TravianTools.SeleniumHost;
using TravianTools.TravianUtils;

namespace TravianTools.Utils
{
    public class Driver
    {
        public  Account                           Account { get; set; }
        public  ChromeDriverService               Service { get; set; }
        private ChromeOptions                     Options { get; set; }
        public  ChromeDriver                      Chrome  { get; set; }
        public  IJavaScriptExecutor               JsExec  { get; set; }
        public  Actions                           Act     { get; set; }
        public  SeleniumHostWPF Host    { get; set; }

        

        public void Init(Account acc)
        {
            Account = acc;
            if (!Directory.Exists($"{g.Settings.UserDataPath}\\{Account.Name}"))
                Directory.CreateDirectory($"{g.Settings.UserDataPath}\\{Account.Name}");
            Logger.Info($"[{Account.Name}]: Start driver initialization");
            Service                         = ChromeDriverService.CreateDefaultService();
            Service.HideCommandPromptWindow = true;
            Options                         = new ChromeOptions();

            //if (acc.UseProxy)
            //{
            //    //var proxy = new Proxy
            //    //            {
            //    //                Kind         = ProxyKind.Manual,
            //    //                IsAutoDetect = false,
            //    //                HttpProxy    = "171.22.214.170"
            //    //            };
            //    //Options.Proxy      = proxy;
            //    Options.AddArgument($"--proxy-server=socks5://117.43.94.79:7890");
            //}

            Options.AddArgument($"user-data-dir={g.Settings.UserDataPath}\\{Account.Name}");
            Chrome             = new ChromeDriver(Service, Options);
            JsExec             = Chrome;
            Act                = new Actions(Chrome);
            Application.Current.Dispatcher.Invoke(() => {
                                                      Host = new SeleniumHostWPF
                                                      {
                                                                 DriverService = Service
                                                             };
                                                  });

            Logger.Info($"[{Account.Name}]: End driver initialization");
        }

        public void DeInit()
        {
            Logger.Info($"[{Account.Name}]: Start driver deinitialization");
            Host.DriverService = null;
            Host               = null;
            Act                = null;
            JsExec             = null;
            Chrome.Dispose();
            Chrome  = null;
            Options = null;
            Service.Dispose();
            Service                            = null;
            Account.Running                    = false;
            Account.FreeInstantBuilder.Working = false;
            Account.FreeInstantBuilder         = null;
            Logger.Info($"[{Account.Name}]: End driver deinitialization");
            Account         = null;
        }

        public string GetCookieString() => Chrome.Manage().Cookies.AllCookies.Aggregate("", (s, c) => $"{s}{c.Name}={c.Value};");

        public string GetSession()
        {
            var cookies  = Chrome.Manage().Cookies.AllCookies;
            var sessionJson = cookies.FirstOrDefault(x => x.Name == "t5SessionKey");
            if (sessionJson == null) return "";
            var     decodedSessionJson        = WebUtility.UrlDecode(sessionJson.Value);
            dynamic dynamicDecodedSessionJson = JObject.Parse(decodedSessionJson);
            return dynamicDecodedSessionJson.key;
        }

        public int GetPlayerId()
        {
            var cookies     = Chrome.Manage().Cookies.AllCookies;
            var sessionJson = cookies.FirstOrDefault(x => x.Name == "t5SessionKey");
            if (sessionJson == null) return -1;
            var     decodedSessionJson        = WebUtility.UrlDecode(sessionJson.Value);
            dynamic dynamicDecodedSessionJson = JObject.Parse(decodedSessionJson);
            return dynamicDecodedSessionJson.id;
        }

        public IWebElement Wait(By by, int timeout = 10)
        {
            try
            {
                var wait = new WebDriverWait(Chrome, TimeSpan.FromSeconds(timeout));
                var el   = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(by));

                Logger.Info($"Browser Wait element={by} timeout={timeout} succ");
                return el;
            }
            catch
            {
                Logger.Info($"Browser Wait element={by} timeout={timeout} err");
                return null;
            }
        }

        public string Post(JObject json)
        {
            var req = (HttpWebRequest)WebRequest.Create(
                                                         $"https://{g.Settings.Server}.{g.Settings.Domain}/api/?c={(json as dynamic).controller}&a={(json as dynamic).action}&t{GetTimeStamp()}");
            var data = Rem(json.ToString());
            var buffer = Encoding.UTF8.GetBytes(data);

            req.Method = "POST";
            req.Accept = "application/json, text/plain, */*";
            req.ContentType = "application/json;charset=UTF-8";
            req.Host = $"{g.Settings.Server}.{g.Settings.Domain}";
            req.Referer = $"https://{g.Settings.Server}.{g.Settings.Domain}/";
            req.UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36 OPR/48.0.2685.50";
            req.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            req.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
            req.Headers.Add("Origin", $"https://{g.Settings.Server}.{g.Settings.Domain}");

            req.Headers.Add("Cookie", GetCookieString());

            req.ContentLength = buffer.Length;
            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            var reqStream = req.GetRequestStream();
            reqStream.Write(buffer, 0, data.Length);
            reqStream.Close();
            var resp = (HttpWebResponse)req.GetResponse();
            var strReader = new StreamReader(resp.GetResponseStream());
            var workingPage = strReader.ReadToEnd();
            resp.Close();
            Logger.Data(workingPage);
            return workingPage;
        }

        public string Rem(string str) => str.Replace("\r", "").Replace("\n", "").Replace(" ", "");
        private string GetTimeStamp() => ((long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds).ToString();

        public bool IsBrowserClosed()
        {
            var isClosed = false;
            try
            {
                _ = Chrome.Title;
            }
            catch
            {
                isClosed = true;
            }

            return isClosed;
        }

        #region TReq

        public void BuildingDestroy(int villageId, int locationId)
        {
            Logger.Info($"[{Account.Name}]: BuildingDestroy ({villageId}, {locationId})");
            try
            {
                var data = JObject.Parse(Post(RPG.BuildingDestroy(GetSession(), villageId, locationId))) as dynamic;
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: BuildingDestroy ({villageId}, {locationId}) Update FAILED");
                    return;
                }

                Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: BuildingDestroy ({villageId}, {locationId}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
            }
        }

        public void BuildingUpgrade(int villageId, int locationId, int buildingType)
        {
            Logger.Info($"[{Account.Name}]: BuildingUpgrade ({villageId}, {locationId}, {buildingType})");
            try
            {
                var data = JObject.Parse(Post(RPG.BuildingUpgrade(GetSession(), villageId, locationId, buildingType))) as dynamic;
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: BuildingUpgrade ({villageId}, {locationId}, {buildingType}) Update FAILED");
                    return;
                }

                Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: BuildingUpgrade ({villageId}, {locationId}, {buildingType}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
            }
        }

        public void FinishNow(int villageId, int queueType, int price)
        {
            Logger.Info($"[{Account.Name}]: FinishNow ({villageId}, {queueType}, {price})");
            try
            {
                var data = JObject.Parse(Post(RPG.FinishBuild(GetSession(), villageId, price, queueType))) as dynamic;
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: FinishNow ({villageId}, {queueType}, {price}) Update FAILED");
                    return;
                }

                Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: FinishNow ({villageId}, {queueType}, {price}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
            }
        }



        public void SolvePuzzle(JArray moves)
        {
            Logger.Info($"[{Account.Name}]: SolvePuzzle");
            try
            {
                Post(RPG.SolvePuzzle(GetSession(), moves));
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: SolvePuzzle FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
            }
        }

        public JObject GetPuzzle()
        {
            Logger.Info($"[{Account.Name}]: GetPuzzle");
            try
            {
                return JObject.Parse(Post(RPG.GetPuzzle(GetSession())));
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetPuzzle FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return null;
            }
        }

        #region Cache

        public void GetCache(List<string> lst)
        {
            Logger.Info($"[{Account.Name}]: GetCache ({string.Join(";", lst)})");
            try
            {
                var data = JObject.Parse(Post(RPG.GetCache(GetSession(), lst))) as dynamic;
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache ({string.Join(";", lst)}) Update FAILED");
                    return;
                }

                Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache ({string.Join(";", lst)}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
            }
        }

        public void GetCache_All()
        {
            Logger.Info($"[{Account.Name}]: GetCache_All");
            try
            {
                var data = JObject.Parse(Post(RPG.GetCache_All(GetSession()))) as dynamic;
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_All Update FAILED");
                    return;
                }

                Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_All Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
            }
        }

        public void GetCache_VillageList()
        {
            Logger.Info($"[{Account.Name}]: GetCache_VillageList");
            try
            {
                var data = JObject.Parse(Post(RPG.GetCache_VillageList(GetSession()))) as dynamic;
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_VillageList Update FAILED");
                    return;
                }

                Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_VillageList Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
            }
        }

        public void GetCache_CollectionHeroItemOwn()
        {
            Logger.Info($"[{Account.Name}]: GetCache_CollectionHeroItemOwn");
            try
            {
                var data = JObject.Parse(Post(RPG.GetCache_CollectionHeroItemOwn(GetSession()))) as dynamic;
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_CollectionHeroItemOwn Update FAILED");
                    return;
                }

                Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_CollectionHeroItemOwn Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
            }
        }

        public void GetCache_Quest()
        {
            Logger.Info($"[{Account.Name}]: GetCache_Quest");
            try
            {
                var data = JObject.Parse(Post(RPG.GetCache_Quest(GetSession()))) as dynamic;
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_Quest Update FAILED");
                    return;
                }

                Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_Quest Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
            }
        }

        public void GetCache_Player(int playerId)
        {
            Logger.Info($"[{Account.Name}]: GetCache_Player ({playerId})");
            try
            {
                var data = JObject.Parse(Post(RPG.GetCache_Player(GetSession(), playerId))) as dynamic;
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_Player ({playerId}) Update FAILED");
                    return;
                }

                Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_Player ({playerId}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
            }
        }

        public void GetCache_Hero(int playerId)
        {
            Logger.Info($"[{Account.Name}]: GetCache_Hero ({playerId})");
            try
            {
                var data = JObject.Parse(Post(RPG.GetCache_Hero(GetSession(), playerId))) as dynamic;
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_Hero ({playerId}) Update FAILED");
                    return;
                }

                Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_Hero ({playerId}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
            }
        }

        public void GetCache_BuildingQueue(int villageId)
        {
            Logger.Info($"[{Account.Name}]: GetCache_BuildingQueue ({villageId})");
            try
            {
                var data = JObject.Parse(Post(RPG.GetCache_BuildingQueue(GetSession(), villageId))) as dynamic;
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_BuildingQueue ({villageId}) Update FAILED");
                    return;
                }

                Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_BuildingQueue ({villageId}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
            }
        }

        public void GetCache_BuildingCollection(int villageId)
        {
            Logger.Info($"[{Account.Name}]: GetCache_BuildingCollection ({villageId})");
            try
            {
                var data = JObject.Parse(Post(RPG.GetCache_BuildingCollection(GetSession(), villageId))) as dynamic;
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_BuildingCollection ({villageId}) Update FAILED");
                    return;
                }

                Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_BuildingCollection ({villageId}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
            }
        }

        public void GetCache_MovingTroopsCollection(int villageId)
        {
            Logger.Info($"[{Account.Name}]: GetCache_MovingTroopsCollection ({villageId})");
            try
            {
                var data = JObject.Parse(Post(RPG.GetCache_MovingTroopsCollection(GetSession(), villageId))) as dynamic;
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_MovingTroopsCollection ({villageId}) Update FAILED");
                    return;
                }

                Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_MovingTroopsCollection ({villageId}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
            }
        }

        public void GetCache_StationaryTroopsCollection(int villageId)
        {
            Logger.Info($"[{Account.Name}]: GetCache_StationaryTroopsCollection ({villageId})");
            try
            {
                var data = JObject.Parse(Post(RPG.GetCache_StationaryTroopsCollection(GetSession(), villageId))) as dynamic;
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_StationaryTroopsCollection ({villageId}) Update FAILED");
                    return;
                }

                Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_StationaryTroopsCollection ({villageId}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
            }
        }

        public void GetCache_MapDetails(int villageId)
        {
            Logger.Info($"[{Account.Name}]: GetCache_MapDetails ({villageId})");
            try
            {
                var data = JObject.Parse(Post(RPG.GetCache_MapDetails(GetSession(), villageId))) as dynamic;
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_MapDetails ({villageId}) Update FAILED");
                    return;
                }

                Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_MapDetails ({villageId}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
            }
        }

        public void GetCache_Building(int buildingId)
        {
            Logger.Info($"[{Account.Name}]: GetCache_Building ({buildingId})");
            try
            {
                var data = JObject.Parse(Post(RPG.GetCache_Building(GetSession(), buildingId))) as dynamic;
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_Building ({buildingId}) Update FAILED");
                    return;
                }

                Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_Building ({buildingId}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
            }
        }

        #endregion

        public dynamic GetDataByName(dynamic data, string name)
        {
            foreach (var x in data)
            {
                string[] dataNames = x.name.ToString().Split(':');
                var      names     = name.Split(':');
                if (dataNames.Length != names.Length) continue;
                var eqc = 0;
                for (var i = 0; i < names.Length; i++)
                    if (string.IsNullOrEmpty(names[i])) eqc++;
                    else if (names[i] == "<>") eqc++;
                    else if (names[i] == dataNames[i]) eqc++;
                if (eqc == names.Length) return x;
            }

            return null;
        }

        public List<dynamic> GetDataArrayByName(dynamic data, string name)
        {
            var lst = new List<dynamic>();
            foreach (var x in data)
            {
                string[] dataNames = x.name.ToString().Split(':');
                var      names     = name.Split(':');
                if (dataNames.Length != names.Length) continue;
                var eqc = 0;
                for (var i = 0; i < names.Length; i++)
                    if (string.IsNullOrEmpty(names[i])) eqc++;
                    else if (names[i] == "<>") eqc++;
                    else if (names[i] == dataNames[i]) eqc++;
                if (eqc == names.Length) lst.Add(x);
            }

            return lst.Count != 0 ? lst : null;
        }

        #endregion
    }
}
