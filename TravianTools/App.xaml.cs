using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using TravianTools.Data.StaticData;
using TravianTools.TravianUtils;
using TravianTools.Utils;
using TravianTools.ViewModels;
using TravianTools.Views;

namespace TravianTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //using(var fs = new FileStream("test.txt", FileMode.Open))
            //using (var sr = new StreamReader(fs))
            //{
            //    var str   = sr.ReadToEnd();
            //    var lines = str.Replace(";", ";\n");
            //    File.WriteAllText("data.txt", lines);
            //}
            //var lst = new List<string>();
            //lst.Add("qwe:123");
            //lst.Add("asd:234");
            //lst.Add("zxc:");
            //var str = $"['{string.Join("','", lst)}']";
            //var q   = RPG.GetCache("123", str);

            //var lst  = BuildingsData.GetAllDep(20);
            //var list = lst.Select(x => (BuildingsData.GetById(x.id).Name, x.lvl)).ToList();

            base.OnStartup(e);
            Logger.Init();
            Settings.Load();
            Accounts.Load();
            var f  = new MainWindowView();
            var vm = new MainWindowViewModel();
            f.DataContext         = vm;
            g.MainWindowViewModel = vm;
            f.ShowDialog();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            g.Shutdown();
            base.OnExit(e);
        }
    }
}
