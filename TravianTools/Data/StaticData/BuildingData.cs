using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.ViewModel;

namespace TravianTools.Data.StaticData
{
    public static class BuildingsData
    {
        public static List<BuildingData> BuildingList { get; set; }

        static BuildingsData()
        {
            BuildingList = new List<BuildingData>();
            BuildingList.Add(new BuildingData(1,  true, AdditionalRestriction.None, "Лесопилка"));
            BuildingList.Add(new BuildingData(2,  true, AdditionalRestriction.None, "Карьер"));
            BuildingList.Add(new BuildingData(3,  true, AdditionalRestriction.None, "Рудник"));
            BuildingList.Add(new BuildingData(4,  true, AdditionalRestriction.None, "Пшено"));
            BuildingList.Add(new BuildingData(5,  true, AdditionalRestriction.None, "Лесопильный завод", (15, 5), (1, 10)));
            BuildingList.Add(new BuildingData(6,  true, AdditionalRestriction.None, "Кирпичный завод",   (15, 5), (2, 10)));
            BuildingList.Add(new BuildingData(7,  true, AdditionalRestriction.None, "Литейная",          (15, 5), (3, 10)));
            BuildingList.Add(new BuildingData(8,  true, AdditionalRestriction.None, "Мельница",          (4, 5)));
            BuildingList.Add(new BuildingData(9,  true, AdditionalRestriction.None, "Пекарня",           (15, 5), (4, 10), (8, 5)));
            BuildingList.Add(new BuildingData(10, true, AdditionalRestriction.None, "Склад",             (15, 1)));
            BuildingList.Add(new BuildingData(11, true, AdditionalRestriction.None, "Амбар",             (15, 1)));
            BuildingList.Add(new BuildingData(13, true, AdditionalRestriction.None, "Кузница",           (15, 3), (22, 1)));
            BuildingList.Add(new BuildingData(14, true, AdditionalRestriction.None, "Арена",             (16, 15)));
            BuildingList.Add(new BuildingData(15, true, AdditionalRestriction.None, "Главное здание"));
            BuildingList.Add(new BuildingData(16, true, AdditionalRestriction.None, "Пункт сбора"));
            BuildingList.Add(new BuildingData(17, true, AdditionalRestriction.None, "Рынок",      (15, 3), (10, 1)));
            BuildingList.Add(new BuildingData(18, true, AdditionalRestriction.None, "Посольство", (15, 1)));
            BuildingList.Add(new BuildingData(19, true, AdditionalRestriction.None, "Казарма",    (15, 3), (16, 1)));
            BuildingList.Add(new BuildingData(20, true, AdditionalRestriction.None, "Конюшня",    (13, 3), (22, 5)));
            BuildingList.Add(new BuildingData(21, true, AdditionalRestriction.None, "Мастерская", (15, 5), (22, 10)));
            BuildingList.Add(new BuildingData(22, true, AdditionalRestriction.None, "Академия",   (15, 3), (19, 3)));
            BuildingList.Add(new BuildingData(23, true, AdditionalRestriction.None, "Тайник"));
            BuildingList.Add(new BuildingData(24, true, AdditionalRestriction.None, "Ратуша",     (15, 10), (22, 10)));
            BuildingList.Add(new BuildingData(25, false,AdditionalRestriction.None, "Резиденция", (26, 0),  (15, 5)));
            BuildingList.Add(new BuildingData(26, false,AdditionalRestriction.NotWorldWonderVillage, "Дворец",     (25, 0),  (15, 5), (18, 1)));
            BuildingList.Add(new BuildingData(27, true, AdditionalRestriction.None, "Сокровищница"));
            BuildingList.Add(new BuildingData(28, true, AdditionalRestriction.None, "Торговая палата", (17, 20), (20, 10)));
            BuildingList.Add(new BuildingData(29, true, AdditionalRestriction.City, "Большая казарма", (19, 20)));
            BuildingList.Add(new BuildingData(30, true, AdditionalRestriction.City, "Большая конюшня", (20, 20)));
            BuildingList.Add(new BuildingData(31, true, AdditionalRestriction.None, "Городская стена"));
            //BuildingList.Add(new BuildingData(32, true, AdditionalRestriction.None, "Земляной вал"));
            //BuildingList.Add(new BuildingData(33, true, AdditionalRestriction.None, "Изгородь"));
            BuildingList.Add(new BuildingData(34, true, AdditionalRestriction.Capital, "Каменотес", (15, 5)));
            BuildingList.Add(new BuildingData(35, true, AdditionalRestriction.Capital, "Пивоварня",  (11, 20), (16, 10)));
            BuildingList.Add(new BuildingData(36, true, AdditionalRestriction.None, "Капканщик", (16, 1)));
            BuildingList.Add(new BuildingData(38, true, AdditionalRestriction.WorldWonderVillage, "Большой склад", (15, 10)));
            BuildingList.Add(new BuildingData(39, true, AdditionalRestriction.WorldWonderVillage, "Большой амбар", (15, 10)));
            BuildingList.Add(new BuildingData(40, true, AdditionalRestriction.WorldWonderVillage, "Чудо света"));
            BuildingList.Add(new BuildingData(41, true, AdditionalRestriction.None, "Водопой", (16, 10), (20, 20)));
            BuildingList.Add(new BuildingData(42, true, AdditionalRestriction.Capital, "Ров"));
            BuildingList.Add(new BuildingData(43, true, AdditionalRestriction.None, "Натарская стена"));
            BuildingList.Add(new BuildingData(45, true, AdditionalRestriction.NotWorldWonderVillage, "Скрытая сокровищница", (27, 0), (15, 3)));
        }

        public static BuildingData GetById(int id) => BuildingList.FirstOrDefault(x => x.Id == id);

        public static List<(int id, int lvl)> GetAllDep(int id)
        {
            var b = GetById(id);
            if (b == null) return null;
            var lst = new List<(int id, int lvl)>();
            foreach (var x in b.Dependencies)
            {
                lst.Add(x);
                GetAllDep(x.id, lst);
            }

            var list = new List<(int id, int lvl)>();

            foreach (var x in lst)
            {
                var item = list.FirstOrDefault(c => c.id == x.id);
                
                if(item == default(ValueTuple<int, int>))
                    list.Add(x);
                else if (x.lvl > item.lvl)
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        if (list[i].id != x.id) continue;
                        list[i] = (x.id, x.lvl);
                    }
                }
            }

            return list;
        }

        private static void GetAllDep(int id, List<(int id, int lvl)> lst)
        {
            var b = GetById(id);
            if (b == null) return;
            foreach (var x in b.Dependencies)
            {
                lst.Add(x);
                GetAllDep(x.id, lst);
            }
        }
    }

    public enum AdditionalRestriction
    {
        None,
        City,
        Capital,
        WorldWonderVillage,
        NotWorldWonderVillage
    }
    public class BuildingData
    {
        public int                     Id           { get; set; }
        public string                  Name         { get; set; }
        public bool                    CanBuildIm   { get; set; }
        public AdditionalRestriction   AddRest      { get; set; }
        public List<(int id, int lvl)> Dependencies { get; set; }

        public BuildingData(int id, bool canBuildIm, AdditionalRestriction rest, string name, params (int id, int lvl)[] args)
        {
            Id           = id;
            Name         = name;
            CanBuildIm   = canBuildIm;
            AddRest      = rest;
            Dependencies = args.ToList();
        }
    }
}
