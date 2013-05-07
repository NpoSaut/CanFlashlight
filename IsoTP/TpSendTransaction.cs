using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;

namespace Communications.Protocols.IsoTP
{
    public class TpSendTransaction : TpTransaction
    {
        public TpPacket Packet { get; private set; }

        public TpSendTransaction(TpPacket Packet, CanPort Port, int Descriptor)
            : base(Port, Descriptor)
        {
            this.Packet = Packet;
        }
    }
}
