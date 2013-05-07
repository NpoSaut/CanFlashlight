using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;

namespace Communications.Protocols.IsoTP.Frames
{
    public enum IsoTpFrameType : byte { Single = 0x0, First = 0x1, Consecutive = 0x2, FlowControl = 0x3 }

    /// <summary>
    /// Базовый класс для кадров ISO-TP
    /// </summary>
    public abstract class IsoTpFrame
    {
        /// <summary>
        /// Тип кадра
        /// </summary>
        public abstract IsoTpFrameType FrameType { get; }

        /// <summary>
        /// Заворачивает кадр ISO-TP в CAN-сообщение
        /// </summary>
        /// <param name="WithDescriptor">Дескриптор CAN-сообщения</param>
        public abstract CanFrame GetCanFrame(int WithDescriptor);

        protected abstract void FillWithBytes(Byte[] buff);

        /// <summary>
        /// Разбирает буфер сообщения, извлекая оттуда кадр ISO-TP
        /// </summary>
        /// <typeparam name="T">Тип кадра ISO-TP</typeparam>
        /// <param name="buff">Буфер данных</param>
        /// <returns>Разобранный кадр</returns>
        public static T ParsePacket<T>(Byte[] buff)
            where T : IsoTpFrame, new()
        {
            var p = new T();
            p.FillWithBytes(buff);
            return p;
        }

        /// <summary>
        /// Определяет тип кадра, содержащегося в буфере
        /// </summary>
        public static IsoTpFrameType GetFrameType(Byte[] buff)
        {
            return (IsoTpFrameType)(buff[0] >> 4);
        }
    }
}
