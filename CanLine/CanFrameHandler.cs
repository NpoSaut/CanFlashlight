using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections.ObjectModel;

namespace Communications.Can
{
    /// <summary>
    /// Хэндлер CAN-фреймов
    /// </summary>
    /// <remarks>
    /// Используется для оптимизации обработки CAN-сообщений: позволяет реагировать только на сообщения с определённым дескриптором
    /// </remarks>
    public class CanFrameHandler : IDisposable
    {
        /// <summary>
        /// Отлавливаемый дескриптор
        /// </summary>
        public int Descriptor { get; private set; }
        /// <summary>
        /// Прослушиваемый порт
        /// </summary>
        public CanPort Port { get; private set; }

        /// <summary>
        /// Событие, возникающее при приёме сообщений с заданным дескриптором
        /// </summary>
        public event CanFramesReceiveEventHandler Recieved;

        /// <summary>
        /// Устанавливает отслеживание сообщений с заданным дескприптором по указанному порту
        /// </summary>
        /// <param name="Port">Прослушиваемый порт</param>
        /// <param name="Descriptor">Отлавливаемый дескриптор</param>
        public CanFrameHandler(CanPort Port, int Descriptor)
        {
            this.Port = Port;
            this.Descriptor = Descriptor;

            Port.Handle(this);
        }

        /// <summary>
        /// Инициирует обработку событий приёма сообщений
        /// </summary>
        /// <param name="Frames">Принятые кадры</param>
        internal void OnRecieved(IList<CanFrame> Frames)
        {
            if (Recieved != null)
                Recieved(this, new CanFramesReceiveEventArgs(Frames, Port));
        }

        public void Dispose()
        {
            Port.Unandle(this);
        }
    }
}
