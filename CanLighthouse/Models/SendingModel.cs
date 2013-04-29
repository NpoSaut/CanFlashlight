using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace CanLighthouse.Models
{
    public class SendingModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(String PropertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }

        private FrameModel _SelectedFrame;
        /// <summary>
        /// Выбранный фрейм
        /// </summary>
        public FrameModel SelectedFrame
        {
            get { return _SelectedFrame; }
            set
            {
                if (_SelectedFrame != value)
                {
                    if (_SelectedFrame != null)
                        SelectedFrame.PropertyChanged -= OnSelectedFrameModifed;
                    
                    _SelectedFrame = value;

                    if (_SelectedFrame != null)
                        SelectedFrame.PropertyChanged += OnSelectedFrameModifed;
                    
                    OnPropertyChanged("SelectedFrame");
                }
            }
        }

        void OnSelectedFrameModifed(object sender, PropertyChangedEventArgs e)
        {
            if (SelectedFrameModifed != null) SelectedFrameModifed(sender, e);
        }

        public event PropertyChangedEventHandler SelectedFrameModifed;
    }
}
