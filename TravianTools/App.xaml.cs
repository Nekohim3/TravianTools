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
using TravianTools.TravianCommands;
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
