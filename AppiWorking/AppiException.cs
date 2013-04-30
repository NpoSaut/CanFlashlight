using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications.Appi
{
    public class AppiException : Exception
    {
        public AppiException()
            : base()
        { }

        public AppiException(String Message)
            : base(Message)
        { }

        public AppiException(String Message, Exception InternalException)
            : base(Message, InternalException)
        { }
    }
}
