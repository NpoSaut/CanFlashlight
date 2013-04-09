using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications.Appi
{
    /// <summary>
    /// Место подключения к АППИ
    /// </summary>
    public abstract class AppiDeviceSlot
    {
        /// <summary>
        /// Проверяет, занято ли устройство
        /// </summary>
        public abstract bool IsFree { get; }

        /// <summary>
        /// Открывает АППИ
        /// </summary>
        /// <returns></returns>
        public AppiDev OpenDevice()
        {
            if (IsFree) return InternalOpenDevice();
            else throw new DeviceSlotAlreadyOpenedException(this);
        }
        protected abstract AppiDev InternalOpenDevice();
    }

    /// <summary>
    /// Исключение возникает в том случае, если слот устройства уже был открыт в другом месте
    /// </summary>
    public class DeviceSlotAlreadyOpenedException : Exception
    {
        public AppiDeviceSlot Slot { get; set; }

        public DeviceSlotAlreadyOpenedException(AppiDeviceSlot slot)
            : base(string.Format("Устройство {0} уже было открыто", slot))
        {
            this.Slot = slot;
        }
    }
}
