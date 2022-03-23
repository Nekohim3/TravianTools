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
    public class UseHeroItem : BaseCommand
    {
        public int Amount    { get; set; }
        public int Id        { get; set; }
        public int VillageId { get; set; }
        public UseHeroItem() : base(null)
        {

        }

        public UseHeroItem(Account acc, int amount, int id, int villageId) : base(acc)
        {
            Amount    = amount;
            Id        = id;
            VillageId = villageId;
        }
        public override void Execute()
        {
            //Http.Post(RPG.UseHeroItem(Account.Session, Amount, Id, VillageId));
        }
    }
}
