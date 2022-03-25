using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using TravianTools.Data;
using TravianTools.Data.StaticData;
using TravianTools.Task;
using TravianTools.TravianCommands;
using TravianTools.Utils;
using TravianTools.Utils.DataBase;
using TravianTools.Views;

namespace TravianTools.ViewModels
{
    public class TemplateTaskListViewModel : NotificationObject
    {
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
                TaskList          = _selectedTaskList != null ? new ObservableCollection<TempTask>(g.TempTaskService.GetAllByTaskListId(_selectedTaskList.Id)) : null;
                TempTaskListEditVM.SetEntity(_selectedTaskList);
                RaisePropertyChanged(() => SelectedTaskList);
                RaiseCanExecChanged();
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
                TempTaskEditVM.SetEntity(_selectedTask);
                RaisePropertyChanged(() => SelectedTask);
                RaiseCanExecChanged();
            }
        }

        private CommandType _selectedCmdType;

        public CommandType SelectedCmdType
        {
            get => _selectedCmdType;
            set
            {
                _selectedCmdType = value;
                RaisePropertyChanged(() => SelectedCmdType);
                RaiseCanExecChanged();
            }
        }

        public TempTaskListEditViewModel TempTaskListEditVM    { get; set; }
        public TempTaskEditViewModel     TempTaskEditVM { get; set; }

        public DelegateCommand AddTaskListCmd    { get; }
        public DelegateCommand EditTaskListCmd   { get; }
        public DelegateCommand RemoveTaskListCmd { get; }
        public DelegateCommand ExportTaskListCmd { get; }
        public DelegateCommand ImportTaskListCmd { get; }

        public DelegateCommand AddTaskCmd    { get; }
        public DelegateCommand EditTaskCmd   { get; }
        public DelegateCommand RemoveTaskCmd { get; }

        public DelegateCommand UpCmd   { get; }
        public DelegateCommand DownCmd { get; }

        public DelegateCommand SaveTaskListCmd   { get; }
        public DelegateCommand CancelTaskListCmd { get; }

        public DelegateCommand SaveTaskCmd   { get; }
        public DelegateCommand CancelTaskCmd { get; }

        public TemplateTaskListViewModel()
        {
            AddTaskListCmd    = new DelegateCommand(OnAddTaskList);
            EditTaskListCmd   = new DelegateCommand(OnEditTaskList,   () => SelectedTaskList != null);
            RemoveTaskListCmd = new DelegateCommand(OnRemoveTaskList, () => SelectedTaskList != null);
            ExportTaskListCmd = new DelegateCommand(OnExportTaskList, () => SelectedTaskList != null);
            ImportTaskListCmd = new DelegateCommand(OnImportTaskList, () => SelectedTaskList != null);

            AddTaskCmd    = new DelegateCommand(OnAddTask,    () => SelectedTaskList != null && SelectedCmdType != default && !TempTaskListEditVM.IsEdit);
            EditTaskCmd   = new DelegateCommand(OnEditTask,   () => SelectedTask     != null && !TempTaskListEditVM.IsEdit);
            RemoveTaskCmd = new DelegateCommand(OnRemoveTask, () => SelectedTask     != null && !TempTaskListEditVM.IsEdit);

            UpCmd   = new DelegateCommand(OnUp,   () => SelectedTask != null && !TempTaskListEditVM.IsEdit);
            DownCmd = new DelegateCommand(OnDown, () => SelectedTask != null && !TempTaskListEditVM.IsEdit);

            SaveTaskListCmd   = new DelegateCommand(OnSaveTaskList);
            CancelTaskListCmd = new DelegateCommand(OnCancelTaskList);
            SaveTaskCmd   = new DelegateCommand(OnSaveTask);
            CancelTaskCmd = new DelegateCommand(OnCancelTask);

            TempTaskListEditVM = new TempTaskListEditViewModel();
            TempTaskEditVM = new TempTaskEditViewModel();

            Init();
        }

        public void Init()
        {
            var ttlId = SelectedTaskList?.Id;
            var ttId  = SelectedTask?.Id;
            TaskListList     = null;
            SelectedTaskList = null;
            TaskList         = null;
            SelectedTask     = null;
            TaskListList     = new ObservableCollection<TempTaskList>(g.TempTaskListService.GetAll());
            if (ttlId.HasValue)
                SelectedTaskList = TaskListList.FirstOrDefault(x => x.Id == ttlId);

            if (ttId.HasValue && TaskList != null)
                SelectedTask = TaskList.FirstOrDefault(x => x.Id == ttId);
        }

        public void RaiseCanExecChanged()
        {
            AddTaskListCmd.RaiseCanExecuteChanged();
            EditTaskListCmd.RaiseCanExecuteChanged();
            RemoveTaskListCmd.RaiseCanExecuteChanged();
            ExportTaskListCmd.RaiseCanExecuteChanged();
            ImportTaskListCmd.RaiseCanExecuteChanged();

            AddTaskCmd.RaiseCanExecuteChanged();
            EditTaskCmd.RaiseCanExecuteChanged();
            RemoveTaskCmd.RaiseCanExecuteChanged();

            UpCmd.RaiseCanExecuteChanged();
            DownCmd.RaiseCanExecuteChanged();

            SaveTaskListCmd.RaiseCanExecuteChanged();
            CancelTaskListCmd.RaiseCanExecuteChanged();
            SaveTaskCmd.RaiseCanExecuteChanged();
            CancelTaskCmd.RaiseCanExecuteChanged();

        }

        private void OnSaveTask()
        {
            if (CommandInputChecker(TempTaskEditVM.CurrentTask.Command))
            {
                g.TempTaskService.Save(TempTaskEditVM.CurrentTask);
                Init();
            }
        }

        private void OnCancelTask()
        {

        }

        private void OnSaveTaskList()
        {
            g.TempTaskListService.Save(TempTaskListEditVM.CurrentTaskList);
            Init();
        }

        private void OnCancelTaskList()
        {
            TempTaskListEditVM.SetEntity(SelectedTaskList);
        }

        private void OnAddTaskList()
        {
            TempTaskListEditVM.InitActionAdd();
        }

        private void OnEditTaskList()
        {
            TempTaskListEditVM.InitActionEdit();
        }

        private void OnRemoveTaskList()
        {
            if (MessageBox.Show($"Точно удалить такслист \"{SelectedTaskList.Name}\"?", "", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            g.TempTaskListService.Remove(SelectedTaskList);
            Init();
        }

        private void OnExportTaskList()
        {

        }

        private void OnImportTaskList()
        {

        }

        private void OnAddTask()
        {
            var t                                  = new TempTask() { Id = g.TempTaskService.GetNewId(),
                                                                        TempTaskListId = SelectedTaskList.Id,
                                                                        CommandType = SelectedCmdType};
            if (SelectedCmdType.Id == 0) t.Command = new BuildingUpgradeCmd();
            if (SelectedCmdType.Id == 1) t.Command = new BuildingDestroyCmd();
            //if (SelectedCmdType.Id == 2) t.Command = new FinishNowCmd();
            if (SelectedCmdType.Id == 3) t.Command = new NpcTradeCmd();
            if (SelectedCmdType.Id == 4) t.Command = new TradeCmd();
            //if (SelectedCmdType.Id == 5) t.Command = new CollectRewardCmd();
            if (SelectedCmdType.Id == 6) t.Command = new SetVillageNameCmd();
            if (SelectedCmdType.Id == 7) t.Command = new UseHeroItem();
            if (SelectedCmdType.Id == 8) t.Command = new HeroAttrAddCmd();
            //if (SelectedCmdType.Id == 9) t.Command = new HeroAdvCmd();
            if (SelectedCmdType.Id == 10) t.Command = new DialogActionCmd();
            //if (SelectedCmdType.Id == 11) t.Command = new RecriutUnitsCmd();
            //if (SelectedCmdType.Id == 12) t.Command = new SendTroopCmd();
            //if (SelectedCmdType.Id == 13) t.Command = new ChooseTribeCmd();
            //if (SelectedCmdType.Id == 14) t.Command = new BuildingUpgradeCmd();
            if(t.Command != null)
                t.CommandString = CommandTypesData.GetByType(t.Command.GetType())?.Name;
            TempTaskEditVM.InitActionAdd(t);
        }

        private void OnEditTask()
        {

        }

        private void OnRemoveTask()
        {

        }

        private void OnUp()
        {

        }

        private void OnDown()
        {

        }

        #region CommandInputChecker

        public bool CommandInputChecker(BaseCommand cmd)
        {
            var errorStr = "";
            if (cmd.Type == typeof(BuildingUpgradeCmd).ToString())
            {
                var q = (BuildingUpgradeCmd)cmd;
                if (q.LocationId < 0 || q.LocationId > 45)
                    errorStr += "Позиция введена неверно\n";
                if (q.VillageId == 0)
                    errorStr += "Деревня не выбрана\n";
                if (q.BuildingType == 0)
                    errorStr += "Постройка не выбрана\n";
            }

            if (!string.IsNullOrEmpty(errorStr))
                MessageBox.Show($"Сохранение не выполнено!\n{errorStr}");
            return string.IsNullOrEmpty(errorStr);
        }

        #endregion
    }
}
