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
    public class NpcTradeCmd : BaseCommand
    {
        public int VillageId  {get;set;}
        public int R1         {get;set;}
        public int R2         {get;set;}
        public int R3         {get;set;}
        public int R4 { get; set; }

        public NpcTradeCmd() : base(null)
        {
            
        }

        public NpcTradeCmd(Account acc, int villageId, int r1, int r2, int r3, int r4) : base(acc)
        {
            VillageId = villageId;
            R1 = r1;
            R2 = r2;
            R3 = r3;
            R4 = r4;
        }

        public override void Execute()
        {
            //Http.Post(RPG.NpcTrade(Account.Session, VillageId, R1, R2, R3, R4));
        }
    }
}
