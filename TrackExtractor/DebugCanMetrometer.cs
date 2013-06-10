using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlokFrames;

namespace TrackExtractor
{
    [FrameDescriptor(0x9983)]
    public class DebugCanMetrometer : BlokFrame
    {
        public int X { get; set; }

        protected override byte[] GetCanFrameData()
        {
            throw new NotImplementedException();
        }

        protected override void FillWithCanFrameData(byte[] Data)
        {
            X = BitConverter.ToInt32(new Byte[] { Data[0], Data[1], Data[2], 0 }, 0);
        }

        public override string ToString()
        {
            return string.Format("X = {0:F0}", X);
        }
    }
}
