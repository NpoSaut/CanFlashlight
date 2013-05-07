using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;

namespace Communications.Can
{
    /// <summary>
    /// Класс, позволяющий читать сообщения в однопоточном режиме
    /// </summary>
    /// <remarks>
    /// Этот класс организует буфер для приёма сообщений с заданным дескриптором с возможностью блокирующего чтения из этого буфера
    /// </remarks>
    public class CanFramesBuffer : IDisposable
    {
        /// <summary>
        /// Отслеживаемый дескриптор
        /// </summary>
        public int Descriptor { get; private set; }
        /// <summary>
        /// Порты, сообщения с которых поступают в буфер
        /// </summary>
        public IList<CanPort> Ports { get { return Handlers.Keys.ToList(); } }
        private Dictionary<CanPort, CanFrameHandler> Handlers = new Dictionary<CanPort, CanFrameHandler>();

        /// <summary>
        /// Создаёт буфер для отлова сообщений с указанным дескриптором на указанном порту
        /// </summary>
        /// <param name="Descriptor">Отлавливаемый дескриптор</param>
        /// <param name="OnPort"></param>
        public CanFramesBuffer(int Descriptor, CanPort OnPort)
            : this(Descriptor, new CanPort[] { OnPort })
        { }
        public CanFramesBuffer(int Descriptor, IEnumerable<CanPort> OnPorts)
        {
            this.Descriptor = Descriptor;
            foreach (var p in OnPorts) RegisterPort(p);
        }

        /// <summary>
        /// Добавляет порт в список прослушиваемых
        /// </summary>
        public void RegisterPort(CanPort Port)
        {
            if (!Handlers.ContainsKey(Port))
            {
                var h = new CanFrameHandler(Port, Descriptor);
                h.Recieved += new CanFramesReceiveEventHandler(Handler_FrameRecieved);
                Handlers.Add(Port, h);
            }
        }

        /// <summary>
        /// Убирает порт из списка прослушиваемых
        /// </summary>
        public void UnregisterPort(CanPort Port)
        {
            if (Handlers.ContainsKey(Port))
            {
                Handlers[Port].Dispose();
                Handlers.Remove(Port);
            }
        }

        public void Dispose()
        {
            foreach (var h in Handlers.Values)
                h.Dispose();
        }

        
        private ConcurrentQueue<CanFrame> FramesBuffer = new ConcurrentQueue<CanFrame>();

        void Handler_FrameRecieved(object sender, CanFramesReceiveEventArgs e)
        {
            foreach (var f in e.Frames)
                FramesBuffer.Enqueue(f);
        }

        /// <summary>
        /// Выполняет блокирующее ожидание фрейма
        /// </summary>
        /// <returns>Найденный фрейм</returns>
        public CanFrame ReadFrame()
        {
            return ReadFrame(TimeSpan.FromMilliseconds(-1));
        }
        public CanFrame ReadFrame(TimeSpan Timeout)
        {
            CanFrame f = CanFrame.NewWithDescriptor(0);
            bool ok = false;
            SpinWait.SpinUntil(() => ok = FramesBuffer.TryDequeue(out f), Timeout);
            if (ok) return f;
            else throw new TimeoutException("Превышено время ожидания пакета");
        }
        /// <summary>
        /// Блокирующе считывает последовательность всех фреймов, приходящих в данный буфер
        /// </summary>
        /// <returns>Последовательность отловленных кадров</returns>
        /// <remarks>
        /// Поскольку функция блокирующая и считывает _все_ приходящие кадры, выход из этого перечисления не предусмотрен.
        /// Для правильной работы следует пользоваться лишь методами вроде методов, извлекающих N первых членов последовательности (First, Take, TakeWhile, ...)
        /// </remarks>
        public IEnumerable<CanFrame> Read()
        {
            while (true)
                yield return ReadFrame();
        }
        /// <summary>
        /// Блокирующе считывает последовательность всех фреймов, приходящих в данный буфер до тех пор, пока в течение указанного интервала приходит хоть одно сообщение
        /// </summary>
        /// <param name="Timeout">Время, по истечении которого чтение следующих пакетов прекратится</param>
        /// <param name="ThrowExceptionOnTimeout">Следует ли пробрасывать TimeoutException по истечении таймаута (если нет - то чтение пакетов просто прекратится)</param>
        /// <returns>Последовательность отловленных кадров</returns>
        /// <remarks>
        /// Можно использовать для отлова серий подряд идущих сообщений (когда другая серия отделяется по времени).
        /// Параметр ThrowExceptionOnTimeout следует использовать, если сообщение гарантированно должно поступить и его задержка является признаком ошибки
        /// </remarks>
        public IEnumerable<CanFrame> Read(TimeSpan Timeout, Boolean ThrowExceptionOnTimeout = false)
        {
            while (true)
            {
                CanFrame f;

                try
                {
                    f = ReadFrame(Timeout);
                }
                catch (TimeoutException)
                {
                    if (ThrowExceptionOnTimeout) throw;
                    else yield break;
                }

                yield return f;
            }
        }
    }
}
