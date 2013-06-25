using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications.Protocols.IsoTP
{
    /// <summary>
    /// Исключение, связанное с нарушением последовательности приёма кадров
    /// </summary>
    public class IsoTpSequenceException : IsoTpProtocolException
    {
        public int ExpectedIndex { get; set; }
        public int RecievedIndex { get; set; }

        public IsoTpSequenceException(int ExpectedIndex, int RecievedIndex)
            : base(string.Format("Получено сообщение с индексом {0}, в то время как ожидалось сообщение с индексом {1}", RecievedIndex, ExpectedIndex))
        {
            this.ExpectedIndex = ExpectedIndex;
            this.RecievedIndex = RecievedIndex;
        }
        public IsoTpSequenceException(String Message, int ExpectedIndex, int RecievedIndex)
            : base(Message)
        {
            this.ExpectedIndex = ExpectedIndex;
            this.RecievedIndex = RecievedIndex;
        }
    }
}
