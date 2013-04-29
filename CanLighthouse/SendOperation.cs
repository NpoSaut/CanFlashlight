using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CanLighthouse.Models;

namespace CanLighthouse
{
    public abstract class SendOperation
    {
    }

    public class SendPushing : SendOperation
    {
        public IList<FrameModel> PushingFrames { get; set; }
    }

    public class SendDelay : SendOperation
    {
        public TimeSpan Duration { get; set; }
    }
}
