using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlokFrames
{
    public class FrameDescriptorAttribute : Attribute
    {
        public int Descriptor { get; set; }

        public FrameDescriptorAttribute(int Descriptor)
        {
            this.Descriptor = Descriptor;
        }
    }
}
