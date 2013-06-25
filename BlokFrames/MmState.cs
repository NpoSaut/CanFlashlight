using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;


namespace BlokFrames
{
    public enum MapTargetKind : byte
    {
        /// <summary>
        /// Светофор
        /// </summary>
        TrafficLight = 1,
        /// <summary>
        /// Станция
        /// </summary>
        Station = 2,
        /// <summary>
        /// Опасное место
        /// </summary>
        DangerousPlace = 3,
        /// <summary>
        /// Мост
        /// </summary>
        Bridge = 4,
        /// <summary>
        /// Переезд
        /// </summary>
        Crossing = 5,
        /// <summary>
        /// Платформа
        /// </summary>
        Platform = 6,
        /// <summary>
        /// Туннель
        /// </summary>
        Tunnel = 7,
        /// <summary>
        /// Стрелка
        /// </summary>
        Switch = 8,
        /// <summary>
        /// Датчик ТКС
        /// </summary>
        Tks = 9,
        /// <summary>
        /// Генератор САУТ
        /// </summary>
        GpuSaut = 10,
        /// <summary>
        /// Тупик
        /// </summary>
        DeadEnd = 11
    }

    /// <summary>
    /// Цель электронной карты
    /// </summary>
    public class MapTarget
    {
        public MapTargetKind Kind { get; set; }
        public Int16 X { get; set; }
        /// <summary>
        /// Наличие АЛС-ЕН
        /// </summary>
        public bool AlsEn { get; set; }
        public bool OnStation { get; set; }
        public bool RadioChannelOnStation { get; set; }
        public UInt16 Length { get; set; }
        /// <summary>
        /// Условно-разрешающий светофор для грузового поезда
        /// </summary>
        public bool ConditionallyAllowLigthForFreight { get; set; }
        public bool PullForwardForPassenger { get; set; }
        public bool PullForwardForFreight { get; set; }
        public bool AlsEnNewTable { get; set; }
        public int Speed { get; set; }
        public int AlsnFreq { get; set; }

        public override string ToString()
        {
            return String.Format("{0} [{1:N0} : {1:N)}]", Kind, X, Length);
        }
    }

    [FrameDescriptor(0x43E8)]
    /// <summary>
    /// Сообщение MM_STATE передаётся модулем ЭК с периодом 100 мкс
    /// Содрежит описание одной из ближайших целей
    /// </summary>
    public class MmState : BlokFrame
    {
        /// <summary>
        /// В карте
        /// </summary>
        public bool InMap { get; set; }
        /// <summary>
        /// Исправность
        /// </summary>
        public bool Good { get; set; }
        /// <summary>
        /// Номер цели (нужен только для попрядка в CAN)
        /// </summary>
        public int TargetNumber { get; set; }
        /// <summary>
        /// Цель на карте
        /// </summary>
        public MapTarget Target { get; set; }

        protected override byte[] GetCanFrameData()
        {
            int[] data =
                new int[]
                {
                    Good ? ( InMap ? 0 : 1 ) : ( 15 ),
                    (TargetNumber & 0x0F) | ( (int)(Target.Kind) << 4 ),
                    Target.X >> 8,
                    Target.X & 0XFF,
                    ( Target.AlsEn ? (1<<7) : 0 )
                       | ( Target.OnStation ? (1<<6) : 0 )
                       | ( Target.RadioChannelOnStation ? (1<<5) : 0 )
                       | ( (Target.Length >> 8) & 0x1F ),
                    Target.Length & 0xFF,
                    ( Target.ConditionallyAllowLigthForFreight ? (1<<7) : 0 )
                        | ( Target.PullForwardForPassenger ? (1<<6) : 0 )
                        | ( Target.ConditionallyAllowLigthForFreight ? (1<<5) : 0 )
                        | ( Target.AlsEnNewTable ? (1<<4) : 0 )
                        | ( ((Target.Speed >> 8) & 1) << 2 )
                        | ( Target.AlsnFreq & 0x3 ),
                    Target.Speed & 0xFF
                };
            return data.Cast<byte>().ToArray();
        }

        protected override void FillWithCanFrameData(byte[] Data)
        {
            InMap = (Data[0] == 0);
            Good = true;
            TargetNumber = Data[1] & 0xF;

            Target =
                new MapTarget()
                {
                    Kind = (MapTargetKind)(Data[1] >> 4),
                    X = BitConverter.ToInt16(new byte[2] { Data[3], Data[2] }, 0),
                    AlsEn = ((Data[4] >> 7) & 1) != 0,
                    OnStation = ((Data[4] >> 6) & 1) != 0,
                    Length = BitConverter.ToUInt16(new byte[2] { Data[5], (byte)(Data[4] & 0x1F) }, 0),
                    ConditionallyAllowLigthForFreight = ((Data[6] >> 7) & 1) != 0,
                    PullForwardForPassenger = ((Data[6] >> 6) & 1) != 0,
                    PullForwardForFreight = ((Data[6] >> 5) & 1) != 0,
                    AlsEnNewTable = ((Data[6] >> 4) & 1) != 0,
                    Speed = BitConverter.ToUInt16(new byte[2] { Data[7], (byte)((Data[6] >> 2) & 1) }, 0),
                    AlsnFreq = (Data[6] >> 0) & 3
                };
        }

        public override string ToString()
        {
            return String.Format("Цель {0} : {1}", TargetNumber, Target);
        }
    }
}
