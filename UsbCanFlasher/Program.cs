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
            using (var d = ds.OpenDevice())
            {
                foreach (var p in d.Ports.Values)
                    p.Recieved += new CanFramesReceiveEventHandler(Value_Recieved);

                d.BeginListen();

                Random r = new Random();

                System.Threading.Thread.Sleep(1500);
                while (Console.ReadKey().Key != ConsoleKey.Escape)
                {
                    Byte[] buff = new Byte[8];
                    r.NextBytes(buff);
                    d.Ports[AppiLine.Can1].Send(CanFrame.NewWithDescriptor(0x0c08, buff));
                }
            }
        }

        static void Value_Recieved(object sender, CanFramesReceiveEventArgs e)
        {
            foreach (var m in e.Frames)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write((sender as CanPort).Name.ToUpper());
                Console.ResetColor();
                Console.Write(" ");

                if (m.Descriptor == 0x1048) Console.ForegroundColor = ConsoleColor.DarkGray;
                if (m.Descriptor == 0x0c08) Console.ForegroundColor = ConsoleColor.Cyan;
                if (m.Descriptor == 0x0c28) Console.ForegroundColor = ConsoleColor.Yellow;

                Console.WriteLine(m);
                Console.ResetColor();
            }
        }
    }
}