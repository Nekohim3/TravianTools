using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Practices.Prism.ViewModel;
using TravianTools.TravianCommands;
using TravianTools.Utils.DataBase;

namespace TravianTools.ViewModels
{
    public class BuildingUpgradeEditViewModel : NotificationObject
    {
        private BuildingUpgradeCmd _currentCmd;

        public BuildingUpgradeCmd CurrentCmd
        {
            get => _currentCmd;
            set
            {
                _currentCmd = value;
                RaisePropertyChanged(() => CurrentCmd);
            }
        }

        private TempTask _currentTask;

        public TempTask CurrentTask
        {
            get => _currentTask;
            set
            {
                _currentTask = value;
                CurrentCmd   = (BuildingUpgradeCmd)_currentTask.Command;
                RaisePropertyChanged(() => CurrentTask);
            }
        }

        private bool _isEdit;

        public bool IsEdit
        {
            get => _isEdit;
            set
            {
                _isEdit = value;
                RaisePropertyChanged(() => IsEdit);
            }
        }
    }
}
