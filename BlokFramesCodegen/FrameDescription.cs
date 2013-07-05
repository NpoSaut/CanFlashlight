using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BlokFramesCodegen.PropertyDescriptions;
using BlokFramesCodegen.CodeGeneration;

namespace BlokFramesCodegen
{
    public class FrameDescription
    {
        public int Descriptor { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public List<PropertyDescription> Properties { get; private set; }

        private void FromXml(XElement XFrameDescription)
        {
            Descriptor = Convert.ToInt32(XFrameDescription.Attribute("Descriptor").Value, 16);
            Name = (string)XFrameDescription.Attribute("Name");
            Description = XFrameDescription.Element("Description").Value.Trim();

            Properties = XFrameDescription.Elements("Property").Select(XProperty => PropertyDescription.GetProperty(XProperty)).ToList();
        }

        public static FrameDescription GetFrameDescription(XElement XFrameDescription)
        {
            var res = new FrameDescription();
            res.FromXml(XFrameDescription);
            return res;
        }

        public static string GetClassName(String MessageName)
        {
            string res = "";
            bool NextToUpper = true;
            foreach (var c in MessageName.ToLower())
            {
                if (c != '_')
                {
                    Char cc = NextToUpper ? c.ToString().ToUpper()[0] : c;
                    res += cc;
                    NextToUpper = false;
                }
                else NextToUpper = true;
            }
            return res;
        }

        public CodeElement GetCode()
        {
            return new CodeBlock()
            {
                new CodeLine("/// <summary>"),
                new CodeLine("/// {0}", Description),
                new CodeLine("/// <summary>"),
                new CodeLine("public class {0} : BlokFrame", GetClassName(Name)),
                new CodeFramedBlock()
                {
                    Properties.Select(p => p.PropertyDefinition),
                    new CodeLine(),
                    new CodeHeaderedBlock("protected override void FillWithCanFrameData(byte[] buff)")
                    {
                        Properties.Select(p => new CodeLine("this.{0} = Decode{0}(buff);", p.Name))
                    },
                    new CodeLine(),
                    Properties.Select(p => new CodeBlock() { p.PropertyDecoder, p.PropertyEncoder })
                }
            };
        }
    }
}
