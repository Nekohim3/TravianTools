using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Practices.Prism.ViewModel;
using TravianTools.TravianUtils;
using TravianTools.Utils;

namespace TravianTools.Data
{
    public class Account : BaseTravianEntity
    {
        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        private string _password;

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                RaisePropertyChanged(() => Password);
            }
        }

        private bool _registrationComplete;

        public bool RegistrationComplete
        {
            get => _registrationComplete;
            set
            {
                _registrationComplete = value;
                RaisePropertyChanged(() => RegistrationComplete);
            }
        }

        private Player _player;

        [XmlIgnore]
        public Player Player
        {
            get => _player;
            set
            {
                _player = value;
                RaisePropertyChanged(() => Player);
            }
        }

        private bool _running;

        [XmlIgnore]
        public bool Running
        {
            get => _running;
            set
            {
                _running = value;
                RaisePropertyChanged(() => Running);
            }
        }

        private string _refLink;

        public string RefLink
        {
            get => _refLink;
            set
            {
                _refLink = value;
                RaisePropertyChanged(() => RefLink);
            }
        }

        [XmlIgnore] public Driver Driver { get; set; }

        //tasklist

        private bool _useProxy;

        public bool UseProxy
        {
            get => _useProxy;
            set
            {
                _useProxy = value;
                RaisePropertyChanged(() => UseProxy);
            }
        }

        [XmlIgnore] public string Session  => Driver?.GetSession();
        [XmlIgnore] public int    PlayerId => Driver?.GetPlayerId() ?? -1;

        public Account()
        {
            Password = "KuroNeko2112";
            Player   = new Player(this);
        }

        public void Run(Account acc)
        {
            if (Running) return;
            Logger.Info($"[{Name}]: Browser run");
            Driver = new Driver();
            ThreadPool.QueueUserWorkItem(state => {
                                             Driver.Init(acc);
                                             Driver.Chrome.Navigate().GoToUrl(!string.IsNullOrEmpty(acc.RefLink) ? acc.RefLink : $"https://{g.Settings.Server}.{g.Settings.Domain}");

                                             var sel = g.Accounts.SelectedAccount;
                                             g.Accounts.SelectedAccount = null;
                                             g.Accounts.SelectedAccount = sel;
            });
        }

        public void Stop()
        {
            if (!Running) return;
            Logger.Info($"[{Name}]: Browser stop");
            Driver.DeInit();
            Driver = null;
        }

        public bool UpdateAll()
        {
            //Logger.Info($"[{Name}]: Update all");
            Driver.GetCache_All();

            //if (data == null || data.cache == null || data.cache.Count == 0 || data.time == null)
            //{
            //    Logger.Info($"[{Name}]: Update all FAILED");
            //    return false;
            //}

            //Update(data, (long) data.time);
            //data = TRequest.GetCache(Session, new List<string> {"Collection:Quest:", "Collection:HeroItem:own"});
            //Update(data, (long) data.time);
            Driver.GetCache(new List<string> {"Collection:Quest:", "Collection:HeroItem:own"});
            //Logger.Info($"[{Name}]: Update all SUCC");
            return true;
        }

        public void Update(dynamic data = null, long time = -1)
        {
            if (data == null) return;

            var playerData = Driver.GetDataArrayByName(data.cache, "Player:");
            if (playerData != null)
                foreach (var x in playerData)
                    Player.Update(x, time);

            var heroData = Driver.GetDataArrayByName(data.cache, "Hero:");
            if (heroData != null)
                foreach (var x in heroData)
                    Player.Hero.Update(x, time);

            var questListData = Driver.GetDataArrayByName(data.cache, "Collection:Quest:<>");
            if (questListData != null)
                foreach (var x in questListData)
                    Player.UpdateQuestList(x, time);

            var villageListData = Driver.GetDataArrayByName(data.cache, "Collection:Village:own");
            if (villageListData != null)
                foreach (var x in villageListData)
                    Player.UpdateVillageList(x, time);

            var villageData = Driver.GetDataArrayByName(data.cache, "Village:");
            if (villageData != null)
            {
                foreach (var x in villageData)
                {
                    var vid = Convert.ToInt32(x.name.ToString().Split(':')[1]);
                    var village = Player.VillageList.FirstOrDefault(c => c.Id == vid);
                    if (village != null)
                        village.Update(x, time);
                    else
                        Player.VillageList.Add(new Village(this, x, time));
                }
            }

            var buildingListData = Driver.GetDataArrayByName(data.cache, "Collection:Building:");
            if (buildingListData != null)
            {
                foreach (var x in buildingListData)
                {
                    var vid = Convert.ToInt32(x.name.ToString().Split(':')[2]);
                    var village = Player.VillageList.FirstOrDefault(c => c.Id == vid);
                    if (village != null)
                        village.UpdateBuildingList(x, time);
                }
            }

            var buildingData = Driver.GetDataArrayByName(data.cache, "Building:");
            if (buildingData != null)
            {
                foreach (var x in buildingData)
                {
                    var vid = Convert.ToInt32(x.data.villageId);
                    var bid = Convert.ToInt32(x.name.ToString().Split(':')[1]);
                    var village = Player.VillageList.FirstOrDefault(c => c.Id == vid);
                    if (village != null)
                    {
                        var building = village.BuildingList.FirstOrDefault(c => c.Id == bid);
                        if (building != null)
                            building.Update(x, time);
                        else
                            village.BuildingList.Add(new Building(this, village, x, time));
                    }
                }
            }

            var buildingQueueData = Driver.GetDataArrayByName(data.cache, "BuildingQueue:<>");
            if (buildingQueueData != null)
            {
                foreach (var x in buildingQueueData)
                {
                    var vid = Convert.ToInt32(x.data.villageId);
                    var village = Player.VillageList.FirstOrDefault(c => c.Id == vid);
                    if (village != null)
                    {
                        village.UpdateBuildingQueue(x, time);
                    }
                }
            }

            var stationaryTroopsData = Driver.GetDataArrayByName(data.cache, "Collection:Troops:stationary:<>");
            if (stationaryTroopsData != null)
            {
                foreach (var x in stationaryTroopsData)
                {
                    var vid = Convert.ToInt32(x.name.ToString().Split(':')[3]);
                    var village = Player.VillageList.FirstOrDefault(c => c.Id == vid);
                    if (village != null)
                    {
                        village.UpdateStationaryTroops(x, time);
                    }
                }
            }

            var movingTroopsData = Driver.GetDataArrayByName(data.cache, "Collection:Troops:moving:<>");
            if (movingTroopsData != null)
            {
                foreach (var x in movingTroopsData)
                {
                    var vid = Convert.ToInt32(x.name.ToString().Split(':')[3]);
                    var village = Player.VillageList.FirstOrDefault(c => c.Id == vid);
                    if (village != null)
                    {
                        village.UpdateMovingTroops(x, time);
                    }
                }
            }

            var heroItemsData = Driver.GetDataArrayByName(data.cache, "Collection:HeroItem:own");
            if (heroItemsData != null)
                foreach (var x in heroItemsData)
                    Player.Hero.UpdateItems(x);

            //Player.Update();
            //Player.UpdateQuestList();
            //Player.UpdateVillageList();
            //Player.Hero.Update();
            //Player.Hero.UpdateItems();
            //Player.VillageList[0].Update();
            //Player.VillageList[0].UpdateBuildingList();
            //Player.VillageList[0].UpdateBuildingQueue();
            //Player.VillageList[0].UpdateMovingTroops();
            //Player.VillageList[0].UpdateStationaryTroops();
            //Player.VillageList[1].Update();
            //Player.VillageList[1].UpdateBuildingList();
            //Player.VillageList[1].UpdateBuildingQueue();
            //Player.VillageList[1].UpdateMovingTroops();
            //Player.VillageList[1].UpdateStationaryTroops();
            //bq
        }

    }
}
