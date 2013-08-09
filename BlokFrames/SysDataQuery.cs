using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlokFrames
{
    /// <summary>
    /// Сообщение используется для запроса значения одного из параметров, хранимых в массиве постоянных характеристик. Обрабатывается модулем МПХ.
    /// </summary>
    [FrameDescriptor(0x0e01)]
    public class SysDataQuery : BlokFrame
    {
        /// <summary>
        /// Номер запрашиваемого свойства
        /// </summary>
        public Int32 Index { get; set; }

        public SysDataQuery() { }
        public SysDataQuery(Int32 Index)
        {
            this.Index = Index;
        }

        protected override byte[] Encode()
        {
            return new Byte[] { (byte)Index };
        }

        protected override void Decode(byte[] Data)
        {
            Index = Data[0];
        }
    }
}
