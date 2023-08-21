namespace Aristocrat.Monaco.Hardware.Serial.Printer
{
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;

    /// <summary>
    ///     Deserializable class from the PDL "PT" command, which means
    ///     "Print Ticket per a template pattern".
    /// </summary>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlRoot("PT", Namespace = "", IsNullable = false)]
    public class PrintCommand
    {
        /// <summary>
        ///     Data fields (for the regions in the template)
        /// </summary>
        [XmlElement("D")]
        public PrintDataField[] DataFields { get; set; }

        /// <summary>
        ///     Id (of the template to use)
        /// </summary>
        [XmlAttribute("id")]
        public byte Id { get; set; }

        /// <summary>
        ///     Indicates whether or not printer defined template should be used.
        /// </summary>
        [XmlIgnore]
        public bool UsePrinterTemplate { get; set; }

        /// <summary>
        ///     Indicates whether or not this command is for an audit ticket.
        /// </summary>
        [XmlIgnore]
        public bool IsAuditTicket { get; set; }

        /// <summary>
        ///     The printer defined template id to use for this command.
        /// </summary>
        [XmlIgnore]
        public string PrinterTemplateId { get; set; }
    }

    /// <summary>
    ///     Deserializable class from the data field of "PT" command.
    /// </summary>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlRoot("D", Namespace = "", IsNullable = false)]
    public class PrintDataField
    {
        /// <summary>
        ///     Data field
        /// </summary>
        [XmlText]
        public string Data { get; set; }

        /// <summary>
        ///     Region-of-interest field number (default 0 == none)
        /// </summary>
        [XmlAttribute("pRoI"), DefaultValue(0)]
        public int IsRegionOfInterest { get; set; }
    }
}
