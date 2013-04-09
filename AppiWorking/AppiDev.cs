using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications.Appi
{
    public enum AppiLine : byte
    { 
        Can1 = 0x02,
        Can2 = 0x03
    }

    /// <summary>
    /// Представление АППИ
    /// </summary>
    public abstract class AppiDev : IDisposable
    {
        /// <summary>
        /// GUID устройства
        /// </summary>
        public const string DeviceGuid = "524cc09a-0a72-4d06-980e-afee3131196e";
        /// <summary>
        /// Размер буфера
        /// </summary>
        public const int BufferSize = 2048;

        protected abstract Byte[] ReadBuffer();
        protected abstract void WriteBuffer(Byte[] Buffer);

        public Dictionary<AppiLine, AppiCanPort> Ports { get; private set; }

        public AppiDev()
        {
            Ports = new Dictionary<AppiLine, AppiCanPort>()
            {
                { AppiLine.Can1, new AppiCanPort(AppiLine.Can1) },
                { AppiLine.Can2, new AppiCanPort(AppiLine.Can2) }
            };
        }
        public virtual void Dispose()
        {
            if (IsListening) StopListening();
        }

        /// <summary>
        /// Считывает текущее состояние и сообщения из АППИ
        /// </summary>
        /// <returns>Сообщения, полученные с момента предыдущего считывания</returns>
        public AppiMessages ReadMessages()
        {
            var buff = ReadBuffer();

            if (buff.Length != BufferSize) return AppiMessages.Empty;

            var MessagesInA = buff[6];
            var MessagesInB = buff[2];

            var messages = new AppiMessages(
                    ParseBuffer(buff, 24, MessagesInA).ToList(),
                    ParseBuffer(buff, 524, MessagesInB).ToList()
                    );

            OnMessagesRecieved(messages);

            return messages;
        }
        /// <summary>
        /// Парсит буфер сообщений АППИ
        /// </summary>
        /// <param name="Buff">Буфер сообщений</param>
        /// <param name="Offset">Отступ от начала буфера</param>
        /// <param name="Count">Количество сообщений в буфере</param>
        private IEnumerable<CanMessage> ParseBuffer(Byte[] Buff, int Offset, int Count)
        {
            Byte[] buff = new Byte[10];
            for (int i = 0; i < Count; i++)
            {
                Buffer.BlockCopy(Buff, Offset + i * buff.Length, buff, 0, buff.Length);
                yield return CanMessage.FromBufferBytes(buff);
            }
        }

        private byte SendMessageCounter = 0;
        /// <summary>
        /// Отправляет список сообщений в указанный канал
        /// </summary>
        /// <param name="Messages">Список сообщений</param>
        /// <param name="Line">Канал связи</param>
        public void SendMessages(IList<CanMessage> Messages, AppiLine Line)
        {
            Byte[] Buff = new Byte[2048];
            Buffer.SetByte(Buff, 0, 0x02);
            Buffer.SetByte(Buff, 1, (byte)Line);
            Buffer.SetByte(Buff, 3, SendMessageCounter);
            Buffer.SetByte(Buff, 3, (byte)Messages.Count);

            var MessagesBuffer = Messages.SelectMany(m => m.ToBufferBytes()).ToArray();
            Buffer.BlockCopy(MessagesBuffer, 0, Buff, 10, MessagesBuffer.Length);

            WriteBuffer(Buff);

            unchecked { SendMessageCounter++; }
        }
        /// <summary>
        /// Отправляет одно сообщение в канал
        /// </summary>
        /// <param name="Message">CAN-Сообщение</param>
        /// <param name="Line">Канал связи</param>
        public void SendMessage(CanMessage Message, AppiLine Line)
        {
            SendMessages(new List<CanMessage>() { Message }, Line);
        }

        public event AppiReceiveEventHandler AppiMessagesRecieved;

        private void OnMessagesRecieved(AppiMessages mes)
        {
            if (AppiMessagesRecieved != null) AppiMessagesRecieved(this, new AppiMessageRecieveEventArgs(mes));

            Ports[AppiLine.Can1].OnMessagesRecieved(mes.ChannelA);
            Ports[AppiLine.Can2].OnMessagesRecieved(mes.ChannelB);
        }

        /// <summary>
        /// Признак действия режима прослушивания линии
        /// </summary>
        public bool IsListening { get; private set; }
        private System.Threading.Thread ListeningThread;
        /// <summary>
        /// Начать прослушивание линии
        /// </summary>
        /// <remarks>Запускает отдельный поток для прослушивания линии</remarks>
        public void BeginListen()
        {
            if (!IsListening)
            {
                ListeningThread = new System.Threading.Thread(ListeningLoop);
                IsListening = true;
                ListeningThread.Start();
            }
        }
        /// <summary>
        /// Петля прослушивания линии
        /// </summary>
        private void ListeningLoop()
        {
            while (IsListening)
            {
                this.ReadMessages();
            }
        }
        /// <summary>
        /// Остановить прослушивание линии
        /// </summary>
        public void StopListening()
        {
            if (IsListening)
            {
                IsListening = false;
                ListeningThread.Abort();
            }
        }
    }


    public delegate void AppiReceiveEventHandler(object sender, AppiMessageRecieveEventArgs e);
    public class AppiMessageRecieveEventArgs : EventArgs
    {
        public AppiMessages Messages { get; set; }

        public AppiMessageRecieveEventArgs(AppiMessages Messages)
        {
            this.Messages = Messages;
        }
    }
}
