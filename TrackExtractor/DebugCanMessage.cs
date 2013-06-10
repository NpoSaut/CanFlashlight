using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlokFrames;

namespace TrackExtractor
{
    public enum DebugMarkerKind : byte { Target = 1, Post = 2, Picket = 3, Cancel = 0xff }

    [FrameDescriptor(0x8401)]
    public class DebugCanMessage : BlokFrame
    {
        public DebugMarkerKind Kind { get; set; }

        protected override byte[] GetCanFrameData()
        {
            throw new NotImplementedException();
        }

        protected override void FillWithCanFrameData(byte[] Data)
        {
            this.Kind = (DebugMarkerKind)Data[0];
        }
    }
}
