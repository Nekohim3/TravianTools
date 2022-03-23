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
        }
    }
    public class BuildingData
    {
        public int    Id   { get; set; }
        public string Name { get; set; }

        public BuildingData(int id, string name)
        {
            Id   = id;
            Name = name;
        }
    }
}
