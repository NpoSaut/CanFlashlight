using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FirmwarePacking
{
    /// <summary>
    /// Информация о пакете с ПО
    /// </summary>
    public class PackageInformation
    {
        /// <summary>Версия прошивки</summary>
        public Version FirmwareVersion { get; set; }
        /// <summary>Дата релиза прошивки</summary>
        public DateTime ReleaseDate { get; set; }
        /// <summary>Целевые модули для прошивки</summary>
        public List<FirmwareTargetInformation> Targets { get; set; }

        public PackageInformation() { }
        public PackageInformation(XElement XInformation)
            : this()
        {
            var XVersionInfo = XInformation.Element("Version");
            FirmwareVersion = new Version(
                major: (int)XVersionInfo.Attribute("Major"),
                minor: (int?)XVersionInfo.Attribute("Minor") ?? 0);
            ReleaseDate = (DateTime)XVersionInfo.Attribute("ReleaseDate");

            Targets = XInformation.Elements("TargetModule").Select(XTarget => new FirmwareTargetInformation(XTarget)).ToList();
        }

        public XElement ToXElement() { return ToXElement("FirmwareInformation"); }
        public XElement ToXElement(String ElementName)
        {
            return new XElement(ElementName,
                Targets.Select(target => target.ToXElement("TargetModule")),
                new XElement("Version",
                    new XAttribute("Major", FirmwareVersion.Major),
                    new XAttribute("Minor", FirmwareVersion.Minor),
                    new XAttribute("ReleaseDate", ReleaseDate.ToString("u"))));
        }

        public static explicit operator XElement(PackageInformation pi) { return pi.ToXElement(); }
        public static explicit operator PackageInformation(XElement xpi) { return new PackageInformation(xpi); }
    }
}
