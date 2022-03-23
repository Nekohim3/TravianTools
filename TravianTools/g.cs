using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using Microsoft.Practices.Prism.ViewModel;
using TravianTools.Data;
using TravianTools.Utils;
using TravianTools.ViewModels;
using TravianTools.Views;

namespace TravianTools
{
    public static class g
    {
        public static Accounts Accounts { get; set; }

        public static Settings Settings { get; set; }

        public static MainWindowViewModel MainWindowViewModel { get; set; }

        static g()
        {
            Accounts = new Accounts();
            Settings = new Settings();
        }

        public static void Shutdown()
        {
            foreach (var x in Accounts.AccountList.Where(x => x.Running))
                x.Stop();
        }
    }

    [Serializable]
    public class Accounts : NotificationObject
    {
        private ObservableCollection<Account> _accountList;

        public ObservableCollection<Account> AccountList
        {
            get => _accountList;
            set
            {
                _accountList = value;
                RaisePropertyChanged(() => AccountList);
                g.MainWindowViewModel?.RaiseCanExecChange();
            }
        }

        private Account _selectedAccount;
        [XmlIgnore]
        public Account SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                _selectedAccount = value;
                RaisePropertyChanged(() => SelectedAccount);
                g.MainWindowViewModel?.RaiseCanExecChange();
            }
        }

        public void Add(Account acc)
        {
            if (AccountList.Count(x => x.Name == acc.Name) != 0)
            {
                MessageBox.Show("Аккаунт с таким именем уже существует");
                return;
            }
            AccountList.Add(acc);
            Save();
            g.MainWindowViewModel?.RaiseCanExecChange();

        }

        public void Remove(Account acc)
        {
            if (MessageBox.Show($"Точно удалить аккаунт {acc.Name}?", "", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            AccountList.Remove(acc);
            Save();
            g.MainWindowViewModel?.RaiseCanExecChange();
        }

        public static void Save()
        {
            var formatter = new XmlSerializer(typeof(Accounts));

            using (var fs = new FileStream($"{g.Settings.UserDataPath}\\Accounts.xml", FileMode.Create))
                formatter.Serialize(fs, g.Accounts);
        }

        public static void Load()
        {
            if (File.Exists($"{g.Settings.UserDataPath}\\Accounts.xml"))
            {
                var formatter = new XmlSerializer(typeof(Accounts));
                using (var fs = new FileStream($"{g.Settings.UserDataPath}\\Accounts.xml", FileMode.Open, FileAccess.Read))
                    g.Accounts = (Accounts)formatter.Deserialize(fs);
            }
            else
            {
                g.Accounts             = new Accounts
                                         {
                                             AccountList = new ObservableCollection<Account>()
                                         };
            }
        }
    }
}
