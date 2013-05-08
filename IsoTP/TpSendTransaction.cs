using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP
{
    public class TpSendTransaction : TpTransaction
    {
        private Byte[] Buff { get; set; }
        private int Pointer { get; set; }

        public int BlockSize { get; private set; }
        public TimeSpan SeparationTime { get; set; }

        public TpSendTransaction(CanPort Port, int Descriptor)
            : base(Port, Descriptor)
        {}

        public void Send(TpPacket Packet)
        {
            if (this.Status != TpTransactionStatus.Ready) throw new IsoTpTransactionReuseException(this);
            this.Status = TpTransactionStatus.Active;

            this.Buff = Packet.Data;
            Port.Send(GetFirstFrame().GetCanFrame(Descriptor));

            using (var FramesReader = new CanFramesBuffer(Descriptor, Port))
            {
                var FramesStream = FramesReader.Read(Timeout, true);

                try
                {
                    var fcf = ProcessFlowControl(FramesStream);

                    if (fcf == FlowControlFlag.ClearToSend)
                    {
                        // Берём очередь для отправки
                        var PushingCanFrames = GetConsFrames().Select(cf => cf.GetCanFrame(Descriptor));

                        // Повторяем, пока не отослали всю очередь
                        while (Pointer < Buff.Length)
                        {
                            // Берём блок
                            var Block = PushingCanFrames.Take(BlockSize).ToList();
                            
                            // Отправляем его либо сразу, либо с SeparationTime
                            if (SeparationTime == TimeSpan.Zero)
                                Port.Send(Block);
                            else
                            {
                                foreach (var f in Block)
                                {
                                    Port.Send(f);
                                    System.Threading.Thread.Sleep(SeparationTime);
                                }
                            }

                            // Дожидаемся FlowControl фрейма
                            ProcessFlowControl(FramesStream);
                        }
                    }
                }
                catch
                {
                    this.Status = TpTransactionStatus.Error;
                    throw;
                }
            }
        }

        private FlowControlFlag ProcessFlowControl(IEnumerable<CanFrame> FramesStream)
        {
            var FC = FramesStream
                        .Where(f => f.GetIsoTpFrameType() == IsoTpFrameType.FlowControl)
                        //.Cast<FlowControlFrame>()
                        .Select(f => (FlowControlFrame)f)
                        .SkipWhile(fc => fc.Flag == FlowControlFlag.Wait)
                        .First();

            // Считываем параметры отправки кадров
            this.BlockSize = FC.BlockSize != 0 ? FC.BlockSize : int.MaxValue;
            this.SeparationTime = FC.SeparationTime;

            // Выбрасываем ошибку, если принимающая сторона отказывается от транзакции
            if (FC.Flag == FlowControlFlag.Abort)
                throw new IsoTpTransactionAbortedException("Принимающая сторона ответила флагом отмены транзакции");

            return FC.Flag;
        }

        int ConsecutiveFrameSent = 0;
        private IEnumerable<ConsecutiveFrame> GetConsFrames()
        {
            while (Pointer < Buff.Length)
            {
                var Data = new Byte[ConsecutiveFrame.DataCapacity];
                int DataLength = Math.Min(Data.Length, Buff.Length - Pointer);
                Buffer.BlockCopy(Buff, Pointer, Data, 0, DataLength);
                Pointer += Data.Length;
                yield return new ConsecutiveFrame(Data, ++ConsecutiveFrameSent & 0x0f);
            }
        }
        private FirstFrame GetFirstFrame()
        {
            var Data = new Byte[FirstFrame.DataCapacity];
            Buffer.BlockCopy(Buff, Pointer, Data, 0, Data.Length);
            Pointer += Data.Length;
            return new FirstFrame(Data, Buff.Length);
        }

    }
}
