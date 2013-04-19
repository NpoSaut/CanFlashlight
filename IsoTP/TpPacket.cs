using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications.Protocols.IsoTP
{
    public class TpPacket
    {
        public const int MaximumDataSize = 4095;
        public Byte[] Data { get; private set; }

        public TpPacket(Byte[] Data)
        {
            if (Data.Length > MaximumDataSize)
                throw new ArgumentOutOfRangeException("Data", string.Format("Размер данных, передаваемых в Single Frame режиме ограничен {0} байтами", MaximumDataSize));
            
            this.Data = Data;
        }
    }
}
