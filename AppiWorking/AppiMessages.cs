using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;

namespace Communications.Appi
{
    /// <summary>
    /// CAN-сообщения из АППИ
    /// </summary>
    public class AppiMessages
    {
        /// <summary>
        /// Сообщения по каналу А (can1)
        /// </summary>
        public IList<CanFrame> ChannelA { get; private set; }
        /// <summary>
        /// Сообщения по каналу B (can2)
        /// </summary>
        public IList<CanFrame> ChannelB { get; private set; }

        public AppiMessages(IList<CanFrame> ChannelA, IList<CanFrame> ChannelB)
        {
            this.ChannelA = ChannelA;
            this.ChannelB = ChannelB;
        }

        public static AppiMessages Empty
        {
            get { return new AppiMessages(new List<CanFrame>(), new List<CanFrame>()); }
        }
    }
}
