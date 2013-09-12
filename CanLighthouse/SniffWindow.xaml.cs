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

namespace CanLighthouse
{
    /// <summary>
    /// Логика взаимодействия для SniffWindow.xaml
    /// </summary>
    public partial class SniffWindow : Window
    {
        private const int CutOffCount = 3000;
        public ObjectiveCommons.Collections.LockableObservableCollection<FrameModel> Frames { get; private set; }
        public ListCollectionView FramesCV { get; set; }
        public HashSet<UInt16> Filters { get; private set; }

        public List<Brush> HighlightBrushes { get; private set; }

        private ResourceDictionary ColorsDic = new ResourceDictionary();

        public ObservableCollection<CanPort> ListeningPorts { get; private set; }

        public SniffWindow(IEnumerable<CanPort> OnPorts)
        {
            // Необъяснимый костыль для поправки локализации строковых конвертеров
            Language = System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag);

            Frames = new ObjectiveCommons.Collections.LockableObservableCollection<FrameModel>();
            FramesCV = new ListCollectionView(Frames);
            Frames.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Frames_CollectionChanged);

            Filters = new HashSet<ushort>();

            HighlightBrushes = new List<Brush>()
                            {
                                Brushes.Black,
                                Brushes.Tomato, Brushes.Orange, Brushes.OrangeRed,
                                Brushes.Green, Brushes.YellowGreen, Brushes.Plum,
                                Brushes.Blue, Brushes.Violet, Brushes.BlueViolet, Brushes.CadetBlue
                            };

            InitializeComponent();

            ListeningPorts = new ObservableCollection<CanPort>();
            ListeningPorts.CollectionChanged += new NotifyCollectionChangedEventHandler(ListeningPorts_CollectionChanged);
            foreach (var lp in OnPorts) ListeningPorts.Add(lp);


            Dispatcher.BeginInvoke((Action<String>)(txt => FiltersEdit.Text = txt), Properties.Settings.Default.LastFilters);
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

        private bool FrameFilter(object fo)
        {
            if (!Filters.Any()) return true;
            else
            {
                var fr = fo as FrameModel;
                return Filters.Contains(fr.Descriptor);
            }
        }

        void CanFrames_Recieved(object sender, Communications.Can.CanFramesReceiveEventArgs e)
        {
            var FrameModels = e.Frames.Select(f => new FrameModel(f) { PortName = (sender as CanPort).Name }).ToList();

            if (FrameModels.Any(f => FrameFilter(f)))
                Beeper.Beep();

            foreach (var f in FrameModels)
                FramesToInterfaceBuffer.Enqueue(f);

            if (!FramesSyncronizationScheduled)
            {
                FramesSyncronizationScheduled = true;

                var xxx = FramesToInterfaceBuffer;
                FramesToInterfaceBuffer = new Queue<FrameModel>();
                Action UpdateAction =
                    () =>
                        Dispatcher.BeginInvoke((Action<Queue<FrameModel>>)SyncronizeFramesOutput,
                            System.Windows.Threading.DispatcherPriority.Background,
                            xxx);
                Task.Factory.StartNew(UpdateAction);
            }
        }
        private bool FramesSyncronizationScheduled = false;
        private Queue<FrameModel> FramesToInterfaceBuffer = new Queue<FrameModel>();
        private void SyncronizeFramesOutput(Queue<FrameModel> Buffer)
        {
            int itemsToDelete = Frames.Count + Buffer.Count - CutOffCount;
            for (int i = 0; i < itemsToDelete; i++)
            {
                if (Frames.Count > 0) Frames.RemoveAt(0);
                else Buffer.Dequeue();
            }

            using (Frames.Locker())
            {
                foreach (var f in Buffer)
                {
                    Frames.Add(f);
                }
            }

            if (AutostrollMenuItem.IsChecked && !LogGrid.Items.IsEmpty)
                ScrollToLastItem();

            System.Threading.Thread.Sleep(0);
            FramesSyncronizationScheduled = false;

        }
        private void ScrollToLastItem()
        {
            var LastVisibleFrame = FramesCV.OfType<FrameModel>().LastOrDefault();
            if (LastVisibleFrame != null)
                LogGrid.ScrollIntoView(LastVisibleFrame);
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
                Filters = new HashSet<ushort>(NewFilters);
                if (Filters.Any()) FramesCV.Filter = FrameFilter;
                else FramesCV.Filter = null;
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
        { AutostrollMenuItem.IsChecked = !AutostrollMenuItem.IsChecked; }

        private void ToggleBeepCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        { BeepMenuItem.IsChecked = !BeepMenuItem.IsChecked; }
    }
}
