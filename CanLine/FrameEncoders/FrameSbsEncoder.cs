using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Communications.Can.FrameEncoders
{
    public class FrameSbsEncoder : FrameStreamEncoder
    {
        public override CanFrame DecodeNext(Stream DataStream)
        {
            Byte[] HeaderBuffer = new Byte[8 + 2];
            if (DataStream.Read(HeaderBuffer, 0, HeaderBuffer.Length) < HeaderBuffer.Length)
                return null;
            var dt = DateTime.FromBinary(BitConverter.ToInt64(HeaderBuffer, 0));
            UInt16 descripter = BitConverter.ToUInt16(HeaderBuffer, 8);

            var frame = CanFrame.NewWithDescriptor(descripter);
            frame.Time = dt;
            DataStream.Read(frame.Data, 0, frame.Data.Length);
            return frame;
        }

        public override void Write(CanFrame Frame, Stream DataStream)
        {
            int length = Frame.Data.Length + 2 + 8;
            var buff = new Byte[length];
            Buffer.BlockCopy(BitConverter.GetBytes(Frame.Time.ToBinary()), 0, buff, 0, 8);
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)Frame.Descriptor), 0, buff, 8, 2);
            Buffer.BlockCopy(Frame.Data, 0, buff, 8 + 2, Frame.Data.Length);

            DataStream.Write(buff, 0, buff.Length);
        }
    }
}
