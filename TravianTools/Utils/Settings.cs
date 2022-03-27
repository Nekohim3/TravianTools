using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Practices.Prism.ViewModel;
using Newtonsoft.Json;
using TravianTools.ViewModels;
using TravianTools.Views;

namespace TravianTools.Utils
{
    public class Settings : NotificationObject
    {
        private string _userDataPath;
        [JsonIgnore]
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

        public string ProxyAddr
        {
            get => _proxy;
            set
            {
                _proxy = value;
                RaisePropertyChanged(() => ProxyAddr);
            }
        }

        private int _proxyPort;

        public int ProxyPort
        {
            get => _proxyPort;
            set
            {
                _proxyPort = value;
                RaisePropertyChanged(() => ProxyPort);
            }
        }

        private string _proxyLogin;

        public string ProxyLogin
        {
            get => _proxyLogin;
            set
            {
                _proxyLogin = value;
                RaisePropertyChanged(() => ProxyLogin);
            }
        }

        private string _proxyPass;

        public string ProxyPass
        {
            get => _proxyPass;
            set
            {
                _proxyPass = value;
                RaisePropertyChanged(() => ProxyPass);
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
            File.WriteAllText($"{g.Settings.UserDataPath}\\Settings", JsonConvert.SerializeObject(g.Settings, Formatting.Indented));
        }

        public static void Load()
        {
            if (!File.Exists($"{g.Settings.UserDataPath}\\Settings"))
            {
                var f  = new ServerSettingsView();
                var vm = new ServerSettingsViewModel(f.Close);
                f.DataContext = vm;
                f.ShowDialog();
            }
            else
            {
                g.Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText($"{g.Settings.UserDataPath}\\Settings"));
            }
        }
    }
}
