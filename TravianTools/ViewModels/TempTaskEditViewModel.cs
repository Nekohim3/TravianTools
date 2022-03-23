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

        private ObservableCollection<Village> _villageList;

        public ObservableCollection<Village> VillageList
        {
            get => _villageList;
            set
            {
                _villageList = value;
                RaisePropertyChanged(() => VillageList);
            }
        }

        private Village _selectedVillage;

        public Village SelectedVillage
        {
            get => _selectedVillage;
            set
            {
                _selectedVillage = value;
                RaisePropertyChanged(() => SelectedVillage);
            }
        }

        private ObservableCollection<Village> _nonExistVillageList;

        public ObservableCollection<Village> NonExistVillageList
        {
            get => _nonExistVillageList;
            set
            {
                _nonExistVillageList = value;
                RaisePropertyChanged(() => NonExistVillageList);
            }
        }

        private Village _selectedNonExistVillage;

        public Village SelectedNonExistVillage
        {
            get => _selectedNonExistVillage;
            set
            {
                _selectedNonExistVillage = value;
                RaisePropertyChanged(() => SelectedNonExistVillage);
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
            g.Accounts?.SelectedAccount?.Player?.UpdateVillageList();
            //VillageList         = new ObservableCollection<Village>(g.Accounts.SelectedAccount.Player.VillageList.ToList());
            NonExistVillageList = new ObservableCollection<Village>()
                                  {
                                      new Village(null){ Id = -1, Name = "Первая"},
                                      new Village(null){ Id = -2, Name = "Вторая"},
                                      new Village(null){ Id = -3, Name = "Третья"},
                                      new Village(null){ Id = -4, Name = "Четвертая"},
                                      new Village(null){ Id = -5, Name = "Пятая"},
                                      new Village(null){ Id = -6, Name = "Шестая"},
                                      new Village(null){ Id = -7, Name = "Седьмая"},
                                      new Village(null){ Id = -8, Name = "Восьмая"},
                                      new Village(null){ Id = -9, Name = "Девятая"},
                                      new Village(null){ Id = -10, Name = "Десятая"},
                                  };
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
