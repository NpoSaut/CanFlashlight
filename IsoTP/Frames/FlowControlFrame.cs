using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;

namespace Communications.Protocols.IsoTP.Frames
{
    /// <summary>
    /// Значения контрольного флага
    /// </summary>
    public enum FlowControlFlag : byte
    {
        /// <summary>
        /// Готов к приёму сообщений
        /// </summary>
        ClearToSend = 0x00,
        /// <summary>
        /// Подождите
        /// </summary>
        Wait = 0x01,
        /// <summary>
        /// Отмена транзакции
        /// </summary>
        Abort = 0x02
    }

    /// <summary>
    /// Кадр контроля потока
    /// </summary>
    /// <remarks>
    /// Отправляется принимающей стороной для контроля входящего потока, позволяет задать размер блока и интенсивность отправки сообщений
    /// </remarks>
    public class FlowControlFrame : IsoTpFrame
    {
        public override IsoTpFrameType FrameType
        {
            get { return IsoTpFrameType.FlowControl; }
        }

        /// <summary>
        /// Контрольный флаг
        /// </summary>
        public FlowControlFlag Flag { get; private set; }
        /// <summary>
        /// Размер блока
        /// </summary>
        public Byte BlockSize { get; private set; }
        /// <summary>
        /// Время ожидания перед отправкой следующего фрейма
        /// </summary>
        public TimeSpan SeparationTime { get; private set; }

        public FlowControlFrame()
        {
        }
        public FlowControlFrame(FlowControlFlag Flag, Byte BlockSize, Byte SeparationTimeCode)
        {
            Initialize(Flag, BlockSize, SeparationTimeFromCode(SeparationTimeCode));
        }
        public FlowControlFrame(FlowControlFlag Flag, Byte BlockSize, TimeSpan SeparationTime)
        {
            Initialize(Flag, BlockSize, SeparationTime);
        }
        private void Initialize(FlowControlFlag Flag, Byte BlockSize, TimeSpan SeparationTime)
        {
            this.Flag = Flag;
            this.BlockSize = BlockSize;
            this.SeparationTime = SeparationTime;
        }

        private TimeSpan SeparationTimeFromCode(Byte Code)
        {
            return Code < 0x7f ?
                    TimeSpan.FromMilliseconds(Code) :
                //TimeSpan.FromMilliseconds((Code - 0xf0) * 0.1);
                    TimeSpan.FromMilliseconds(1);
        }
        private Byte SeparationCodeFromTime(TimeSpan Time)
        {
            if (Time == TimeSpan.Zero) return 0;
            return Time.TotalMilliseconds < 1 ?
                        (byte)(Time.TotalMilliseconds * 10 + 0xf0) :
                        (byte)Math.Min(Time.TotalMilliseconds, 0x7f);
        }

        public override CanFrame GetCanFrame(int WithDescriptor)
        {
            var buff = new Byte[8];

            buff[0] = (byte)(((byte)FrameType & 0x0f) << 4 | (byte)Flag & 0x0f);
            buff[1] = BlockSize;
            buff[2] = SeparationCodeFromTime(SeparationTime);
            
            return CanFrame.NewWithDescriptor(WithDescriptor, buff);
        }

        protected override void FillWithBytes(byte[] buff)
        {
            this.Flag = (FlowControlFlag)(buff[0] & 0x0f);
            this.BlockSize = buff[1];
            this.SeparationTime = SeparationTimeFromCode(buff[2]);
        }

        public static implicit operator FlowControlFrame(Communications.Can.CanFrame cFrame)
        {
            return IsoTpFrame.ParsePacket<FlowControlFrame>(cFrame.Data);
        }

        /// <summary>
        /// Создаёт фрейм отмены потока (с FlowControlFlag = Abort)
        /// </summary>
        public static FlowControlFrame AbortFrame
        {
            get { return new FlowControlFrame(FlowControlFlag.Abort, 0, 0); }
        }
    }
}
