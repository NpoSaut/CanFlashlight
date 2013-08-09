using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BlokFramesCodegen.CodeGeneration;

namespace BlokFramesCodegen.PropertyDescriptions
{
    [PropertyTypeName("Enum")]
    public class EnumPropertyDescription : PropertyDescription
    {
        public List<EnumValueDescription> Values { get; private set; }

        protected override void FillFromXml(XElement XPropertyDescription)
        {
            base.FillFromXml(XPropertyDescription);
            Values = XPropertyDescription
                        .Elements("Value")
                        .Select(XValueDescription =>
                            new EnumValueDescription()
                            {
                                Name = (String)XValueDescription.Attribute("Name"),
                                Description = (String)XValueDescription.Attribute("Description"),
                                Key = (int)XValueDescription.Attribute("Key")
                            })
                        .ToList();
            TypeName = string.Format("{0}Kind", Name);
        }

        protected override CodeGeneration.CodeElement GetPropertyEncoderBody()
        {
            return Placement.GetSetter("buff", "(int)value");
        }

        protected override CodeGeneration.CodeElement GetPropertyDecoderBody()
        {
            return new CodeBlock()
            {
                Placement.GetExtractor("buff", "raw"),
                new CodeLine("return ({0})raw;", TypeName)
            };
        }

        public override CodeBlock PropertyDefinition
        {
            get
            {
                return new CodeBlock()
                {
                    new CodeLine("///<summary>{0}</summary>", Description),
                    new CodeHeaderedBlock("public enum {0} : int", TypeName)
                    {
                        Values.Select(value =>
                            new CodeBlock()
                            {
                                new CodeLine("[System.ComponentModel.Description(\"{0}\")]", value.Description),
                                new CodeLine("///<summary>{0}</summary>", value.Description),
                                new CodeLine("{0} = {1},", value.Name, value.Key)
                            })
                    },
                    base.PropertyDefinition,
                };
            }
        }
    }

    public class EnumValueDescription
    {
        public String Name { get; set; }
        public String Description { get; set; }
        public int Key { get; set; }
    }
}
