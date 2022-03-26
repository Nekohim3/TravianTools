﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using Newtonsoft.Json;
using TravianTools.Data;
using TravianTools.TravianCommands;
using TravianTools.TravianUtils;
using TravianTools.Utils.DataBase;
using TravianTools.Views;

namespace TravianTools.ViewModels
{
    public class MainWindowViewModel : NotificationObject
    {
        #region Properties

        private Account _addAccount;

        public Account AddAccount
        {
            get => _addAccount;
            set
            {
                _addAccount = value;
                RaisePropertyChanged(() => AddAccount);
            }
        }

        private bool _showAccounts;

        public bool ShowAccounts
        {
            get => _showAccounts;
            set
            {
                _showAccounts = value;
                RaisePropertyChanged(() => ShowAccounts);
            }
        }

        private bool _showTaskList;

        public bool ShowTaskList
        {
            get => _showTaskList;
            set
            {
                _showTaskList = value;
                RaisePropertyChanged(() => ShowTaskList);
            }
        }

        private bool _showData;

        public bool ShowData
        {
            get => _showData;
            set
            {
                _showData = value;
                RaisePropertyChanged(() => ShowData);
            }
        }

        private ObservableCollection<TempTaskList> _taskListList;

        public ObservableCollection<TempTaskList> TaskListList
        {
            get => _taskListList;
            set
            {
                _taskListList = value;
                RaisePropertyChanged(() => TaskListList);
            }
        }

        private TempTaskList _selectedTaskList;

        public TempTaskList SelectedTaskList
        {
            get => _selectedTaskList;
            set
            {
                _selectedTaskList = value;
                RaisePropertyChanged(() => SelectedTaskList);
                if (value != null)
                {
                    g.Accounts.SelectedAccount.CurrentTaskListId = value.Id;
                    if (value.Id == 0)
                        g.Accounts.SelectedAccount.CurrentTaskId = 0;
                    TaskList                                     = new ObservableCollection<TempTask>(g.TempTaskService.GetAllByTaskListId(value.Id));
                    SelectedTask                                 = TaskList.FirstOrDefault(x => x.Id == g.Accounts.SelectedAccount.CurrentTaskId);
                    Accounts.Save();
                }

            }
        }

        private ObservableCollection<TempTask> _taskList;

        public ObservableCollection<TempTask> TaskList
        {
            get => _taskList;
            set
            {
                _taskList = value;
                RaisePropertyChanged(() => TaskList);
            }
        }

        private TempTask _selectedTask;

        public TempTask SelectedTask
        {
            get => _selectedTask;
            set
            {
                _selectedTask = value;
                RaisePropertyChanged(() => SelectedTask);
                if (value != null)
                {
                    g.Accounts.SelectedAccount.CurrentTaskId = value.Id;
                    Accounts.Save();
                }
            }
        }

        #endregion

        #region Commands

        public DelegateCommand AddAccountCmd    { get; }
        public DelegateCommand RemoveAccountCmd { get; }
        public DelegateCommand SaveAccountCmd   { get; }
        public DelegateCommand CancelAccountCmd { get; }

        public DelegateCommand ShowAccountsCmd { get; }
        public DelegateCommand SettingsCmd        { get; }

        public DelegateCommand StartBrowserCmd { get; }
        public DelegateCommand StopBrowserCmd  { get; }

        public DelegateCommand TaskListCmd       { get; }
        public DelegateCommand TaskListEditorCmd { get; }
        public DelegateCommand ShowDataCmd       { get; }

        public DelegateCommand TestUpdateCmd { get; }

        #endregion

        #region Ctor

        public MainWindowViewModel()
        {
            AddAccountCmd    = new DelegateCommand(OnAddAccount);
            RemoveAccountCmd = new DelegateCommand(OnRemoveAccount, () => g.Accounts.SelectedAccount != null);
            SaveAccountCmd   = new DelegateCommand(OnSaveAccount);
            CancelAccountCmd = new DelegateCommand(OnCancelAccount);

            ShowAccountsCmd = new DelegateCommand(OnShowAccounts);
            SettingsCmd     = new DelegateCommand(OnSettings);

            StartBrowserCmd = new DelegateCommand(OnStartBrowser, () => g.Accounts.SelectedAccount != null);
            StopBrowserCmd  = new DelegateCommand(OnStopBrowser,  () => g.Accounts.SelectedAccount != null);

            TaskListCmd       = new DelegateCommand(OnTaskList);
            TaskListEditorCmd = new DelegateCommand(OnTaskListEditor);
            ShowDataCmd       = new DelegateCommand(OnShowData);

            TestUpdateCmd = new DelegateCommand(OnTestUpdateCmd);

            ShowAccounts = true;
            ShowTaskList = true;
        }

        public void TaskListInit()
        {
            TaskListList = new ObservableCollection<TempTaskList>(g.TempTaskListService.GetAll());
            TaskListList.Insert(0, new TempTaskList() {Id = 0, Name = "Не выбрано"});
            SelectedTaskList = TaskListList.FirstOrDefault(x => x.Id == g.Accounts.SelectedAccount.CurrentTaskListId);

        }

        #endregion

        #region CmdExec

        private void OnShowData()
        {
            ShowData = !ShowData;
        }

        private void OnTestUpdateCmd()
        {
            var q = g.Accounts.SelectedAccount.Driver.PostJo(RPG.FinishBuild(g.Accounts.SelectedAccount.Driver.GetSession(), 0, 0, 1));
            //var cmd = new BuildingUpgradeCmd(g.Accounts.SelectedAccount, 0, 25, 17, false) as BaseCommand;
            //cmd.Execute();
            //g.Accounts.SelectedAccount.UpdateAll();
        }

        private void OnTaskListEditor()
        {
            var f  = new TemplateTaskListView();
            var vm = new TemplateTaskListViewModel();
            f.DataContext = vm;
            f.ShowDialog();
            
        }

        private void OnTaskList()
        {
            ShowTaskList = !ShowTaskList;
        }

        private void OnStartBrowser()
        {
            if(g.Accounts.SelectedAccount == null) return;
            g.Accounts.SelectedAccount.Run(g.Accounts.SelectedAccount);
        }

        private void OnStopBrowser()
        {
            if (g.Accounts.SelectedAccount == null) return;
            g.Accounts.SelectedAccount.Stop();
        }

        private void OnSettings()
        {
            if (MessageBox.Show("После изменения настроек приложение закроется, и нужно будет открыть заного. Продолжить?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var f  = new ServerSettingsView();
                var vm = new ServerSettingsViewModel(f.Close) {Domain = g.Settings.Domain, Server = g.Settings.Server, Proxy = g.Settings.Proxy};
                f.DataContext = vm;
                f.ShowDialog();
                Application.Current.Shutdown();
            }
        }

        private void OnShowAccounts()
        {
            ShowAccounts = !ShowAccounts;
        }

        private void OnAddAccount()
        {
            AddAccount = new Account() {UseProxy = true, UseFastBuilding = true};
        }

        private void OnRemoveAccount()
        {
            g.Accounts.Remove(g.Accounts.SelectedAccount);
        }

        private void OnSaveAccount()
        {
            if (string.IsNullOrEmpty(AddAccount.Name) || string.IsNullOrEmpty(AddAccount.Password))
            {
                MessageBox.Show("Надо заполнить логин и пароль");
                return;
            }

            if (g.Accounts.AccountList.Any(x => x.Name == AddAccount.Name))
            {
                MessageBox.Show("Аккаунт с таким именем уже существует");
                return;
            }
            g.Accounts.Add(AddAccount);
            AddAccount = null;
        }

        private void OnCancelAccount()
        {
            AddAccount = null;
        }

        #endregion

        #region Funcs

        public void RaiseCanExecChange()
        {
            AddAccountCmd.RaiseCanExecuteChanged();
            RemoveAccountCmd.RaiseCanExecuteChanged();
            SaveAccountCmd.RaiseCanExecuteChanged();
            CancelAccountCmd.RaiseCanExecuteChanged();
            StartBrowserCmd.RaiseCanExecuteChanged();
            StopBrowserCmd.RaiseCanExecuteChanged();
        }

        #endregion
    }

    [ValueConversion(typeof(object), typeof(Visibility))]
    public class ObjIsNullToVisibilityConverter :  IValueConverter
    {
        public object Convert(object                           value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
        {
            return (value != null) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object                           value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(object), typeof(Visibility))]
    public class ObjIsNullToVisibilityReverseConverter : IValueConverter
    {
        public object Convert(object                           value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
        {
            return (value != null) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object                           value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(object), typeof(bool))]
    public class ObjIsNullToBoolReverseConverter : IValueConverter
    {
        public object Convert(object                           value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object                           value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(object), typeof(bool))]
    public class ObjIsNullToBoolConverter : IValueConverter
    {
        public object Convert(object                           value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
        {
            return value == null;
        }

        public object ConvertBack(object                           value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object                           value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
        {
            return (value != null && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object                           value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityReverseConverter : IValueConverter
    {
        public object Convert(object                           value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
        {
            return (value != null && (bool)value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object                           value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(int), typeof(Visibility))]
    public class CmdTypeIdToVisibilityConverter : IValueConverter
    {
        public object Convert(object                           value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter == null) return Visibility.Collapsed;
            
            var q = (int)value;
            return q == System.Convert.ToInt32(parameter) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object                           value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
