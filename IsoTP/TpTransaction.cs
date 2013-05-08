using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;

namespace Communications.Protocols.IsoTP
{
    public enum TpTransactionStatus { Ready, Active, Done, Error }
    /// <summary>
    /// Представляет абстракцию ISO-TP-транзакции
    /// </summary>
    public abstract class TpTransaction
    {
        /// <summary>
        /// Используемый порт
        /// </summary>
        public CanPort Port { get; private set; }
        /// <summary>
        /// Используемый дескриптор
        /// </summary>
        public int Descriptor { get; private set; }
        /// <summary>
        /// Время ожидания пакета
        /// </summary>
        public TimeSpan Timeout { get; set; }

        private TpTransactionStatus _Status;
        public event EventHandler StatusChanged;
        /// <summary>
        /// Статус транзакции
        /// </summary>
        public TpTransactionStatus Status
        {
            get { return _Status; }
            set
            {
                if (_Status != value)
                {
                    _Status = value;
                    if (StatusChanged != null) StatusChanged(this, new EventArgs());
                }
            }
        }

        public TpTransaction(CanPort Port, int Descriptor)
        {
            this.Descriptor = Descriptor;
            this.Port = Port;
            this.Timeout = TimeSpan.FromSeconds(3);
            this.Status = TpTransactionStatus.Ready;
        }
    }
}
