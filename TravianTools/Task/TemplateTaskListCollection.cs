using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Xml.Serialization;
using Microsoft.Practices.Prism.ViewModel;
using Newtonsoft.Json;
using TravianTools.TravianCommands;
using TravianTools.Utils;
using TravianTools.ViewModels;
using TravianTools.Views;
using MessageBox = System.Windows.MessageBox;

namespace TravianTools.Task
{
    [Serializable]
    public class TemplateTaskListCollection : NotificationObject
    {
        public delegate void                            TemplateTaskListCollectionDelegate(TemplateTaskList newttl);
        public event TemplateTaskListCollectionDelegate AfterSave;
        public event TemplateTaskListCollectionDelegate SelectedChanged;

        private ObservableCollection<TemplateTaskList> _taskListList;

        public ObservableCollection<TemplateTaskList> TaskListList
        {
            get => _taskListList;
            set
            {
                _taskListList = value;
                RaisePropertyChanged(() => TaskListList);
            }
        }

        private TemplateTaskList _selectedTaskList;
        [JsonIgnore]
        public TemplateTaskList SelectedTaskList
        {
            get => _selectedTaskList;
            set
            {
                _selectedTaskList = value;
                TaskListForEdit   = _selectedTaskList != null ? GetById(_selectedTaskList.Id) : null;
                RaisePropertyChanged(() => SelectedTaskList);
                SelectedChanged?.Invoke(_selectedTaskList);
            }
        }

        private TemplateTaskList _taskListForEdit;
        [JsonIgnore]
        public TemplateTaskList TaskListForEdit
        {
            get => _taskListForEdit;
            set
            {
                _taskListForEdit = value;
                RaisePropertyChanged(() => TaskListForEdit);
            }
        }

        public TemplateTaskListCollection()
        {
            Load();
        }

        public void ResetChanges()
        {
            TaskListForEdit = _selectedTaskList != null ? GetById(_selectedTaskList.Id) : null;
        }

        public void SaveAll()
        {
            File.WriteAllText($"{g.Settings.UserDataPath}\\TemplateTaskListCollection", JsonConvert.SerializeObject(TaskListList, Formatting.Indented));
        }

        public void SaveEdited()
        {
            var  entity = GetById(TaskListForEdit.Id);
            bool newent = true;
            if(entity == null)
                TaskListList.Add(TaskListForEdit);
            else
            {
                entity.Id       = TaskListForEdit.Id;
                entity.Name     = TaskListForEdit.Name;
                entity.TaskList = TaskListForEdit.TaskList;
                newent          = false;
            }
            SaveAll();
            AfterSave?.Invoke(newent ? TaskListList.LastOrDefault() : entity);
        }

        public void Load()
        {
            if (!File.Exists($"{g.Settings.UserDataPath}\\TemplateTaskListCollection"))
            {
                TaskListList = new ObservableCollection<TemplateTaskList>();
                SaveAll();
            }

            TaskListList = JsonConvert.DeserializeObject<ObservableCollection<TemplateTaskList>>(File.ReadAllText($"{g.Settings.UserDataPath}\\TemplateTaskListCollection"),
                                                                                                 new JsonSerializerSettings() {Converters = new List<JsonConverter>() {new CommandConverter()}});
        }

        public void Delete(int id)
        {
            var item = GetById(id);
            if (MessageBox.Show($"Точно удалить {item.Name}?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                TaskListList.Remove(item);
                SaveAll();
                AfterSave?.Invoke(null);
            }
        }


        public TemplateTaskList GetNewById(int id)
        {
            return JsonConvert.DeserializeObject<ObservableCollection<TemplateTaskList>>(File.ReadAllText($"{g.Settings.UserDataPath}\\TemplateTaskListCollection"),
                                                                                         new JsonSerializerSettings() { Converters = new List<JsonConverter>() { new CommandConverter() } })
                              .FirstOrDefault(x => x.Id == id);
        }

        public TemplateTaskList GetById(int id)
        {
            return TaskListList.FirstOrDefault(x => x.Id == id);
        }
    }
}
