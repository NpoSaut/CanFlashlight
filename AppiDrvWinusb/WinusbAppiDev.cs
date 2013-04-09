using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MadWizard.WinUSBNet;

namespace Communications.Appi.Winusb
{
    public class WinusbAppiDev : AppiDev
    {   
        private USBDevice Device { get; set; }
        private USBPipe ReadPipe { get; set; }
        private USBPipe WritePipe { get; set; }

        public static IList<AppiDeviceSlot> GetDevices()
        {
            return USBDevice.GetDevices(DeviceGuid).Select(di => new WinusbAppiDeviceSlot(di)).Cast<AppiDeviceSlot>().ToList();
        }


        internal WinusbAppiDev(USBDeviceInfo di)
            : base()
        {
            Device = new USBDevice(di);
            ReadPipe = Device.Pipes.First(p => p.IsIn);
            WritePipe = Device.Pipes.First(p => p.IsOut);
        }

        protected override byte[] ReadBuffer()
        {
            Byte[] buff = new Byte[BufferSize];
            ReadPipe.Read(buff);
            return buff.SkipWhile(b => b == 0).ToArray();
        }

        protected override void WriteBuffer(byte[] Buffer)
        {
            WritePipe.Write(Buffer);
        }

        public override void Dispose()
        {
            base.Dispose();
            Device.Dispose();
        }
    }
}
