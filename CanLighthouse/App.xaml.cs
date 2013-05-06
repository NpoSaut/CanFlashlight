using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Communications.Appi;
using System.Collections.ObjectModel;
using Communications.Can;
using CanLighthouse.Describing;

namespace CanLighthouse
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public ObservableCollection<AppiDev> AppiDevices { get; set; }
        private ObservableCollection<AppiDeviceSlot> AppiDeviceSlots { get; set; }
        public ObservableCollection<CanPort> Ports { get; private set; }

        public ProtocolDescription Protocol { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppiDeviceSlots = new ObservableCollection<AppiDeviceSlot>(
                                    Communications.Appi.Winusb.WinusbAppiDev.GetDevices());

            AppiDevices = new ObservableCollection<AppiDev>();
            AppiDevices.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(AppiDevices_CollectionChanged);
            Ports = new ObservableCollection<CanPort>();
            foreach (var ds in AppiDeviceSlots)
                AppiDevices.Add(ds.OpenDevice(true));

            Protocol = ProtocolDescription.Load("Default.xpd");
            Protocol.FillupResource(this.Resources);
        }

        void AppiDevices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (var NewDev in e.NewItems.OfType<AppiDev>())
                {
                    foreach (var NewPort in NewDev.Ports.Values)
                        Ports.Add(NewPort);
                    NewDev.Disconnected += AppiDev_Disconnected;
                }
            if (e.OldItems != null)
                foreach (var RemovedDev in e.OldItems.OfType<AppiDev>())
                {
                    foreach (var RemovedPort in RemovedDev.Ports.Values)
                        Ports.Remove(RemovedPort);
                    RemovedDev.Disconnected -= AppiDev_Disconnected;
                }
        }

        void AppiDev_Disconnected(object sender, EventArgs e)
        {
            AppiDevices.Remove(sender as AppiDev);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            foreach (var d in AppiDevices)
                d.Dispose();
        }
    }
}
