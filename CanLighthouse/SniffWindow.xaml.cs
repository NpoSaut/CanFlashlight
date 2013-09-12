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
using Communications.Can.LogReader;
using System.IO;
using CanLighthouse.Models;
using System.Collections.ObjectModel;
using Communications.Can;
using System.Xml.Linq;
using System.Collections.Specialized;
using System.Collections.Concurrent;
using CanLighthouse.Describing;
using System.Threading.Tasks;
using System.ComponentModel;

namespace CanLighthouse
{
    /// <summary>
    /// Логика взаимодействия для SniffWindow.xaml
    /// </summary>
    public partial class SniffWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(String PropertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }

        public List<Brush> HighlightBrushes { get; private set; }
        private ResourceDictionary ColorsDic = new ResourceDictionary();

        public FramesProcessor Frames { get; set; }

        private bool _Autoscroll = true;
        public bool Autoscroll
        {
            get { return _Autoscroll; }
            set
            {
                if (_Autoscroll != value)
                {
                    _Autoscroll = value;
                    OnPropertyChanged("Autoscroll");
                }
            }
        }

        private bool _BeepOnFrames = false;
        public bool BeepOnFrames
        {
            get { return _BeepOnFrames; }
            set
            {
                if (_BeepOnFrames != value)
                {
                    _BeepOnFrames = value;
                    Frames.BeepWhenFrameReceived = value;
                    OnPropertyChanged("BeepOnFrames");
                }
            }
        }

        public ObservableCollection<CanPort> ListeningPorts { get; private set; }

        public SniffWindow(IEnumerable<CanPort> OnPorts)
        {
            // Необъяснимый костыль для поправки локализации строковых конвертеров
            Language = System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag);

            HighlightBrushes = new List<Brush>()
                            {
                                Brushes.Black,
                                Brushes.Tomato, Brushes.Orange, Brushes.OrangeRed,
                                Brushes.Green, Brushes.YellowGreen, Brushes.Plum,
                                Brushes.Blue, Brushes.Violet, Brushes.BlueViolet, Brushes.CadetBlue
                            };

            Frames = new FramesProcessor();
            Frames.NewItemAdEnd += new EventHandler<FramesProcessor.NewItemAdEndEventArgs>(Frames_NewItemAdEnd);

            InitializeComponent();

            ListeningPorts = new ObservableCollection<CanPort>();
            ListeningPorts.CollectionChanged += new NotifyCollectionChangedEventHandler(ListeningPorts_CollectionChanged);
            foreach (var lp in OnPorts) ListeningPorts.Add(lp);


            Dispatcher.BeginInvoke((Action<String>)(txt => FiltersEdit.Text = txt), Properties.Settings.Default.LastFilters);
        }

        void Frames_NewItemAdEnd(object sender, FramesProcessor.NewItemAdEndEventArgs e)
        {
            if (Autoscroll)
                LogGrid.ScrollIntoView(e.LastItem);
        }

        void ListeningPorts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (var np in e.NewItems.OfType<CanPort>())
                    np.Recieved += CanFrames_Recieved;

            if (e.OldItems != null)
                foreach (var np in e.OldItems.OfType<CanPort>())
                    np.Recieved -= CanFrames_Recieved;

            this.Title = String.Format("Прослушивание {0}", string.Join(", ", ListeningPorts.Select(p => p.Name)));
        }

        void CanFrames_Recieved(object sender, Communications.Can.CanFramesReceiveEventArgs e)
        {
            var FrameModels = e.Frames.Select(f => new FrameModel(f) { PortName = (sender as CanPort).Name }).ToList();
            Frames.PushFrames(FrameModels);
        }

        private void FiltersEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!e.Changes.Any()) return;
            var s = sender as TextBox;

            var NewFilters = new List<ushort>();
            foreach (var str in s.Text.Split(new Char[] { '\n' }))
            {
                var descr = GetDescriptor(str);
                if (descr > 0)
                    NewFilters.Add(descr);
            }

            if (!Frames.Filters.SequenceEqual(NewFilters))
            {
                Frames.Filters = new HashSet<ushort>(NewFilters);
            }

            Properties.Settings.Default.LastFilters = (sender as TextBox).Text;
            Properties.Settings.Default.Save();
        }

        private ushort GetDescriptor(String str)
        {
            if (str.Trim().StartsWith("//")) return 0;

            var dm = System.Text.RegularExpressions.Regex.Match(str, @"[0-9a-fA-F]{4}");
            if (dm.Success)
            {
                return Convert.ToUInt16(dm.Value, 16);
            }
            else return 0;
        }

        private int GetEditingDescriptor()
        {
            string line = FiltersEdit.GetLineText(FiltersEdit.GetLineIndexFromCharacterIndex(FiltersEdit.SelectionStart));
            return GetDescriptor(line ?? "");
        }

        private void FiltersEdit_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var descr = GetEditingDescriptor();
            var brush = TryFindResource(ProtocolDescription.GetBrushResourceNameForDescriptor(descr));
            ColorPicker.SelectedItem = brush;
        }

        private void ColorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var SetBrush = (Brush)(sender as ComboBox).SelectedItem;

            var des = GetEditingDescriptor();
            var resourceKey = ProtocolDescription.GetBrushResourceNameForDescriptor(des);
            if (!Resources.Contains(resourceKey)) Resources.Add(resourceKey, SetBrush);
            else Resources[resourceKey] = SetBrush;
        }

        private void LogGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var frame = e.Row.DataContext as FrameModel;
            e.Row.SetResourceReference(Control.ForegroundProperty, ProtocolDescription.GetBrushResourceNameForDescriptor(frame.Descriptor));
        }

        private void sw_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void LogGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StatisticsGrid.DataContext = new SniffStatisticsModel((sender as ListBox).SelectedItems.OfType<FrameModel>().ToList());
        }

        // Команда на очистку экрана
        private void ClearCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        { e.CanExecute = true; }
        private void ClearCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Frames.Clear();
        }

        private void ToggleAutoScrollCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        { Autoscroll = !Autoscroll; }

        private void ToggleBeepCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        { BeepOnFrames = !BeepOnFrames; }

        private void LogItem_Loaded(object sender, RoutedEventArgs e)
        {
            var s = sender as ListBoxItem;
            var f = s.DataContext as FrameModel;
            s.SetResourceReference(Control.ForegroundProperty, ProtocolDescription.GetBrushResourceNameForDescriptor(f.Descriptor));
        }
    }
}
