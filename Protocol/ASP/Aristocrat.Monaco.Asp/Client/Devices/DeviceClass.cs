namespace Aristocrat.Monaco.Asp.Client.Devices
{
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using Contracts;

    public class DeviceClass : INamedId, IDeviceClassFeature
    {
        [XmlElement(ElementName = "DeviceType")]
        public List<DeviceType> Types = new List<DeviceType>();

        public IReadOnlyList<DeviceType> DeviceTypes => Types;

        [XmlAttribute]
        public string Version { get; set; }

        public DeviceType this[int typeId]
        {
            get { return Types.Find(x => x.Id == typeId); }
        }

        [XmlAttribute(AttributeName = "ID")]
        public int Id { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public bool SharedDeviceTypeEventReport { get; set; }
    }
}