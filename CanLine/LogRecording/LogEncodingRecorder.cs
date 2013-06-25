using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Communications.Can.FrameEncoders;

namespace Communications.Can.LogRecording
{
    public class LogEncodingRecorder<TEncoder> : LogRecorder
        where TEncoder : FrameStreamEncoder, new()
    {
        public FrameStreamEncoder Encoder { get; private set; }

        public LogEncodingRecorder(CanPort Port, FileInfo LogFile)
            : base(Port, LogFile)
        {
            Encoder = new TEncoder();
        }

        public override void WriteFrames(IEnumerable<CanFrame> Frames)
        {
            Encoder.Write(Frames, FileStream);
        }
    }
}
