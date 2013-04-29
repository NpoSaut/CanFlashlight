using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications.Can
{
    /// <summary>
    /// CAN-сообщение
    /// </summary>
    public class CanFrame
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

        /// <summary>
        /// Показывает, является ли данный пакет Loopback-пакетом
        /// </summary>
        public bool IsLoopback { get; private set; }

        /// <summary>
        /// Является ли этот фрейм обработанным
        /// </summary>
        /// <remarks>
        /// При передаче принятого пакета по стеку, можно использовать для того, чтобы отметить пакет, на который уже осуществлена реакция
        /// </remarks>
        public bool Processed { get; set; }

        protected CanFrame()
        {
        }

        /// <summary>
        /// Создаёт CAN-сообщение с заданным ID
        /// </summary>
        /// <param name="Id">Идентификатор сообщения</param>
        /// <param name="Data">Данные сообщения</param>
        public static CanFrame NewWithId(int Id, Byte[] Data)
        {
            return new CanFrame() { Id = Id, Data = Data };
        }
        /// <summary>
        /// Создаёт CAN-сообщение с заданным ID
        /// </summary>
        /// <param name="Id">Идентификатор сообщения</param>
        /// <param name="Data">Данные сообщения</param>
        /// <param name="Offset">Отступ от начала массива данных (в байтах)</param>
        /// <param name="Length">Длинна данных в буфере</param>
        public static CanFrame NewWithId(int Id, Byte[] DataBuffer, int Offset, int Length)
        {
            Byte[] Data = new Byte[Length];
            Buffer.BlockCopy(DataBuffer, Offset, Data, 0, Length);
            return new CanFrame() { Id = Id, Data = Data };
        }
        /// <summary>
        /// Создаёт пустое CAN-сообщение с заданными ID и длиной
        /// </summary>
        /// <param name="Id">Идентификатор сообщения</param>
        /// <param name="Data">Длина сообщения</param>
        public static CanFrame NewWithId(int Id, int Length)
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
        public static CanFrame NewWithDescriptor(int Descriptor, Byte[] DataBuffer, int Offset = 0)
        {
            int DescriptedLength = Descriptor % 0x20;
            Byte[] data = new Byte[DescriptedLength];
            Array.Copy(DataBuffer, Offset, data, 0, Math.Min(DescriptedLength, DataBuffer.Length));
            return new CanFrame() { Id = Descriptor / 0x20, Data = data };
        }
        /// <summary>
        /// Создаёт пустое CAN-сообщение с заданным Дескриптором
        /// </summary>
        /// <param name="Descriptor">Дескриптор сообщения</param>
        public static CanFrame NewWithDescriptor(int Descriptor)
        {
            return NewWithDescriptor(Descriptor, new Byte[0]);
        }

        /// <summary>
        /// Создаёт новую объект фрейма
        /// </summary>
        public CanFrame Clone()
        {
            return new CanFrame()
            {
                Id = this.Id,
                Data = this.Data
            };
        }
        /// <summary>
        /// Создаёт Loopback-пакет для данного
        /// </summary>
        public CanFrame GetLoopbackFrame()
        {
            var l = this.Clone();
            l.IsLoopback = true;
            return l;
        }

        public override string ToString()
        {
            return string.Format("{0:X4} | {1}", Descriptor, string.Join(" ", Data.Select(b => b.ToString("X2"))));
        }
    }
}
