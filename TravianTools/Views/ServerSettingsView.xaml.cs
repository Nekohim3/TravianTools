using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TravianTools.Utils;
using TravianTools.ViewModels;

namespace TravianTools.Views
{
    /// <summary>
    /// Interaction logic for ServerSettingsView.xaml
    /// </summary>
    public partial class ServerSettingsView : Window
    {
        public ServerSettingsView()
        {
            InitializeComponent();
        }

        private void ServerSettingsView_OnClosing(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(((ServerSettingsViewModel)DataContext).Domain) || string.IsNullOrEmpty(((ServerSettingsViewModel)DataContext).Server))
            {
                MessageBox.Show("Надо заполнить поля");
                e.Cancel = true;
            }
        }
    }
}
