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

namespace CanLighthouse
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        AppiDev appi;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            appi = WinusbAppiDev.GetDevices().First().OpenDevice();
            appi.BeginListen();
            var rp = appi.Ports[AppiLine.Can1];

            (new SendWindow(rp)).Show();

            (new SniffWindow(new List<CanPort>() { rp })).Show();

            this.Close();
        }

        public void Dispose()
        {
            appi.Dispose();
        }
    }
}
