using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace BlokFrames
{
    [FrameDescriptor(0x4268)]
    public class MmAltLongFrame : BlokFrame
    {
        /// <summary>
        /// Достоверность данных
        /// </summary>
        public bool Reliable { get; set; }
        /// <summary>
        /// Широта (град.)
        /// </summary>
        /// <remarks>По Y</remarks>
        public Double Latitude { get; set; }
        /// <summary>
        /// Долгота (град.)
        /// </summary>
        /// <remarks>По X</remarks>
        public Double Longitude { get; set; }

        #region Перевод в радианы
        /// <summary>
        /// Широта (в радианах)
        /// </summary>
        /// <remarks>По Y</remarks>
        public Double LatitudeRad
        {
            get { return Latitude * Math.PI / 180.0; }
            set { Latitude = value * 180.0 / Math.PI; }
        }
        /// <summary>
        /// Долгота (в радианах)
        /// </summary>
        /// <remarks>По X</remarks>
        public Double LongitudeRad
        {
            get { return Longitude * Math.PI / 180.0; }
            set { Longitude = value * 180.0 / Math.PI; }
        } 
        #endregion

        protected override byte[] GetCanFrameData()
        {
            throw new NotImplementedException();
        }

        protected override void FillWithCanFrameData(byte[] Data)
        {
            var ds = ByteArrayToStructure<DataStruct>(Data);
            this.Latitude = ds.Lat * 10e-9 * 180 / Math.PI;
            this.Longitude = (Int32)(ds.Lon | ((ds.Lon & (1 << 30)) << 1)) * 10e-9 * 180 / Math.PI;
            this.Reliable = (ds.Rel & (1 << 7)) != 0;
        }


        [StructLayout(LayoutKind.Explicit, Size = 8, Pack = 1)]
        protected struct DataStruct
        {
            [MarshalAs(UnmanagedType.I4)]
            [FieldOffset(0)]
            public Int32 Lat;

            [MarshalAs(UnmanagedType.U4)]
            [FieldOffset(4)]
            public UInt32 Lon;

            [MarshalAs(UnmanagedType.U1)]
            [FieldOffset(7)]
            public byte Rel;
        }

    }
}
