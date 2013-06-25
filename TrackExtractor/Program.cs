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

namespace TrackExtractor
{
    class Program
    {
        const UInt16 Dsc_MM_ALT_LONG = 0x4268;


        static void Main(string[] args)
        {
            String CanRecordPath = args[0];
            String TrackPath = args[1];

            FrameSbsEncoder enc = new FrameSbsEncoder();
            using (var fs = new FileStream(CanRecordPath, FileMode.Open))
            {
                var Frames = enc.DecodeStream(fs);

                EarthPoint PrewPoint = null;

                foreach (var f in Frames)
                {
                    switch (f.Descriptor)
                    {
                        case Dsc_MM_ALT_LONG:
                            var mmll = BlokFrame.GetBlokFrame<MmAltLongFrame>(f);
                            //var ThisPoint = new EarthPoint();
                            break;
                    }
                }
            }
        }
    }
}
