using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications.Protocols.IsoTP.Frames
{
    public class ConsecutiveFrame : IsoTpFrame
    {
        public Byte[] Data { get; private set; }
        public int Index { get; private set; }

        public override IsoTpFrameType FrameType
        {
            get { return IsoTpFrameType.Consecutive; }
        }

        internal ConsecutiveFrame()
        {
        }
        public ConsecutiveFrame(Byte[] Data, int Index)
        {
            if (Data.Length > )
        }

        public override Can.CanFrame GetCanFrame(int WithDescriptor)
        {
            Byte[] buff = new Byte[8];

            buff[0] = (byte)(((byte)FrameType & 0x0f) | (Index & 0x0f) << 4);
            Buffer.BlockCopy(Data, 0, buff, 1, 7);

            return Can.CanFrame.NewWithDescriptor(WithDescriptor, buff);
        }

        protected override void FillWithBytes(byte[] buff)
        {
            this.Index = (buff[0] & 0xf0) >> 4;

            Data = new Byte[8];
            Buffer.BlockCopy(buff, 1, Data, 0, 7);
        }
    }
}
