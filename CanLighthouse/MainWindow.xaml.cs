using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Communications.Can.LogReader;
using System.IO;
using Communications.Can;
using Communications.Appi.Winusb;
using Communications.Appi;
using System.Collections.ObjectModel;

namespace CanLighthouse
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public ObservableCollection<CanPort> Ports
        {
            get { return (App.Current as App).Ports; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            (new SendWindow(Ports.First()) { Owner = this }).Show();

            (new SniffWindow(Ports) { Owner = this } ).Show();

            //(new WatchWindow(Ports.First()) { Owner = this }).Show();
        }
    }
}
