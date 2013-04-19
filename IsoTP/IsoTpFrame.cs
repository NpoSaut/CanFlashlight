using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;

namespace Communications.Protocols.IsoTP
{
    public enum IsoTpFrameType : byte { Single = 0x0, First = 0x1, Consecutive = 0x2, FlowControl = 0x3 }

    public abstract class IsoTpFrame
    {
        public abstract IsoTpFrameType FrameType { get; }

        public abstract CanFrame GetCanFrame(int WithDescriptor);

        protected abstract void FillWithBytes(Byte[] buff);

        public T ParsePacket<T>(Byte[] buff)
            where T : IsoTpFrame, new()
        {
            var p = new T();
            p.FillWithBytes(buff);
            return p;
        }
    }
}
