using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntermoduleResolving
{
    class BitAdress
    {
        public int BitOffset { get; set; }
        public int StartByte
        {
            get { return BitOffset / 8; }
            set
            {
                BitOffset = value * 8 + StartBit;
            }
        }
        public int StartBit
        {
            get { return BitOffset % 8; }
            set
            {
                if (value > 7 || value < 0) throw new ArgumentException("Стартовый бит должен лежать в пределах 0 до 7");
                BitOffset = StartByte * 8 + value;
            }
        }

        public BitAdress()
        { }
        public BitAdress(int StartByte, int StartBit)
        {
            this.StartByte = StartByte;
            this.StartBit  = StartBit;
        }
        public BitAdress(string str)
        {
            var ss = str.Split(new char[] { '.' });
            this.StartByte = int.Parse(ss[0]);
            if (ss.Length > 1) this.StartBit = int.Parse(ss[1]);
            else this.StartBit = 0;
        }

        public static BitAdress operator + (BitAdress s, BitLength l)
        {
            return new BitAdress() { BitOffset = s.BitOffset + l.TotalBits };
        }
        public static BitLength operator - (BitAdress a, BitAdress b)
        {
            return new BitLength() { TotalBits = a.BitOffset - b.BitOffset };
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BitAdress)) return false;
            BitAdress a = (BitAdress)obj;
            return this.BitOffset == a.BitOffset;
        }
        public override int GetHashCode()
        {
            return BitOffset.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format("{0}.{1}", StartByte, StartBit);
        }
    }

    class BitLength
    {
        public int TotalBits { get; set; }
        public int Bytes
        {
            get { return TotalBits / 8; }
            set
            {
                TotalBits = value * 8 + Bits;
            }
        }
        public int Bits
        {
            get { return TotalBits % 8; }
            set
            {
                if (value > 7 || value < 0) throw new ArgumentException("Количество бит должно быть в пределах от 0 до 7");
                TotalBits = Bytes * 8 + value;
            }
        }

        public BitLength()
        { }
        public BitLength(int Bytes, int Bits = 0)
        {
            TotalBits = Bytes * 8 + Bits;
        }
        public BitLength(string str)
        {
            var ss = str.Split(new char[] { '.' });
            this.Bytes = int.Parse(ss[0]);
            if (ss.Length > 1) this.Bits = int.Parse(ss[1]);
            else this.Bits = 0;
        }

        public void AddBits(int dBits)
        {
            this.TotalBits += dBits;
        }
        public void AddBytes(int dBytes)
        {
            this.Bytes += dBytes;
        }

        public static BitLength operator + (BitLength a, BitLength b)
        {
            return new BitLength() { TotalBits = a.TotalBits + b.TotalBits };
        }
        public static BitLength operator * (int c, BitLength l)
        {
            return new BitLength() { TotalBits = l.TotalBits * c };
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BitLength)) return false;
            BitLength l = (BitLength)obj;
            return this.TotalBits == l.TotalBits;
        }
        public override int GetHashCode()
        {
            return TotalBits.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format("{0}.{1}", Bytes, Bits);
        }
    }

    class BitLocation
    {
        public BitAdress Start { get; set; }
        public BitLength Length { get; set; }
        public BitAdress End
        {
            get { return Start + Length; }
            set { Length = Start - value; }
        }

        public BitLocation(BitAdress Start, BitLength Length)
        {
            this.Start = Start;
            this.Length = Length;
        }
        public BitLocation(string str)
        {
            var ss = str.Split(new char[] { '|' });
            this.Start = new BitAdress(ss[0]);
            this.Length = new BitLength(ss[1]);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BitLocation)) return false;
            BitLocation ln = (BitLocation)obj;
            return this.Start == ln.Start && this.Length == ln.Length;
        }
        public override int GetHashCode()
        {
            return Start.GetHashCode() ^ Length.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format("{0}|{1}", Start, Length);
        }

        public Byte[] ExtractConsequentially(Byte[] Data)
        {
            var res = new Byte[(int)Math.Ceiling(Length.TotalBits / 0.8)];

            for (int i = 0; i < res.Length; i++)
            {
                BitAdress takeAdr = Start + new BitLength(i);
                res[i] = (Byte)(Data[takeAdr.StartByte] >> takeAdr.StartBit);
                if (iBit > 0)
                    res[i] |= 
                res[i] &= Mask(i * 8 - Length.TotalBits, 0);
            }

            return res;
        }
        public Byte[] ExtractMasked(Byte[] Data)
        {
            throw new NotImplementedException();

            int len = (int)(Math.Ceiling(End.BitOffset / 8.0) - Start.StartByte);
            var res = new Byte[len];

            for (int i = 0; i < res.Length; i++)
            {
                res[i] = (Byte)(Data[i + Start.StartByte] & Mask(End.BitOffset - i*8, Start.BitOffset + i * 8));
            }

            return res;
        }
        private Byte Mask(int CutHigh, int CutLow)
        {
            CutHigh = Math.Max(Math.Min(CutHigh, 8), 0);
            CutLow  = Math.Max(Math.Min(CutLow,  8), 0);
            return (Byte)(0xff & (((0x00ffff >> CutHigh) & (0xffff00 << CutLow)) >> 8));
        }
    }

    class BitArray
    {
        public Byte Byte { get; set; }

        public BitArray(Byte b)
        {
            this.Byte = b;
        }

        public Byte this[int index]
        {
            get { return (Byte)((this.Byte >> index) & 0x01); }
            set
            {
                int msk = (value & 0x01) << index;
                this.Byte = (Byte)((Byte & ~(1 << index)) | msk);
            }
        }

        public static implicit operator BitArray(Byte b)
        {
            return new BitArray(b);
        }
    }
}
