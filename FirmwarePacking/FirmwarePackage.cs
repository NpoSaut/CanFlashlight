using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;

namespace FirmwarePacking
{
    public class FirmwarePackage
    {
        public PackageInformation Information { get; set; }

        public static FirmwarePackage Open(FileInfo File)
        {
            using (ZipStorer pack = ZipStorer.Open(File.OpenRead(), FileAccess.Read))
            {
                var files = pack.ReadCentralDir();
                var index = files.Single(f => f.FilenameInZip == "index.xml");
                var ms = new MemoryStream();
                pack.ExtractFile(index, ms);
                ms.Seek(0, SeekOrigin.Begin);

                XDocument doc = XDocument.Load(ms);

                return
                    new FirmwarePackage()
                    {
                        Information = new PackageInformation(doc.Root)
                    };
            }
        }

        private FirmwarePackage()
        {
        }
    }
}
