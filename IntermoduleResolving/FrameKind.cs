using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntermoduleResolving
{
    public class FrameKind
    {
        public String Name { get; set; }
        public String Description { get; set; }

        public List<PropertyKind> DescribingProperties { get; set; }
    }
}
