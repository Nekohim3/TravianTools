using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TravianTools.Data;
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


        public BuildingDestroyCmd() : base(null)
        {
            
        }

        public BuildingDestroyCmd(Account acc, int villageId, int locationId, int buildingType, bool useNpc) : base(acc)
        {
            VillageId    = villageId;
            LocationId   = locationId;
            BuildingType = buildingType;
            UseNpc       = useNpc;
        }

        public override void Execute()
        {
            if (VillageId <= 0)
            {
                //Account.Player.UpdateAll();
                VillageId = Account.Player.VillageList[Math.Abs(VillageId)].Id;
            }

            //Http.Post(RPG.BuildingDestroy(Account.Session, VillageId, LocationId));
        }
    }
}
