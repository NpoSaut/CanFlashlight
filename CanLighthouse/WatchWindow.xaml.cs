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
using System.Windows.Shapes;
using Communications.Can;
using CanLighthouse.Models;
using System.Collections.ObjectModel;

namespace CanLighthouse
{
    /// <summary>
    /// Логика взаимодействия для WatchWindow.xaml
    /// </summary>
    public partial class WatchWindow : Window
    {
        public CanPort Port { get; private set; }

        public WatchWindow(CanPort OnPort)
        {
            Port = OnPort;
            Port.Recieved += new CanFramesReceiveEventHandler(Port_Recieved);
            HandlerModels = new ObservableCollection<FrameHandlerModel>();
            InitializeComponent();
        }

        public ObservableCollection<FrameHandlerModel> HandlerModels { get; private set; }

        void Port_Recieved(object sender, CanFramesReceiveEventArgs e)
        {
            lock (HandlerModels)
            {
                foreach (var f in e.Frames)
                    if (!HandlerModels.Any(hm => hm.Descriptor == f.Descriptor && hm.Port == e.Port))
                    {
                        var hm = new FrameHandlerModel(f.Descriptor, e.Port);
                        hm.Initialize();
                        Dispatcher.BeginInvoke((Action<FrameHandlerModel>)HandlerModels.Add, hm);
                    }
            }
        }
    }
}
