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

namespace CanLighthouse
{
    /// <summary>
    /// Логика взаимодействия для SniffWindow.xaml
    /// </summary>
    public partial class SniffWindow : Window
    {
        public ObservableCollection<FrameModel> Frames { get; private set; }
        public ListCollectionView FramesCV { get; set; }
        public List<UInt16> Filters { get; private set; }

        public List<Brush> HighlightBrushes { get; private set; }

        private ResourceDictionary ColorsDic = new ResourceDictionary();

        public SniffWindow()
        {
            // Необъяснимый костыль для поправки локализации строковых конвертеров
            Language = System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag);

            Frames = new ObservableCollection<FrameModel>();
            FramesCV = new ListCollectionView(Frames);
            //FramesCV.Filter = FrameFilter;

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

            LogReaderPort rp = new LogReaderPort(new FileInfo("can.txt"));

            rp.Recieved += new Communications.Can.CanFramesReceiveEventHandler(rp_Recieved);
            rp.Start();
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

        void rp_Recieved(object sender, Communications.Can.CanFramesReceiveEventArgs e)
        {
            foreach (var f in e.Frames)
                Dispatcher.BeginInvoke((Action<FrameModel>)Frames.Add, new FrameModel(f));
        }

        private void FiltersEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!e.Changes.Any()) return;
            var s = sender as TextBox;

            var NewFilters = new List<ushort>();
            foreach (var str in s.Text.Split(new Char[] { '\n' }))
            {
                var descr = GetDescripter(str);
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

        private ushort GetDescripter(String str)
        {
            if (str.Trim().StartsWith("//")) return 0;

            var dm = System.Text.RegularExpressions.Regex.Match(str, @"[0-9a-fA-F]{4}");
            if (dm.Success)
            {
                return Convert.ToUInt16(dm.Value, 16);
            }
            else return 0;
        }
        private string GetResourceNameForDescripter(uint descripter)
        {
            return string.Format("Brush{0:X4}", descripter);
        }

        private void DataGridRow_Loaded(object sender, RoutedEventArgs e)
        {
            var s = sender as DataGridRow;
            var frame = s.DataContext as FrameModel;
            s.SetResourceReference(Control.ForegroundProperty, GetResourceNameForDescripter(frame.Descriptor));
        }

        private uint GetEditingDescripter()
        {
            string line = FiltersEdit.GetLineText(FiltersEdit.GetLineIndexFromCharacterIndex(FiltersEdit.SelectionStart));
            return GetDescripter(line ?? "");
        }

        private void FiltersEdit_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var descr = GetEditingDescripter();
            var brush = TryFindResource(GetResourceNameForDescripter(descr));
            ColorPicker.SelectedItem = brush;
        }

        private void ColorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var SetBrush = (Brush)(sender as ComboBox).SelectedItem;

            var des = GetEditingDescripter();
            var resourceKey = GetResourceNameForDescripter(des);
            if (!Resources.Contains(resourceKey)) Resources.Add(resourceKey, SetBrush);
            else Resources[resourceKey] = SetBrush;
        }
    }
}
