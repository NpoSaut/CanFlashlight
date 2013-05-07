using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications.Protocols.IsoTP.Frames
{
    /// <summary>
    /// Первый кадр транзакции
    /// </summary>
    /// <remarks>
    /// Этим кадром инициируется транзакция
    /// </remarks>
    public class FirstFrame : IsoTpFrame
    {
        /// <summary>
        /// Размер полного пакета
        /// </summary>
        public int PacketSize { get; private set; }
        /// <summary>
        /// Данные
        /// </summary>
        public Byte[] Data { get; private set; }

        public override IsoTpFrameType FrameType
        {
            get { return IsoTpFrameType.First; }
        }

        public FirstFrame(Byte[] Data, int PacketSize)
        {
            if (Data.Length != 6) throw new ArgumentOutOfRangeException("Data", "Первый фрейм должен содержать ровно 6 байт данных");

            this.Data = Data;
            this.PacketSize = PacketSize;
        }
        public FirstFrame()
        {
        }

        public override Can.CanFrame GetCanFrame(int WithDescriptor)
        {
            Byte[] buff = new Byte[8];

            buff[0] = (byte)(((byte)FrameType & 0x0f) | (PacketSize & 0x00f) << 4);
            buff[1] = (byte)(PacketSize & 0xff0 >> 4);
            Buffer.BlockCopy(Data, 0, buff, 2, 6);

            return Can.CanFrame.NewWithDescriptor(WithDescriptor, buff);
        }

        protected override void FillWithBytes(byte[] buff)
        {
            PacketSize = ((buff[0] & 0xf0) >> 4) | (buff[1] << 4);

            Data = new Byte[8];
            Buffer.BlockCopy(buff, 2, Data, 0, 6);
        }
    }
}
