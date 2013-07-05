using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BlokFramesCodegen.CodeGeneration;

namespace BlokFramesCodegen.PropertyDescriptions
{
    [PropertyTypeName("int", "int32", "uint32", "int16", "uint16", "byte")]
    public class NumericPropertyDescription : PropertyDescription
    {
        public PropertyPlacement Placement { get; set; }

        protected override void FillFromXml(XElement XPropertyDescription)
        {
            Placement = PropertyPlacement.GetPlacement(XPropertyDescription);
        }

        private int _sizeof()
        {
            if (TypeName.Contains("32")) return 4;
            else if (TypeName.Contains("16")) return 2;
            else if (TypeName.ToLower() == "byte") return 1;
            else throw new Exception("Формат не поддерживается");
        }

        protected override CodeGeneration.CodeBlock GetPropertyDecoderBody()
        {
            return new CodeBlock()
            {
                Placement.GetExtractor("buff", "dat", _sizeof()),
                new CodeLine("return BitConverter.To{0}(dat, 0);", TypeName)
            };
        }

        protected override CodeBlock GetPropertyEncoderBody()
        {
            return new CodeBlock();
        }
    }
}
