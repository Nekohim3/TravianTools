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
        public  Account             Account { get; set; }
        public  ChromeDriverService Service { get; set; }
        private ChromeOptions       Options { get; set; }
        public  ChromeDriver        Chrome  { get; set; }
        public  IJavaScriptExecutor JsExec  { get; set; }
        public  Actions             Act     { get; set; }
        public  SeleniumHostWPF     Host    { get; set; }
        private DateTime _lastRespDate = DateTime.MinValue;


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
            Chrome = new ChromeDriver(Service, Options);
            JsExec = Chrome;
            Act    = new Actions(Chrome);
            Application.Current.Dispatcher.Invoke(() =>
                                                  {
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
            Logger.Info($"[{Account.Name}]: End driver deinitialization");
        }

        public string GetCookieString() => Chrome.Manage().Cookies.AllCookies.Aggregate("", (s, c) => $"{s}{c.Name}={c.Value};");

        public string GetSession()
        {
            var cookies     = Chrome.Manage().Cookies.AllCookies;
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

        public dynamic PostJo(JObject json)
        {
            var counter = 0;
            while (counter < 5)
            {
                try
                {
                    while ((DateTime.Now - _lastRespDate).TotalMilliseconds < 300)
                        Thread.Sleep(10);
                    _lastRespDate = DateTime.Now;
                    var req = (HttpWebRequest) WebRequest.Create(
                                                                 $"https://{g.Settings.Server}.{g.Settings.Domain}/api/?c={(json as dynamic).controller}&a={(json as dynamic).action}&t{GetTimeStamp()}");
                    var data   = Rem(json.ToString());
                    var buffer = Encoding.UTF8.GetBytes(data);

                    req.Method      = "POST";
                    req.Accept      = "application/json, text/plain, */*";
                    req.ContentType = "application/json;charset=UTF-8";
                    req.Host        = $"{g.Settings.Server}.{g.Settings.Domain}";
                    req.Referer     = $"https://{g.Settings.Server}.{g.Settings.Domain}/";
                    req.UserAgent =
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36 OPR/48.0.2685.50";
                    req.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                    req.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
                    req.Headers.Add("Origin",          $"https://{g.Settings.Server}.{g.Settings.Domain}");

                    req.Headers.Add("Cookie", GetCookieString());

                    req.ContentLength          = buffer.Length;
                    req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    var reqStream = req.GetRequestStream();
                    reqStream.Write(buffer, 0, data.Length);
                    reqStream.Close();
                    var resp        = (HttpWebResponse) req.GetResponse();
                    var strReader   = new StreamReader(resp.GetResponseStream());
                    var workingPage = strReader.ReadToEnd();
                    resp.Close();
                    Logger.Data(workingPage);
                    var jo = JObject.Parse(workingPage) as dynamic;
                    if (jo == null || jo.cache == null || jo.cache.Count == 0 || jo.time == null || jo.error != null)
                    {
                        counter++;
                        Logger.Info($"Post error {counter}");
                    }
                    else
                        return jo;
                }
                catch (Exception e)
                {
                    Logger.Info(e.ToString());
                    counter++;
                }
            }

            return null;
        }

        public  string Rem(string str) => str.Replace("\r", "").Replace("\n", "").Replace(" ", "");
        private string GetTimeStamp()  => ((long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds).ToString();

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


        public bool BuildingUpgrade(int villageId, int locationId, int buildingType)
        {
            Logger.Info($"[{Account.Name}]: BuildingUpgrade ({villageId}, {locationId}, {buildingType})");
            try
            {
                var data = PostJo(RPG.BuildingUpgrade(GetSession(), villageId, locationId, buildingType));
                if (data == null)
                {
                    Logger.Info($"[{Account.Name}]: BuildingUpgrade ({villageId}, {locationId}, {buildingType}) Update FAILED");
                    return false;
                }
                if (data.response != null) return true;

                Account.Update(data, (long) data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: BuildingUpgrade ({villageId}, {locationId}, {buildingType}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
        }

        public bool BuildingDestroy(int villageId, int locationId)
        {
            Logger.Info($"[{Account.Name}]: BuildingDestroy ({villageId}, {locationId})");
            try
            {
                var data = PostJo(RPG.BuildingDestroy(GetSession(), villageId, locationId));
                if (data == null)
                {
                    Logger.Info($"[{Account.Name}]: BuildingDestroy ({villageId}, {locationId}) Update FAILED");
                    return false;
                }
                if (data.response != null) return true;

                Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: BuildingDestroy ({villageId}, {locationId}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
        }

        public bool NpcTrade(int villageId, Resource res)
        {
            Logger.Info($"[{Account.Name}]: NpcTrade ({villageId}, {res})");
            try
            {
                var data = PostJo(RPG.NpcTrade(GetSession(), villageId, res.Wood, res.Clay, res.Iron, res.Crop));
                if (data == null)
                {
                    Logger.Info($"[{Account.Name}]: NpcTrade ({villageId}, {res}) Update FAILED");
                    return false;
                }
                if (data.response != null) return true;

                Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: NpcTrade ({villageId}, {res}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
        }

        public bool FinishNow(int villageId, int queueType, int price)
        {
            Logger.Info($"[{Account.Name}]: FinishNow ({villageId}, {queueType}, {price})");
            try
            {
                var data = PostJo(RPG.FinishBuild(GetSession(), villageId, price, queueType));
                if (data == null)
                {
                    Logger.Info($"[{Account.Name}]: FinishNow ({villageId}, {queueType}, {price}) Update FAILED");
                    return false;
                }

                if (data.response != null) return true;

                Account.Update(data, (long) data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: FinishNow ({villageId}, {queueType}, {price}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
        }



        public void SolvePuzzle(JArray moves)
        {
            Logger.Info($"[{Account.Name}]: SolvePuzzle");
            try
            {
                PostJo(RPG.SolvePuzzle(GetSession(), moves));
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
                return JObject.Parse(PostJo(RPG.GetPuzzle(GetSession())));
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetPuzzle FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return null;
            }
        }

        #region Cache

        public bool GetCache(List<string> lst)
        {
            Logger.Info($"[{Account.Name}]: GetCache ({string.Join(";", lst)})");
            try
            {
                var data = PostJo(RPG.GetCache(GetSession(), lst));
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache ({string.Join(";", lst)}) Update FAILED");
                    return false;
                }

                Account.Update(data, (long) data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache ({string.Join(";", lst)}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
        }

        public bool GetCache_All()
        {
            Logger.Info($"[{Account.Name}]: GetCache_All");
            try
            {
                var data = PostJo(RPG.GetCache_All(GetSession()));
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_All Update FAILED");
                    return false;
                }

                Account.Update(data, (long) data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_All Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
        }

        public bool GetCache_VillageList()
        {
            Logger.Info($"[{Account.Name}]: GetCache_VillageList");
            try
            {
                var data = PostJo(RPG.GetCache_VillageList(GetSession()));
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_VillageList Update FAILED");
                    return false;
                }

                Account.Update(data, (long) data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_VillageList Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
        }

        public bool GetCache_CollectionHeroItemOwn()
        {
            Logger.Info($"[{Account.Name}]: GetCache_CollectionHeroItemOwn");
            try
            {
                var data = PostJo(RPG.GetCache_CollectionHeroItemOwn(GetSession()));
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_CollectionHeroItemOwn Update FAILED");
                    return false;
                }

                Account.Update(data, (long) data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_CollectionHeroItemOwn Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
        }

        public bool GetCache_Quest()
        {
            Logger.Info($"[{Account.Name}]: GetCache_Quest");
            try
            {
                var data = PostJo(RPG.GetCache_Quest(GetSession()));
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_Quest Update FAILED");
                    return false;
                }

                Account.Update(data, (long) data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_Quest Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
        }

        public bool GetCache_Player(int playerId)
        {
            Logger.Info($"[{Account.Name}]: GetCache_Player ({playerId})");
            try
            {
                var data = PostJo(RPG.GetCache_Player(GetSession(), playerId));
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_Player ({playerId}) Update FAILED");
                    return false;
                }

                Account.Update(data, (long) data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_Player ({playerId}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
        }

        public bool GetCache_Hero(int playerId)
        {
            Logger.Info($"[{Account.Name}]: GetCache_Hero ({playerId})");
            try
            {
                var data = PostJo(RPG.GetCache_Hero(GetSession(), playerId));
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_Hero ({playerId}) Update FAILED");
                    return false;
                }

                Account.Update(data, (long) data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_Hero ({playerId}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
        }

        public bool GetCache_BuildingQueue(int villageId)
        {
            Logger.Info($"[{Account.Name}]: GetCache_BuildingQueue ({villageId})");
            try
            {
                var data = PostJo(RPG.GetCache_BuildingQueue(GetSession(), villageId));
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_BuildingQueue ({villageId}) Update FAILED");
                    return false;
                }

                Account.Update(data, (long) data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_BuildingQueue ({villageId}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
        }

        public bool GetCache_BuildingCollection(int villageId)
        {
            Logger.Info($"[{Account.Name}]: GetCache_BuildingCollection ({villageId})");
            try
            {
                var data = PostJo(RPG.GetCache_BuildingCollection(GetSession(), villageId));
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_BuildingCollection ({villageId}) Update FAILED");
                    return false;
                }

                Account.Update(data, (long) data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_BuildingCollection ({villageId}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
        }

        public bool GetCache_MovingTroopsCollection(int villageId)
        {
            Logger.Info($"[{Account.Name}]: GetCache_MovingTroopsCollection ({villageId})");
            try
            {
                var data = PostJo(RPG.GetCache_MovingTroopsCollection(GetSession(), villageId));
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_MovingTroopsCollection ({villageId}) Update FAILED");
                    return false;
                }

                Account.Update(data, (long) data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_MovingTroopsCollection ({villageId}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
        }

        public bool GetCache_StationaryTroopsCollection(int villageId)
        {
            Logger.Info($"[{Account.Name}]: GetCache_StationaryTroopsCollection ({villageId})");
            try
            {
                var data = PostJo(RPG.GetCache_StationaryTroopsCollection(GetSession(), villageId));
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_StationaryTroopsCollection ({villageId}) Update FAILED");
                    return false;
                }

                Account.Update(data, (long) data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_StationaryTroopsCollection ({villageId}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
        }

        public bool GetCache_MapDetails(int villageId)
        {
            Logger.Info($"[{Account.Name}]: GetCache_MapDetails ({villageId})");
            try
            {
                var data = PostJo(RPG.GetCache_MapDetails(GetSession(), villageId));
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_MapDetails ({villageId}) Update FAILED");
                    return false;
                }

                Account.Update(data, (long) data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_MapDetails ({villageId}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
        }

        public bool GetCache_Building(int buildingId)
        {
            Logger.Info($"[{Account.Name}]: GetCache_Building ({buildingId})");
            try
            {
                var data = PostJo(RPG.GetCache_Building(GetSession(), buildingId));
                if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
                {
                    Logger.Info($"[{Account.Name}]: GetCache_Building ({buildingId}) Update FAILED");
                    return false;
                }

                Account.Update(data, (long) data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: GetCache_Building ({buildingId}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
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
