using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MadWizard.WinUSBNet;

namespace Communications.Appi.Winusb
{
    public class WinusbAppiDeviceSlot : AppiDeviceSlot
    {
        public USBDeviceInfo DeviceInfo { get; private set; }

        public WinusbAppiDeviceSlot(USBDeviceInfo OnUsbDeviceInfo)
        {
            this.DeviceInfo = OnUsbDeviceInfo;
        }

        public override bool IsFree
        {
            get { return true; }
        }

        protected override AppiDev InternalOpenDevice()
        {
            return new WinusbAppiDev(DeviceInfo);
        }
    }
}
