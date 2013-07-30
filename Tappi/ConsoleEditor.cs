using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Tappi
{
    public class ConsoleEditor
    {
        public string Text { get; set; }
        private readonly int CursorStartPositionLeft;
        public int CursorPositionLeft { get; set; }

        private ConsoleEditor(string StartString)
        {
            Text = StartString;
            CursorStartPositionLeft = Console.CursorLeft;
            CursorPositionLeft = CursorStartPositionLeft + Text.Length;
        }

        private string prewPrintText;
        protected virtual void Print()
        {
            if (prewPrintText == null)
            {
                SubPrint(0, Text.Length);
            }
            else
            {
                int offset;
                for (offset = 0; offset < Math.Min(prewPrintText.Length, Text.Length); offset++)
                { if (prewPrintText[offset] != Text[offset]) break; }

                int tail;
                for (tail = 0; tail < Math.Min(prewPrintText.Length - offset, Text.Length - offset); tail++)
                { if (prewPrintText[prewPrintText.Length - tail - 1] != Text[Text.Length - tail - 1]) break; }

                if (tail > 0)
                    Console.MoveBufferArea(CursorStartPositionLeft + prewPrintText.Length - tail, Console.CursorTop, tail, 1, CursorStartPositionLeft + Text.Length - tail, Console.CursorTop);

                if (Text.Length < prewPrintText.Length)
                {
                    Console.CursorLeft = Text.Length;
                    Console.Write(new string(Enumerable.Repeat('\0', prewPrintText.Length - Text.Length).ToArray()));
                }

                if (Text.Length - tail - offset > 0)
                    SubPrint(offset, Text.Length - tail - offset);
            }
            prewPrintText = Text;

            Console.CursorLeft = CursorPositionLeft;
        }
        protected virtual void SubPrint(int offset, int count)
        {
            Console.CursorLeft = CursorStartPositionLeft + offset;
            for (int i = 0; i < count; i++)
            {
                if (i < Text.Length)
                {
                    Console.Write(Text[offset + i]);
                }
                else
                    Console.Write('\0');
            }
        }

        public void Read()
        {
            Print();
            ConsoleKeyInfo k;
            while ((k = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                switch(k.Key)
                {
                    case ConsoleKey.LeftArrow:
                        if (CursorPositionLeft > 0)
                            CursorPositionLeft--;
                        break;
                    case ConsoleKey.RightArrow:
                        if (CursorPositionLeft < Text.Length)
                            CursorPositionLeft++;
                        break;
                    case ConsoleKey.Backspace:
                        if (CursorPositionLeft > 0)
                        {
                            Text = Text.Remove(CursorPositionLeft-1, 1);
                            CursorPositionLeft--;
                        }
                        break;
                    case ConsoleKey.Delete:
                        if (CursorPositionLeft < Text.Length)
                        {
                            Text = Text.Remove(CursorPositionLeft, 1);
                        }
                        break;
                    case ConsoleKey.Home:
                        CursorPositionLeft = 0;
                        break;
                    case ConsoleKey.End:
                        CursorPositionLeft = Text.Length - 1;
                        break;
                }
                if (!char.IsControl(k.KeyChar))
                {
                    Text = Text.Insert(CursorPositionLeft, k.KeyChar.ToString());
                    CursorPositionLeft++;
                }
                Print();
            }
        }


        internal static string ReadString(string DefaultString)
        {
            var e = new ConsoleEditor(DefaultString);
            e.Read();
            return e.Text;
        }
    }
}
