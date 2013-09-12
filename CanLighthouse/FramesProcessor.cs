using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using ObjectiveCommons.Collections;
using CanLighthouse.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.ComponentModel;

namespace CanLighthouse
{
    public class FramesProcessor : DispatcherObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(String PropertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }

        private object PushLocker = new object(),
                       DisplayLocker = new object();

        private const int CutOffCount = 100;
        public LinkedList<FrameModel> AllFrames { get; private set; }
        public LockableObservableCollection<FrameModel> FilteredFrames { get; private set; }

        private HashSet<UInt16> _Filters = new HashSet<ushort>();
        public HashSet<UInt16> Filters
        {
            get { return _Filters; }
            set
            {
                if (!value.SequenceEqual(_Filters))
                {
                    _Filters = value;
                    OnFiltersChanged();
                }
            }
        }

        private int _TotalFrames;
        public int TotalFrames
        {
            get { return _TotalFrames; }
            set
            {
                if (_TotalFrames != value)
                {
                    _TotalFrames = value;
                    OnPropertyChanged("TotalFrames");
                }
            }
        }

        public bool BeepWhenFrameReceived { get; set; }

        public FramesProcessor()
        {
            AllFrames = new LinkedList<FrameModel>();
            FilteredFrames = new LockableObservableCollection<FrameModel>();
            Filters = new HashSet<ushort>();
        }

        public void PushFrames(List<FrameModel> PushFrames)
        {
            lock (PushLocker)
            {
                //int FramesToCut = Math.Max(AllFrames.Count + PushFrames.Count - CutOffCount, 0);
                //if (FramesToCut > 0)
                //    FramesToCut -= CutTail(FramesToCut);

                foreach (var f in PushFrames)
                    AllFrames.AddLast(f);

                TotalFrames = AllFrames.Count;

                var PassedFrames =
                    Filters.Any() ?
                    PushFrames.Where(FrameFilter).ToList() :
                    PushFrames;

                if (PassedFrames.Any())
                {
                    foreach (var f in PassedFrames)
                        FramesToInterfaceBuffer.Enqueue(f);

                    if (BeepWhenFrameReceived)
                        Beeper.Beep();

                    if (FramesToInterfaceBuffer.Any() && !FramesSyncronizationScheduled)
                    {
                        FramesSyncronizationScheduled = true;

                        var xxx = FramesToInterfaceBuffer;
                        FramesToInterfaceBuffer = new Queue<FrameModel>();
                        Action UpdateAction =
                            () =>
                                Dispatcher.BeginInvoke((Action<Queue<FrameModel>>)SyncronizeFramesOutput,
                                    System.Windows.Threading.DispatcherPriority.Background,
                                    xxx);
                        Task.Factory.StartNew(UpdateAction);
                    }
                }
            }
        }
        private bool FramesSyncronizationScheduled = false;
        private Queue<FrameModel> FramesToInterfaceBuffer = new Queue<FrameModel>();
        private void SyncronizeFramesOutput(Queue<FrameModel> Buffer)
        {
            lock (DisplayLocker)
            {
                foreach (var f in Buffer)
                {
                    FilteredFrames.Add(f);
                }
                if (FilteredFrames.Any() && NewItemAdEnd != null)
                    NewItemAdEnd(this, new NewItemAdEndEventArgs() { LastItem = FilteredFrames.Last() });

                System.Threading.Thread.Sleep(50);
                FramesSyncronizationScheduled = false;
            }
        }

        private void OnFiltersChanged()
        {
            Dispatcher.Invoke((Action)SyncronizeFilter);
        }
        private void SyncronizeFilter()
        {
            lock(PushLocker) lock(DisplayLocker)
                using(FilteredFrames.Locker())
                {
                    FilteredFrames.Clear();
                    var yo =
                        Filters.Any() ?
                        AllFrames.ToList().AsParallel().Where(FrameFilter).ToList() :
                        AllFrames.ToList();
                    foreach (var f in yo)
                        FilteredFrames.Add(f);
                }
        }

        public event EventHandler<NewItemAdEndEventArgs> NewItemAdEnd;
        public class NewItemAdEndEventArgs : EventArgs
        {
            public FrameModel LastItem { get; set; }
        }

        private bool FrameFilter(FrameModel f)
        {
            return Filters.Contains(f.Descriptor);
        }

        private int CutTail(int CutCount)
        {
            int i;
            lock (PushLocker) lock (DisplayLocker)
            {
                var FilteredFirst = FilteredFrames.FirstOrDefault();
                for (i = 0; i < CutCount; i++)
                {
                    var f = AllFrames.First;
                    if (f == null) break;
                    if (FilteredFirst != null && f.Value == FilteredFirst)
                    {
                        FilteredFrames.RemoveAt(0);
                        FilteredFirst = FilteredFrames.FirstOrDefault();
                    }
                    AllFrames.RemoveFirst();
                }
            }
            return i;
        }

        internal void Clear()
        {
            lock (PushLocker) lock (DisplayLocker)
            {
                AllFrames.Clear();
                FilteredFrames.Clear();
                FramesToInterfaceBuffer.Clear();
            }
        }
    }
}
