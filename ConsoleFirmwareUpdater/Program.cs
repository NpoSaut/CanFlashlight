using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FirmwarePacking;

namespace ConsoleFirmwareUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = FirmwarePackage.Open(new System.IO.FileInfo("pack.zip"));
        }
    }
}
