namespace Aristocrat.Monaco.Asp.Client.Devices.Fields
{
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using Contracts;

    public class FieldPrototype : IFieldPrototype
    {
        [XmlElement(ElementName = "Mask")]
        public List<Mask> MasksInternal { get; set; }

        [XmlAttribute(AttributeName = "Value")]
        public string DefaultValue { get; set; }

        [XmlAttribute]
        public FieldType Type { get; set; }

        [XmlAttribute(AttributeName = "Size")]
        public int SizeInBytes { get; set; }

        [XmlAttribute(AttributeName = "ID")]
        public int Id { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string DataSourceName { get; set; }

        [XmlAttribute]
        public string DataMemberName { get; set; }

        [XmlIgnore]
        public IDataSource DataSource { get; set; }

        [XmlIgnore]
        public IReadOnlyList<IMask> Masks => MasksInternal;
    }
}