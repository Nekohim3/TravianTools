using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TravianTools.Data;
using TravianTools.Data.StaticData;
using TravianTools.TravianUtils;

namespace TravianTools.TravianCommands
{
    [Serializable]
    public class BuildingDestroyCmd : BaseCommand
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

        private bool _disassemble;

        public bool Disassemble
        {
            get => _disassemble;
            set
            {
                _disassemble = value;
                RaisePropertyChanged(() => Disassemble);
            }
        }

        private bool _disassembleInst;

        public bool DisassembleInst
        {
            get => _disassembleInst;
            set
            {
                _disassembleInst = value;
                RaisePropertyChanged(() => DisassembleInst);
            }
        }


        public BuildingDestroyCmd() : base(null)
        {
            
        }

        public BuildingDestroyCmd(Account acc, int villageId, int buildingType, bool buildInst, bool dis, bool disInst) : base(acc)
        {
            VillageId    = villageId;
            BuildingType = buildingType;
            BuildInst    = buildInst;
            Disassemble = dis;
            DisassembleInst = disInst;
        }

        public override bool Execute()
        {
            var     locId = 0;
            Village village;
            if (VillageId < 0 && Account.Player.VillageList.Count < Math.Abs(VillageId))
            {
                Account.Player.UpdateVillageList();
                village = Account.Player.VillageList[Math.Abs(VillageId)];
                VillageId = village.Id;
            }
            else
            {
                return false;
            }

            village.UpdateBuildingList();

            var gz       = village.BuildingList.FirstOrDefault(x => x.BuildingType == 15);
            if (gz == null) return false;
            var building = village.BuildingList.FirstOrDefault(x => x.BuildingType == BuildingType);
            if (building == null)
            {
                return false;
            }
            else
                locId = building.Location;

            village.UpdateBuildingQueue();
            while (village.Queue.FreeSlots.First(x => x.id == 5).freeCount == 0)
            {
                Thread.Sleep(10000);
                if (!Account.TaskListExecutor.Working) return false;
                village.UpdateBuildingQueue();
            }

            if (building.IsRuin) return false;

            Account.Driver.BuildingDestroy(VillageId, locId);

            if (BuildInst)
            {
                var q = village.Queue.Queue.First(x => x.idq == 5);
                if (village.Queue.UpdateTimeStamp + 295 >= q.finishTime)
                    Account.Driver.FinishNow(VillageId, q.idq, 0);
                else
                    Account.Driver.FinishNow(VillageId, q.idq, Account.Player.Hero.HasBuildItem ? -1 : 1);
            }

            if (Disassemble)
            {
                if (!BuildInst)
                {
                    village.UpdateBuildingQueue();
                    while (village.Queue.FreeSlots.First(x => x.id == 5).freeCount == 0)
                    {
                        Thread.Sleep(10000);
                        if (!Account.TaskListExecutor.Working) return false;
                        village.UpdateBuildingQueue();
                    }
                }

                Account.Driver.BuildingUpgrade(VillageId, locId, BuildingType);

                if (DisassembleInst)
                {
                    var q = village.Queue.Queue.First(x => x.idq == 2);
                    if (village.Queue.UpdateTimeStamp + 295 >= q.finishTime)
                        Account.Driver.FinishNow(VillageId, q.idq, 0);
                    else
                        Account.Driver.FinishNow(VillageId, q.idq, Account.Player.Hero.HasBuildItem ? -1 : 1);
                }
            }

            return true;
        }
    }
}
