using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Practices.Prism.ViewModel;
using TravianTools.ViewModels;
using TravianTools.Views;

namespace TravianTools.Utils
{
    [Serializable]
    public class Settings : NotificationObject
    {
        private string _userDataPath;
        [XmlIgnore]
        public string UserDataPath
        {
            get => _userDataPath;
            set
            {
                _userDataPath = value;
                RaisePropertyChanged(() => UserDataPath);
            }
        }

        private string _server;

        public string Server
        {
            get => _server;
            set
            {
                _server = value;
                RaisePropertyChanged(() => Server);
            }
        }

        private string _domain;

        public string Domain
        {
            get => _domain;
            set
            {
                _domain = value;
                RaisePropertyChanged(() => Domain);
            }
        }

        private string _proxy;

        public string Proxy
        {
            get => _proxy;
            set
            {
                _proxy = value;
                RaisePropertyChanged(() => Proxy);
            }
        }

        public Settings()
        {
            UserDataPath = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            if (!Directory.Exists($"{UserDataPath}\\UserData"))
                Directory.CreateDirectory($"{UserDataPath}\\UserData");
            UserDataPath = $"{UserDataPath}\\UserData";
        }

        public static void Save()
        {
            var formatter = new XmlSerializer(typeof(Settings));

            using (var fs = new FileStream($"{g.Settings.UserDataPath}\\Settings.xml", FileMode.Create))
                formatter.Serialize(fs, g.Settings);
        }

        public static void Load()
        {
            if (!File.Exists($"{g.Settings.UserDataPath}\\Settings.xml"))
            {
                var f  = new ServerSettingsView();
                var vm = new ServerSettingsViewModel(f.Close);
                f.DataContext = vm;
                f.ShowDialog();
            }
            else
            {
                var formatter = new XmlSerializer(typeof(Settings));
                using (var fs = new FileStream($"{g.Settings.UserDataPath}\\Settings.xml", FileMode.Open, FileAccess.Read))
                    g.Settings = (Settings) formatter.Deserialize(fs);
            }
        }
    }
}
