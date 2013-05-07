using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP
{
    public class TpRecieveTransaction : TpTransaction
    {
        private Byte[] Buff { get; set; }
        private int Pointer { get; set; }

        public TimeSpan SeparationTime { get; set; }
        public Byte BlockSize { get; set; }

        public TpRecieveTransaction(CanPort Port, int Descriptor)
            : base(Port, Descriptor)
        {
            this.SeparationTime = TimeSpan.Zero;
            this.BlockSize = 20;
        }

        public Byte[] Recieve()
        {
            using (var CanHandler = new CanFrameHandler(Descriptor))
            {
                CanFrame f;
                while (IsoTpFrame.GetFrameType((f = CanHandler.WaitFor()).Data) != IsoTpFrameType.First) { }

                var First = IsoTpFrame.ParsePacket<FirstFrame>(f.Data);
                Buff = new Byte[First.PacketSize];
                Buffer.BlockCopy(First.Data, 0, Buff, 0, First.Data.Length);
                Pointer += First.Data.Length;

                var FlowControl = new FlowControlFrame(FlowControlFlag.ClearToSend, BlockSize, SeparationTime);
                Port.Send(FlowControl.GetCanFrame(Descriptor));



            }
            return Buff;
        }
    }
}
