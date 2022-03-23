using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.ViewModel;
using TravianTools.TravianCommands;

namespace TravianTools.Task
{
    [Serializable]
    public class TemplateTask : NotificationObject
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
