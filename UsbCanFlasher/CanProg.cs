using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlokFramesCodegen
{
    public class DevFileInfo
    {
        public String Name { get; set; }
        public int Size { get; set; }
        public uint Checksum { get; set; }
    }
    public class DevId
    {
        public int SystemId { get; set; }
        public int BlockId { get; set; }
        public int BlockModification { get; set; }
    }

    public enum PropertyKind : int { MajorVersion = 1, MinorVersion = 2 }

    public class CanProg : IDisposable
    {
        Dictionary<PropertyKind, int> Properties;

        public List<DevFileInfo> ListFiles()
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(String FileName)
        {
            throw new NotImplementedException();
        }

        public void CreateFile(String FileName, Byte[] Data)
        {
            throw new NotImplementedException();
        }

        public Byte[] ReadFile(String FileName)
        {
            throw new NotImplementedException();
        }

        public void MrProper()
        {
            throw new NotImplementedException();
        }

        public void RefreshProperties()
        {
            throw new NotImplementedException();
        }

        public void SetProperty(PropertyKind property, int value)
        {
            throw new NotImplementedException();
        }

        public const int FuInit = 0xfc28;
        public const int FuProg = 0xfc48;
        public const int FuDev  = 0xfc68;


        public void Dispose()
        {
            // Эта функция вызовется тогда, когда нужно будет закрыть соединение.
            // Тут нужно будет сказать "досвидания"
            throw new NotImplementedException();
        }

        //public static CanProg Connect(CanPort Port, DevId Device)
        //{
        //    // Тут выполняется подключение к устройству: отправляется "затравочное сообщение"
        //    // Когда загрузчик ответил на PROG_INIT, создаётся объект класса CanProg, заполняются его Properties.
        //    // Это означает, что устройство готово к программированию
        //    throw new NotImplementedException();
        //}
    }
}
