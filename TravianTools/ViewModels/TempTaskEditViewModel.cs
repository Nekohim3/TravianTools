using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.ViewModel;
using TravianTools.Data;
using TravianTools.TravianCommands;
using TravianTools.Utils.DataBase;

namespace TravianTools.ViewModels
{
    public class TempTaskEditViewModel : NotificationObject
    {
        private TempTask _currentTask;

        public TempTask CurrentTask
        {
            get => _currentTask;
            set
            {
                _currentTask = value;
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

        private bool _outerVisible;

        public bool OuterVisible
        {
            get => _outerVisible;
            set
            {
                _outerVisible = value;
                RaisePropertyChanged(() => OuterVisible);
            }
        }

        private bool _innerVisible;

        public bool InnerVisible
        {
            get => _innerVisible;
            set
            {
                _innerVisible = value;
                RaisePropertyChanged(() => InnerVisible);
            }
        }

        private bool _isTemplate;

        public bool IsTemplate
        {
            get => _isTemplate;
            set
            {
                _isTemplate = value;
                RaisePropertyChanged(() => IsTemplate);
            }
        }

        

        public TempTaskEditViewModel()
        {

        }

        public void SetEntity(TempTask tt)
        {
            IsEdit      = false;
            CurrentTask = tt == null ? null : g.TempTaskService.GetById(tt.Id);
        }

        public void InitActionAdd(TempTask t)
        {
            CurrentTask = t;
            IsEdit      = true;
        }

        public void InitActionEdit()
        {
            IsEdit = true;
        }
    }
}
