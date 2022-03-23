using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TravianTools.Data;
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

        public BuildingUpgradeCmd(Account acc, int villageId, int locationId, int buildingType) : base(acc)
        {
            VillageId    = villageId;
            LocationId   = locationId;
            BuildingType = buildingType;
        }

        public override void Execute()
        {
            if (VillageId < 0 && Account.Player.VillageList.Count < Math.Abs(VillageId))
            {
                Account.Player.UpdateVillageList();
                VillageId = Account.Player.VillageList[Math.Abs(VillageId)].Id;
            }



            Account.Driver.BuildingUpgrade(VillageId, LocationId, BuildingType);
        }
    }
    
    
}
