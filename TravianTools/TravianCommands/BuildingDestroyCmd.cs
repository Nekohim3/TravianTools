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
        public int VillageId  { get; set; }
        public int LocationId { get; set; }

        public BuildingDestroyCmd() : base(null)
        {
            
        }

        public BuildingDestroyCmd(Account acc, int villageId, int locationId) : base(acc)
        {
            VillageId = villageId;
            LocationId = locationId;
        }
        public override void Execute()
        {
            if (VillageId <= 0)
            {
                //Account.Player.UpdateAll();
                VillageId = Account.Player.VillageList[Math.Abs(VillageId)].Id;
            }

            Http.Post(RPG.BuildingDestroy(Account.Session, VillageId, LocationId));
        }
    }
}
