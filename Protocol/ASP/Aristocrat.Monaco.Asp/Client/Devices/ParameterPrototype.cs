namespace Aristocrat.Monaco.Asp.Client.Devices
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;
    using Contracts;
    using Fields;

    public class ParameterPrototype : IParameterPrototype
    {
        [XmlElement(ElementName = "Field")]
        public List<FieldPrototype> FieldsPrototypeInternal = new List<FieldPrototype>();

        [XmlIgnore]
        public INamedId ClassId { get; set; }

        [XmlIgnore]
        public INamedId TypeId { get; set; }

        [XmlIgnore]
        public IReadOnlyList<IFieldPrototype> FieldsPrototype =>
            FieldsPrototypeInternal.Cast<IFieldPrototype>().ToList().AsReadOnly();

        [XmlAttribute(AttributeName = "EGMAccess")]
        public AccessType EgmAccessType { get; set; }

        [XmlAttribute(AttributeName = "EventAccess")]
        public EventAccessType EventAccessType { get; set; }

        [XmlAttribute(AttributeName = "MCIAccess")]
        public AccessType MciAccessType { get; set; }

        [XmlAttribute(AttributeName = "ID")]
        public int Id { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        public int SizeInBytes => FieldsPrototypeInternal.Sum(x => x.SizeInBytes);
    }
}