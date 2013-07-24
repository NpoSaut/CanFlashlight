using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlokFrames.Halfset
{
    /// <summary>
    /// Базовый класс для значения, содержащегося в различных полукомплектах
    /// </summary>
    public abstract class HalfsetValue
    {
        /// <summary>
        /// Индикатор корректности значений полукомплектов
        /// </summary>
        public abstract bool IsValid { get; }
    }

    /// <summary>
    /// Значение, содержащееся в двух полукомплектах
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HalfsetValue<T> : HalfsetValue
    {
        /// <summary>
        /// Значение полукомплекта A
        /// </summary>
        public T ValueA { get; set; }
        /// <summary>
        /// Значение полукомплекта B
        /// </summary>
        public T ValueB { get; set; }

        public HalfsetValue()
            : base()
        { }

        public HalfsetValue(T v1, T v2)
            : this()
        {
            ValueA = v1; ValueB = v2;
        }

        public override bool IsValid
        {
            get { return ValueA.Equals(ValueB); }
        }

        /// <summary>
        /// Общее для полукомплектов значение, если таковое валидно
        /// </summary>
        public virtual T Value
        {
            get
            {
                if (!IsValid) throw new HalfsetValuesMismatchException();
                return ValueA;
            }
        }

        public static implicit operator T(HalfsetValue<T> hsv)
        {
            return hsv.Value;
        }

        public override string ToString()
        {
            if (IsValid) return Value.ToString();
            else return string.Format("{0} / {1}", ValueA, ValueB);
        }
        public override int GetHashCode()
        {
            return ValueA.GetHashCode() ^ ValueB.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            var hs = obj as HalfsetValue<T>;
            if (hs == null) return false;
            return
                this.ValueA.Equals(hs.ValueA) &&
                this.ValueB.Equals(hs.ValueB);
        }
    }

    [Serializable]
    public class HalfsetValuesMismatchException : Exception
    {
        public HalfsetValuesMismatchException() { }
        public HalfsetValuesMismatchException(string message) : base(message) { }
        public HalfsetValuesMismatchException(string message, Exception inner) : base(message, inner) { }
        protected HalfsetValuesMismatchException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
