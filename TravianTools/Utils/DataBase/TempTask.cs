using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.ViewModel;
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
    }
}
