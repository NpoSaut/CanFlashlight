using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlokFramesCodegen.CodeGeneration
{
    public abstract class CodeElement
    {
        public abstract void Echo(StringBuilder sb, int OffsetIndex);

        public String Text
        {
            get
            {
                var sb = new StringBuilder();
                Echo(sb, 0);
                return sb.ToString();
            }
        }
    }
}
