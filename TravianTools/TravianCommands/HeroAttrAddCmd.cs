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
    public class HeroAttrAddCmd : BaseCommand
    {
        public int FightStrengthPoints  {get;set;}
        public int AttBonusPoints       {get;set;}
        public int DefBonusPoints       {get;set;}
        public int ResBonusPoints       {get;set;}
        public int ResBonusType { get; set; }

        public HeroAttrAddCmd() : base(null, typeof(HeroAttrAddCmd))
        {
            
        }

        public HeroAttrAddCmd(Account acc, int fightStrengthPoints, int attBonusPoints, int defBonusPoints, int resBonusPoints, int resBonusType) : base(acc, typeof(HeroAttrAddCmd))
        {
            FightStrengthPoints = fightStrengthPoints;
            AttBonusPoints      = attBonusPoints;
            DefBonusPoints      = defBonusPoints;
            ResBonusPoints      = resBonusPoints;
            ResBonusType        = resBonusType;
        }

        public override void Execute()
        {
            //Http.Post(RPG.HeroAttribute(Account.Session, FightStrengthPoints, AttBonusPoints, DefBonusPoints, ResBonusPoints, ResBonusType));
        }
    }
}
