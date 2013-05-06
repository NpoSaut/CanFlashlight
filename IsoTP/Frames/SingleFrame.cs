using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;

namespace Communications.Protocols.IsoTP.Frames
{
    /// <summary>
    /// Кадр единичной посылки
    /// </summary>
    /// <remarks>
    /// Отправляется в виде одного сообщения, и может содержать максимум 7 байт данных
    /// </remarks>
    public class SingleFrame : IsoTpFrame
    {
        public Byte[] Data { get; private set; }

        public override IsoTpFrameType FrameType
        {
            get { return IsoTpFrameType.Single; }
        }

        public SingleFrame(Byte[] Data)
        {
            if (Data == null) throw new ArgumentNullException("Data");
            if (Data.Length > 7) throw new ArgumentOutOfRangeException("Data", "Размер данных, передаваемых в Single Frame режиме ограничен 7 байтами");

            this.Data = Data;
        }
        internal SingleFrame()
        {
        }

        public override CanFrame GetCanFrame(int WithDescriptor)
        {
            Byte[] buff = new Byte[8];

            buff[0] = (byte)(((byte)FrameType & 0x0f) | (Data.Length & 0x0f) << 4);
            Buffer.BlockCopy(Data, 0, buff, 1, Data.Length);

            return CanFrame.NewWithDescriptor(WithDescriptor, buff);
        }

        protected override void FillWithBytes(byte[] buff)
        {
            int len = (buff[0] & 0xf0) >> 4;

            Data = new Byte[len];
            Buffer.BlockCopy(buff, 1, Data, 0, len);
        }
    }
}
