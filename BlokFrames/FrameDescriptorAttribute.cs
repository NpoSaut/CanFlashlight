using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlokFrames
{
    public enum HalfsetKind { Uniset, SetA, SetB }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
    public class FrameDescriptorAttribute : Attribute
    {
        public int Descriptor { get; set; }
        public HalfsetKind Halfset { get; set; }

        public FrameDescriptorAttribute(int Descriptor)
        {
            this.Descriptor = Descriptor;
            this.Halfset = HalfsetKind.Uniset;
        }
        public FrameDescriptorAttribute(int Descriptor, HalfsetKind Halfset)
            : this(Descriptor)
        {
            this.Halfset = Halfset;
        }
    }
}
