using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TravianTools.TravianCommands;

namespace TravianTools.Utils.DataBase
{
    public class TempTaskService
    {
        public int GetNewId()
        {
            var lst = GetAll();
            if (lst.Count == 0) return 1;
            return lst.Max(x => x.Id) + 1;
        }

        public List<TempTask> GetAll()
        {
            if (!File.Exists($"{g.Settings.UserDataPath}\\TemplateTaskLCollection"))
            {
                File.WriteAllText($"{g.Settings.UserDataPath}\\TemplateTaskCollection", "[]");
            }

            return JsonConvert.DeserializeObject<List<TempTask>>(File.ReadAllText($"{g.Settings.UserDataPath}\\TemplateTaskCollection"),
                                                                     new JsonSerializerSettings() { Converters = new List<JsonConverter>() { new CommandConverter() } });
        }

        public List<TempTask> GetAllByTaskListId(int id)
        {
            return GetAll().Where(x => x.TempTaskListId == id).ToList();
        }

        public void SaveAll(List<TempTask> lst)
        {
            File.WriteAllText($"{g.Settings.UserDataPath}\\TemplateTaskCollection", JsonConvert.SerializeObject(lst, Formatting.Indented));
        }

        public TempTask GetById(int id)
        {
            return GetAll().FirstOrDefault(x => x.Id == id);
        }

        public void Add(TempTask tt)
        {
            var lst = GetAll();
            lst.Add(tt);
            SaveAll(lst);
        }

        public void Save(TempTask tt)
        {
            if (tt.Id == 0) return;
            var lst  = GetAll();
            var item = lst.FirstOrDefault(x => x.Id == tt.Id);
            if (item == null) 
                lst.Add(tt);
            else
                lst[lst.IndexOf(item)] = tt;
            SaveAll(lst);
        }

        public void Remove(int id)
        {
            if (id == 0) return;
            var lst  = GetAll();
            var item = lst.FirstOrDefault(x => x.Id == id);
            if (item == null) return;
            lst.RemoveAt(lst.IndexOf(item));
            SaveAll(lst);
        }

        public void Remove(TempTask tt) => Remove(tt.Id);
    }
}
