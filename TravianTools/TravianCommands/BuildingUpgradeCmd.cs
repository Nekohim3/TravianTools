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

        private bool _buildInst;

        public bool BuildInst
        {
            get => _buildInst;
            set
            {
                _buildInst = value;
                RaisePropertyChanged(() => BuildInst);
            }
        }

        public BuildingUpgradeCmd() : base(null)
        {
            
        }

        public BuildingUpgradeCmd(Account acc, int villageId, int locationId, int buildingType, bool useNpc, bool buildInst) : base(acc)
        {
            VillageId    = villageId;
            LocationId   = locationId;
            BuildingType = buildingType;
            UseNpc       = useNpc;
            BuildInst    = buildInst;
        }

        public override bool Execute()
        {
            Village village;
            if (VillageId < 0 && Account.Player.VillageList.Count < Math.Abs(VillageId))
            {
                Account.Player.UpdateVillageList();
                village   = Account.Player.VillageList[Math.Abs(VillageId)];
                VillageId = village.Id;
            }
            else
            {
                return false;
            }

            village.UpdateBuildingList();
            
            var building = village.BuildingList.FirstOrDefault(x => x.BuildingType == BuildingType);
            if (building == null)
            {
                if (LocationId == 0)
                {
                    var freePlaceList = village.BuildingList.Where(x => x.BuildingType == 0).ToList();
                    LocationId = freePlaceList[g.Rand.Next(0, freePlaceList.Count)].Location;
                }

                building   = new Building(null) {UpgradeCost = BuildingsData.GetById(BuildingType).BuildRes};
            }
            else
                LocationId = building.Location;

            if (!building.IsRuin)
            {
                village.Update();
                if (UseNpc)
                {
                    while (village.Storage.MultiRes < building.UpgradeCost.MultiRes)
                    {
                        Thread.Sleep(10000);
                        if (!Account.TaskListExecutor.Working) return false;
                        village.Update();
                    }
                }
                else
                {
                    while (!village.Storage.IsGreaterOrEq(building.UpgradeCost))
                    {
                        Thread.Sleep(10000);
                        if (!Account.TaskListExecutor.Working) return false;
                        village.Update();
                    }
                }

                village.UpdateBuildingQueue();
                while (village.Queue.FreeSlots.First(x => x.id == 1).freeCount == 0 ||
                       village.Queue.FreeSlots.First(x => x.id == 2).freeCount == 0)
                {
                    Thread.Sleep(10000);
                    if (!Account.TaskListExecutor.Working) return false;
                    village.UpdateBuildingQueue();
                }

                village.Update();

                if (UseNpc && !village.Storage.IsGreaterOrEq(building.UpgradeCost))
                {
                    if (village.Storage.AddProduction(village.Production).IsGreaterOrEq(building.UpgradeCost))
                        Thread.Sleep(5 * 1000 * 60);
                    else
                        Account.Driver.NpcTrade(VillageId, village.Storage.Npc(building.UpgradeCost));
                }
            }

            Account.Driver.BuildingUpgrade(VillageId, LocationId, BuildingType);

            if (BuildInst)
            {
                var q = village.Queue.Queue.First(x => x.idq == (LocationId > 18 ? 2 : 1));
                if (village.Queue.UpdateTimeStamp + 295 >= q.finishTime)
                    Account.Driver.FinishNow(VillageId, q.idq, 0);
                else
                    Account.Driver.FinishNow(VillageId, q.idq, Account.Player.Hero.HasBuildItem ? -1 : 1);
            }

            return true;
        }
    }
    
    
}
