using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MadWizard.WinUSBNet;

namespace Communications.Appi.Winusb
{
    /// <summary>
    /// Связь с АППИ через WinUSB
    /// </summary>
    public class WinusbAppiDev : AppiDev
    {
        /// <summary>
        /// GUID устройства
        /// </summary>
        public readonly static List<string> DeviceGuids = new List<string>()
        {
            "524cc09a-0a72-4d06-980e-afee3131196e",
            "3af3f480-41b5-4c24-b2a9-6aacf7de3d01"
        };
        //public const string DeviceGuid = "524cc09a-0a72-4d06-980e-afee3131196e";
        //public const string DeviceGuid = "3af3f480-41b5-4c24-b2a9-6aacf7de3d01";
        private String DevicePath { get; set; }
        /// <summary>
        /// USB-устройство
        /// </summary>
        private USBDevice Device { get; set; }
        /// <summary>
        /// Труба чтения
        /// </summary>
        private USBPipe ReadPipe { get; set; }
        /// <summary>
        /// Труба записи
        /// </summary>
        private USBPipe WritePipe { get; set; }

        /// <summary>
        /// Перечисляет все слоты подключенных устройств АППИ
        /// </summary>
        /// <returns></returns>
        public static IList<AppiDeviceSlot> GetDevices()
        {
            return DeviceGuids.SelectMany(DeviceGuid =>
                USBDevice.GetDevices(DeviceGuid).Select(di => new WinusbAppiDeviceSlot(di)).Cast<AppiDeviceSlot>()).ToList();
        }

        internal WinusbAppiDev(USBDeviceInfo di)
            : base()
        {
            Device = new USBDevice(di);
            DevicePath = di.DevicePath;
            
            ReadPipe = Device.Pipes.First(p => p.IsIn);
            ReadPipe.Policy.PipeTransferTimeout = 100;
            ReadPipe.Policy.AutoClearStall = true;
            ReadPipe.Flush();
            
            WritePipe = Device.Pipes.First(p => p.IsOut);
            WritePipe.Policy.PipeTransferTimeout = 100;
            
            OpenedDevices.Add(this);
        }

        #region Чтение и запись буфера
        /// <summary>
        /// Чтение буфера
        /// </summary>
        protected override byte[] ReadBuffer()
        {
            try
            {
                Byte[] buff = new Byte[BufferSize];
                ReadPipe.Read(buff);
                //return buff.SkipWhile(b => b == 0).ToArray();
                return buff;
            }
            catch (USBException)
            {
                this.OnDisconnected();
                return new byte[0];
            }
        }
        /// <summary>
        /// Запись буфера
        /// </summary>
        /// <param name="Buffer">Данные для записи</param>
        protected override void WriteBuffer(byte[] Buffer)
        {
            WritePipe.Write(Buffer);
        } 
        #endregion

        internal static List<WinusbAppiDev> OpenedDevices { get; set; }

        static WinusbAppiDev()
        {
            OpenedDevices = new List<WinusbAppiDev>();

            //var f = new System.Windows.Forms.Form();
            //f.Show();

            //USBNotifier n = new USBNotifier(f, DeviceGuid);
            //n.Removal += new USBEventHandler(n_Removal);
            //n.Arrival += new USBEventHandler(n_Arrival);
        }

        static void n_Arrival(object sender, USBEvent e)
        {
            throw new NotImplementedException();
        }

        static void n_Removal(object sender, USBEvent e)
        {
            var DisconnectedOpenedDevice = OpenedDevices.FirstOrDefault(d => d.DevicePath == e.DevicePath);
            if (DisconnectedOpenedDevice != null)
                DisconnectedOpenedDevice.OnDisconnected();
        }

        public override void Dispose()
        {
            //ReadPipe.Abort();
            //WritePipe.Abort();

            OpenedDevices.Remove(this);
            base.Dispose();
            Device.Dispose();
        }

        internal static bool IsDeviceOpened(string DevicePath)
        {
            return OpenedDevices.Any(d => d.DevicePath == DevicePath);
        }
    }
}
