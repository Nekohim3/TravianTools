using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using Microsoft.Practices.Prism.ViewModel;
using Newtonsoft.Json;
using TravianTools.Data;
using TravianTools.Utils;
using TravianTools.Utils.DataBase;
using TravianTools.ViewModels;
using TravianTools.Views;

namespace TravianTools
{
    public static class g
    {
        public static TempTaskListService TempTaskListService { get; set; }
        public static TempTaskService     TempTaskService     { get; set; }
        public static Accounts            Accounts            { get; set; }
        public static RandomGenerator     Rand                { get; set; }

        public static Settings Settings { get; set; }

        public static MainWindowViewModel MainWindowViewModel { get; set; }

        static g()
        {
            Accounts            = new Accounts();
            Settings            = new Settings();
            TempTaskListService = new TempTaskListService();
            TempTaskService     = new TempTaskService();
            Rand                = new RandomGenerator();
        }

        public static void Shutdown()
        {
            if(Accounts.AccountList != null)
                foreach (var x in Accounts.AccountList)
                    x.Stop();
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                                        .Select(s => s[Rand.Next(0, s.Length)]).ToArray());
        }

        public static string DecodeQuotedPrintables(string input, string charSet = "")
        {
            if (string.IsNullOrEmpty(charSet))
            {
                var charSetOccurences = new Regex(@"=\?.*\?Q\?", RegexOptions.IgnoreCase);
                var charSetMatches = charSetOccurences.Matches(input);
                foreach (Match match in charSetMatches)
                {
                    charSet = match.Groups[0].Value.Replace("=?", "").Replace("?Q?", "");
                    input = input.Replace(match.Groups[0].Value, "").Replace("?=", "");
                }
            }

            Encoding enc = new ASCIIEncoding();
            if (!string.IsNullOrEmpty(charSet))
            {
                try
                {
                    enc = Encoding.GetEncoding(charSet);
                }
                catch
                {
                    enc = new ASCIIEncoding();
                }
            }

            //decode iso-8859-[0-9]
            var occurences = new Regex(@"=[0-9A-Z]{2}", RegexOptions.Multiline);
            var matches = occurences.Matches(input);
            foreach (Match match in matches)
            {
                try
                {
                    byte[] b = new byte[] { byte.Parse(match.Groups[0].Value.Substring(1), System.Globalization.NumberStyles.AllowHexSpecifier) };
                    char[] hexChar = enc.GetChars(b);
                    input = input.Replace(match.Groups[0].Value, hexChar[0].ToString());
                }
                catch { }
            }

            //decode base64String (utf-8?B?)
            occurences = new Regex(@"\?utf-8\?B\?.*\?", RegexOptions.IgnoreCase);
            matches = occurences.Matches(input);
            foreach (Match match in matches)
            {
                byte[] b = Convert.FromBase64String(match.Groups[0].Value.Replace("?utf-8?B?", "").Replace("?UTF-8?B?", "").Replace("?", ""));
                string temp = Encoding.UTF8.GetString(b);
                input = input.Replace(match.Groups[0].Value, temp);
            }

            input = input.Replace("=\r\n", "");
            return input;
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
        [JsonIgnore]
        public Account SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                _selectedAccount = value;
                RaisePropertyChanged(() => SelectedAccount);
                g.MainWindowViewModel?.RaiseCanExecChange();
                g.MainWindowViewModel?.TaskListInit();
            }
        }

        public void UpdateSelectedAccount()
        {
            RaisePropertyChanged(() => SelectedAccount);
            g.MainWindowViewModel?.RaiseCanExecChange();
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
            if(g.Accounts.AccountList == null) return;
            File.WriteAllText($"{g.Settings.UserDataPath}\\Accounts", JsonConvert.SerializeObject(g.Accounts, Formatting.Indented));
        }

        public static void Load()
        {
            if (File.Exists($"{g.Settings.UserDataPath}\\Accounts"))
            {
                g.Accounts = JsonConvert.DeserializeObject<Accounts>(File.ReadAllText($"{g.Settings.UserDataPath}\\Accounts"));
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
