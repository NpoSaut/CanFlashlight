using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlokFramesCodegen.CodeGeneration
{
    public class CodeFramedBlock : CodeBlock
    {
        public override void Echo(StringBuilder sb, int OffsetIndex)
        {
            CodeLine.Echo(sb, OffsetIndex, "{");
            base.Echo(sb, OffsetIndex + 1);
            CodeLine.Echo(sb, OffsetIndex, "}");
            CodeLine.Echo(sb, OffsetIndex, "");
        }

        public CodeFramedBlock()
            : base()
        { }

        public CodeFramedBlock(IEnumerable<CodeElement> elements)
            : this()
        {
            foreach (var e in elements)
                this.SubElements.AddLast(e);
        }
        public CodeFramedBlock(CodeElement element)
            : this(new CodeElement[] { element })
        { }
    }
}
