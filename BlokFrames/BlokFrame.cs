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
        /// Полукомплект
        /// </summary>
        public HalfsetKind FrameHalfset { get; set; }

        /// <summary>
        /// Дескриптор, соответствующий данному сообщению
        /// </summary>
        public IDictionary<HalfsetKind, int> Descriptors
        {
            get { return GetDescriptors(this.GetType()); }
        }
        public static IDictionary<HalfsetKind, int> GetDescriptors(Type T)
        {
            return T.GetCustomAttributes(typeof(FrameDescriptorAttribute), false).OfType<FrameDescriptorAttribute>().ToDictionary(a => a.Halfset, a => a.Descriptor);
        }
        public static IDictionary<HalfsetKind, int> GetDescriptors<T>() where T : BlokFrame
        {
            return GetDescriptors(typeof(T));
        }

        protected abstract Byte[] GetCanFrameData();
        protected abstract void FillWithCanFrameData(Byte[] Data);

        /// <summary>
        /// Создаёт CAN-фрейм, содержащий данное сообщение
        /// </summary>
        /// <returns>CAN-фрейм, содержащий данное сообщение</returns>
        public CanFrame GetCanFrame()
        {
            return CanFrame.NewWithDescriptor(this.Descriptors[this.FrameHalfset], this.GetCanFrameData());
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
            HalfsetKind? hs = GetDescriptors<T>().Where(p => p.Value == f.Descriptor).Select(p => p.Key).FirstOrDefault();

            if (!GetDescriptors<T>().Values.Contains(f.Descriptor))
                throw new DescriptorMismatchException("Дескриптор расшифровываемого фрейма не соответствует дескриптору  типа");

            var res = new T() { FrameHalfset = hs ?? HalfsetKind.Uniset };
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
