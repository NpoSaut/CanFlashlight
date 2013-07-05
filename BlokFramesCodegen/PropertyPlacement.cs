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

        public abstract CodeElement GetExtractor(string BufferName, string VariableName, int size);

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

    public class BytePropertyPlacement : PropertyPlacement
    {
        public int ByteOffset { get; set; }
        public int ByteLength { get; set; }

        protected override void FillFromElement(XElement element)
        {
            var p = new Position(element.Attribute("Byte").Value);
            this.ByteOffset = p.Offset;
            this.ByteLength = p.Length;
        }

        public override CodeElement GetExtractor(string BufferName, string VariableName, int size)
        {
            return new CodeBlock()
            {
                new CodeLine("Byte[] {0} = new Byte[{1}];", VariableName, size),
                new CodeLine("Buffer.BlockCopy({1}, {2}, {0}, 0, {3});", VariableName, BufferName, ByteOffset, ByteLength)
            };
        }
    }
    public class BitPropertyPlacement : PropertyPlacement
    {
        public int ByteOffset { get; set; }
        public int BitOffset { get; set; }
        public int BitLength { get; set; }

        protected override void FillFromElement(XElement element)
        {
            var ByteP = new Position(element.Attribute("Byte").Value);
            this.ByteOffset = ByteP.Offset;
            var BitP = new Position(element.Attribute("Bit").Value);
            this.BitOffset = BitP.Offset;
            this.BitLength = BitP.Length;
        }

        public override CodeElement GetExtractor(string BufferName, string VariableName, int size)
        {
            string LengthMask = ((1 << BitLength) - 1).ToString("X2").ToLower();
            return new CodeLine("Byte[] {4} = new Byte[] {{ ({0}[{1}] >> {2}) & 0x{3} }};",
                BufferName, ByteOffset, BitOffset, LengthMask, VariableName);
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

        public override CodeElement GetExtractor(string BufferName, string VariableName, int size)
        {
            int position = 0;
            var boogles = new List<string>();
            for (int i = 0; i < BitSections.Count; i++)
            {
                BitSection s = BitSections[i];
                var mask = "0x" + s.GetMask().ToString("X2");
                var str = string.Format("buff[{0}] & {1}", s.Start.ByteOffset, mask);
                if (position - s.Start.BitOffset != 0)
                {
                    str = string.Format("({0}) << {1}", str, position - s.Start.BitOffset);
                }
                boogles.Add(str);
                position += s.Length;
            }
            return new CodeLine("Int64 {0} = {1};", VariableName, string.Join(" | ", boogles.Select(b => "(" + b + ")")));
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
            ByteOffset = Int32.Parse(ss[0]);
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
                Start.BitOffset = 0;
                End = new BitPosition(Start.ByteOffset, 7);
            }
        }

        public int GetMask()
        {
            return ((1 << End.BitOffset - Start.BitOffset + 1) - 1) << Start.BitOffset;
        }
    }
}
