using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Communications.Can.LogReader
{
    public class TextLogReaderPort : CanVirtualPort
    {
        private TextReader tr { get; set; }
        public String Pattern { get; set; }

        public TextLogReaderPort(FileInfo LogFile)
            : base(LogFile.Name)
        {
            Pattern = @"(?<descriptor>[0-9a-fA-F]{4})[\s]((?<databyte>[0-9a-fA-F]{2})\s?){1,8}";
            tr = new StreamReader(LogFile.FullName);
        }

        protected override CanFrame ReadNextFrame()
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

        public override void Dispose()
        {
            base.Dispose();
            tr.Close();
        }
    }
}
