using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public IList<CanMessage> ChannelA { get; private set; }
        /// <summary>
        /// Сообщения по каналу B (can2)
        /// </summary>
        public IList<CanMessage> ChannelB { get; private set; }

        public AppiMessages(IList<CanMessage> ChannelA, IList<CanMessage> ChannelB)
        {
            this.ChannelA = ChannelB;
            this.ChannelB = ChannelB;
        }
    }
}
