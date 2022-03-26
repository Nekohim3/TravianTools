﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TravianTools.Data.StaticData;
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
            if (!File.Exists($"{g.Settings.UserDataPath}\\TemplateTaskCollection"))
            {
                File.WriteAllText($"{g.Settings.UserDataPath}\\TemplateTaskCollection", "[]");
            }

            var lst = JsonConvert.DeserializeObject<List<TempTask>>(File.ReadAllText($"{g.Settings.UserDataPath}\\TemplateTaskCollection"),
                                                                     new JsonSerializerSettings() { Converters = new List<JsonConverter>() { new CommandConverter() } });
            var ids = lst.Select(x => x.TempTaskListId).Distinct().ToList();
            foreach (var list in ids.Select(x => lst.Where(c => c.TempTaskListId == x).ToList()))
            {
                for (var i = 0; i < list.Count; i++)
                    list[i].Number = i;
            }

            SetDesc(lst);
            return lst;
        }

        public TempTask GetNextTask(int tlId, int id)
        {
            var lst = GetAllByTaskListId(tlId);
            var ind = lst.IndexOf(lst.First(x => x.Id == id));
            return ind + 1 == lst.Count ? null : lst[ind + 1];
        }

        public TempTask GetNextTask(TempTask t) => GetNextTask(t.TempTaskListId, t.Id);
        

        public List<TempTask> GetAllByTaskListId(int id)
        {
            return GetAll().Where(x => x.TempTaskListId == id).ToList();
        }

        public void SaveAll(List<TempTask> lst)
        {
            lst = lst.OrderBy(x => x.TempTaskListId).ThenBy(x => x.Id).ToList();
            
            
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

        private void SetDesc(List<TempTask> lst)
        {
            foreach (var x in lst)
            {
                if (x.Command.Type == typeof(BuildingUpgradeCmd).ToString())
                {
                    var cmd = (BuildingUpgradeCmd)x.Command;
                    x.Desc = $"[{x.Number}] {CommandTypesData.GetByType(typeof(BuildingUpgradeCmd)).Name}: {BuildingsData.GetById(cmd.BuildingType).Name} " +
                             $"{{{NonExitsVillagesData.VillageList.FirstOrDefault(c => c.Id == cmd.VillageId).Name}; {cmd.LocationId}}}";
                    x.CommandString = CommandTypesData.GetByType(typeof(BuildingUpgradeCmd)).Name;
                }
                if (x.Command.Type == typeof(BuildingDestroyCmd).ToString())
                {
                    var cmd = (BuildingDestroyCmd)x.Command;
                    x.Desc = $"[{x.Number}]{CommandTypesData.GetByType(typeof(BuildingDestroyCmd)).Name}: {BuildingsData.GetById(cmd.BuildingType).Name} " +
                             $"{{{NonExitsVillagesData.VillageList.FirstOrDefault(c => c.Id == cmd.VillageId).Name}}}";
                    x.CommandString = CommandTypesData.GetByType(typeof(BuildingDestroyCmd)).Name;
                }

                if(!string.IsNullOrEmpty(x.Note))
                    x.Desc += $" //{x.Note}";
            }
        }

        public void Remove(TempTask tt) => Remove(tt.Id);
    }
}
