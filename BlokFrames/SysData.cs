using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlokFrames
{
    /// <summary>
    /// SYS_DATA
    /// Чтение данных из МПХ
    /// </summary>
    [FrameDescriptor(0x6265, HalfsetKind.SetA)]
    [FrameDescriptor(0x6285, HalfsetKind.SetB)]
    public class SysData : InputData
    {
        public enum ErrorKind : byte
        {
            NoError = 0,
            Busy = 1,
            Unwritten = 2,
            WrongCrc = 3,
            UnknownNum = 4,
            Unknown = 5
        }
        public ErrorKind Error { get; set; }

        public override int Data
        {
            get
            {
                if (Error == ErrorKind.NoError)
                    return base.Data;
                else
                    throw new Exception("МПХ вернул ошибку при чтении данных");
            }
            set
            {
                base.Data = value;
            }
        }

        protected override byte[] GetCanFrameData()
        {
            if (Error == ErrorKind.NoError) return base.GetCanFrameData();
            else return new Byte[] { (byte)(Index | 0x08), 0, 0, 0, (byte)Error };
        }

        protected override void FillWithCanFrameData(byte[] Data)
        {
            if ((Data[0] >> 7) == 0)
            {
                Error = ErrorKind.NoError;
                base.FillWithCanFrameData(Data);
            }
            else
            {
                Index = Data[0] & 0x7F;
                Error = (ErrorKind)(Data[4]);
            }
        }

        public override string ToString()
        {
            if (Error == ErrorKind.NoError)
                return String.Format("{0} : {1}", Index, Data);
            else
                return String.Format("{0} : {{{1}}}", Index, Error);
        }
    }
}
