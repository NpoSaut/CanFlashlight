﻿using System;
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

namespace CanLighthouse
{
    /// <summary>
    /// Логика взаимодействия для SniffWindow.xaml
    /// </summary>
    public partial class SniffWindow : Window
    {
        public ObservableCollection<FrameModel> Frames { get; private set; }
        public ListCollectionView FramesCV { get; set; }
        public ObservableCollection<FilterModel> Filters { get; private set; }

        public List<Brush> HighlightBrushes { get; private set; }

        public SniffWindow()
        {
            // Необъяснимый костыль для поправки локализации строковых конвертеров
            Language = System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag);

            Frames = new ObservableCollection<FrameModel>();
            FramesCV = new ListCollectionView(Frames);

            Filters = new ObservableCollection<FilterModel>();

            HighlightBrushes = new List<Brush>()
                            {
                                Brushes.Black,
                                Brushes.Tomato, Brushes.Orange, Brushes.OrangeRed,
                                Brushes.Green, Brushes.YellowGreen, Brushes.Plum,
                                Brushes.Blue, Brushes.Violet, Brushes.BlueViolet, Brushes.CadetBlue
                            };

            InitializeComponent();

            LogReaderPort rp = new LogReaderPort(new FileInfo("can.txt"));

            rp.Recieved += new Communications.Can.CanFramesReceiveEventHandler(rp_Recieved);
            rp.Start();
        }
        private bool FrameFilter(object fo)
        {
            var fr = fo as FrameModel;
            return Filters.Where(f => f.IsEnabled).Any(f => f.Descriptor == fr.Descriptor);
        }

        void rp_Recieved(object sender, Communications.Can.CanFramesReceiveEventArgs e)
        {
            foreach (var f in e.Frames)
                Dispatcher.BeginInvoke((Action<FrameModel>)Frames.Add, new FrameModel(f));
        }

        private void FiltersEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!e.Changes.Any()) return;

            var s = sender as RichTextBox;

            Filters.Clear();

            foreach (var p in s.Document.Blocks.OfType<Paragraph>())
            {
                var str = (new TextRange(p.ContentStart, p.ContentEnd)).Text;
                var dm = System.Text.RegularExpressions.Regex.Match(str, @"[0-9a-fA-F]{4}");
                if (dm.Success)
                {
                    var descr = Convert.ToUInt16(dm.Value, 16);
                    FilterModel f;
                    Filters.Add((f = new FilterModel() { Descriptor = descr }));

                    f.Foreground = p.Foreground;
                    f.Background = p.Background;

                    f.IsEnabled = !str.Trim().StartsWith("//");

                    p.DataContext = f;
                }
            }

            if (Filters.Where(f => f.IsEnabled).Any()) FramesCV.Filter = FrameFilter;
            else FramesCV.Filter = null;
        }

        private void DataGridRow_Loaded(object sender, RoutedEventArgs e)
        {
            var s = sender as DataGridRow;
            var frame = s.DataContext as FrameModel;
            //var filter = Filters.SingleOrDefault(f => f.Descriptor == frame.Descriptor);
            //if (filter != null)
            //{
            //    s.Foreground = filter.Foreground;
            //    s.Background = filter.Background;
            //}
            s.SetResourceReference(Control.ForegroundProperty, string.Format("Brush{0}", frame.Descriptor.ToString("X4")));
        }

        private void FiltersEdit_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (FiltersEdit.Selection.Start.Paragraph != null)
                ColorPicker.SelectedItem = HighlightBrushes.SingleOrDefault(b => b.Equals(FiltersEdit.Selection.Start.Paragraph.Foreground)) ?? Foreground;
        }

        private void ColorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var p = FiltersEdit.Selection.Start.Paragraph;
            if (p != null)
            {
                var SetBrush = (Brush)(sender as ComboBox).SelectedItem;
                p.Foreground = SetBrush;
                var f = p.DataContext as FilterModel;
                if (f != null)
                {
                    var resourceKey = string.Format("Brush{0}", f.Descriptor.ToString("X4"));
                    if (!Resources.Contains(resourceKey)) Resources.Add(resourceKey, SetBrush);
                    else Resources[resourceKey] = SetBrush;
                }
            }
        }
    }
}
