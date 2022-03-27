using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using TravianTools.Utils;

namespace TravianTools.ViewModels
{
    public class ServerSettingsViewModel : NotificationObject
    {
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

        private Action _close;

        public DelegateCommand SaveCmd { get; }

        public ServerSettingsViewModel(Action close)
        {
            SaveCmd = new DelegateCommand(OnSave);
            _close  = close;
        }

        private void OnSave()
        {
            if (string.IsNullOrEmpty(_server) || string.IsNullOrEmpty(_domain))
            {
                MessageBox.Show("Надо заполнить поля");
                return;
            }

            g.Settings.Domain     = Domain;
            g.Settings.Server     = Server;
            g.Settings.ProxyAddr  = ProxyAddr;
            g.Settings.ProxyPort  = ProxyPort;
            g.Settings.ProxyLogin = ProxyLogin;
            g.Settings.ProxyPass  = ProxyPass;
            Settings.Save();
            _close.Invoke();
        }
    }
}
