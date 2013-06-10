using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;
using Communications.Can.FrameEncoders;
using System.IO;
using System.Runtime.InteropServices;
using GMapElements;
using BlokFrames;
using System.Xml.Linq;
using System.Globalization;

namespace TrackExtractor
{
    enum MarkerKind : byte { Target = 1, Kilometer = 2, Piket = 3, EmapTarget, XxTarget }

    class MarkerOnTrack
    {
        public MarkerKind Kind { get; set; }
        public EarthPoint Point { get; set; }
        public DateTime Time { get; set; }

        public override string ToString()
        {
            if (Name != null) return Name;
            switch (Kind)
            {
                case MarkerKind.Kilometer: return "КМ";
                case MarkerKind.Piket: return "ПК";
                case MarkerKind.Target: return "Цель";
                default: return Kind.ToString();
            }
        }

        public String Name { get; set; }
        public String Description { get; set; }
    }

    class Program
    {
        const UInt16 Dsc_MM_ALT_LONG = 0x4268;
        const UInt16 Dsc_OBJECT = 0x8401;
        const UInt16 Dsc_METROMETER = 0x9983;
        const UInt16 Dsc_MM_STATE = 0x43E8;
        const UInt16 Dsc_XX_STATE = 0x8448;

        static void Main(string[] args)
        {
            String CanRecordPath = args[0];
            String TracksDirectoryPath = args[1];

            List<MarkerOnTrack> Markers = new List<MarkerOnTrack>();

            FrameSbsEncoder enc = new FrameSbsEncoder();
            using (var fs = new FileStream(CanRecordPath, FileMode.Open))
            {
                var Frames = enc.DecodeStream(fs);

                List<MarkerOnTrack> MarkersToBePositioned = new List<MarkerOnTrack>();
                MmAltLongFrame PrewPoint = null;
                DebugCanMetrometer PrewX = null;

                MmState[] EmapTargets = new MmState[10];
                DebugMmState[] XxTargets = new DebugMmState[10];

                foreach (var f in Frames)
                {
                    switch (f.Descriptor)
                    {
                        // Точка трека
                        case Dsc_MM_ALT_LONG:
                            var ThisPoint = BlokFrame.GetBlokFrame<MmAltLongFrame>(f);

                            if (PrewPoint != null)
                            {
                                foreach (var m in MarkersToBePositioned)
                                {
                                    m.Point = InterpolatePoint(PrewPoint, ThisPoint, m.Time);
                                    Markers.Add(m);
                                }
                                MarkersToBePositioned.Clear();
                            }

                            PrewPoint = ThisPoint;
                            break;
                                                
                        case Dsc_METROMETER:
                            var thisX = BlokFrame.GetBlokFrame<DebugCanMetrometer>(f);

                            if (thisX.Time.Hour >= 19 && thisX.Time.Minute > 30)
                            { }

                            if (PrewX != null)
                            {
                                Pook(MarkersToBePositioned, PrewX, EmapTargets, thisX, MarkerKind.EmapTarget);
                                Pook(MarkersToBePositioned, PrewX, XxTargets, thisX, MarkerKind.XxTarget);
                            }

                            PrewX = thisX;
                            break;

                        case Dsc_MM_STATE:
                            var mmst = BlokFrame.GetBlokFrame<MmState>(f);
                            EmapTargets[mmst.TargetNumber] = mmst;
                            break;

                        case Dsc_XX_STATE:
                            var dmmst = BlokFrame.GetBlokFrame<DebugMmState>(f);
                            XxTargets[dmmst.TargetNumber] = dmmst;
                            break;

                        // Нажатие отладочной кнопки
                        case Dsc_OBJECT:
                            var debugMess = BlokFrame.GetBlokFrame<DebugCanMessage>(f);

                            switch (debugMess.Kind)
                            {
                                case DebugMarkerKind.Target:
                                case DebugMarkerKind.Post:
                                case DebugMarkerKind.Picket:
                                    MarkersToBePositioned.Add(
                                        new MarkerOnTrack()
                                        {
                                            Time = f.Time,
                                            Kind = (MarkerKind)((byte)debugMess.Kind)
                                        });
                                    break;

                                case DebugMarkerKind.Cancel:
                                    if (MarkersToBePositioned.Any())
                                        MarkersToBePositioned.Remove(MarkersToBePositioned.Last());
                                    else
                                        Markers.Remove(Markers.Last());
                                    break;
                            }

                            break;
                    }
                }

                Directory.CreateDirectory(TracksDirectoryPath);

                ExportMarkers(Markers.Where(m => m.Kind == MarkerKind.Target), Path.Combine(TracksDirectoryPath, "RealTargets.kml"));
                ExportMarkers(Markers.Where(m => m.Kind == MarkerKind.Kilometer), Path.Combine(TracksDirectoryPath, "RealKilometers.kml"));
                ExportMarkers(Markers.Where(m => m.Kind == MarkerKind.Piket), Path.Combine(TracksDirectoryPath, "RealPickets.kml"));
                ExportMarkers(Markers.Where(m => m.Kind == MarkerKind.EmapTarget), Path.Combine(TracksDirectoryPath, "EmapTargets.kml"));
                ExportMarkers(Markers.Where(m => m.Kind == MarkerKind.XxTarget), Path.Combine(TracksDirectoryPath, "XxTargets.kml"));
            }
        }

        private static void Pook(List<MarkerOnTrack> MarkersToBePositioned, DebugCanMetrometer PrewX, MmState[] TargetsArray, DebugCanMetrometer thisX, MarkerKind mk)
        {
            foreach (var PassedTarget in TargetsArray.Where(t => t != null && t.Target.Kind != MapTargetKind.InvalidTarget &&
                                                                (t.Target.Kind == MapTargetKind.TrafficLight || t.Target.Kind == MapTargetKind.Crossing) &&
                                                                 t.Target.X >= (UInt16)PrewX.X && t.Target.X <= (UInt16)thisX.X))
            {
                double xRatio;
                if (PrewX.X != thisX.X)
                    xRatio = (double)(PassedTarget.Target.X - (UInt16)PrewX.X) / (thisX.X - (UInt16)PrewX.X);
                else
                    xRatio = 0;
                DateTime PassingTime = DateTime.FromBinary((long)(PrewX.Time.Ticks * (1 - xRatio) + thisX.Time.Ticks * xRatio));

                MarkersToBePositioned.Add(
                    new MarkerOnTrack()
                    {
                        Kind = mk,
                        Name = PassedTarget.Target.Kind.ToString(),
                        Description = string.Format("{0}\n{1}\n{2:X8}\n{3:X8}", PassedTarget.Target.Kind.ToString(), PassingTime, PassedTarget.Target.X, thisX.X),
                        Time = PassingTime
                    });

                //TargetsArray[PassedTarget.TargetNumber] = null;
            }
        }

        private static EarthPoint InterpolatePoint(MmAltLongFrame p1, MmAltLongFrame p2, DateTime PointTime)
        {
            double ratio = (PointTime - p1.Time).TotalMilliseconds / (p2.Time - p1.Time).TotalMilliseconds;
            var res = new EarthPoint(
                p1.Latitude * (1 - ratio) + p2.Latitude * ratio,
                p1.Longitude * (1 - ratio) + p2.Longitude * ratio
                );
            return res;
        }
        private static Double InterpolatePoint(DebugCanMetrometer p1, DebugCanMetrometer p2, DateTime PointTime)
        {
            double ratio = (PointTime - p1.Time).TotalMilliseconds / (p2.Time - p1.Time).TotalMilliseconds;
            return p1.X + (1 - ratio) + p2.X * ratio;
        }

        private static void ExportMarkers(IEnumerable<MarkerOnTrack> markers, string FileName)
        {
            var KMap = new XDocument(
                new XElement(XName.Get("kml", "http://www.opengis.net/kml/2.2"),
                    new XElement("Document",
                        new XElement("name", "Green Field"),
                            markers.Select(m => 
                                new XElement("Placemark",
                                    new XElement("name", m.ToString()),
                                    new XElement("description", m.Description),
                                    new XElement("Point",
                                        new XElement("coordinates", string.Format(CultureInfo.InvariantCulture.NumberFormat,
                                                                                    "{0},{1},0",
                                                                                    m.Point.Longitude, m.Point.Latitude))))))
                    ));
            KMap.Save(FileName);
        }
    }
}
