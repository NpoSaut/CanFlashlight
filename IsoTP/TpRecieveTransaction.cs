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
        /// <summary>
        /// Буфер приёма пакета
        /// </summary>
        private Byte[] Buff { get; set; }
        /// <summary>
        /// Указатель на место, в которое осуществляется приём пакета
        /// </summary>
        private int Pointer { get; set; }
        /// <summary>
        /// Ожидаемый последовательный номер пакета
        /// </summary>
        private int ExpectingConsIndex { get; set; }

        public TimeSpan SeparationTime { get; set; }
        public Byte BlockSize { get; set; }

        public TpRecieveTransaction(CanPort Port, int TransmitDescriptor, int AcknowlegmentDescriptor)
            : base(Port, TransmitDescriptor, AcknowlegmentDescriptor)
        {
            this.SeparationTime = TimeSpan.Zero;
            this.BlockSize = 20;
        }

        public Byte[] Recieve()
        {
            if (this.Status != TpTransactionStatus.Ready) throw new IsoTpTransactionReuseException(this);
            this.Status = TpTransactionStatus.Active;

            using (var FramesReader = new CanFramesBuffer(TransmitDescriptor, Port))
            {
                // Инициализируем чтение с заданным таймаутом,
                // при истечении таймаута - выбрасываем ошибку.
                var FramesStream = FramesReader.Read(Timeout, true);

                try
                {
                    // Ждём первого кадра передачи
                    var First = FramesStream
                                    .Where(f => f.GetIsoTpFrameType() == IsoTpFrameType.First)
                                    //.Cast<FirstFrame>()
                                    .Select(f => (FirstFrame)f)
                                    .First();

                    // После того, как поймали первый кадр - подготавливаем буфер
                    Buff = new Byte[First.PacketSize];
                    Buffer.BlockCopy(First.Data, 0, Buff, 0, First.Data.Length);
                    Pointer += First.Data.Length;

                    ExpectingConsIndex = 1;

                    // Начинаем приём данных
                    while (Pointer < Buff.Length)
                    {
                        SendFlowControl();          // Сообщаем о готовности
                        ReadBlock(FramesStream);    // Читаем следующий блок
                    }
                }
                catch
                {
                    // При любой ошибке отправляем пакет отмены передачи
                    this.Status = TpTransactionStatus.Error;
                    Port.Send(FlowControlFrame.AbortFrame.GetCanFrame(AcknowlegmentDescriptor));
                    throw;      // и пробрасываем ошибку дальше по стеку
                }
            }

            this.Status = TpTransactionStatus.Done;
            return Buff;
        }

        private void ReadBlock(IEnumerable<CanFrame> FromStream)
        {
            var Consequence = FromStream
                    .Where(f => f.GetIsoTpFrameType() == IsoTpFrameType.Consecutive)
                    //.Cast<ConsecutiveFrame>()
                    .Select(f => (ConsecutiveFrame)f)
                    .Take(BlockSize);

            foreach (var cf in Consequence)
            {
                if (cf.Index != ExpectingConsIndex) throw new IsoTpSequenceException(ExpectingConsIndex, cf.Index);

                int DataLength = Math.Min(cf.Data.Length, Buff.Length - Pointer);
                Buffer.BlockCopy(cf.Data, 0, Buff, Pointer, DataLength);
                Pointer += cf.Data.Length;
                ExpectingConsIndex = (ExpectingConsIndex + 1) & 0x0f;

                if (Pointer >= Buff.Length) break;
            }
        }
        private void SendFlowControl()
        {
            Port.Send(GenerateFlowControl().GetCanFrame(AcknowlegmentDescriptor));
        }
        private FlowControlFrame GenerateFlowControl()
        {
            return new FlowControlFrame(FlowControlFlag.ClearToSend, BlockSize, SeparationTime);
        }
    }
}
