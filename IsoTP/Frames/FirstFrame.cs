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
        /// Вместимость пакета
        /// </summary>
        public const int DataCapacity = 6;
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
            if (Data.Length != DataCapacity) throw new ArgumentOutOfRangeException("Data", string.Format("Первый фрейм должен содержать ровно {0} байт данных", DataCapacity));

            this.Data = Data;
            this.PacketSize = PacketSize;
        }
        public FirstFrame()
        {
        }

        public override Can.CanFrame GetCanFrame(int WithDescriptor)
        {
            Byte[] buff = new Byte[8];

            buff[0] = (byte)(((byte)FrameType & 0x0f) << 4 | (PacketSize & 0xf00) >> 8);
            buff[1] = (byte)(PacketSize & 0x0ff);
            
            Buffer.BlockCopy(Data, 0, buff, 2, DataCapacity);

            return Can.CanFrame.NewWithDescriptor(WithDescriptor, buff);
        }

        protected override void FillWithBytes(byte[] buff)
        {
            PacketSize = buff[1] | (buff[0] & 0x0f) << 8;

            Data = new Byte[DataCapacity];
            Buffer.BlockCopy(buff, 2, Data, 0, DataCapacity);
        }

        public static implicit operator FirstFrame(Communications.Can.CanFrame cFrame)
        {
            return IsoTpFrame.ParsePacket<FirstFrame>(cFrame.Data);
        }
    }
}
