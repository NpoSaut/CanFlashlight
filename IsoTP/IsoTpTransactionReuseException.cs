using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications.Protocols.IsoTP
{
    /// <summary>
    /// Исключение, возникающее при попытке повторно использовать ISO-TP транзакцию
    /// </summary>
    public class IsoTpTransactionReuseException : IsoTpProtocolException
    {
        public TpTransaction Transaction { get; set; }

        public IsoTpTransactionReuseException(TpTransaction Transaction)
            : base("Невозможно повторно использовать ISO-TP транзакцию")
        {
            this.Transaction = Transaction;
        }
    }
}
