using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TravianTools.Data;
using TravianTools.Data.StaticData;
using TravianTools.TravianUtils;

namespace TravianTools.TravianCommands
{
    [Serializable]
    public class BuildingUpgradeCmd : BaseCommand
    {
        private int _villageId;

        public int VillageId
        {
            get => _villageId;
            set
            {
                _villageId = value;
                RaisePropertyChanged(() => VillageId);
            }
        }

        private int _locationId;

        public int LocationId
        {
            get => _locationId;
            set
            {
                _locationId = value;
                RaisePropertyChanged(() => LocationId);
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

        private bool _useNpc;

        public bool UseNpc
        {
            get => _useNpc;
            set
            {
                _useNpc = value;
                RaisePropertyChanged(() => UseNpc);
            }
        }

        public BuildingUpgradeCmd() : base(null)
        {
            
        }

        public BuildingUpgradeCmd(Account acc, int villageId, int locationId, int buildingType, bool useNpc) : base(acc)
        {
            VillageId    = villageId;
            LocationId   = locationId;
            BuildingType = buildingType;
            UseNpc       = useNpc;
        }

        public override void Execute()
        {
            Village village = null;
            if (VillageId < 0 && Account.Player.VillageList.Count < Math.Abs(VillageId))
            {
                Account.Player.UpdateVillageList();
                village   = Account.Player.VillageList[Math.Abs(VillageId)];
                VillageId = village.Id;
            }

            var building = village.BuildingList.FirstOrDefault(x => x.BuildingType == BuildingType);
            if (building == null)
            {
                var freePlaceList = village.BuildingList.Where(x => x.BuildingType == 0).ToList();
                LocationId = freePlaceList[g.Rand.Next(0, freePlaceList.Count)].Location;
                building   = new Building(null) {UpgradeCost = BuildingsData.GetById(BuildingType).BuildRes};
            }
            else
                LocationId = building.Location;

            village.Update();
            if (UseNpc)
            {
                while (village.Storage.MultiRes < building.UpgradeCost.MultiRes))
                {
                    Thread.Sleep(10000);
                    village.Update();
                }
            }
            else
            {
                while (!village.Storage.IsGreaterOrEq(building.UpgradeCost))
                {
                    Thread.Sleep(10000);
                    village.Update();
                }
            }

            village.UpdateBuildingQueue();
            while (village.Queue.FreeSlots.First(x => x.id == 1).freeCount == 0 ||
                   village.Queue.FreeSlots.First(x => x.id == 2).freeCount == 0)
            {
                Thread.Sleep(10000);
                village.UpdateBuildingQueue();
            }
            
            Account.Driver.BuildingUpgrade(VillageId, LocationId, BuildingType);
        }
    }
    
    
}
