using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;

namespace CanLighthouse
{
    public static class Beeper
    {
        public class BeepInfo
        {
            public int Frequency { get; set; }
            public int Duration { get; set; }
            public DateTime OrderTime { get; set; }

            public BeepInfo()
            {
                this.Frequency = 1000;
                this.Duration = 50;
                OrderTime = DateTime.Now;
            }

            public override string ToString()
            {
                return string.Format("{0} @ {1} Hz", Duration, Frequency);
            }
        }

        private static ConcurrentQueue<BeepInfo> BeepsQueue = new ConcurrentQueue<BeepInfo>();
        private static BeepInfo LastBeep;
        private static Thread BeepingThread;

        static Beeper()
        {
            BeepingThread = new Thread(BeepingThreadLoop) { IsBackground = true };
            BeepingThread.Start();
        }
        private static void BeepingThreadLoop()
        {
            while(true)
            {
                BeepInfo beep;
                if (BeepsQueue.TryDequeue(out beep))
                {
                    if ((DateTime.Now - beep.OrderTime).TotalMilliseconds < beep.Duration + 50)
                    {
                        Console.Beep(beep.Frequency, beep.Duration);
                        Thread.Sleep(50);
                    }
                }
                Thread.Sleep(5);
            }
        }

        public static void Beep() { Beep(new BeepInfo()); }
        public static void Beep(BeepInfo beep)
        {
            if (LastBeep == null || (beep.OrderTime - LastBeep.OrderTime).TotalMilliseconds > LastBeep.Duration)
            {
                BeepsQueue.Enqueue(beep);
                LastBeep = beep;
            }
        }
    }
}
