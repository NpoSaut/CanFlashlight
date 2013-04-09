using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications.Appi
{
    /// <summary>
    /// CAN-сообщение
    /// </summary>
    public class CanMessage
    {
        /// <summary>
        /// Идентификатор сообщения
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Данные сообщения
        /// </summary>
        public Byte[] Data { get; set; }

        /// <summary>
        /// Дескриптор сообщения
        /// </summary>
        public int Descriptor
        {
            get { return Id * 0x20 + Data.Length; }
        }

        private CanMessage()
        {
        }

        /// <summary>
        /// Создаёт CAN-сообщение с заданным ID
        /// </summary>
        /// <param name="Id">Идентификатор сообщения</param>
        /// <param name="Data">Данные сообщения</param>
        public static CanMessage NewWithId(int Id, Byte[] Data)
        {
            return new CanMessage() { Id = Id, Data = Data };
        }
        /// <summary>
        /// Создаёт CAN-сообщение с заданным ID
        /// </summary>
        /// <param name="Id">Идентификатор сообщения</param>
        /// <param name="Data">Данные сообщения</param>
        /// <param name="Offset">Отступ от начала массива данных (в байтах)</param>
        /// <param name="Length">Длинна данных в буфере</param>
        public static CanMessage NewWithId(int Id, Byte[] DataBuffer, int Offset, int Length)
        {
            Byte[] Data = new Byte[Length];
            Buffer.BlockCopy(DataBuffer, Offset, Data, 0, Length);
            return new CanMessage() { Id = Id, Data = Data };
        }
        /// <summary>
        /// Создаёт пустое CAN-сообщение с заданными ID и длиной
        /// </summary>
        /// <param name="Id">Идентификатор сообщения</param>
        /// <param name="Data">Длина сообщения</param>
        public static CanMessage NewWithId(int Id, int Length)
        {
            return NewWithId(Id, new Byte[Length]);
        }

        /// <summary>
        /// Создаёт CAN-сообщение с заданным Дескриптором
        /// </summary>
        /// <param name="Descriptor">Дескриптор сообщения</param>
        /// <param name="DataBuffer">Данные сообщения</param>
        /// <param name="Offset">Отступ от начала массива данных (в байтах)</param>
        /// <remarks>Из массива данных берётся только первый N байт, где N вычисляется на основании дескриптора</remarks>
        public static CanMessage NewWithDescriptor(int Descriptor, Byte[] DataBuffer, int Offset = 0)
        {
            int DescriptedLength = Descriptor % 0x20;
            Byte[] data = new Byte[DescriptedLength];
            Array.Copy(DataBuffer, Offset, data, 0, Math.Min(DescriptedLength, DataBuffer.Length));
            return new CanMessage() { Id = Descriptor / 0x20, Data = data };
        }
        /// <summary>
        /// Создаёт пустое CAN-сообщение с заданным Дескриптором
        /// </summary>
        /// <param name="Descriptor">Дескриптор сообщения</param>
        public static CanMessage NewWithDescriptor(int Descriptor)
        {
            return NewWithDescriptor(Descriptor, new Byte[0]);
        }

        /// <summary>
        /// Собирает байты для буфера АППИ
        /// </summary>
        /// <returns>10 байт: 2 байта дескриптор + 8 байт данных</returns>
        internal Byte[] ToBufferBytes()
        {
            Byte[] buff = new Byte[10];

            BitConverter.GetBytes((UInt16)Descriptor).Reverse().ToArray().CopyTo(buff, 0);
            Data.CopyTo(buff, 2);

            return buff;
        }
        /// <summary>
        /// Восстанавливает CAN-сообщение из буфера АППИ
        /// </summary>
        /// <param name="Buff">10 байт буфера</param>
        internal static CanMessage FromBufferBytes(Byte[] Buff)
        {
            int id = (int)BitConverter.ToUInt16(Buff.Take(2).Reverse().ToArray(), 0) >> 4;
            int len = Buff[1] & 0x0f;
            return CanMessage.NewWithId(id, Buff, 2, len);
        }

        public override string ToString()
        {
            return string.Format("{0:X4} | {1}", Descriptor, string.Join(" ", Data.Select(b => b.ToString("X2"))));
        }
    }
}
