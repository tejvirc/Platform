namespace Aristocrat.Monaco.Hardware.EdgeLight.Strips
{
    using System;
    using System.Xml.Serialization;

    public class HexId
    {
        [XmlAttribute(AttributeName = "Id", DataType = "string")]
        public string HexStringId
        {
            get => "0x" + Id.ToString("x");
            set => Id = Convert.ToInt32(value, 16);
        }

        [XmlIgnore]
        public int Id { get; set; } = int.MinValue;
    }
}