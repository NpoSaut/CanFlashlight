using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlokFrames
{
    /// <summary>
    /// Исключение несоответствия дескриптора
    /// </summary>
    public class DescriptorMismatchException : Exception
    {
        public DescriptorMismatchException()
            : base()
        { }

        public DescriptorMismatchException(String message)
            : base(message)
        { }
    }
}
