using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntermoduleResolving
{
    public class PropertyKind
    {
        public String Name { get; set; }
        public String Description { get; set; }

        public BitLocation Location { get; set; }

        public PropertyValue GetValue(Byte[] data)
        {

        }
    }
}
