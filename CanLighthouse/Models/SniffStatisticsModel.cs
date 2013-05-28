using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace CanLighthouse.Models
{
    public class SniffStatisticsModel
    {
        public TimeSpan Duration { get; private set; }
        public int FramesCount { get; set; }
        public ReadOnlyCollection<FrameModel> DistinctedFrames { get; private set; }

        public SniffStatisticsModel(IList<FrameModel> Frames)
        {
            FramesCount = Frames.Count;
            Duration = Frames.Select(f => f.RecieveTime).Max() - Frames.Select(f => f.RecieveTime).Min();
            var Distincts = Frames.Distinct(new Distinctator()).ToList();
            DistinctedFrames = new ReadOnlyCollection<FrameModel>(Distincts);
        }

        private class Distinctator : EqualityComparer<FrameModel>
        {
            public override bool Equals(FrameModel x, FrameModel y)
            {
                return x.Id == y.Id && x.Data.SequenceEqual(y.Data);
            }

            public override int GetHashCode(FrameModel obj)
            {
                return obj.Id.GetHashCode() ^ obj.Data.Aggregate(0, (h, b) => b.GetHashCode());
            }
        }
    }
}
