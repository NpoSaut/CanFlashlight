using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Communications.Can.LogReader
{
    public class StreamEncoderPort<TEncoder> : CanVirtualPort
        where TEncoder : FrameEncoders.FrameStreamEncoder, new()
    {
        public TEncoder Encoder { get; private set; }
        private Stream DataStream { get; set; }

        public StreamEncoderPort(FileInfo LogFile)
            : base(LogFile.Name)
        {
            Encoder = new TEncoder();
            DataStream = LogFile.OpenRead();
        }

        protected override CanFrame ReadNextFrame()
        {
            return Encoder.DecodeNext(DataStream);
        }

        public override void Dispose()
        {
            base.Dispose();
            DataStream.Close();
        }
    }
}
