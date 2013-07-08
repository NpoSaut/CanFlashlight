using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;
using System.Runtime.InteropServices;

namespace BlokFrames
{
    public abstract class BlokFrame
    {
        /// <summary>
        /// Дескриптор, соответствующий данному сообщению
        /// </summary>
        public int Descriptor
        {
            get { return GetDescriptor(this.GetType()); }
        }
        public static int GetDescriptor(Type T)
        {
            return T.GetCustomAttributes(typeof(FrameDescriptorAttribute), false).OfType<FrameDescriptorAttribute>().First().Descriptor;
        }
        public static int GetDescriptor<T>() where T : BlokFrame
        {
            return GetDescriptor(typeof(T));
        }

        protected abstract Byte[] GetCanFrameData();
        protected abstract void FillWithCanFrameData(Byte[] Data);

        /// <summary>
        /// Создаёт CAN-фрейм, содержащий данное сообщение
        /// </summary>
        /// <returns>CAN-фрейм, содержащий данное сообщение</returns>
        public CanFrame GetCanFrame()
        {
            return CanFrame.NewWithDescriptor(this.Descriptor, this.GetCanFrameData());
        }

        /// <summary>
        /// Расшифровывает CAN-фрейм в соответствии с указанным типом сообщения
        /// </summary>
        /// <typeparam name="T">Тип сообщения системы БЛОК</typeparam>
        /// <param name="f">CAN-фрейм</param>
        /// <returns>Расшифрованное сообщение системы БЛОК</returns>
        public static T GetBlokFrame<T>(CanFrame f)
            where T: BlokFrame, new()
        {
            if (GetDescriptor<T>() != f.Descriptor)
                throw new DescriptorMismatchException("Дескриптор расшифровываемого фрейма не соответствует дескриптору  типа");
            var res = new T();
            res.FillWithCanFrameData(f.Data);
            return res;
        }

        public static implicit operator CanFrame(BlokFrame bf)
        {
            return bf.GetCanFrame();
        }


        /// <summary>
        /// Преобразует массив байт к структуре
        /// </summary>
        /// <typeparam name="T">Тип структуры</typeparam>
        /// <param name="bytes">Байтовый массив</param>
        /// <returns>Объект структуры, заполненный указанным массивом</returns>
        protected static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return stuff;
        }
    }
}
