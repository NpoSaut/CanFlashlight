using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace CanLighthouse.Describing
{
    public class FrameDescription
    {
        public int Descriptor { get; set; }
        public int Identifer
        {
            get { return Descriptor / 0x20; }
            set { Descriptor = value * 0x20 + (Descriptor % 0x20); }
        }

        public Color HighlightColor { get; set; }
        public String Name { get; set; }
    }
}
