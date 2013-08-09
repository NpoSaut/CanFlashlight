using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BlokFramesCodegen.PropertyDescriptions;
using BlokFramesCodegen.CodeGeneration;
using System.IO;

namespace BlokFramesCodegen
{
    class Program
    {
        static void Main(string[] args)
        {
            var root = XDocument.Load("frames.xml").Root;

            var FrameDescriptions = root.Elements("Frame").Select(XFrame => FrameDescription.GetFrameDescription(XFrame));

            //var props = frame.Elements("Property").Select(XProperty => PropertyDescription.GetProperty(XProperty)).ToList();

            //foreach (var p in props.Where(pp => pp != null))
            //    Console.WriteLine(p.PropertyDecoder.Text);

            var Code = new CodeBlock()
            {
                new CodeLine("using System;"),
                new CodeLine("using System.Collections.Generic;"),
                new CodeLine("using System.Linq;"),
                new CodeLine("using System.Text;"),
                new CodeLine(),
                new CodeHeaderedBlock("namespace BlokFrames")
                {
                    FrameDescriptions.Select(fd => fd.GetCode())
                }
            };

            using (TextWriter tw = new StreamWriter("Frames.cs"))
            {
                tw.WriteLine(Code.Text);
            }

            Console.WriteLine(Code.Text);

            Console.Read();
        }
    }
}
