using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Timers;
using System.Text.RegularExpressions;

namespace Communications.Can.LogReader
{
    public abstract class LogReaderPort : CanPort, IDisposable
    {
        public FileInfo LogFile { get; private set; }
        private Timer ReadTimer { get; set; }

        public LogReaderPort(FileInfo LogFile)
            : base(LogFile.Name)
        {
            this.LogFile = LogFile;
            ReadTimer = new Timer(1000);
            ReadTimer.Elapsed += new ElapsedEventHandler(ReadTimer_Elapsed);
        }

        /// <summary>
        /// Начинает чтение лога
        /// </summary>
        public void Start()
        {
            ReadTimer.Start();
        }
        /// <summary>
        /// Начинает чтение лога с заданным интервалом
        /// </summary>
        public void Start(double interval)
        {
            ReadTimer.Interval = interval;
            ReadTimer.Start();
        }

        protected abstract CanFrame ReadNextFrame();

        void ReadTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var f = ReadNextFrame();
            if (f != null) OnFrameRecieved(f);
            else ReadTimer.Stop();
        }

        protected override void SendImplementation(IList<CanFrame> Frames)
        {
            foreach (var fr in Frames)
                Console.WriteLine(String.Format("{0}  <<-  {1}", Name, fr));
        }

        public virtual void Dispose()
        {
            ReadTimer.Stop();
        }
    }
}
