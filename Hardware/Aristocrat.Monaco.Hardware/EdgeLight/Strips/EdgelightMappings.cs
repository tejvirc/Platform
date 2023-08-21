namespace Aristocrat.Monaco.Hardware.EdgeLight.Strips
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class EdgeLightMappings
    {
        [XmlElement(ElementName = "EdgeLightMapping")]
        public List<EdgeLightMapping> Mappings { get; set; }
    }

    public class EdgeLightMapping
    {
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "MainScreenSize")]
        public int MainScreenSize { get; set; }

        [XmlAttribute(AttributeName = "TopScreenSize")]
        public int TopScreenSize { get; set; }

        [XmlAttribute(AttributeName = "TopperScreenSize")]
        public int TopperScreenSize { get; set; }

        [XmlAttribute(AttributeName = "KeyboardScreenSize")]
        public int KeyboardScreenSize { get; set; }

        [XmlElement(ElementName = "HardwareStrip")]
        public List<HardwareStrip> HardwareStrips { get; set; }

        [XmlElement(ElementName = "LogicalStrip")]
        public List<LogicalStrip> LogicalStrips { get; set; }
    }
}