using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tappi
{
    public class FilterModel
    {
        public int Descriptor { get; set; }
        public bool Sound { get; set; }

        public FilterModel() { }
        public FilterModel(int Descriptor)
            : this()
        {
            this.Descriptor = Descriptor;
        }
    }
}
