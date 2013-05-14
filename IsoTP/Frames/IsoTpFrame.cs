using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;

namespace Communications.Protocols.IsoTP.Frames
{
    public enum IsoTpFrameType : byte { Single = 0x0, First = 0x1, Consecutive = 0x2, FlowControl = 0x3, Invalid = 0xff }

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
            var TypeCode = (buff[0] & 0xf0) >> 4;
            if (TypeCode > 0x3) return IsoTpFrameType.Invalid;
            else return (IsoTpFrameType)(TypeCode);
        }
    }

    public static class IsoTpFramesHelper
    {
        /// <summary>
        /// Возвращает тип пакета ISO-TP
        /// </summary>
        public static IsoTpFrameType GetIsoTpFrameType(this CanFrame Frame)
        {
            return IsoTpFrame.GetFrameType(Frame.Data);
        }
        /// <summary>
        /// Выполняет парсинг ISO-TP фрейма из CAN-сообщения
        /// </summary>
        /// <typeparam name="FrameType">Тип ISO-TP фрейма</typeparam>
        /// <param name="Frame">CAN-сообщение</param>
        public static FrameType ParseAs<FrameType>(this CanFrame Frame)
            where FrameType : IsoTpFrame, new ()
        {
            return IsoTpFrame.ParsePacket<FrameType>(Frame.Data);
        }
    }
}
