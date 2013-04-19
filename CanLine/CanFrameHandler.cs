using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications.Can
{
    public class CanFrameHandler
    {
        public int Descriptor { get; set; }

        public event CanFramesReceiveEventHandler Recieved;

        public CanFrameHandler(int Descriptor)
        {
            this.Descriptor = Descriptor;
        }

        internal void OnRecieved(IList<CanFrame> Frames, CanPort FromPort)
        {
            if (Recieved != null) Recieved(this, new CanFramesReceiveEventArgs(Frames, FromPort));

            lock (WaitLocker)
            {
                if (IsWaiting)
                {
                    PendingFrame = Frames.First();
                    IsWaiting = false;
                }
            }
        }

        private Boolean IsWaiting { get; set; }
        private object WaitLocker = new object();
        private CanFrame PendingFrame;

        public CanFrame WaitFor()
        {
            IsWaiting = true;
            bool w = IsWaiting;
            while (w)
            {
                lock (WaitLocker)
                { w = IsWaiting; }
            }
            return PendingFrame;
        }
    }
}
