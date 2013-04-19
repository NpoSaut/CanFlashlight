using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace Communications.Can
{
    /// <summary>
    /// Абстракция CAN-порта
    /// </summary>
    public abstract class CanPort
    {
        /// <summary>
        /// Имя порта
        /// </summary>
        public String Name { get; set; }
        /// <summary>
        /// Событие приёма сообщения по линии
        /// </summary>
        public event CanFramesReceiveEventHandler Recieved;

        protected CanPort(String PortName)
        {
            this.Name = PortName;
            Handlers = new ConcurrentBag<CanFrameHandler>();
        }

        #region Отправка сообщений
        /// <summary>
        /// Отправка нескольких фреймов в линию
        /// </summary>
        /// <param name="Frames">Фреймы для отправки</param>
        public abstract void Send(IList<CanFrame> Frames);
        /// <summary>
        /// Отправка одного фрейма в линию
        /// </summary>
        /// <param name="Frame">Фрейм для отправки</param>
        public void Send(CanFrame Frame)
        {
            Send(new List<CanFrame>() { Frame });
        } 
        #endregion

        /// <summary>
        /// Обработка принятых фреймов
        /// </summary>
        /// <param name="Frames">Принятые фреймы</param>
        protected void OnFramesRecieved(IList<CanFrame> Frames)
        {
            if (Frames.Any() && Recieved != null) Recieved(this, new CanFramesReceiveEventArgs(Frames, this));

            foreach (var d in Frames.GroupBy(f => f.Descriptor))
                foreach (var h in Handlers.Where(hh => hh.Descriptor == d.Key))
                    h.OnRecieved(d.ToList(), this);
        }

        public ConcurrentBag<CanFrameHandler> Handlers { get; private set; }
    }   
    
    public delegate void CanFramesReceiveEventHandler(object sender, CanFramesReceiveEventArgs e);
    public class CanFramesReceiveEventArgs : EventArgs
    {
        public CanPort Port { get; set; }
        public IList<CanFrame> Frames { get; set; }

        public CanFramesReceiveEventArgs(IList<CanFrame> Frames, CanPort Port)
        {
            this.Frames = Frames;
        }
    }
}
