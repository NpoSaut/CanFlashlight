using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BlokFramesCodegen.CodeGeneration;

namespace BlokFramesCodegen
{
    public abstract class PropertyPlacement
    {
        protected abstract void FillFromElement(XElement element);

        public static T GetPlacement<T>(XElement element)
            where T : PropertyPlacement, new()
        {
            T res = new T();
            res.FillFromElement(element);
            return res;
        }

        public static PropertyPlacement GetPlacement(XElement element)
        {
            return GetPlacement<CustomPropertyPlacement>(element);
            //if (element.Attribute("Bit") != null) return GetPlacement<BitPropertyPlacement>(element);
            //else return GetPlacement<BytePropertyPlacement>(element);
        }

        public abstract CodeElement GetExtractor(string BufferName, string VariableName);
        public abstract CodeElement GetSetter(string BufferName, string VariableName);

        protected class Position
        {
            public int Offset { get; set; }
            public int Length { get; set; }

            public Position(String StringPosition)
            {
                var sp = StringPosition.Split(new Char[] { ':' });
                Offset = Int32.Parse(sp[0]);
                Length = sp.Length > 1 ? Int32.Parse(sp[0]) : 1;
            }
        }
    }

    public class CustomPropertyPlacement : PropertyPlacement
    {
        List<BitSection> BitSections;

        protected override void FillFromElement(XElement element)
        {
            BitSections = element.Attribute("Placement").Value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => new BitSection(s)).ToList();
        }

        public override CodeElement GetExtractor(string BufferName, string VariableName)
        {
            int position = 0;
            var boogles = new List<string>();
            foreach (var s in BitSections)
            {
                string str = string.Format("{0}[{1}]", BufferName, s.Start.ByteOffset);
                str = ApplyMask(str, s.GetMask());
                str = ApplyShift(str, position - s.Start.BitOffset);

                boogles.Add(str);
                position += s.Length;
            }
            return new CodeLine("int {0} = {1};", VariableName, string.Join(" | ", boogles));
        }

        private String ApplyMask(string str, int mask)
        {
            if (mask == 0xff) return str;
            else return str = string.Format("({0} & 0x{1:x2})", str, mask);
        }
        private String ApplyShift(string str, int shift)
        {
            if (shift == 0) return str;
            else return string.Format("({0} {1} {2})", str, shift >= 0 ? "<<" : ">>", Math.Abs(shift));
        }

        public override CodeElement GetSetter(string BufferName, string VariableName)
        {
            CodeBlock res = new CodeBlock();
            int position = 0;
            foreach (var sec in BitSections)
            {
                res.Add(SetValueString(
                    string.Format("{0}[{1}]", BufferName, sec.Start.ByteOffset),
                    ApplyShift(VariableName, -position),
                    sec.GetMask()));

                position += sec.Length;

                //buff[3] = (byte)((buff[3] & ~ByteMask) | ((val >> sec.Start.ByteOffset*8) & ByteMask));
            }
            return res;
        }

        private string SetValueString(string recipient, string val, int mask)
        {
            if (mask == 0xff) return string.Format("{0} = (byte)({1} & 0x{2:x2});", recipient, val, mask);
            else return string.Format("{0} = (byte)(({0} & ~0x{2:x2}) | ({1} & 0x{2:x2}));", recipient, val, mask);
        }
    }




    class BitPosition
    {
        public int ByteOffset { get; set; }
        public int BitOffset { get; set; }

        public BitPosition(int ByteOffset, int BitOffset = -1)
        {
            this.ByteOffset = ByteOffset;
            this.BitOffset = BitOffset;
        }
        public BitPosition(String str)
        {
            var ss = str.Split(new char[] { '.' });
            ByteOffset = Int32.Parse(ss[0]) - 1;
            BitOffset = ss.Length > 1 ? Int32.Parse(ss[1]) : -1;
        }
    }
    class BitSection
    {
        private string s;

        public BitPosition Start { get; set; }
        public BitPosition End { get; set; }

        public int Length
        {
            get { return End.BitOffset - Start.BitOffset + 1; }
        }

        public BitSection(BitPosition start, BitPosition end)
        {
            this.Start = start;
            this.End = end;
        }

        public BitSection(string s)
        {
            var ss = s.Split("-".ToArray());
            Start = new BitPosition(ss[0]);
            if (ss.Length > 1) End = new BitPosition(ss[1]);
            else
            {
                if (Start.BitOffset == -1)
                {
                    Start.BitOffset = 0;
                    End = new BitPosition(Start.ByteOffset, 7);
                }
                else End = new BitPosition(Start.ByteOffset, Start.BitOffset);
            }
        }

        public int GetMask()
        {
            return ((1 << End.BitOffset - Start.BitOffset + 1) - 1) << Start.BitOffset;
        }
    }
}
