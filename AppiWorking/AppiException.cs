using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications.Appi
{
    [Serializable]
    public class AppiException : ApplicationException
    {
        public AppiException() { }
        public AppiException(string message) : base(message) { }
        public AppiException(string MessageFormat, params object[] MessageArguments) : base(string.Format(MessageFormat, MessageArguments)) { }
        public AppiException(string message, Exception inner) : base(message, inner) { }
        protected AppiException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class AppiBufferDecodeException : AppiException
    {
        public AppiBufferDecodeException() : base("Ошибка декодирования буфера") { }
        public AppiBufferDecodeException(string message) : base("Ошибка декодирования буфера: " + message) { }
        public AppiBufferDecodeException(string MessageFormat, params object[] MessageArguments) : base("Ошибка декодирования буфера: " + MessageFormat, MessageArguments) { }
        public AppiBufferDecodeException(string message, Exception inner) : base("Ошибка декодирования буфера: " + message, inner) { }
        protected AppiBufferDecodeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
