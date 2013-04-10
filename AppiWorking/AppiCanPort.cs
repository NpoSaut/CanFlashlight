using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;

namespace Communications.Appi
{
    public class AppiCanPort : CanPort
    {
        public AppiLine Line { get; private set; }

        private AppiDev Device { get; set; }

        internal AppiCanPort(AppiDev Device, AppiLine Line)
            : base(Line.ToString())
        {
            this.Device = Device;
            this.Line = Line;
        }

        public override void Send(IList<CanFrame> Frames)
        {
            Device.SendFrames(Frames, Line);
        }

        internal void OnAppiFramesRecieved(IList<CanFrame> Frames)
        {
            OnFramesRecieved(Frames);
        }
    }
}
