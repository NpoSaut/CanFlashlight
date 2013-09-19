using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Appi.Winusb;

namespace WirelessTerminal
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var d = WinusbAppiDev.GetDevices().First().OpenDevice(true))
            {
                var p = d.WirelessPort;

                while (true)
                {
                    foreach (var b in p.ReadAll())
                        Console.Write(b.ToString("x2") + " ");
                    System.Threading.Thread.Sleep(200);
                    p.Write(new Byte[] { 0xff, 0x90, 0x70 });
                    Console.WriteLine();
                }
            }
        }
    }
}
