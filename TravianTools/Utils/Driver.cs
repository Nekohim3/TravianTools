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
using OpenQA.Selenium.Chrome.ChromeDriverExtensions;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using RestSharp;
using SmorcIRL.TempMail;
using TravianTools.Data;
using TravianTools.SeleniumHost;
using TravianTools.TravianUtils;
using Cookie = System.Net.Cookie;
using DataFormat = RestSharp.DataFormat;

namespace TravianTools.Utils
{
    public class Driver
    {
        public  Account             Account     { get; set; }
        public  ChromeDriverService Service     { get; set; }
        private ChromeOptions       Options     { get; set; }
        public  ChromeDriver        Chrome      { get; set; }
        public  IJavaScriptExecutor JsExec      { get; set; }
        public  Actions             Act         { get; set; }
        public  SeleniumHostWPF     Host        { get; set; }
        public  RestClientOptions   RestOptions { get; set; }
        public  RestClient          RestClient  { get; set; }
        private DateTime            _lastRespDate = DateTime.MinValue;
        private Thread              _regTh  { get; set; }
        public  MailClient          MClient { get; set; }

        public void Init(Account acc)
        {
            Account = acc;
            if (!Directory.Exists($"{g.Settings.UserDataPath}\\{Account.Name}"))
                Directory.CreateDirectory($"{g.Settings.UserDataPath}\\{Account.Name}");
            Logger.Info($"[{Account.Name}]: Start driver initialization");
            Service                         = ChromeDriverService.CreateDefaultService();
            Service.HideCommandPromptWindow = true;
            Options                         = new ChromeOptions();

            if (acc.UseProxy)
            {
                Options.AddHttpProxy(g.Settings.ProxyAddr, g.Settings.ProxyPort, g.Settings.ProxyLogin, g.Settings.ProxyPass);
            }

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

            RestOptions = new RestClientOptions($"https://{g.Settings.Server}.{g.Settings.Domain}")
                          {
                              Proxy = new WebProxy(new Uri($"http://{g.Settings.ProxyAddr}:{g.Settings.ProxyPort}"), true, null, new NetworkCredential(g.Settings.ProxyLogin, g.Settings.ProxyPass)),
                              UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36 OPR/48.0.2685.50"
                          };
            RestClient = new RestClient(RestOptions);
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

        public dynamic PostJo(JObject json, bool hasResponse = false)
        {
            var counter = 0;
            while (counter < 5)
            {
                try
                {
                    while ((DateTime.Now - _lastRespDate).TotalMilliseconds < 300)
                        Thread.Sleep(10);
                    _lastRespDate = DateTime.Now;
                    
                    var req = new RestRequest("/api/", Method.Post);
                    req.AddParameter("c", (string)(json as dynamic).controller.ToString(), ParameterType.QueryString);
                    req.AddParameter("a", (string)(json as dynamic).action.ToString(),     ParameterType.QueryString);
                    req.AddParameter("t", GetTimeStamp(),                                  ParameterType.QueryString);
                    var data = Rem(json.ToString());
                    var buffer = Encoding.UTF8.GetBytes(data);
                    req.AddHeader("Accept",      "application/json, text/plain, */*");
                    req.AddHeader("Accept-Encoding", "gzip, deflate, br");
                    req.AddHeader("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
                    req.AddHeader("ContentType", "application/json;charset=UTF-8");
                    req.AddHeader("Host",        $"{g.Settings.Server}.{g.Settings.Domain}");
                    req.AddHeader("Referer",     $"https://{g.Settings.Server}.{g.Settings.Domain}/");
                    req.AddHeader("Origin", $"https://{g.Settings.Server}.{g.Settings.Domain}");
                    req.AddHeader("Content-Type", "application/json");
                    var cookies = Chrome.Manage().Cookies.AllCookies;
                    
                    RestOptions.CookieContainer = new CookieContainer();
                    foreach (var cookie in cookies)
                        RestOptions.CookieContainer?.Add(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
                    req.AddBody(data, "application/json");
                    
                    var res = RestClient.ExecuteAsync(req).GetAwaiter().GetResult();
                    Logger.Data(res.Content);
                    var jo = JObject.Parse(res.Content) as dynamic;
                    if (hasResponse && jo.response != null) return jo;
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

        public void Registration()
        {
            _regTh = new Thread(RegThFunc);
            _regTh.Start();
        }

        public void RegThFunc()
        {
            MClient = new MailClient();
            var domain = MClient.GetFirstAvailableDomainName().GetAwaiter().GetResult();
            MClient.Register($"{g.Accounts.SelectedAccount.Name}2112wwe@{domain}", g.Accounts.SelectedAccount.Password).GetAwaiter().GetResult();
            Login($"{Account.Name}2112wwe@{domain}", Account.Password);
            Thread.Sleep(15000);
            ChooseTribe(2);
            Thread.Sleep(3000);
            PostJo(JObject.Parse(
                                 "{\"controller\":\"player\",\"action\":\"changeSettings\",\"params\":{\"newSettings\":{\"premiumConfirmation\":3,\"lang\":\"ru\",\"onlineStatusFilter\":2,\"extendedSimulator\":false,\"musicVolume\":0,\"soundVolume\":0,\"uiSoundVolume\":50,\"muteAll\":true,\"timeZone\":\"3.0\",\"timeFormat\":0,\"attacksFilter\":2,\"mapFilter\":123,\"enableTabNotifications\":true,\"disableAnimations\":true,\"enableHelpNotifications\":true,\"enableWelcomeScreen\":true,\"notpadsVisible\":false}},\"session\":\"" +
                                 GetSession() + "\"}"));
            DialogAction(1, 1, "setName", Account.Name);
            var msgArr = MClient.GetMessages(1).GetAwaiter().GetResult();
            while (msgArr.Length == 0)
            {
                Thread.Sleep(1000);
                msgArr = MClient.GetMessages(1).GetAwaiter().GetResult();
            }
            var msg = MClient.GetMessageSource(msgArr.FirstOrDefault(x => x.Subject.ToLower().Contains("travian kingdoms")).Id).GetAwaiter().GetResult();
            var str = g.DecodeQuotedPrintables(msg.Data);
            var link = str.Substring(str.IndexOf("http://www.kingdoms.com/ru/#action=activation;token="), 92);
            JsExec.ExecuteScript("window.open()");
            Chrome.SwitchTo().Window(Chrome.WindowHandles.Last());
            Chrome.Navigate().GoToUrl(link);
            Thread.Sleep(5000);
            Chrome.Close();
            Chrome.SwitchTo().Window(Chrome.WindowHandles.First());
            DialogAction(1, 1, "activate");
            Thread.Sleep(1500);
            Account.Player.Update();
            Account.Player.UpdateVillageList();
            var vid = Account.Player.VillageList.First().Id;
            SendTroops(vid, 536920065, 3, false, "resources", 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
            Thread.Sleep(10000);
            DialogAction(1, 2, "backToVillage");
            Thread.Sleep(1500);
            BuildingUpgrade(vid, 33, 22);
            Thread.Sleep(6000);
            BuildingUpgrade(vid, 29, 19);
            Thread.Sleep(1500);
            RecruitUnits(vid, 29, 19, "12", 3);
            Thread.Sleep(1500);
            DialogAction(30, 1, "attack");
            Thread.Sleep(10000);
            DialogAction(34, 1, "activate");
            Thread.Sleep(1500);
            DialogAction(34, 1, "face");
            Thread.Sleep(1500);
            DialogAction(35, 1, "activate");
            Thread.Sleep(1500);
            BuildingUpgrade(vid, 2, 4);
            Thread.Sleep(6000);
            DialogAction(203, 1, "activate");
            Thread.Sleep(1500);
            DialogAction(203, 1, "become_governor");
            Thread.Sleep(1500);
            DialogAction(204, 1, "activate");
            Thread.Sleep(3000);
            new MapSolver().Solve(Account);
            Thread.Sleep(2000); 
            DialogAction(302, 1, "activate");
            Thread.Sleep(1500);

            var data    = PostJo(RPG.GetCache_MapDetails(GetSession(), vid));
            //var mdList  = (dynamic)JObject.Parse(Http.Post(RPG.GetCache_MapDetails()));
            var newList = new List<int>();
            foreach (var q in data.cache)
                if (q.data.npcInfo != null)
                    newList.Add(Convert.ToInt32(q.name.ToString().Split(':')[1]));
            var d1    = Math.Abs(vid - newList[0]);
            var d2    = Math.Abs(vid - newList[1]);
            var destv = d1 >= d2 ? newList[0] : newList[1];

            SendTroops(vid, destv, 3, false, "resources", 3, 6, 0, 0, 0, 0, 0, 0, 0, 0, 1);
            Thread.Sleep(20000);
            DialogAction(303, 1, "activate");
            Thread.Sleep(2000);
            Account.Player.Hero.Update();
            Account.Player.Hero.UpdateItems();
            UseHeroItem(1, Account.Player.Hero.Items.First(x => x.ItemType == 120).Id, vid);
            Thread.Sleep(10000);
            DialogAction(399, 1, "activate");
            Thread.Sleep(3000);
            DialogAction(399, 1, "finish");
            Thread.Sleep(1500);
            CollectReward(vid, 205);
            Thread.Sleep(1500);
            UpdateVillageName(vid, Account.Name);
            Thread.Sleep(1500);
            CollectReward(vid, 202);
            Thread.Sleep(3000);
            Chrome.Navigate().Refresh();
            Thread.Sleep(5000);
            Account.RegistrationComplete = true;
            Accounts.Save();
        }
        
        public void Login(string email, string pass)
        {
            Chrome.SwitchTo().Frame(Chrome.FindElementsByTagName("iframe").FirstOrDefault(x => x.GetAttribute("Class") == "mellon-iframe"));
            Chrome.SwitchTo().Frame(Chrome.FindElementByTagName("iframe"));
            Chrome.FindElement(By.Name("email")).SendKeys(email);
            Chrome.FindElement(By.Name("password[password]")).SendKeys(pass);
            JsExec.ExecuteScript("arguments[0].click();", Chrome.FindElement(By.Name("termsAccepted")));
            Chrome.FindElement(By.Name("submit")).Click();
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
                ////if (data.response != null) return true;

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
                ////if (data.response != null) return true;

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
                //if (data.response != null) return true;

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

                //if (data.response != null) return true;

                Account.Update(data, (long) data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: FinishNow ({villageId}, {queueType}, {price}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
        }

        public bool SendTroops(int villageId, int destVid, int movType, bool redeployHero, string spyMission, int t1, int t2, int t3, int t4, int t5, int t6, int t7, int t8, int t9, int t10, int t11)
        {
            Logger.Info($"[{Account.Name}]: SendTroops ({villageId}, {destVid}, {movType}, {redeployHero}, {spyMission}, {t1}, {t2}, {t3}, {t4}, {t5}, {t6}, {t7}, {t8}, {t9}, {t10}, {t11})");
            try
            {
                var data = PostJo(RPG.SendTroops(GetSession(), villageId, destVid, movType, redeployHero, spyMission, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11));
                if (data == null)
                {
                    Logger.Info($"[{Account.Name}]: SendTroops ({villageId}, {destVid}, {movType}, {redeployHero}, {spyMission}, {t1}, {t2}, {t3}, {t4}, {t5}, {t6}, {t7}, {t8}, {t9}, {t10}, {t11}) Update FAILED");
                    return false;
                }
                ////if (data.response != null) return true;

                Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: SendTroops ({villageId}, {destVid}, {movType}, {redeployHero}, {spyMission}, {t1}, {t2}, {t3}, {t4}, {t5}, {t6}, {t7}, {t8}, {t9}, {t10}, {t11}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
        }

        public bool RecruitUnits(int villageId, int locationId, int buildingType, string unitId, int count)
        {
            Logger.Info($"[{Account.Name}]: RecruitUnits ({villageId}, {locationId}, {buildingType}, {unitId}, {count})");
            try
            {
                var data = PostJo(RPG.RecruitUnits(GetSession(), villageId, locationId, buildingType, unitId, count));
                if (data == null)
                {
                    Logger.Info($"[{Account.Name}]: RecruitUnits ({villageId}, {locationId}, {buildingType}, {unitId}, {count}) Update FAILED");
                    return false;
                }
                ////if (data.response != null) return true;

                Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: RecruitUnits ({villageId}, {locationId}, {buildingType}, {unitId}, {count}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
        }

        public bool ChooseTribe(int tribeId)
        {
            Logger.Info($"[{Account.Name}]: ChooseTribe ");
            try
            {
                var data = PostJo(RPG.ChooseTribe(GetSession(), tribeId));
                //if (data == null)
                //{
                //    Logger.Info($"[{Account.Name}]: BuildingUpgrade ({villageId}, {locationId}, {buildingType}) Update FAILED");
                //    return false;
                //}
                //////if (data.response != null) return true;

                //Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: ChooseTribe  Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
        }

        public bool DialogAction(int qid, int did, string cmd, string input = "")
        {
            Logger.Info($"[{Account.Name}]: DialogAction ({qid}, {did}, {cmd}, {input})");
            try
            {
                var data = PostJo(RPG.DialogAction(GetSession(), qid, did, cmd, input));
                //if (data == null)
                //{
                //    Logger.Info($"[{Account.Name}]: BuildingUpgrade ({villageId}, {locationId}, {buildingType}) Update FAILED");
                //    return false;
                //}
                //////if (data.response != null) return true;

                //Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: DialogAction ({qid}, {did}, {cmd}, {input}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
        }

        public bool UseHeroItem(int amount, int id, int villageId)
        {
            Logger.Info($"[{Account.Name}]: UseHeroItem ({amount}, {id}, {villageId})");
            try
            {
                var data = PostJo(RPG.UseHeroItem(GetSession(), amount, id, villageId));
                if (data == null)
                {
                    Logger.Info($"[{Account.Name}]: UseHeroItem ({amount}, {id}, {villageId}) Update FAILED");
                    return false;
                }
                ////if (data.response != null) return true;

                Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: UseHeroItem ({amount}, {id}, {villageId}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
        }

        public bool CollectReward(int villageId, int questId)
        {
            Logger.Info($"[{Account.Name}]: CollectReward ({villageId}, {questId})");
            try
            {
                var data = PostJo(RPG.CollectReward(GetSession(), villageId, questId));
                if (data == null)
                {
                    Logger.Info($"[{Account.Name}]: CollectReward ({villageId}, {questId}) Update FAILED");
                    return false;
                }
                ////if (data.response != null) return true;

                Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: CollectReward ({villageId}, {questId}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
                return false;
            }

            return true;
        }

        public bool UpdateVillageName(int villageId, string villageName)
        {
            Logger.Info($"[{Account.Name}]: UpdateVillageName ({villageId}, {villageName})");
            try
            {
                var data = PostJo(RPG.SetVillageName(GetSession(), villageId, villageName));
                if (data == null)
                {
                    Logger.Info($"[{Account.Name}]: UpdateVillageName ({villageId}, {villageName}) Update FAILED");
                    return false;
                }
                ////if (data.response != null) return true;

                Account.Update(data, (long)data.time);
            }
            catch (Exception e)
            {
                Logger.Info($"[{Account.Name}]: UpdateVillageName ({villageId}, {villageName}) Update FAILED with exception:\n{e}\n{e.InnerException}\n{e.InnerException?.InnerException}");
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
