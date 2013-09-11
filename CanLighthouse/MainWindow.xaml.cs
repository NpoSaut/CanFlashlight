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
using Communications.Can.LogRecording;
using Communications.Can.FrameEncoders;

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
            get { return App.CurrentApp.Ports; }
        }

        public SniffWindow SniffingWindow { get; private set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory("CanLog");

#if VCAN
            if (File.Exists("emulate_can.bin"))
            {
                var ppp = new StreamEncoderPort<FrameSbsEncoder>(new FileInfo("emulate_can.bin"));
                Ports.Add(ppp);
                ppp.Start(1);
            }
#endif

            if (Ports.Any()) (new SendWindow(Ports.First()) { Owner = this }).Show();
            (SniffingWindow = (new SniffWindow(Ports) { Owner = this } )).Show();

            Ports.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Ports_CollectionChanged);

            foreach (var p in Ports)
            {
                //new LogEncodingRecorder<FrameSbsEncoder>(p, new FileInfo(System.IO.Path.Combine("CanLog", string.Format("log {0}.bin", p.Name))));
                //new LogEncodingRecorder<FrameTextEncoder>(p, new FileInfo(System.IO.Path.Combine("CanLog", string.Format("log {0}.txt", p.Name))));
                RegisterNewRecorder(p);
            }

            //ppp.Start();
        }

        private List<LogRecorder> Recorders = new List<LogRecorder>();

        private void RegisterNewRecorder(CanPort forPort)
        {
            Recorders.Add(new LogEncodingRecorder<FrameSbsEncoder>(forPort,
                              new FileInfo(System.IO.Path.Combine("CanLog", string.Format("log {0}.bin", forPort.Name)))));
            Recorders.Add(new LogEncodingRecorder<FrameTextEncoder>(forPort,
                              new FileInfo(System.IO.Path.Combine("CanLog", string.Format("log {0}.txt", forPort.Name)))));
        }

        void Ports_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (var newPort in e.NewItems.OfType<CanPort>())
                {
                    RegisterNewRecorder(newPort);
                    SniffingWindow.ListeningPorts.Add(newPort);
                }
            if (e.OldItems != null)
                foreach (var removedPort in e.OldItems.OfType<CanPort>())
                {
                    foreach (var recorder in Recorders.Where(rec => rec.Port == removedPort).ToList())
                    {
                        Recorders.Remove(recorder);
                        recorder.Dispose();
                    }

                    SniffingWindow.ListeningPorts.Remove(removedPort);
                }
        }
    }
}
