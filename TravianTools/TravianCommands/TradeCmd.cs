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
    public class TradeCmd : BaseCommand
    {
        public int VillageId        {get;set;}
        public int OfferedResource  {get;set;}
        public int OfferedAmount    {get;set;}
        public int SearchedResource {get;set;}
        public int SearchedAmount   {get;set;}
        public bool KingdomOnly { get; set; }

        public TradeCmd() : base(null)
        {
            
        }

        public TradeCmd(Account acc, int villageId, int offeredResource, int offeredAmount, int searchedResource, int searchedAmount, bool kingdomOnly) : base(acc)
        {
            VillageId        = villageId;
            OfferedResource  = offeredResource;
            OfferedAmount    = offeredAmount;
            SearchedResource = searchedResource;
            SearchedAmount   = searchedAmount;
            KingdomOnly      = kingdomOnly;
        }

        public override void Execute()
        {
            //Http.Post(RPG.Trade(Account.Session, VillageId, OfferedResource, OfferedAmount, SearchedResource, SearchedAmount, KingdomOnly));
        }
    }
}
