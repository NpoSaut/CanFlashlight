using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications.Protocols.IsoTP
{
    /// <summary>
    /// Исключение, возникающее, если транзакция была отменена принимающей стороной
    /// </summary>
    public class IsoTpTransactionAbortedException : IsoTpProtocolException
    {
        public IsoTpTransactionAbortedException()
            : base()
        { }
        public IsoTpTransactionAbortedException(String Message)
            : base(Message)
        { }
        public IsoTpTransactionAbortedException(String Message, Exception InternalException)
            : base(Message, InternalException)
        { }
    }
}
