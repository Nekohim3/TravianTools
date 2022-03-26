using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravianTools.TravianCommands;

namespace TravianTools.Data.StaticData
{
    public static class CommandTypesData
    {
        public static List<CommandType> CommandTypeList { get; set; }
        static CommandTypesData()
        {
            CommandTypeList = new List<CommandType>()
                              {
                                  new CommandType() { Id = 0, Name  = "Построить" },
                                  new CommandType() { Id = 1, Name  = "Снести" },
                                  new CommandType() { Id = 2, Name  = "Завершить строительство" },
                                  new CommandType() { Id = 3, Name  = "NPC торговец" },
                                  new CommandType() { Id = 4, Name  = "Рынок" },
                                  new CommandType() { Id = 5, Name  = "Собрать награду" },
                                  new CommandType() { Id = 6, Name  = "Установить имя деревни" },
                                  new CommandType() { Id = 7, Name  = "Юзнуть итем" },
                                  new CommandType() { Id = 8, Name  = "Аттрибуты героя" },
                                  new CommandType() { Id = 9, Name  = "Приключение" },
                                  new CommandType() { Id = 10, Name = "DialogAction" },
                                  new CommandType() { Id = 11, Name = "Нанять юнитов" },
                                  new CommandType() { Id = 12, Name = "Отправить войска" },
                                  new CommandType() { Id = 13, Name = "Выбрать рассу" },
                                  new CommandType() { Id = 14, Name = "Чето с оазисом" },
                              };
        }

        public static CommandType GetByType(Type t)
        {
            if (t == typeof(BuildingUpgradeCmd)) return CommandTypeList[0];
            if (t == typeof(BuildingDestroyCmd)) return CommandTypeList[1];
            return null;
        }
    }

    public class CommandType
    {
        public int    Id   { get; set; }
        public string Name { get; set; }
    }
}
