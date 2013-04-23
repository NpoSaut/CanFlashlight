using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Communications.Can;

namespace CanLighthouse.Models
{
    public class FrameModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(String PropertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }

        public CanFrame BasedOn { get; private set; }

        public DateTime RecieveTime { get; private set; }
        public UInt16 Descriptor { get; private set; }
        public UInt16 Id { get; private set; }
        /// <summary>
        /// Представление в виде побайтовой строки
        /// </summary>
        public String HexString
        {
            get { return string.Join(" ", Data.Select(b => b.ToString("X2"))); }
        }
        /// <summary>
        /// Представление в виде побитовой строки
        /// </summary>
        public String BinaryString
        {
            get { return string.Join("-", Data.Select(B => String.Join("", Enumerable.Range(0, 8).Select(i => (B >> i) & 0x01).Reverse()) ).Reverse()); }
        }
        private Byte[] _Data;
        /// <summary>
        /// Данные фрейма
        /// </summary>
        public Byte[] Data
        {
            get { return _Data; }
            set
            {
                if (_Data != value)
                {
                    _Data = value;
                    OnPropertyChanged("Data");
                    OnPropertyChanged("HexString");
                    OnPropertyChanged("BinaryString");
                }
            }
        }

        public FrameModel(CanFrame OnFrame)
        {
            this.BasedOn = OnFrame;
            Descriptor = (UInt16)OnFrame.Descriptor;
            Id = (UInt16)OnFrame.Id;
            RecieveTime = DateTime.Now;
            Data = OnFrame.Data.ToArray();
        }
    }
}
