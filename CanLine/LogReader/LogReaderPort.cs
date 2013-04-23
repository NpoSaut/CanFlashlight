using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Timers;
using System.Text.RegularExpressions;

namespace Communications.Can.LogReader
{
    public class LogReaderPort : CanPort, IDisposable
    {
        public FileInfo LogFile { get; private set; }
        private TextReader tr { get; set; }
        private Timer ReadTimer { get; set; }
        public String Pattern { get; set; }

        public LogReaderPort(FileInfo LogFile)
            : base(LogFile.Name)
        {
            Pattern = @"(?<descriptor>[0-9a-fA-F]{4})[\s]((?<databyte>[0-9a-fA-F]{2})\s?){1,8}";

            this.LogFile = LogFile;
            tr = new StreamReader(LogFile.FullName);
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

        private CanFrame ReadNextFrame()
        {
            var l = tr.ReadLine();
            if (l == null) return null;

            Regex regex = new Regex(Pattern);
            Match match = regex.Match(l);
            if (!match.Success) return null;

            var desc = Convert.ToUInt16(match.Groups["descriptor"].Value, 16);
            var data = match.Groups["databyte"].Captures.OfType<Capture>().Select(c => Convert.ToByte(c.Value, 16)).ToArray();

            return CanFrame.NewWithDescriptor(desc, data);
        }

        void ReadTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var f = ReadNextFrame();
            if (f != null) OnFrameRecieved(f);
            else ReadTimer.Stop();
        }

        public override void Send(IList<CanFrame> Frames)
        {
            throw new NotImplementedException("Не поддерживается отправка в файл");
        }

        public void Dispose()
        {
            ReadTimer.Stop();
            tr.Close();
        }
    }
}
