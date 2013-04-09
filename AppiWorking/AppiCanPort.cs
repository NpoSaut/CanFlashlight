using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications.Appi
{
    public class AppiCanPort
    {
        public AppiLine Line { get; private set; }
        public event CanMessagesReceiveEventHandler Recieved;

        public AppiCanPort(AppiLine Line)
        {
            this.Line = Line;
        }

        internal void OnMessagesRecieved(IList<CanMessage> Messages)
        {
            if (Messages.Any() && Recieved != null) Recieved(this, new CanMessagesReceiveEventArgs(Messages, Line));
        }
    }

    public delegate void CanMessagesReceiveEventHandler(object sender, CanMessagesReceiveEventArgs e);
    public class CanMessagesReceiveEventArgs : EventArgs
    {
        public IList<CanMessage> Messages { get; set; }
        public AppiLine Line { get; private set; }

        public CanMessagesReceiveEventArgs(IList<CanMessage> Messages, AppiLine Line)
        {
            this.Messages = Messages;
            this.Line = Line;
        }
    }
}
