using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;

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
                { AppiLine.Can1, new AppiCanPort(this, AppiLine.Can1) },
                { AppiLine.Can2, new AppiCanPort(this, AppiLine.Can2) }
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
        private IEnumerable<CanFrame> ParseBuffer(Byte[] Buff, int Offset, int Count)
        {
            Byte[] buff = new Byte[10];
            for (int i = 0; i < Count; i++)
            {
                Buffer.BlockCopy(Buff, Offset + i * buff.Length, buff, 0, buff.Length);
                yield return AppiCanFrameConstructor.FromBufferBytes(buff);
            }
        }

        private byte SendMessageCounter = 0;
        /// <summary>
        /// Отправляет список сообщений в указанный канал
        /// </summary>
        /// <param name="Frames">Список сообщений</param>
        /// <param name="Line">Канал связи</param>
        internal void SendFrames(IList<CanFrame> Frames, AppiLine Line)
        {
            Byte[] Buff = new Byte[2048];
            Buffer.SetByte(Buff, 0, 0x02);
            Buffer.SetByte(Buff, 1, (byte)Line);
            Buffer.SetByte(Buff, 3, SendMessageCounter);
            Buffer.SetByte(Buff, 3, (byte)Frames.Count);

            var MessagesBuffer = Frames.SelectMany(m => m.ToBufferBytes()).ToArray();
            Buffer.BlockCopy(MessagesBuffer, 0, Buff, 10, MessagesBuffer.Length);

            WriteBuffer(Buff);

            unchecked { SendMessageCounter++; }
        }
        /// <summary>
        /// Отправляет одно сообщение в канал
        /// </summary>
        /// <param name="Frame">CAN-Сообщение</param>
        /// <param name="Line">Канал связи</param>
        internal void SendFrame(CanFrame Frame, AppiLine Line)
        {
            SendFrames(new List<CanFrame>() { Frame }, Line);
        }

        public event AppiReceiveEventHandler AppiMessagesRecieved;

        private void OnMessagesRecieved(AppiMessages mes)
        {
            if (AppiMessagesRecieved != null) AppiMessagesRecieved(this, new AppiMessageRecieveEventArgs(mes));

            Ports[AppiLine.Can1].OnAppiFramesRecieved(mes.ChannelA);
            Ports[AppiLine.Can2].OnAppiFramesRecieved(mes.ChannelB);
        }

        /// <summary>
        /// Признак действия режима прослушивания линии
        /// </summary>
        public bool IsListening { get; private set; }
        private object IsListeningSynchronizingObject = new object();
        private System.Threading.Thread ListeningThread;
        /// <summary>
        /// Начать прослушивание линии
        /// </summary>
        /// <remarks>Запускает отдельный поток для прослушивания линии</remarks>
        public void BeginListen()
        {
            lock (IsListeningSynchronizingObject)
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
            while (true)
            {
                lock (IsListeningSynchronizingObject)
                {
                    if (!IsListening) break;
                    else this.ReadMessages();
                }
            }
        }
        /// <summary>
        /// Остановить прослушивание линии
        /// </summary>
        public void StopListening()
        {
            lock (IsListeningSynchronizingObject)
                if (IsListening)
                {
                    IsListening = false;
                    //ListeningThread.Abort();
                }
        }

        /// <summary>
        /// Возникает при отключении устройства
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// События при отключении устройства.
        /// </summary>
        protected virtual void OnDisconnected()
        {
            if (Disconnected != null) Disconnected(this, new EventArgs());
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

    internal static class AppiCanFrameConstructor
    {
        /// <summary>
        /// Собирает байты для буфера АППИ
        /// </summary>
        /// <returns>10 байт: 2 байта дескриптор + 8 байт данных</returns>
        public static Byte[] ToBufferBytes(this CanFrame Frame)
        {
            Byte[] buff = new Byte[10];

            BitConverter.GetBytes((UInt16)Frame.Descriptor).Reverse().ToArray().CopyTo(buff, 0);
            Frame.Data.CopyTo(buff, 2);

            return buff;
        }

        /// <summary>
        /// Восстанавливает CAN-сообщение из буфера АППИ
        /// </summary>
        /// <param name="Buff">10 байт буфера</param>
        public static CanFrame FromBufferBytes(Byte[] Buff)
        {
            int id = (int)BitConverter.ToUInt16(Buff.Take(2).Reverse().ToArray(), 0) >> 4;
            int len = Buff[1] & 0x0f;
            return CanFrame.NewWithId(id, Buff, 2, len);
        }
    }
}
