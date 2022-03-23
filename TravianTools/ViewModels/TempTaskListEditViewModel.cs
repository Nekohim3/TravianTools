using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.ViewModel;
using TravianTools.Utils.DataBase;

namespace TravianTools.ViewModels
{
    public class TempTaskListEditViewModel : NotificationObject
    {
        private TempTaskList _currentTaskList;

        public TempTaskList CurrentTaskList
        {
            get => _currentTaskList;
            set
            {
                _currentTaskList = value;
                RaisePropertyChanged(() => CurrentTaskList);
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

        public TempTaskListEditViewModel()
        {
            
        }

        public void SetEntity(TempTaskList ttl)
        {
            IsEdit          = false;
            CurrentTaskList = ttl == null ? null : g.TempTaskListService.GetById(ttl.Id);
        }

        public void InitActionAdd()
        {
            CurrentTaskList = new TempTaskList() {Id = g.TempTaskListService.GetNewId()};
            IsEdit          = true;
        }

        public void InitActionEdit()
        {
            IsEdit = true;
        }
    }
}
