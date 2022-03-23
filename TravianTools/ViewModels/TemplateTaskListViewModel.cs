using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using TravianTools.Data;
using TravianTools.Task;
using TravianTools.TravianCommands;
using TravianTools.Utils;
using TravianTools.Views;

namespace TravianTools.ViewModels
{
    [Serializable]
    public class TemplateTaskListViewModel : NotificationObject
    {
        private TemplateTaskListCollection _taskListCollection;

        public TemplateTaskListCollection TaskListCollection
        {
            get => _taskListCollection;
            set
            {
                _taskListCollection = value;
                RaisePropertyChanged(() => TaskListCollection);
            }
        }

        [XmlIgnore] public DelegateCommand AddTaskListCmd    { get; }
        [XmlIgnore] public DelegateCommand EditTaskListCmd   { get; }
        [XmlIgnore] public DelegateCommand RemoveTaskListCmd { get; }
        [XmlIgnore] public DelegateCommand ExportTaskListCmd { get; }
        [XmlIgnore] public DelegateCommand ImportTaskListCmd { get; }

        [XmlIgnore] public DelegateCommand AddTaskCmd    { get; }
        [XmlIgnore] public DelegateCommand EditTaskCmd   { get; }
        [XmlIgnore] public DelegateCommand RemoveTaskCmd { get; }

        [XmlIgnore] public DelegateCommand UpCmd   { get; }
        [XmlIgnore] public DelegateCommand DownCmd { get; }

        public TemplateTaskListViewModel()
        {
            AddTaskListCmd    = new DelegateCommand(OnAddTaskList);
            EditTaskListCmd   = new DelegateCommand(OnEditTaskList,   () => TaskListCollection?.SelectedTaskList != null);
            RemoveTaskListCmd = new DelegateCommand(OnRemoveTaskList, () => TaskListCollection?.SelectedTaskList != null);
            ExportTaskListCmd = new DelegateCommand(OnExportTaskList, () => TaskListCollection?.SelectedTaskList != null);
            ImportTaskListCmd = new DelegateCommand(OnImportTaskList, () => TaskListCollection?.SelectedTaskList != null);

            AddTaskCmd    = new DelegateCommand(OnAddTask,    () => TaskListCollection?.SelectedTaskList               != null);
            EditTaskCmd   = new DelegateCommand(OnEditTask,   () => TaskListCollection?.SelectedTaskList?.SelectedTask != null);
            RemoveTaskCmd = new DelegateCommand(OnRemoveTask, () => TaskListCollection?.SelectedTaskList?.SelectedTask != null);

            UpCmd   = new DelegateCommand(OnUp,   () => TaskListCollection?.SelectedTaskList?.SelectedTask != null);
            DownCmd = new DelegateCommand(OnDown, () => TaskListCollection?.SelectedTaskList?.SelectedTask != null);

            TaskListCollection = new TemplateTaskListCollection()
                                 {
                                     TaskListList = new ObservableCollection<TemplateTaskList>()
                                                    {
                                                        new TemplateTaskList()
                                                        {
                                                            Id   = 123,
                                                            Name = "qwe",
                                                            TaskList = new ObservableCollection<TemplateTask>()
                                                                       {
                                                                           new TemplateTask()
                                                                           {
                                                                               Id      = 234,
                                                                               Command = new BuildingUpgradeCmd(null, 33, 44, 55){NPC = true, Account = new Account()},
                                                                           }

                                                                       }
                                                        }
                                                    }
                                 };

            TaskListCollection.Save();

            TaskListCollection = null;

            TaskListCollection = TemplateTaskListCollection.Load();
        }



        private void OnAddTaskList()
        {

        }

        private void OnEditTaskList()
        {

        }

        private void OnRemoveTaskList()
        {

        }

        private void OnExportTaskList()
        {

        }

        private void OnImportTaskList()
        {

        }

        private void OnAddTask()
        {

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
    }
}
