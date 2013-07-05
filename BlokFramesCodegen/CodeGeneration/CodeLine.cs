using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlokFramesCodegen.CodeGeneration
{
    public class CodeLine : CodeElement
    {
        public const string Tab = "    ";

        public String Line { get; set; }

        public CodeLine()
        { }
        public CodeLine(String Line)
        {
            this.Line = Line;
        }
        public CodeLine(String Format, params object[] args)
        {
            this.Line = string.Format(Format, args);
        }

        public static implicit operator CodeLine(String l)
        {
            return new CodeLine(l);
        }

        private static String GetTab(int OffsetIndex)
        {
            return Enumerable.Range(0, OffsetIndex).Aggregate("", (s, i) => s += Tab);
        }

        public static void Echo(StringBuilder sb, int OffsetIndex, String line)
        {
            sb.AppendLine(GetTab(OffsetIndex) + line);
        }
        public override void Echo(StringBuilder sb, int OffsetIndex)
        {
            Echo(sb, OffsetIndex, Line);
        }
    }
}
