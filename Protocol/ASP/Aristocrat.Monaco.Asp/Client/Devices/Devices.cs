namespace Aristocrat.Monaco.Asp.Client.Devices
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class Devices
    {
        [XmlElement(ElementName = "DeviceClass")]
        public List<DeviceClass> Classes = new List<DeviceClass>();

        [XmlIgnore]
        public IReadOnlyList<DeviceClass> DeviceClasses => Classes;

        public DeviceClass this[int classId]
        {
            get { return Classes.Find(x => x.Id == classId); }
        }
    }
}