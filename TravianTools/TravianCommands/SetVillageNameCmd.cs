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
    public class SetVillageNameCmd : BaseCommand
    {
        public int    VillageId   { get; set; }
        public string VillageName { get; set; }

        public SetVillageNameCmd() : base(null, typeof(SetVillageNameCmd))
        {
            
        }

        public SetVillageNameCmd(Account acc, int villageId, string name) : base(acc, typeof(SetVillageNameCmd))
        {
            VillageId  = villageId;
            VillageName = name;
        }

        public override void Execute()
        {
            //Http.Post(RPG.SetVillageName(Account.Session, VillageId, VillageName));
        }
    }
}
