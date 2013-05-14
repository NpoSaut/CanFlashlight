using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications.Protocols.IsoTP.Frames
{
    /// <summary>
    /// Последовательный кадр
    /// </summary>
    /// <remarks>
    /// Кадр, несущий передающий основную часть информации
    /// </remarks>
    public class ConsecutiveFrame : IsoTpFrame
    {
        /// <summary>
        /// Вместимость пакета
        /// </summary>
        public const int DataCapacity = 7;
        /// <summary>
        /// Данные кадра
        /// </summary>
        public Byte[] Data { get; private set; }
        /// <summary>
        /// Порядковый номер кадра
        /// </summary>
        public int Index { get; private set; }

        public override IsoTpFrameType FrameType
        {
            get { return IsoTpFrameType.Consecutive; }
        }

        public ConsecutiveFrame()
        {
        }
        public ConsecutiveFrame(Byte[] Data, int Index)
        {
            if (Data.Length > DataCapacity) throw new ArgumentOutOfRangeException("Data", string.Format("Размер данных, передаваемых в каждом Consecutive режиме ограничен {0} байтами", DataCapacity));

            this.Data = Data;
            this.Index = Index;
        }

        public override Can.CanFrame GetCanFrame(int WithDescriptor)
        {
            Byte[] buff = new Byte[8];

            buff[0] = (byte)(((byte)FrameType & 0x0f) << 4 | Index & 0x0f);
            Buffer.BlockCopy(Data, 0, buff, 1, DataCapacity);

            return Can.CanFrame.NewWithDescriptor(WithDescriptor, buff);
        }

        protected override void FillWithBytes(byte[] buff)
        {
            this.Index = buff[0] & 0x0f;

            Data = new Byte[DataCapacity];
            Buffer.BlockCopy(buff, 1, Data, 0, DataCapacity);
        }

        public static implicit operator ConsecutiveFrame(Communications.Can.CanFrame cFrame)
        {
            return IsoTpFrame.ParsePacket<ConsecutiveFrame>(cFrame.Data);
        }
    }
}
