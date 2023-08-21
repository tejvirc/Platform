namespace Aristocrat.Monaco.Hardware.EdgeLight.Strips
{
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using Hardware.Contracts.EdgeLighting;

    public class StripCloneMappings
    {
        [XmlElement(ElementName = "StripCloneMapping")]
        public List<StripCloneMapping> Mappings { get; set; }
    }

    public class StripCloneMapping
    {
        [XmlElement]
        public Mapping From { get; set; }

        [XmlElement]
        public List<Mapping> To { get; set; }
    }

    public class Mapping
    {
        public static int AllStrips = -1;

        [XmlAttribute]
        public StripIDs Id { get; set; }

        [XmlAttribute]
        public int LedStart { get; set; }

        [XmlAttribute]
        public int Count { get; set; } = AllStrips;
    }
}