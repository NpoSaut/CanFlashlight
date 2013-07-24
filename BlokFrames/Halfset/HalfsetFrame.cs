using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;

namespace BlokFrames.Halfset
{
    public class HalfsetFrame<FT> : HalfsetValue<FT>
        where FT : BlokFrame, new()
    {
        public HalfsetFrame()
            : base()
        {
        }

        public override bool IsValid
        {
            get
            {
                if (ValueA == null || ValueB == null) return false;
                return Enumerable.SequenceEqual(ValueA.GetCanFrame().Data, ValueB.GetCanFrame().Data);
            }
        }

        public HalfsetValue<T> ExtractValue<T>(Func<FT, T> Extractor)
        {
            return new HalfsetValue<T>(Extractor(ValueA), Extractor(ValueB));
        }

        public static CanFramesBuffer OpenFramesBuffer(params CanPort[] Ports)
        {
            var FrameDescriptors = BlokFrame.GetDescriptors<FT>();
            return new CanFramesBuffer(BlokFrame.GetDescriptors<FT>().Values, Ports);
        }

        public static HalfsetFrame<FT> GetSet(IEnumerable<FT> frames)
        {
            var FrameDescriptors = BlokFrame.GetDescriptors<FT>();

            if (!(FrameDescriptors.ContainsKey(HalfsetKind.SetA) && FrameDescriptors.ContainsKey(HalfsetKind.SetB)))
                throw new NonHalfsetFrameException();

            FT a = null, b = null;
            try
            {
                foreach (var f in frames)
                {
                    if (f.FrameHalfset == HalfsetKind.SetA) a = f;
                    if (f.FrameHalfset == HalfsetKind.SetB) b = f;
                    if (a != null && b != null) break;
                }
            }
            catch (TimeoutException te)
            {
                // Если ответил хотя бы один полукомплект, значит, связь есть
                // и второй полукомплект просто сдох
                if (a == null && b == null) throw;
            }
            return new HalfsetFrame<FT>() { ValueA = a, ValueB = b };
        }

        /// <summary>
        /// Сообщение не является двухполукомплектным
        /// </summary>
        [Serializable]
        public class NonHalfsetFrameException : Exception
        {
            public NonHalfsetFrameException() : base("Сообщение не является двухколукомплектным") { }
            public NonHalfsetFrameException(string message) : base(message) { }
            public NonHalfsetFrameException(string message, Exception inner) : base(message, inner) { }
            protected NonHalfsetFrameException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
        }
    }
}
