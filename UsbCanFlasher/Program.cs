using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MadWizard.WinUSBNet;
using System.IO;
using Communications.Appi;
using Communications.Can;
using Communications.Protocols.IsoTP;

namespace UsbCanFlasher
{
    class Program
    {
        static void Main(string[] args)
        {
            var ds = Communications.Appi.Winusb.WinusbAppiDev.GetDevices().First();
            using (var d = ds.OpenDevice(true))
            {
                d.Ports[AppiLine.Can1].Recieved += new CanFramesReceiveEventHandler(Program_Recieved);

                TpRecieveTransaction rt = new TpRecieveTransaction(d.Ports[AppiLine.Can1], 0x3008) { SeparationTime = TimeSpan.FromMilliseconds(50) };

                var tt = System.Threading.Tasks.Task<byte[]>.Factory.StartNew(rt.Recieve);

                Byte[] data = new Byte[4095];
                (new Random()).NextBytes(data);

                Console.WriteLine("Подготавливаемся");
                TpPacket sp = new TpPacket(data);
                TpSendTransaction st = new TpSendTransaction(d.Ports[AppiLine.Can1], 0x3008);
                Console.WriteLine("Начинаем отправку");
                st.Send(sp);

                tt.Wait();
                var xxx = tt.Result;
                if (xxx.SequenceEqual(data)) Console.WriteLine("Yeeeaaaaahhhhhh!! =)");
                else Console.WriteLine("=(");

                Console.WriteLine("----------------------");

                Console.Read();
            }
        }

        static void Program_Recieved(object sender, CanFramesReceiveEventArgs e)
        {
            foreach (var f in e.Frames.Where(f => f.Descriptor == 0x3008))
                Console.WriteLine("{1} {0}", f, f.IsLoopback ? "*" : " ");
        }
    }
}