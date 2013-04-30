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

namespace CanLighthouse
{
    /// <summary>
    /// Логика взаимодействия для SniffWindow.xaml
    /// </summary>
    public partial class SniffWindow : Window
    {
        private const int CutOffCount = 100;
        public ObservableCollection<FrameModel> Frames { get; private set; }
        public ListCollectionView FramesCV { get; set; }
        public List<UInt16> Filters { get; private set; }

        public List<Brush> HighlightBrushes { get; private set; }

        private ResourceDictionary ColorsDic = new ResourceDictionary();

        public ObservableCollection<CanPort> ListeningPorts { get; private set; }

        public SniffWindow(IEnumerable<CanPort> OnPorts)
        {
            // Необъяснимый костыль для поправки локализации строковых конвертеров
            Language = System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag);

            Frames = new ObservableCollection<FrameModel>();
            FramesCV = new ListCollectionView(Frames);
            Frames.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Frames_CollectionChanged);

            Filters = new List<ushort>();

            HighlightBrushes = new List<Brush>()
                            {
                                Brushes.Black,
                                Brushes.Tomato, Brushes.Orange, Brushes.OrangeRed,
                                Brushes.Green, Brushes.YellowGreen, Brushes.Plum,
                                Brushes.Blue, Brushes.Violet, Brushes.BlueViolet, Brushes.CadetBlue
                            };

            InitializeComponent();

            LoadColorScheme("Default.chl");

            ListeningPorts = new ObservableCollection<CanPort>();
            ListeningPorts.CollectionChanged += new NotifyCollectionChangedEventHandler(ListeningPorts_CollectionChanged);
            foreach (var lp in OnPorts) ListeningPorts.Add(lp);
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

        void Frames_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //if (e.NewItems != null)
            //    LogGrid.ScrollIntoView(e.NewItems[e.NewItems.Count - 1]);
        }

        private void LoadColorScheme(String FileName)
        {
            var Highlights = XDocument.Load(FileName).Root
                                .Elements("highlight")
                                .Select(hl =>
                                    new
                                    {
                                        desc = hl.Attribute("descriptor").Value,
                                        Brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hl.Attribute("color").Value))
                                    });
            foreach (var hl in Highlights)
            {
                this.Resources.Add(string.Format("Brush{0}", hl.desc), hl.Brush);
                HighlightBrushes.Add(hl.Brush);
            }
        }

        private bool FrameFilter(object fo)
        {
            var fr = fo as FrameModel;
            return Filters.Contains(fr.Descriptor);
        }

        void CanFrames_Recieved(object sender, Communications.Can.CanFramesReceiveEventArgs e)
        {
            var FrameModels = e.Frames.Select(f => new FrameModel(f) { PortName = (sender as CanPort).Name }).ToList();
            foreach (var f in FrameModels)
                FramesToInterfaceBuffer.Enqueue(f);

            if (!FramesSyncronizationScheduled)
            {
                FramesSyncronizationScheduled = true;
                Dispatcher.BeginInvoke((Action)SyncronizeFramesOutput,
                    System.Windows.Threading.DispatcherPriority.Background);
            }
        }
        private bool FramesSyncronizationScheduled = false;
        private ConcurrentQueue<FrameModel> FramesToInterfaceBuffer = new ConcurrentQueue<FrameModel>();
        private void SyncronizeFramesOutput()
        {
            //for (int i = 0; i < Frames.Count + FramesToInterfaceBuffer.Count - CutOffCount; i++)
            //    Frames.RemoveAt(0);
            this.Title = string.Join("", Enumerable.Repeat('.', FramesToInterfaceBuffer.Count));
            FrameModel f;
            while (FramesToInterfaceBuffer.TryDequeue(out f))
            {
                Frames.Add(f);
            }
            FramesSyncronizationScheduled = false;
            LogGrid.ScrollIntoView(Frames.Last());
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

            if (!Filters.SequenceEqual(NewFilters))
            {
                Filters.Clear();
                Filters.AddRange(NewFilters);
                if (Filters.Any()) FramesCV.Filter = FrameFilter;
                else FramesCV.Filter = null;
            }
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
        private string GetResourceNameForDescriptor(uint descriptor)
        {
            return string.Format("Brush{0:X4}", descriptor);
        }

        private uint GetEditingDescriptor()
        {
            string line = FiltersEdit.GetLineText(FiltersEdit.GetLineIndexFromCharacterIndex(FiltersEdit.SelectionStart));
            return GetDescriptor(line ?? "");
        }

        private void FiltersEdit_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var descr = GetEditingDescriptor();
            var brush = TryFindResource(GetResourceNameForDescriptor(descr));
            ColorPicker.SelectedItem = brush;
        }

        private void ColorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var SetBrush = (Brush)(sender as ComboBox).SelectedItem;

            var des = GetEditingDescriptor();
            var resourceKey = GetResourceNameForDescriptor(des);
            if (!Resources.Contains(resourceKey)) Resources.Add(resourceKey, SetBrush);
            else Resources[resourceKey] = SetBrush;
        }

        private void LogGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var frame = e.Row.DataContext as FrameModel;
            e.Row.SetResourceReference(Control.ForegroundProperty, GetResourceNameForDescriptor(frame.Descriptor));
        }
    }
}
