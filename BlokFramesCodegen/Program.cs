using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BlokFramesCodegen.PropertyDescriptions;

namespace BlokFramesCodegen
{
    class Program
    {
        static void Main(string[] args)
        {
            var root = XDocument.Load("frames.xml").Root;

            var frame = FrameDescription.GetFrameDescription(root.Elements("Frame").First());

            //var props = frame.Elements("Property").Select(XProperty => PropertyDescription.GetProperty(XProperty)).ToList();

            //foreach (var p in props.Where(pp => pp != null))
            //    Console.WriteLine(p.PropertyDecoder.Text);

            Console.WriteLine(frame.GetCode().Text);
            Console.Read();
        }
    }
}
