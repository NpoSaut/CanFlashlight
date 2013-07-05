using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlokFramesCodegen.CodeGeneration
{
    public class CodeHeaderedBlock : CodeFramedBlock
    {
        public CodeElement Header { get; set; }

        public CodeHeaderedBlock()
            : base()
        { }

        public CodeHeaderedBlock(CodeElement header)
            : this()
        {
            Header = header;
        }
        public CodeHeaderedBlock(String line)
            : this(new CodeLine(line))
        { }
        public CodeHeaderedBlock(String Format, params object[] args)
            : this(new CodeLine(Format, args))
        { }

        public override void Echo(StringBuilder sb, int OffsetIndex)
        {
            Header.Echo(sb, OffsetIndex);
            base.Echo(sb, OffsetIndex);
        }
    }
}
