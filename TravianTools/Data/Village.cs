﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.ViewModel;
using Newtonsoft.Json.Linq;
using TravianTools.TravianUtils;
using TravianTools.Utils;

namespace TravianTools.Data
{
    public class Village : BaseTravianEntity
    {

        private int _id;

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged(() => Id);
            }
        }

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

        private int _x;

        public int X
        {
            get => _x;
            set
            {
                _x = value;
                RaisePropertyChanged(() => X);
            }
        }

        private int _y;

        public int Y
        {
            get => _y;
            set
            {
                _y = value;
                RaisePropertyChanged(() => Y);
            }
        }

        private Resource _production = new Resource();

        public Resource Production
        {
            get => _production;
            set
            {
                _production = value;
                RaisePropertyChanged(() => Production);
            }
        }

        private Resource _storage = new Resource();

        public Resource Storage
        {
            get => _storage;
            set
            {
                _storage = value;
                RaisePropertyChanged(() => Storage);
            }
        }

        private Resource _capacity = new Resource();

        public Resource Capacity
        {
            get => _capacity;
            set
            {
                _capacity = value;
                RaisePropertyChanged(() => Capacity);
            }
        }

        private ObservableCollection<Building> _buildingList = new ObservableCollection<Building>();

        public ObservableCollection<Building> BuildingList
        {
            get => _buildingList;
            set
            {
                _buildingList = value;
                RaisePropertyChanged(() => BuildingList);
            }
        }

        private ObservableCollection<Troops> _movingTroopList = new ObservableCollection<Troops>();

        public ObservableCollection<Troops> MovingTroopList
        {
            get => _movingTroopList;
            set
            {
                _movingTroopList = value;
                RaisePropertyChanged(() => MovingTroopList);
            }
        }

        private ObservableCollection<Troops> _stationaryTroopList = new ObservableCollection<Troops>();

        public ObservableCollection<Troops> StationaryTroopList
        {
            get => _stationaryTroopList;
            set
            {
                _stationaryTroopList = value;
                RaisePropertyChanged(() => StationaryTroopList);
            }
        }

        private BuildingQueue _queue;

        public BuildingQueue Queue
        {
            get => _queue;
            set
            {
                _queue = value;
                RaisePropertyChanged(() => Queue);
            }
        }

        private Account _account;

        public Account Account
        {
            get => _account;
            set
            {
                _account = value;
                RaisePropertyChanged(() => Account);
            }
        }

        public Village(Account acc)
        {
            Account = acc;
        }

        public Village(Account acc, dynamic data, long time)
        {
            Account = acc;
            Update(data, time);
        }

        public void UpdateBuildingList(dynamic data = null, long time = -1)
        {
            Logger.Info($"[{Account.Name}:{Name}]: BuildingList update start");
            if (data == null && time == -1)
            {
                Logger.Info($"[{Account.Name}:{Name}]: BuildingList update load data");
                Account.Driver.GetCache_BuildingCollection(Id);
                return;
            }

            var buildingIdList = new List<int>();

            foreach (var x in data.data.cache)
            {
                if (BuildingList.Count(c => c.Id == Convert.ToInt32(x.name.ToString().Split(':')[1])) == 0)
                    BuildingList.Add(new Building(Account, this, x, time));
                else
                    BuildingList.First(c => c.Id == Convert.ToInt32(x.name.ToString().Split(':')[1])).Update(x, time);
                buildingIdList.Add(Convert.ToInt32(x.name.ToString().Split(':')[1]));
            }

            foreach (var x in BuildingList.ToList().Where(x => !buildingIdList.Contains(x.Id)))
                BuildingList.Remove(x);

            Logger.Info($"[{Account.Name}:{Name}]: BuildingList update SUCC");
        }

        public void UpdateBuildingQueue(dynamic data = null, long time = -1)
        {
            Logger.Info($"[{Account.Name}:{Name}]: BuildingQueue update start");
            if (data == null && time == -1)
            {
                Logger.Info($"[{Account.Name}:{Name}]: BuildingQueue update load data");
                Account.Driver.GetCache_BuildingQueue(Id);
                return;
            }

            Queue = new BuildingQueue(data.data, time);
            Logger.Info($"[{Account.Name}:{Name}]: BuildingQueue update SUCC");
        }

        public void UpdateMovingTroops(dynamic data = null, long time = -1)
        {
            Logger.Info($"[{Account.Name}:{Name}]: MovingTroops update start");
            if (data == null && time == -1)
            {
                Logger.Info($"[{Account.Name}:{Name}]: MovingTroops update load data");
                Account.Driver.GetCache_MovingTroopsCollection(Id);
                return;
            }

            MovingTroopList.Clear();
            foreach (var x in data.data.cache)
                if (x.data.units.Count != 0)
                {
                    var t = new Troops(x.data.units, time);
                    if (!t.IsMineMerchTroop && !t.IsIncomingTroop)
                        MovingTroopList.Add(t);
                }

            Logger.Info($"[{Account.Name}:{Name}]: MovingTroops update SUCC");
        }

        public void UpdateStationaryTroops(dynamic data = null, long time = -1)
        {
            Logger.Info($"[{Account.Name}:{Name}]: StationaryTroops update start");
            if (data == null && time == -1)
            {
                Logger.Info($"[{Account.Name}:{Name}]: StationaryTroops update load data");
                Account.Driver.GetCache_StationaryTroopsCollection(Id);
                return;
            }

            StationaryTroopList.Clear();
            foreach (var x in data.data.cache)
                if (x.data.units.Count != 0)
                {
                    var t = new Troops(x.data.units, time);
                    if (!t.IsMineMerchTroop && !t.IsIncomingTroop)
                        StationaryTroopList.Add(t);
                }

            Logger.Info($"[{Account.Name}:{Name}]: StationaryTroops update SUCC");
        }

        public void Update(dynamic data = null, long time = -1)
        {
            Logger.Info($"[{Account.Name}:{Name}]: Village update start");
            if (data == null && time == -1)
            {
                Logger.Info($"[{Account.Name}:{Name}]: Village update load data");
                Account.Driver.GetCache_Building(Id);
                return;
            }

            Id   = data.data.villageId;
            Name = data.data.name;
            X    = data.data.coordinates.x;
            Y    = data.data.coordinates.y;

            Storage = new Resource(data.data.storage["1"],
                                   data.data.storage["2"],
                                   data.data.storage["3"],
                                   data.data.storage["4"],
                                   time);
            Capacity = new Resource(data.data.storageCapacity["1"],
                                    data.data.storageCapacity["2"],
                                    data.data.storageCapacity["3"],
                                    data.data.storageCapacity["4"],
                                    time);
            Production = new Resource(data.data.production["1"],
                                      data.data.production["2"],
                                      data.data.production["3"],
                                      data.data.production["4"],
                                      time);

            UpdateTime      = DateTime.Now;
            UpdateTimeStamp = time;

            Logger.Info($"[{Account.Name}:{Name}]: Village update SUCC");
        }
    }
}
