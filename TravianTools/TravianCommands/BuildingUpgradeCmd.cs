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
    public class BuildingUpgradeCmd : ResourceCommand
    {
        public int VillageId       {get;set;}
        public int LocationId      {get;set;}
        public int BuildingType { get; set; }
        //Использовать нпц обмен
            //остальные ресы
                //поровну
                //ввести вручную
                //% текущему
                //% задать вручную
                //
                //

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
            if (VillageId <= 0 && Account.Player.VillageList.Count < Math.Abs(VillageId) + 1)
            {
                Account.Player.UpdateVillageList();
                VillageId = Account.Player.VillageList[Math.Abs(VillageId)].Id;
            }



            Account.Driver.BuildingUpgrade(VillageId, LocationId, BuildingType);
        }
    }

    public abstract class ResourceCommand : BaseCommand
    {
        protected ResourceCommand(Account account) : base(account)
        {

        }
    }
}
