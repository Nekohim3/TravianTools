using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravianTools.Data;

namespace TravianTools.TravianCommands
{
    public abstract class BaseCommand
    {
        public virtual Account Account { get; set; }

        protected BaseCommand(Account account)
        {
            Account = account;
        }
        public abstract void    Execute();
    }
}
