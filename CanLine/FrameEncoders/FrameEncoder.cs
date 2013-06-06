using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications.Can.FrameEncoders
{
    /// <summary>
    /// Абстрактный класс для кодирования фреймов
    /// </summary>
    /// <typeparam name="TOut">Тип закодированного фрейма</typeparam>
    public abstract class FrameEncoder<TOut>
        where TOut: new()
    {
        public abstract CanFrame Decode(TOut serialization);
        public abstract TOut Encode(CanFrame frame);
    }
}
