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

                #region Текст
                String message = @"Джим Землин (Jim Zemlin), исполнительный директор Linux Foundation, наверное, один из тех немногих людей, кто был в гуще событий при появлении и развитии Linux, крупнейшей совместной разработки в истории вычислительной техники. Он понимает, что скорость инноваций и качество разработки ПО диктуется теми, кто смотрит вперёд и работает в сотрудничестве друг с другом. Вот почему он был пригашен на недавний TEDx, с рассказом о том, чему ИТ-индустрия научилась у Linuх и особенно у основателя этого проекта Линуса Торвальдса (Linus Torvalds).";
                message += @"Месяц назад, когда цена биткоина достигла 250 долларов, а затем упала до 50, у меня появилось желание поучаствовать в этом веселье, написав торгового бота, который бы зарабатывал на подобных изменениях.

Выяснилось, что две наиболее популярные биржи, на которых торгуют биткоинами — это MtGox и BTC-e. Я положил деньги на одну из них и принялся думать, как предсказать изменение цены, а также, как это автоматизировать. Дело осложнялось тем, что на этих биржах можно покупать и продавать только на свои средства, поэтому нельзя играть на понижение, занимая короткую позицию, потому что, как говорил Матроскин: «Чтобы продать что-нибудь ненужное, нужно сначала купить что-нибудь ненужное». ";
                message += @"Сегодня, 11 мая 2013 года, в 01:41:39.8 UTC (05:41:39.8 московского времени) в русском разделе Википедии появилась миллионная статья. По случайному совпадению, русский раздел именно сегодня отмечает своё 11-летие. Статью Life Extension Foundation создал участник UG72. Уже разгорелись споры о том, имеет ли статья право на жизнь, но тот факт, что рубеж взяла именно она, установлен однозначно.

Счётчик статей в Википедии показывает количество статей, в которых есть хотя бы одна ссылка (существуют и две другие настройки правил). Таким образом, на его значение может повлиять как создание и удаление статей, так и переименование и даже любая правка. Добавим к этому то, что в преддверии юбилея участники начинают массово заливать свои заготовки в надежде, что одна из них всё-таки окажется юбилейной статьёй, и то, что счётчик, как не очень важная обычно вещь, обновляется асинхронно. В итоге вычислить нужную статью становится очень непросто. Но всем же интересно!

Выкрутиться всё-таки можно"; 
                #endregion


                Byte[] data = Encoding.UTF8.GetBytes(message).Take(4095).ToArray();

                Console.WriteLine("Поехали");
                try
                {
                    //TpRecieveTransaction rt = new TpRecieveTransaction(d.Ports[AppiLine.Can1], 0x3008, 0x4008)
                    //{
                    //    //SeparationTime = TimeSpan.FromMilliseconds(1),
                    //    Timeout = TimeSpan.FromMilliseconds(-1),
                    //    BlockSize = 1
                    //};
                    //var xxx = rt.Recieve();

                    //var tt = System.Threading.Tasks.Task<byte[]>.Factory.StartNew(rt.Recieve);

                    //Byte[] data = new Byte[4095];
                    //(new Random()).NextBytes(data);

                    TpPacket sp = new TpPacket(data);
                    TpSendTransaction st = new TpSendTransaction(d.Ports[AppiLine.Can1], 0x3008, 0x4008);
                    Console.WriteLine("Начинаем отправку");
                    st.Send(sp);

                    //tt.Wait();
                    //var xxx = tt.Result;

                    //Console.WriteLine("Приняли немножко текста: {0}", Encoding.UTF8.GetString(xxx));
                    //using (var f = File.Create("rcv.txt"))
                    //    f.Write(xxx, 0, xxx.Length);

                    //if (xxx.SequenceEqual(data)) Console.WriteLine("Yeeeaaaaahhhhhh!! =)");
                    //else Console.WriteLine("=(");

                    Console.WriteLine("");
                    Console.WriteLine("{0} байт принято", data.Length);
                }
                catch (Exception exc)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(exc.Message);
                    Console.ResetColor();
                }

                Console.WriteLine("----------------------");

                Console.Read();
            }
        }

        private static Dictionary<int, ConsoleColor> hls = new Dictionary<int, ConsoleColor>()
        {
            { 0x3008, ConsoleColor.Cyan },
            { 0x4008, ConsoleColor.Yellow }
        };

        static DateTime ddd = DateTime.Now;
        static void Program_Recieved(object sender, CanFramesReceiveEventArgs e)
        {
            //foreach (var f in e.Frames.Where(f => hls.ContainsKey(f.Descriptor)))
            foreach (var f in e.Frames)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write("{0}мс ", (DateTime.Now - ddd).TotalMilliseconds.ToString("F0").PadLeft(3));

                Console.ForegroundColor =
                    hls.ContainsKey(f.Descriptor) ?
                        hls[f.Descriptor] :
                        ConsoleColor.Gray;

                Console.WriteLine("{1} {0}", f, f.IsLoopback ? "*" : " ");
                ddd = DateTime.Now;
                Console.ResetColor();
            }
        }
    }
}