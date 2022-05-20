namespace Aristocrat.Monaco.Hardware.EdgeLight.Strips
{
    using System.Xml.Serialization;
    using Device.Packets;

    public class HardwareStrip : HexId
    {
        [XmlAttribute(AttributeName = "ResponseCommand")]
        public ResponseType Response { get; set; } = ResponseType.InvalidResponse;

        [XmlAttribute(AttributeName = "LEDCount")]
        public int LedCount { get; set; }

        [XmlAttribute(AttributeName = "BytesPerLed")]
        public int BytesPerLed { get; set; } = 3;
    }
}