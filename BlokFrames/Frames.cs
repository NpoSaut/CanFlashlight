using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlokFrames
{
    [FrameDescriptor(0x0803)]
    /// <summary>
    /// Сообщение формируется субмодулем B модуля МЦО нулевого комплекта каждые 500
    /// <summary>
    public class McoMode : BlokFrame
    {
        ///<summary>Режим движения</summary>
        public enum DriveModeKind : int
        {
            [System.ComponentModel.Description("Поездной")]
            ///<summary>Поездной</summary>
            TrainMode = 0,
            [System.ComponentModel.Description("Маневровый")]
            ///<summary>Маневровый</summary>
            ShuntingMode = 1,
            [System.ComponentModel.Description("Рабочий (Для КЛУБ-УП)")]
            ///<summary>Рабочий (Для КЛУБ-УП)</summary>
            WorkingMode = 2,
            [System.ComponentModel.Description("Двойная тяга")]
            ///<summary>Двойная тяга</summary>
            DoublePowerMode = 3,
        }
        
        [System.ComponentModel.Description("Режим движения")]
        /// <summary>Режим движения</summary>
        public DriveModeKind DriveMode { get; set; }
        
        ///<summary>Автоблокировочный режим движения</summary>
        public enum AutolockModeKind : int
        {
            [System.ComponentModel.Description("Обычное движение")]
            ///<summary>Обычное движение</summary>
            Normal = 0,
            [System.ComponentModel.Description("Движение по ПАБу")]
            ///<summary>Движение по ПАБу</summary>
            HalfAutolock = 1,
            [System.ComponentModel.Description("Движение по закрытой АБ")]
            ///<summary>Движение по закрытой АБ</summary>
            Autolock = 2,
            [System.ComponentModel.Description("Запрещённая комбинация")]
            ///<summary>Запрещённая комбинация</summary>
            Forbiddern = 3,
        }
        
        [System.ComponentModel.Description("Автоблокировочный режим движения")]
        /// <summary>Автоблокировочный режим движения</summary>
        public AutolockModeKind AutolockMode { get; set; }
        
        [System.ComponentModel.Description("Система многих единиц")]
        /// <summary>Система многих единиц</summary>
        public Boolean MultipleUnit { get; set; }
        
        [System.ComponentModel.Description("Диагностика САУТа")]
        /// <summary>Диагностика САУТа</summary>
        public Boolean SautDiag { get; set; }
        
        [System.ComponentModel.Description("Скорость движения по ПАБу или ЗАБу")]
        /// <summary>Скорость движения по ПАБу или ЗАБу</summary>
        public Byte RestrictedSpeed { get; set; }
        
        [System.ComponentModel.Description("Режим движения с подвижными блок-участками")]
        /// <summary>Режим движения с подвижными блок-участками</summary>
        public Boolean SlippingSections { get; set; }
        
        
        protected override void Decode(byte[] buff)
        {
            this.DriveMode = DecodeDriveMode(buff);
            this.AutolockMode = DecodeAutolockMode(buff);
            this.MultipleUnit = DecodeMultipleUnit(buff);
            this.SautDiag = DecodeSautDiag(buff);
            this.RestrictedSpeed = DecodeRestrictedSpeed(buff);
            this.SlippingSections = DecodeSlippingSections(buff);
        }
        
        protected override byte[] Encode()
        {
            var buff = new Byte[FrameLength];
            EncodeDriveMode(buff, DriveMode);
            EncodeAutolockMode(buff, AutolockMode);
            EncodeMultipleUnit(buff, MultipleUnit);
            EncodeSautDiag(buff, SautDiag);
            EncodeRestrictedSpeed(buff, RestrictedSpeed);
            EncodeSlippingSections(buff, SlippingSections);
            return buff;
        }
        
        private DriveModeKind DecodeDriveMode(Byte[] buff)
        {
            int raw = ((buff[0] & 0x30) >> 4);
            return (DriveModeKind)raw;
        }
        
        private void EncodeDriveMode(Byte[] buff, DriveModeKind value)
        {
            buff[0] = (byte)((buff[0] & ~0x30) | ((int)value & 0x30));
        }
        
        private AutolockModeKind DecodeAutolockMode(Byte[] buff)
        {
            int raw = ((buff[0] & 0x0c) >> 2);
            return (AutolockModeKind)raw;
        }
        
        private void EncodeAutolockMode(Byte[] buff, AutolockModeKind value)
        {
            buff[0] = (byte)((buff[0] & ~0x0c) | ((int)value & 0x0c));
        }
        
        private Boolean DecodeMultipleUnit(Byte[] buff)
        {
            int raw = ((buff[0] & 0x02) >> 1);
            return raw != 0;
        }
        
        private void EncodeMultipleUnit(Byte[] buff, Boolean value)
        {
            buff[0] = (byte)((buff[0] & ~0x02) | ((value ? 1 : 0) & 0x02));
        }
        
        private Boolean DecodeSautDiag(Byte[] buff)
        {
            int raw = (buff[0] & 0x01);
            return raw != 0;
        }
        
        private void EncodeSautDiag(Byte[] buff, Boolean value)
        {
            buff[0] = (byte)((buff[0] & ~0x01) | ((value ? 1 : 0) & 0x01));
        }
        
        private Byte DecodeRestrictedSpeed(Byte[] buff)
        {
            int raw = (buff[1] & 0x3f) | ((buff[2] & 0x0e) << 5);
            return unchecked((Byte)raw);
        }
        
        private void EncodeRestrictedSpeed(Byte[] buff, Byte value)
        {
            buff[1] = (byte)((buff[1] & ~0x3f) | (value & 0x3f));
            buff[2] = (byte)((buff[2] & ~0x0e) | ((value >> 6) & 0x0e));
        }
        
        private Boolean DecodeSlippingSections(Byte[] buff)
        {
            int raw = ((buff[2] & 0x80) >> 7);
            return raw != 0;
        }
        
        private void EncodeSlippingSections(Byte[] buff, Boolean value)
        {
            buff[2] = (byte)((buff[2] & ~0x80) | ((value ? 1 : 0) & 0x80));
        }
        
    }
    
    [FrameDescriptor(0x1888)]
    /// <summary>
    /// Описание параметров движения
    /// <summary>
    public class IpdState : BlokFrame
    {
        [System.ComponentModel.Description("Номер выполненного теста")]
        /// <summary>Номер выполненного теста</summary>
        public int TestNumber { get; set; }
        
        [System.ComponentModel.Description("Результат теста")]
        /// <summary>Результат теста</summary>
        public int TestResult { get; set; }
        
        ///<summary>Направление движения</summary>
        public enum DirectionKind : int
        {
            [System.ComponentModel.Description("Вперёд")]
            ///<summary>Вперёд</summary>
            Ahead = 0,
            [System.ComponentModel.Description("Назад")]
            ///<summary>Назад</summary>
            Back = 1,
        }
        
        [System.ComponentModel.Description("Направление движения")]
        /// <summary>Направление движения</summary>
        public DirectionKind Direction { get; set; }
        
        ///<summary>Знак ускорения</summary>
        public enum AccelerationSignKind : int
        {
            [System.ComponentModel.Description("Положительное")]
            ///<summary>Положительное</summary>
            Positive = 0,
            [System.ComponentModel.Description("Отрицательное")]
            ///<summary>Отрицательное</summary>
            Negative = 1,
        }
        
        [System.ComponentModel.Description("Знак ускорения")]
        /// <summary>Знак ускорения</summary>
        public AccelerationSignKind AccelerationSign { get; set; }
        
        [System.ComponentModel.Description("Наличие импульсов ДПС")]
        /// <summary>Наличие импульсов ДПС</summary>
        public Boolean SpeedPulsesAvailable { get; set; }
        
        [System.ComponentModel.Description("Фактическая скорость")]
        /// <summary>Фактическая скорость</summary>
        public int Speed { get; set; }
        
        [System.ComponentModel.Description("Линейная ордината")]
        /// <summary>Линейная ордината</summary>
        public int LinearOrdinate { get; set; }
        
        [System.ComponentModel.Description("Признак виртуальной кабины")]
        /// <summary>Признак виртуальной кабины</summary>
        public Boolean IsVirtualCabine { get; set; }
        
        ///<summary>Номер виртуальной кабины</summary>
        public enum VirtualCabineKind : int
        {
            [System.ComponentModel.Description("1 кабина")]
            ///<summary>1 кабина</summary>
            Cabine1 = 0,
            [System.ComponentModel.Description("2 кабина")]
            ///<summary>2 кабина</summary>
            Cabine2 = 1,
        }
        
        [System.ComponentModel.Description("Номер виртуальной кабины")]
        /// <summary>Номер виртуальной кабины</summary>
        public VirtualCabineKind VirtualCabine { get; set; }
        
        [System.ComponentModel.Description("Определение местоположения в ЭК")]
        /// <summary>Определение местоположения в ЭК</summary>
        public Boolean EmapPosition { get; set; }
        
        ///<summary>Тест пассивного датчика по скорости</summary>
        public enum PassiveSensorSpeedTestStateKind : int
        {
            [System.ComponentModel.Description("Исправен")]
            ///<summary>Исправен</summary>
            Correct = 0,
            [System.ComponentModel.Description("Сбой")]
            ///<summary>Сбой</summary>
            Fault = 1,
        }
        
        [System.ComponentModel.Description("Тест пассивного датчика по скорости")]
        /// <summary>Тест пассивного датчика по скорости</summary>
        public PassiveSensorSpeedTestStateKind PassiveSensorSpeedTestState { get; set; }
        
        ///<summary>Номер активного датчика</summary>
        public enum ActiveSpeedSensorKind : int
        {
            [System.ComponentModel.Description("Датчик 1")]
            ///<summary>Датчик 1</summary>
            Sensor1 = 0,
            [System.ComponentModel.Description("Датчик 2")]
            ///<summary>Датчик 2</summary>
            Sensor2 = 1,
        }
        
        [System.ComponentModel.Description("Номер активного датчика")]
        /// <summary>Номер активного датчика</summary>
        public ActiveSpeedSensorKind ActiveSpeedSensor { get; set; }
        
        ///<summary>Тест пассивного датчика по количеству импульсов</summary>
        public enum PassiveSensorImpulseTestStateKind : int
        {
            [System.ComponentModel.Description("Исправен")]
            ///<summary>Исправен</summary>
            Correct = 0,
            [System.ComponentModel.Description("Сбой")]
            ///<summary>Сбой</summary>
            Fault = 1,
        }
        
        [System.ComponentModel.Description("Тест пассивного датчика по количеству импульсов")]
        /// <summary>Тест пассивного датчика по количеству импульсов</summary>
        public PassiveSensorImpulseTestStateKind PassiveSensorImpulseTestState { get; set; }
        
        
        protected override void Decode(byte[] buff)
        {
            this.TestNumber = DecodeTestNumber(buff);
            this.TestResult = DecodeTestResult(buff);
            this.Direction = DecodeDirection(buff);
            this.AccelerationSign = DecodeAccelerationSign(buff);
            this.SpeedPulsesAvailable = DecodeSpeedPulsesAvailable(buff);
            this.Speed = DecodeSpeed(buff);
            this.LinearOrdinate = DecodeLinearOrdinate(buff);
            this.IsVirtualCabine = DecodeIsVirtualCabine(buff);
            this.VirtualCabine = DecodeVirtualCabine(buff);
            this.EmapPosition = DecodeEmapPosition(buff);
            this.PassiveSensorSpeedTestState = DecodePassiveSensorSpeedTestState(buff);
            this.ActiveSpeedSensor = DecodeActiveSpeedSensor(buff);
            this.PassiveSensorImpulseTestState = DecodePassiveSensorImpulseTestState(buff);
        }
        
        protected override byte[] Encode()
        {
            var buff = new Byte[FrameLength];
            EncodeTestNumber(buff, TestNumber);
            EncodeTestResult(buff, TestResult);
            EncodeDirection(buff, Direction);
            EncodeAccelerationSign(buff, AccelerationSign);
            EncodeSpeedPulsesAvailable(buff, SpeedPulsesAvailable);
            EncodeSpeed(buff, Speed);
            EncodeLinearOrdinate(buff, LinearOrdinate);
            EncodeIsVirtualCabine(buff, IsVirtualCabine);
            EncodeVirtualCabine(buff, VirtualCabine);
            EncodeEmapPosition(buff, EmapPosition);
            EncodePassiveSensorSpeedTestState(buff, PassiveSensorSpeedTestState);
            EncodeActiveSpeedSensor(buff, ActiveSpeedSensor);
            EncodePassiveSensorImpulseTestState(buff, PassiveSensorImpulseTestState);
            return buff;
        }
        
        private int DecodeTestNumber(Byte[] buff)
        {
            int raw = ((buff[0] & 0xf0) >> 4);
            return unchecked((int)raw);
        }
        
        private void EncodeTestNumber(Byte[] buff, int value)
        {
            buff[0] = (byte)((buff[0] & ~0xf0) | (value & 0xf0));
        }
        
        private int DecodeTestResult(Byte[] buff)
        {
            int raw = (buff[0] & 0x0f);
            return unchecked((int)raw);
        }
        
        private void EncodeTestResult(Byte[] buff, int value)
        {
            buff[0] = (byte)((buff[0] & ~0x0f) | (value & 0x0f));
        }
        
        private DirectionKind DecodeDirection(Byte[] buff)
        {
            int raw = ((buff[1] & 0x80) >> 7);
            return (DirectionKind)raw;
        }
        
        private void EncodeDirection(Byte[] buff, DirectionKind value)
        {
            buff[1] = (byte)((buff[1] & ~0x80) | ((int)value & 0x80));
        }
        
        private AccelerationSignKind DecodeAccelerationSign(Byte[] buff)
        {
            int raw = ((buff[1] & 0x20) >> 5);
            return (AccelerationSignKind)raw;
        }
        
        private void EncodeAccelerationSign(Byte[] buff, AccelerationSignKind value)
        {
            buff[1] = (byte)((buff[1] & ~0x20) | ((int)value & 0x20));
        }
        
        private Boolean DecodeSpeedPulsesAvailable(Byte[] buff)
        {
            int raw = ((buff[1] & 0x04) >> 2);
            return raw != 0;
        }
        
        private void EncodeSpeedPulsesAvailable(Byte[] buff, Boolean value)
        {
            buff[1] = (byte)((buff[1] & ~0x04) | ((value ? 1 : 0) & 0x04));
        }
        
        private int DecodeSpeed(Byte[] buff)
        {
            int raw = buff[2] | ((buff[1] & 0x01) << 8);
            return unchecked((int)raw);
        }
        
        private void EncodeSpeed(Byte[] buff, int value)
        {
            buff[2] = (byte)(value & 0xff);
            buff[1] = (byte)((buff[1] & ~0x01) | ((value >> 8) & 0x01));
        }
        
        private int DecodeLinearOrdinate(Byte[] buff)
        {
            int raw = buff[4] | (buff[3] << 8) | (buff[5] << 16);
            return unchecked((int)raw);
        }
        
        private void EncodeLinearOrdinate(Byte[] buff, int value)
        {
            buff[4] = (byte)(value & 0xff);
            buff[3] = (byte)((value >> 8) & 0xff);
            buff[5] = (byte)((value >> 16) & 0xff);
        }
        
        private Boolean DecodeIsVirtualCabine(Byte[] buff)
        {
            int raw = ((buff[6] & 0x80) >> 7);
            return raw != 0;
        }
        
        private void EncodeIsVirtualCabine(Byte[] buff, Boolean value)
        {
            buff[6] = (byte)((buff[6] & ~0x80) | ((value ? 1 : 0) & 0x80));
        }
        
        private VirtualCabineKind DecodeVirtualCabine(Byte[] buff)
        {
            int raw = ((buff[6] & 0x40) >> 6);
            return (VirtualCabineKind)raw;
        }
        
        private void EncodeVirtualCabine(Byte[] buff, VirtualCabineKind value)
        {
            buff[6] = (byte)((buff[6] & ~0x40) | ((int)value & 0x40));
        }
        
        private Boolean DecodeEmapPosition(Byte[] buff)
        {
            int raw = ((buff[6] & 0x20) >> 5);
            return raw != 0;
        }
        
        private void EncodeEmapPosition(Byte[] buff, Boolean value)
        {
            buff[6] = (byte)((buff[6] & ~0x20) | ((value ? 1 : 0) & 0x20));
        }
        
        private PassiveSensorSpeedTestStateKind DecodePassiveSensorSpeedTestState(Byte[] buff)
        {
            int raw = ((buff[6] & 0x10) >> 4);
            return (PassiveSensorSpeedTestStateKind)raw;
        }
        
        private void EncodePassiveSensorSpeedTestState(Byte[] buff, PassiveSensorSpeedTestStateKind value)
        {
            buff[6] = (byte)((buff[6] & ~0x10) | ((int)value & 0x10));
        }
        
        private ActiveSpeedSensorKind DecodeActiveSpeedSensor(Byte[] buff)
        {
            int raw = ((buff[6] & 0x08) >> 3);
            return (ActiveSpeedSensorKind)raw;
        }
        
        private void EncodeActiveSpeedSensor(Byte[] buff, ActiveSpeedSensorKind value)
        {
            buff[6] = (byte)((buff[6] & ~0x08) | ((int)value & 0x08));
        }
        
        private PassiveSensorImpulseTestStateKind DecodePassiveSensorImpulseTestState(Byte[] buff)
        {
            int raw = ((buff[6] & 0x04) >> 2);
            return (PassiveSensorImpulseTestStateKind)raw;
        }
        
        private void EncodePassiveSensorImpulseTestState(Byte[] buff, PassiveSensorImpulseTestStateKind value)
        {
            buff[6] = (byte)((buff[6] & ~0x04) | ((int)value & 0x04));
        }
        
    }
    
}


