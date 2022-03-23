﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.ViewModel;
using TravianTools.TravianUtils;

namespace TravianTools.Data
{
    public class Building : BaseTravianEntity
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

        private int _buildingType;

        public int BuildingType
        {
            get => _buildingType;
            set
            {
                _buildingType = value;
                RaisePropertyChanged(() => BuildingType);
            }
        }

        private int _location;

        public int Location
        {
            get => _location;
            set
            {
                _location = value;
                RaisePropertyChanged(() => Location);
            }
        }

        private int _level;

        public int Level
        {
            get => _level;
            set
            {
                _level = value;
                RaisePropertyChanged(() => Level);
                RaisePropertyChanged(() => IsMaxLevel);
            }
        }

        private int _maxLevel;

        public int MaxLevel
        {
            get => _maxLevel;
            set
            {
                _maxLevel = value;
                RaisePropertyChanged(() => MaxLevel);
                RaisePropertyChanged(() => IsMaxLevel);
            }
        }

        public bool IsMaxLevel => Level == MaxLevel;

        private Village _village;

        public Village Village
        {
            get => _village;
            set
            {
                _village = value;
                RaisePropertyChanged(() => Village);
            }
        }

        private Resource _upgradeCost;

        public Resource UpgradeCost
        {
            get => _upgradeCost;
            set
            {
                _upgradeCost = value;
                RaisePropertyChanged(() => UpgradeCost);
            }
        }

        private long _upgradeTime;

        public long UpgradeTime
        {
            get => _upgradeTime;
            set
            {
                _upgradeTime = value;
                RaisePropertyChanged(() => UpgradeTime);
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

        public Building(Account acc)
        {
            Account = acc;
        }

        public Building(Account acc, Village v, dynamic data, long time)
        {
            Account = acc;
            Village = v;
            Update(data, time);
        }

        public void Update(dynamic data = null, long time = -1)
        {
            if (data == null && time == -1)
            {
                data = TRequest.GetCache_Building(Account.Session, Id);
                if (data == null) return;
                time = data.time;
                data = TRequest.GetDataByName(data, $"Building:{Id}");
            }

            if (data == null) return;

            Id           = Convert.ToInt32(data.name.ToString().Split(':')[1]);
            BuildingType = Convert.ToInt32(data.data.buildingType);
            Location     = Convert.ToInt32(data.data.locationId);
            Level        = Convert.ToInt32(data.data.lvl);
            MaxLevel     = data.data.lvlMax;
            UpgradeCost = new Resource(
                                       data.data.upgradeCosts["1"],
                                       data.data.upgradeCosts["2"],
                                       data.data.upgradeCosts["3"],
                                       data.data.upgradeCosts["4"],
                                       time
                                      );
            UpgradeTime = data.data.upgradeTime;

            UpdateTime      = DateTime.Now;
            UpdateTimeStamp = time;

        }

        public Building(int buildingType, int locationId)
        {
            BuildingType = buildingType;
            Location     = locationId;
        }
    }
}
