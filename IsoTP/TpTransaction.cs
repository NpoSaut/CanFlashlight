using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;

namespace Communications.Protocols.IsoTP
{
    public abstract class TpTransaction
    {
        public CanPort Port { get; private set; }
        public int Descriptor { get; private set; }

        public TpTransaction(CanPort Port, int Descriptor)
        {
            this.Descriptor = Descriptor;
            this.Port = Port;
        }
    }
}
