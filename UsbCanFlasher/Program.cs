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

                var h = new CanFrameHandler(0x0c28);
                d.Ports[AppiLine.Can1].Handlers.Add(h);

                d.BeginListen();

                Random r = new Random();

                System.Threading.Thread.Sleep(1500);
                
                ConsoleKey K;

                while ((K = Console.ReadKey().Key) != ConsoleKey.Escape)
                {
                    if (K == ConsoleKey.F9)
                    {
                        d.Ports[AppiLine.Can1].Send(CanFrame.NewWithDescriptor(0x1001, new Byte[] { 0x01 }));
                    }
                    else if (K == ConsoleKey.F10)
                    {
                        d.Ports[AppiLine.Can1].Send(CanFrame.NewWithDescriptor(0x1001, new Byte[] { 0x00 }));
                    }
                    else if (K == ConsoleKey.F11)
                    {
                        for (int j = 0; j < 10; j++)
                            d.Ports[AppiLine.Can1].Send(
                                Enumerable.Range(0, 100)
                                    .Select(i => CanFrame.NewWithDescriptor(0x2008, BitConverter.GetBytes((long)(i*j))))
                                    .ToList());
                        Console.WriteLine("sent");
                    }
                    else
                    {
                        Byte[] buff = new Byte[8];
                        r.NextBytes(buff);
                        d.Ports[AppiLine.Can1].Send(CanFrame.NewWithDescriptor(0x0c08, buff));
                        var dt1 = DateTime.Now;
                        var rxb = h.WaitFor();
                        var dt2 = DateTime.Now;
                        Console.Write("{0}ms ", (dt2 - dt1).TotalMilliseconds);
                        Console.WriteLine(buff.SequenceEqual(rxb.Data) ? "ok" : "error");
                    }
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