using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications.Protocols.IsoTP
{
    /// <summary>
    /// Исключение, связанное с реализацией протокола ISO-TP
    /// </summary>
    public class IsoTpProtocolException : Exception
    {
        public IsoTpProtocolException()
            : base()
        { }
        public IsoTpProtocolException(String Message)
            : base(Message)
        { }
        public IsoTpProtocolException(String Message, Exception InnerException)
            : base(Message, InnerException)
        { }
    }
}
