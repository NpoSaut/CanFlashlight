using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Communications.Can;

namespace CanLighthouse.Models
{
    public class FrameHandlerModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(String PropertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }

        public int Descriptor { get; private set; }
        public CanPort Port { get; private set; }
        private CanFrameHandler Handler { get; set; }

        public FrameHandlerModel(int Descriptor, CanPort Port)
        {
            this.Descriptor = Descriptor;
            this.Port = Port;
            LastFrame = null;
        }

        public void Initialize()
        {
            if (Handler != null)
                Deinitialize();

            this.Handler = new CanFrameHandler(this.Descriptor);
            Port.AddHandler(this.Handler);
            Handler.Recieved += Handler_Recieved;
        }
        public void Deinitialize()
        {
            Port.RemoveHandler(Handler);
            Handler.Recieved -= Handler_Recieved;
            Handler.Dispose();
            LastFrame = null;
        }

        void Handler_LastFrameValidChanged(object sender, EventArgs e)
        {
        }

        void Handler_Recieved(object sender, CanFramesReceiveEventArgs e)
        {
        }

        private FrameModel _LastFrame;
        /// <summary>
        /// Последнее принятое значение
        /// </summary>
        public FrameModel LastFrame
        {
            get { return _LastFrame; }
            private set
            {
                if (_LastFrame != value)
                {
                    _LastFrame = value;
                    OnPropertyChanged("LastFrame");
                }
            }
        }

        private bool _LastDataValid;
        /// <summary>
        /// Актуально ли последнее значение
        /// </summary>
        public bool LastDataValid
        {
            get { return _LastDataValid; }
            set
            {
                if (_LastDataValid != value)
                {
                    _LastDataValid = value;
                    OnPropertyChanged("LastDataValid");
                }
            }
        }

    }
}
