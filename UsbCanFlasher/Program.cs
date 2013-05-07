using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MadWizard.WinUSBNet;
using System.IO;
using Communications.Appi;
using Communications.Can;

namespace UsbCanFlasher
{
    class Program
    {
        static void Main(string[] args)
        {
            var ds = Communications.Appi.Winusb.WinusbAppiDev.GetDevices().First();
            using (var d = ds.OpenDevice(true))
            {
                using (var b = new CanFramesBuffer(0x1048, d.Ports.Values))
                {
                    foreach (var f in b.Read(TimeSpan.FromMilliseconds(1500)))
                        Console.WriteLine(f);
                }
                Console.WriteLine("----------------------");
                Console.Read();
            }
        }
    }
}