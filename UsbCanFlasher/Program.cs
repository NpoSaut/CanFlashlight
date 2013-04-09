using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MadWizard.WinUSBNet;
using System.IO;
using Communications.Appi;

namespace UsbCanFlasher
{
    class Program
    {
        static void Main(string[] args)
        {
            var ds = Communications.Appi.Winusb.WinusbAppiDev.GetDevices().First();
            var d = ds.OpenDevice();

            System.Threading.Thread.Sleep(50);

            d.SendMessage(CanMessage.NewWithDescriptor(0x1001, new Byte[] { 0x00 }), AppiLine.Can1);

            Random r = new Random();

            while (true)
            {
                //d.SendMessage(CanMessage.NewWithDescriptor(0x0c08, new Byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 }), AppiLine.Can1);
                
                Byte[] buff = new Byte[8];
                r.NextBytes(buff);
                d.SendMessage(CanMessage.NewWithDescriptor(0x0c08, buff), AppiLine.Can1);

                System.Threading.Thread.Sleep(50);
                foreach (var m in d.ReadMessages())
                {
                    if (m.Descriptor == 0x1048) Console.ForegroundColor = ConsoleColor.DarkGray;
                    if (m.Descriptor == 0x0c08) Console.ForegroundColor = ConsoleColor.Cyan;
                    if (m.Descriptor == 0x0c28) Console.ForegroundColor = ConsoleColor.Yellow;

                    Console.WriteLine(m);
                    Console.ResetColor();
                }
                System.Threading.Thread.Sleep(500);
            }
        }
    }
}