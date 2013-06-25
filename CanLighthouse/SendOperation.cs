using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CanLighthouse.Models;

namespace CanLighthouse
{
    public class SendOperation
    {
        public IList<FrameModel> PushingFrames { get; set; }
        public TimeSpan DelayAfter { get; set; }

        public SendOperation()
        {
            PushingFrames = new List<FrameModel>();
            DelayAfter = TimeSpan.Zero;
        }
    }
}
