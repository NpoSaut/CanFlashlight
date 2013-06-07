using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Communications.Can.FrameEncoders
{
    public abstract class FrameStreamEncoder
    {
        public FrameStreamEncoder()
        { }

        public abstract CanFrame DecodeNext(Stream DataStream);
        public abstract void Write(CanFrame Frame, Stream DataStream);

        public IEnumerable<CanFrame> DecodeStream(Stream DataStream)
        {
            while (DataStream.Position < DataStream.Length)
            {
                yield return DecodeNext(DataStream);
            }
        }
        public void Write(IEnumerable<CanFrame> Frames, Stream DataStream)
        {
            foreach (var f in Frames)
                Write(f, DataStream);
        }
    }
}
