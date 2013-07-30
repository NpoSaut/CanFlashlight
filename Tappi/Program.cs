using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;
using System.Collections.ObjectModel;

namespace Tappi
{
    class Program
    {
        public static Dictionary<int, ConsoleColor> Colors { get; set; }
        public static List<CanPort> Ports { get; set; }
        public static List<FilterModel> Filters { get; set; }

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
                Ports = new List<CanPort>(d.Ports.Values);


                foreach (var p in Ports) RegisterPort(p);

                InitializeKeyBindings();
                while (true)
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
        }

        private static bool IsInput = false;
        private static void ReadInput()
        {
            var k = Console.ReadKey(false);
            IsInput = true;
            if (KeyBindings.ContainsKey(k))
                KeyBindings[k]();
            IsInput = false;
        }

        private static Dictionary<ConsoleKeyInfo, Action> KeyBindings { get; set; }
        private static void InitializeKeyBindings()
        {
            KeyBindings = new Dictionary<ConsoleKeyInfo, Action>()
            {
                { new ConsoleKeyInfo('\0', ConsoleKey.F2, false, false, false), EditFilters }
            };
        }

        private static void EditFilters()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Редактирование фильтров:");
            string s = ConsoleEditor.ReadString(string.Join(" ", Filters.Select(f => f.Descriptor.ToString("x4"))));
            Console.ReadLine();
        }
    }
}
