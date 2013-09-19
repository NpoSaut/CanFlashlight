using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;
using System.Collections.ObjectModel;
using BlokFrames;
using System.ComponentModel;

namespace Tappi
{
    class Program
    {
        public static Dictionary<int, ConsoleColor> Colors { get; set; }
        public static List<CanPort> Ports { get; set; }
        public static List<FilterModel> Filters { get; set; }

        private static bool quitRequested = false;
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Text APPI");
            var ds = Communications.Appi.Winusb.WinusbAppiDev.GetDevices().First();

            Colors = new Dictionary<int, ConsoleColor>()
            {
                { 0x0A08, ConsoleColor.Yellow },
                { 0x1888, ConsoleColor.Green }
            };
            Filters = new List<FilterModel>() { new FilterModel(0x1888) };

            using (var d = ds.OpenDevice(true))
            {
                Ports = new List<CanPort>(d.CanPorts.Values);


                foreach (var p in Ports) RegisterPort(p);

                InitializeKeyBindings();
                while (!quitRequested)
                {
                    ReadInput();
                }
            }
        }

        private static void RegisterPort(CanPort p)
        {
            p.Recieved += new CanFramesReceiveEventHandler(CanFrame_Recieved);
        }

        static void CanFrame_Recieved(object sender, CanFramesReceiveEventArgs e)
        {
            foreach (var f in e.Frames)
                PrintFrame(f);
        }

        private static void PrintFrame(CanFrame f)
        {
            lock (InputLocker)
            {
                if (f.Descriptor != 0x1888) return;

                if (IsInput) return;
                var hl = Colors.ContainsKey(f.Descriptor) ? Colors[f.Descriptor] : ConsoleColor.White;
                Console.Write("{0:HH:mm:ss.fff} ", f.Time);
                Console.ForegroundColor = Console.BackgroundColor;
                Console.BackgroundColor = hl;
                Console.Write("{0:X4}", f.Descriptor);
                Console.ResetColor();
                Console.ForegroundColor = hl;
                Console.Write(" ");
                Console.Write(string.Join(" ", f.Data.Select(b => b.ToString("X2"))));
                Console.WriteLine();
                Console.ResetColor();

                if (f.Descriptor == 0x1888)
                {
                    var F = BlokFrame.GetBlokFrame(f);
                    var vals = F.GetType().GetProperties().Select(p =>
                        new
                        {
                            p.Name,
                            da = ((DescriptionAttribute)DescriptionAttribute.GetCustomAttribute(p, typeof(DescriptionAttribute))),
                            val = p.GetValue(F, null)
                        })
                        .Where(v => v.da != null);
                    foreach (var v in vals)
                        Console.WriteLine("   {0} = {1}", v.da.Description, v.val);
                }


                Console.WriteLine();
            }
        }

        private static object InputLocker = new object();
        private static bool IsInput = false;
        private static void ReadInput()
        {
            var k = Console.ReadKey(false);
            lock (InputLocker) IsInput = true;
            if (KeyBindings.ContainsKey(k))
                KeyBindings[k]();
            lock (InputLocker) IsInput = false;
        }

        private static Dictionary<ConsoleKeyInfo, Action> KeyBindings { get; set; }
        private static void InitializeKeyBindings()
        {
            KeyBindings = new Dictionary<ConsoleKeyInfo, Action>()
            {
                { new ConsoleKeyInfo('\0', ConsoleKey.F2, false, false, false), EditFilters },
                { new ConsoleKeyInfo('\0', ConsoleKey.F10, false, false, false), () => quitRequested = true }
            };
        }

        private static void EditFilters()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Редактирование фильтров:");
            string s = ConsoleEditor.ReadString(string.Join(" ", Filters.Select(f => f.Descriptor.ToString("x4"))));
        }
    }
}
