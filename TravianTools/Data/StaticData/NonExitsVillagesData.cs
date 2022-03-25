using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravianTools.Data.StaticData
{
    public class NonExitsVillagesData
    {
        public static List<VillageData> VillageList { get; set; }

        static NonExitsVillagesData()
        {
            VillageList = new List<VillageData>()
                          {
                              new VillageData(){ Id = -1, Name  = "Первая"},
                              new VillageData(){ Id = -2, Name  = "Вторая"},
                              new VillageData(){ Id = -3, Name  = "Третья"},
                              new VillageData(){ Id = -4, Name  = "Четвертая"},
                              new VillageData(){ Id = -5, Name  = "Пятая"},
                              new VillageData(){ Id = -6, Name  = "Шестая"},
                              new VillageData(){ Id = -7, Name  = "Седьмая"},
                              new VillageData(){ Id = -8, Name  = "Восьмая"},
                              new VillageData(){ Id = -9, Name  = "Девятая"},
                              new VillageData(){ Id = -10, Name = "Десятая"},
                          };
        }
    }

    public class VillageData
    {
        public int    Id   { get; set; }
        public string Name { get; set; }
    }
}
