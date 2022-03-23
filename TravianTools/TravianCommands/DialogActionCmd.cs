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
    public class DialogActionCmd : BaseCommand
    {
        public int    QuestId    {get;set;}
        public int    DialogId   {get;set;}
        public string Command    {get;set;}
        public string Input { get; set; }

        public DialogActionCmd() : base(null)
        {
            
        }

        public DialogActionCmd(Account acc, int questId, int dialogId, string command, string input = "") : base(acc)
        {
            QuestId    = questId;
            DialogId   = dialogId;
            Command    = command;
            Input = input;
        }

        public override void Execute()
        {
            Http.Post(RPG.DialogAction(Account.Session, QuestId, DialogId, Command, Input));
        }
    }
}
