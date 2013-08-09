using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;
using System.Runtime.InteropServices;
using System.Reflection;

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

        public int GetLengthOf<T>() where T : BlokFrame
        {
            return GetLengthOf(typeof(T));
        }
        public int GetLengthOf(Type t)
        {
            return GetDescriptors(t).Values.First() % 20;
        }
        public int FrameLength
        {
            get { return GetLengthOf(this.GetType()); }
        }

        protected abstract Byte[] Encode();
        protected abstract void Decode(Byte[] Data);

        /// <summary>
        /// Создаёт CAN-фрейм, содержащий данное сообщение
        /// </summary>
        /// <returns>CAN-фрейм, содержащий данное сообщение</returns>
        public CanFrame GetCanFrame()
        {
            return CanFrame.NewWithDescriptor(this.Descriptors[this.FrameHalfset], this.Encode());
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
            res.Decode(f.Data);
            return res;
        }
        public static BlokFrame GetBlokFrame(CanFrame f)
        {
            if (!FrameTypes.ContainsKey(f.Descriptor))
                throw new ApplicationException("Не найдено описание сообщения с таким дескриптором");

            Type t = FrameTypes[f.Descriptor];
            var res = (BlokFrame)t.GetConstructor(new Type[0]).Invoke(new object[0]);
            res.Decode(f.Data);
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

        private static Dictionary<int, Type> _FrameTypes = GetBlockFrameTypes();
        public static Dictionary<int, Type> FrameTypes
        {
            get { return _FrameTypes; }
        }
        private static Dictionary<int, Type> GetBlockFrameTypes()
        {
            return
                Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(BlokFrame)))
                    .SelectMany(t => GetDescriptors(t).Select(d => new { d = d.Value, t }))
                    .ToDictionary(td => td.d, td => td.t);
        }
    }
}
