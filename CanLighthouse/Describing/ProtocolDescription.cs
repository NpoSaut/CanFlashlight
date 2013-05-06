using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Windows.Media;
using System.Windows;
using System.Collections.Concurrent;

namespace CanLighthouse.Describing
{
    public class ProtocolDescription
    {
        public String Name { get; set; }
        public ConcurrentDictionary<int, FrameDescription> FrameDescriptions { get; private set; }

        public ProtocolDescription()
        {
            FrameDescriptions = new ConcurrentDictionary<int,FrameDescription>();
        }

        public static ProtocolDescription Load(String FileName)
        {
            XDocument doc = XDocument.Load(FileName);

            return new ProtocolDescription()
            {
                Name = (String)doc.Root.Attribute("Name"),
                FrameDescriptions = new ConcurrentDictionary<int,FrameDescription>(
                doc.Root.Elements("FrameDescription").Select(XFrameDescription =>
                    new FrameDescription()
                    {
                        Descriptor = Convert.ToInt32(XFrameDescription.Attribute("Descriptor").Value, 16),
                        Name = (String)XFrameDescription.Attribute("Name"),
                        HighlightColor = XFrameDescription.Attribute("Color") != null ?
                                            (Color)ColorConverter.ConvertFromString(XFrameDescription.Attribute("Color").Value) :
                                            Colors.Black
                    }).ToDictionary(fd => fd.Descriptor))
            };
        }

        public void FillupResource(ResourceDictionary res)
        {
            foreach (var fd in FrameDescriptions.Values)
            {
                res.Add(GetBrushResourceNameForDescriptor(fd.Descriptor),
                    new SolidColorBrush(fd.HighlightColor));
            }
        }

        public static string GetBrushResourceNameForDescriptor(int Descriptor)
        {
            return string.Format("Brush{0:X4}", Descriptor);
        }
    }
}
