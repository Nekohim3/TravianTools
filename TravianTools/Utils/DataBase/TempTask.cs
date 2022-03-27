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
    public enum TaskStatus{Wait, Exec, Finished, Error}
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

        private TaskStatus _status;
        [JsonIgnore]
        public TaskStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                RaisePropertyChanged(() => Status);
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

        private int _number;
        [JsonIgnore]
        public int Number
        {
            get => _number;
            set
            {
                _number = value;
                RaisePropertyChanged(() => Number);
            }
        }

        private string _note;

        public string Note
        {
            get => _note;
            set
            {
                _note = value;
                RaisePropertyChanged(() => Note);
            }
        }

        private BaseCommand _command;

        public BaseCommand Command
        {
            get => _command;
            set
            {
                _command = value;
                if(value != null)
                    CommandType = CommandTypesData.GetByType(Command?.GetType());
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
