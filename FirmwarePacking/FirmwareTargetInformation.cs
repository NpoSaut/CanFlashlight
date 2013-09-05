using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FirmwarePacking
{
    /// <summary>
    /// Информация о модуле назначения прошивки
    /// </summary>
    public class FirmwareTargetInformation
    {
        /// <summary>Идентификатор системы</summary>
        public int SystemId { get; set; }
        /// <summary>Идентификатор ячейки</summary>
        public int CellId { get; set; }
        /// <summary>Модификация ячейки</summary>
        public int CellModification { get; set; }
        /// <summary>Номер канала (полукомплекта)</summary>
        public int Channel { get; set; }
        /// <summary>Номер модуля</summary>
        public int Module { get; set; }

        public FirmwareTargetInformation()
        { }
        public FirmwareTargetInformation(XElement XTarget)
            : this()
        {
            SystemId = (int)XTarget.Attribute("System");
            CellId = (int)XTarget.Attribute("Cell");
            CellModification = (int)XTarget.Attribute("Modification");
            Channel = (int)XTarget.Attribute("Channel");
            Module = (int)XTarget.Attribute("Module");
        }

        public XElement ToXElement() { return ToXElement("TargetModule"); }
        public XElement ToXElement(String ElementName)
        {
            return new XElement(ElementName,
                new XElement("System", SystemId),
                new XElement("Cell", CellId),
                new XElement("Modification", CellModification),
                new XElement("Channel", Channel),
                new XElement("Module", Module)
                );
        }

        public static explicit operator XElement(FirmwareTargetInformation ti) { return ti.ToXElement(); }
        public static explicit operator FirmwareTargetInformation(XElement xti) { return new FirmwareTargetInformation(xti); }

        public override string ToString()
        {
            return string.Format("Sys={0} Cell={1}[{2}]/{3} Module={4}", SystemId, CellId, CellModification, Channel, Module);
        }
    }
}
