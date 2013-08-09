using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BlokFramesCodegen.CodeGeneration;

namespace BlokFramesCodegen.PropertyDescriptions
{
    [PropertyTypeName("Boolean")]
    public class BooleanPropertyDescription : PropertyDescription
    {
        protected override CodeGeneration.CodeElement GetPropertyEncoderBody()
        {
            return Placement.GetSetter("buff", "(value ? 1 : 0)");
        }

        protected override CodeGeneration.CodeElement GetPropertyDecoderBody()
        {
            return new CodeBlock()
            {
                Placement.GetExtractor("buff", "raw"),
                new CodeLine("return raw != 0;")
            };
        }
    }
}
