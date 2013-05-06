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

        public event CanFramesReceiveEventHandler Recieved;

        public CanFrameHandler(int Descriptor)
        {
            this.Descriptor = Descriptor;
            this.ValueValidnessTime = TimeSpan.FromMilliseconds(1000);

            lock (Handlers)
                Handlers.Add(this);
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

        private object LastValueLocker = new object();
        public TimeSpan ValueValidnessTime { get; set; }
        public CanFrame LastFrame { get; private set; }
        public DateTime LastFrameRecieveTime { get; private set; }
        private bool _LastFrameValid;
        public event EventHandler LastFrameValidChanged;
        /// <summary>
        /// Показывает, актуально ли значение последнего принятого кадра
        /// </summary>
        public bool LastFrameValid
        {
            get { return _LastFrameValid; }
            private set
            {
                if (_LastFrameValid != value)
                {
                    _LastFrameValid = value;
                    if (LastFrameValidChanged != null) LastFrameValidChanged(this, new EventArgs());
                }
            }
        }
        private void SetLastFrame(CanFrame Frame)
        {
            lock (LastValueLocker)
            {
                LastFrame = Frame;
                LastFrameRecieveTime = DateTime.Now;
                LastFrameValid = true;
            }
        }

        private static void LastFrameValidnessCheckJob()
        {
            while (true)
            {
                foreach (var h in Handlers)
                {
                    DateTime dt = DateTime.Now;
                    lock (h.LastValueLocker)
                    {
                        h.LastFrameValid = h.LastFrameValid && (h.LastFrameRecieveTime.Add(h.ValueValidnessTime) >= dt);
                    }
                }
                Thread.Yield();
            }
        }

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

        private static List<CanFrameHandler> Handlers { get; set; }
        public CanFrameHandler()
        {
            Handlers = new List<CanFrameHandler>();
            Thread ValidnessCheckThread = new Thread(new ThreadStart(LastFrameValidnessCheckJob));
            ValidnessCheckThread.IsBackground = true;
            ValidnessCheckThread.Start();
        }

        public void Dispose()
        {
            lock (Handlers)
                Handlers.Remove(this);
        }
    }
}
