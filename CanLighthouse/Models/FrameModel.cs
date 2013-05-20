using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Communications.Can;
using System.Text.RegularExpressions;
using CanLighthouse.Describing;

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

        private String _FrameName;
        /// <summary>
        /// Название кадра
        /// </summary>
        public String FrameName
        {
            get { return _FrameName; }
            set
            {
                if (_FrameName != value)
                {
                    _FrameName = value;
                    OnPropertyChanged("FrameName");
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
                    _Id = (UInt16)(Descriptor / 0x20);
                    OnPropertyChanged("Descriptor");
                    OnPropertyChanged("Id");
                }
            }
        }

        private UInt16 _Id;
        /// <summary>
        /// Идентификатор
        /// </summary>
        public UInt16 Id
        {
            get { return _Id; }
            set
            {
                if (_Id != value)
                {
                    _Id = value;
                    _Descriptor = (UInt16)(Id * 0x20 + Data.Length);
                    OnPropertyChanged("Id");
                    OnPropertyChanged("Descriptor");
                }
            }
        }
        
        /// <summary>
        /// Представление в виде побайтовой строки
        /// </summary>
        public String HexString
        {
            get { return string.Join(" ", Data.Select(b => b.ToString("X2"))); }
            set { Data = DecodeDataString(value, 16); }
        }
        /// <summary>
        /// Представление в виде побитовой строки
        /// </summary>
        public String BinaryString
        {
            get
            {
                return string.Join(" ",
                    Data.Select(B =>
                        String.Join("",
                            Enumerable.Range(0, 8)
                                .Select(i => (B >> i) & 0x01)
                                .Reverse()).Insert(4, " ")));
            }
            set { Data = DecodeDataString(value, 2); }
        }
        /// <summary>
        /// Представление в виде десятичной строки
        /// </summary>
        public String DecString
        {
            get { return string.Join(" ", Data); }
            set { Data = DecodeDataString(value, 10); }
        }
        
        private Byte[] DecodeDataString(String str, int Base, bool reorder = false)
        {
            str = str.ToLower().Replace(" ", "");

            string basepattern = "0-" + Math.Min(9, Base-1);
            if (Base > 10) basepattern += "a-" + (Char)((int)'a' + Base - 11);

            string itemsperbyte = Math.Ceiling(Math.Log(256, Base)).ToString();

            string Pattern = @"((?<databyte>[" + basepattern + @"]{1," + itemsperbyte + @"})[\s-]*){1,8}";

            Regex regex = new Regex(Pattern);
            Match match = regex.Match(str);
            if (!match.Success)
                throw new ArgumentException("Строка с данными имела неверный формат!");

            var CapturesSequence = match.Groups["databyte"].Captures.OfType<Capture>();
            if (reorder) CapturesSequence = CapturesSequence.Reverse();

            return CapturesSequence.Select(c => Convert.ToByte(c.Value, Base)).ToArray();
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
                    OnDataChanged();
                }
            }
        }

        protected void OnDataChanged()
        {
            OnPropertyChanged("Data");
            OnPropertyChanged("HexString");
            OnPropertyChanged("BinaryString");
            OnPropertyChanged("DecString");
        }

        private string _PortName;
        /// <summary>
        /// Имя порта
        /// </summary>
        public string PortName
        {
            get { return _PortName; }
            set
            {
                if (_PortName != value)
                {
                    _PortName = value;
                    OnPropertyChanged("PortName");
                }
            }
        }

        public FrameModel()
        {
        }
        public FrameModel(CanFrame OnFrame)
        {
            this.BasedOn = OnFrame;
            Descriptor = (UInt16)OnFrame.Descriptor;
            Id = (UInt16)OnFrame.Id;
            RecieveTime = DateTime.Now;
            Data = OnFrame.Data.ToArray();
            FindDescription();
        }
        public void FindDescription()
        {
            var pd = (App.Current as App).Protocol;
            if (pd.FrameDescriptions.ContainsKey(this.Descriptor))
                this.FrameName = pd.FrameDescriptions[this.Descriptor].Name;
        }

        public CanFrame GetFrame()
        {
            return CanFrame.NewWithId(this.Id, this.Data.ToArray());
        }
    }
}
