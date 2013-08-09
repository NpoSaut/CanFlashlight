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
        public PropertyPlacement Placement { get; private set; }

        protected override void FillFromXml(XElement XPropertyDescription)
        {
            Placement = PropertyPlacement.GetPlacement(XPropertyDescription);
        }

        protected override CodeGeneration.CodeElement GetPropertyEncoderBody()
        {
            return Placement.GetSetter("buff", "value");
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
