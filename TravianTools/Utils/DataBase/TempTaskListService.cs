using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TravianTools.Task;
using TravianTools.TravianCommands;

namespace TravianTools.Utils.DataBase
{
    public class TempTaskListService
    {
        public int GetNewId()
        {
            var lst = GetAll();
            if (lst.Count == 0) return 1;
            return lst.Max(x => x.Id) + 1;
        }

        public List<TempTaskList> GetAll()
        {
            if (!File.Exists($"{g.Settings.UserDataPath}\\TemplateTaskListCollection"))
            {
                File.WriteAllText($"{g.Settings.UserDataPath}\\TemplateTaskListCollection", "[]");
            }

            return JsonConvert.DeserializeObject<List<TempTaskList>>(File.ReadAllText($"{g.Settings.UserDataPath}\\TemplateTaskListCollection"),
                                                                     new JsonSerializerSettings() { Converters = new List<JsonConverter>() { new CommandConverter() } });
        }

        public void SaveAll(List<TempTaskList> lst)
        {
            File.WriteAllText($"{g.Settings.UserDataPath}\\TemplateTaskListCollection", JsonConvert.SerializeObject(lst, Formatting.Indented));
        }

        public TempTaskList GetById(int id)
        {
            return GetAll().FirstOrDefault(x => x.Id == id);
        }

        public void Save(TempTaskList ttl)
        {
            if (ttl.Id == 0) return;
            var lst  = GetAll();
            var item = lst.FirstOrDefault(x => x.Id == ttl.Id);
            if (item == null)
                lst.Add(ttl);
            else
                lst[lst.IndexOf(item)] = ttl;
            SaveAll(lst);
        }

        public void Remove(int id)
        {
            if (id == 0) return;
            var lst  = GetAll();
            var item = lst.FirstOrDefault(x => x.Id == id);
            if(item == null) return;
            lst.RemoveAt(lst.IndexOf(item));
            SaveAll(lst);
        }

        public void Remove(TempTaskList ttl) => Remove(ttl.Id);
    }
}
