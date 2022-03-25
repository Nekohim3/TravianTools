using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.ViewModel;
using Newtonsoft.Json;
using TravianTools.Data.StaticData;
using TravianTools.TravianCommands;

namespace TravianTools.Utils.DataBase
{
    public class TempTask : NotificationObject
    {
        private int _id;

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged(() => Id);
            }
        }

        private string _desc;
        [JsonIgnore]
        public string Desc
        {
            get => _desc;
            set
            {
                _desc = value;
                RaisePropertyChanged(() => Desc);
            }
        }

        private int _tempTaskListId;

        public int TempTaskListId
        {
            get => _tempTaskListId;
            set
            {
                _tempTaskListId = value;
                RaisePropertyChanged(() => TempTaskListId);
            }
        }

        private BaseCommand _command;

        public BaseCommand Command
        {
            get => _command;
            set
            {
                _command = value;
                RaisePropertyChanged(() => Command);
            }
        }

        private string _commandString;
        [JsonIgnore]
        public string CommandString
        {
            get => _commandString;
            set
            {
                _commandString = value;
                RaisePropertyChanged(() => CommandString);
            }
        }

        private CommandType _commandType;
        [JsonIgnore]
        public CommandType CommandType
        {
            get => _commandType;
            set
            {
                _commandType = value;
                RaisePropertyChanged(() => CommandType);
            }
        }
    }
}
