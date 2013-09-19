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
using Communications.Can.LogReader;

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

        public static App CurrentApp { get { return App.Current as App; } }

        public ProtocolDescription Protocol { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //AppiDeviceSlots = new ObservableCollection<AppiDeviceSlot>(
            //                        Communications.Appi.Winusb.WinusbAppiDev.GetDevices());

            AppiDevices = new ObservableCollection<AppiDev>();
            AppiDevices.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(AppiDevices_CollectionChanged);
            Ports = new ObservableCollection<CanPort>();
            //foreach (var ds in AppiDeviceSlots)
            //    AppiDevices.Add(ds.OpenDevice(true));


            Protocol = ProtocolDescription.Load("Default.xpd");
            Protocol.FillupResource(this.Resources);

            var t = new System.Threading.Thread(Reconnector) { IsBackground = true, Name = "AppiReconnector" } ;
            t.Start();
        }

        private bool TryReconnect = true;
        void Reconnector()
        {
            int i = 0;
            while(TryReconnect)
            {
                i++;
                AppiDeviceSlots = new ObservableCollection<AppiDeviceSlot>(
                                        Communications.Appi.Winusb.WinusbAppiDev.GetDevices());
                foreach (var d in AppiDeviceSlots.Where(s => s.IsFree))
                    try
                    {
                        var dev = d.OpenDevice(true);
                        AppiDevices.Add(dev);
                    } catch { }

                //if (!System.Threading.Thread.Yield())
                System.Threading.Thread.Sleep(500);
            }
        }

        void AppiDevices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke((Action)(delegate()
                {
                    if (e.NewItems != null)
                        foreach (var NewDev in e.NewItems.OfType<AppiDev>())
                        {
                            foreach (var NewPort in NewDev.CanPorts.Values)
                                Ports.Add(NewPort);
                            NewDev.Disconnected += AppiDev_Disconnected;
                        }
                    if (e.OldItems != null)
                        foreach (var RemovedDev in e.OldItems.OfType<AppiDev>())
                        {
                            foreach (var RemovedPort in RemovedDev.CanPorts.Values)
                                Ports.Remove(RemovedPort);
                            RemovedDev.Disconnected -= AppiDev_Disconnected;
                            RemovedDev.Dispose();
                        }
                }));
        }

        void AppiDev_Disconnected(object sender, EventArgs e)
        {
            AppiDevices.Remove(sender as AppiDev);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            TryReconnect = false;
            foreach (var d in AppiDevices)
                d.Dispose();
            foreach (var p in Ports.OfType<CanVirtualPort>())
                p.Dispose();
            base.OnExit(e);
        }
    }
}
