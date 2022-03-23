using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Practices.Prism.ViewModel;
using Newtonsoft.Json;
using TravianTools.TravianCommands;
using TravianTools.Utils;
using TravianTools.ViewModels;
using TravianTools.Views;

namespace TravianTools.Task
{
    [Serializable]
    public class TemplateTaskListCollection : NotificationObject
    {
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
                RaisePropertyChanged(() => SelectedTaskList);
            }
        }

        public TemplateTaskListCollection()
        {
            TaskListList = new ObservableCollection<TemplateTaskList>();
        }

        public void Save()
        {
            File.WriteAllText($"{g.Settings.UserDataPath}\\TemplateTaskListCollection.xml", JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static TemplateTaskListCollection Load()
        {
            return !File.Exists($"{g.Settings.UserDataPath}\\TemplateTaskListCollection.xml")
                       ? new TemplateTaskListCollection()
                       : JsonConvert.DeserializeObject<TemplateTaskListCollection>(File.ReadAllText($"{g.Settings.UserDataPath}\\TemplateTaskListCollection.xml"),
                                                                                   new JsonSerializerSettings() {Converters = new List<JsonConverter>() {new CommandConverter()}});
        }
    }
}
