using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Media;

namespace CanLighthouse.Models
{
    public class FilterModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(String PropertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }

        private bool _IsEnabled;
        /// <summary>
        /// Фильтр включен
        /// </summary>
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set
            {
                if (_IsEnabled != value)
                {
                    _IsEnabled = value;
                    OnPropertyChanged("IsEnabled");
                }
            }
        }

        private UInt16 _Descriptor;
        /// <summary>
        /// Дескриптор
        /// </summary>
        public UInt16 Descriptor
        {
            get { return _Descriptor; }
            set
            {
                if (_Descriptor != value)
                {
                    _Descriptor = value;
                    OnPropertyChanged("Descriptor");
                }
            }
        }

        //private Brush _Foreground;
        ///// <summary>
        ///// Подсветка текста
        ///// </summary>
        //public Brush Foreground
        //{
        //    get { return _Foreground; }
        //    set
        //    {
        //        if (_Foreground != value)
        //        {
        //            _Foreground = value;
        //            OnPropertyChanged("Foreground");
        //        }
        //    }
        //}

        //private Brush _Background;
        ///// <summary>
        ///// Подсветка фона
        ///// </summary>
        //public Brush Background
        //{
        //    get { return _Background ?? Brushes.White; }
        //    set
        //    {
        //        if (_Background != value)
        //        {
        //            _Background = value;
        //            OnPropertyChanged("Background");
        //        }
        //    }
        //}
    }
}
