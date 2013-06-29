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
            return USBDevice.GetDevices(DeviceGuid).Select(di => new WinusbAppiDeviceSlot(di)).Cast<AppiDeviceSlot>().ToList();
        }

        internal WinusbAppiDev(USBDeviceInfo di)
            : base()
        {
            Device = new USBDevice(di);
            DevicePath = di.DevicePath;
            ReadPipe = Device.Pipes.First(p => p.IsIn);
            WritePipe = Device.Pipes.First(p => p.IsOut);
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
                return buff.SkipWhile(b => b == 0).ToArray();
            }
            catch (Exception)
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
