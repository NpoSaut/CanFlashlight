using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;

namespace Communications.Can
{
    public class CanFrameHandler : IDisposable
    {
        public int Descriptor { get; set; }
        internal List<CanPort> OnPorts;

        public event CanFramesReceiveEventHandler Recieved;

        public CanFrameHandler(int Descriptor)
        {
            this.Descriptor = Descriptor;
            this.OnPorts = new List<CanPort>();

            lock (AllHandlers)
                AllHandlers.Add(this);
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

        internal static List<CanFrameHandler> AllHandlers { get; set; }
        static CanFrameHandler()
        {
            AllHandlers = new List<CanFrameHandler>();
        }

        public void Dispose()
        {
            foreach (var p in OnPorts)
                p.RemoveHandler(this);

            lock (AllHandlers)
                AllHandlers.Remove(this);
        }
    }
}
