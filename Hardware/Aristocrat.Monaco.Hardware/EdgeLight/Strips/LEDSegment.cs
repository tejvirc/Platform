namespace Aristocrat.Monaco.Hardware.EdgeLight.Strips
{
    using System;
    using System.Xml.Serialization;
    using Contracts;

    public class LedSegment : HexId
    {
        [XmlIgnore] public IStrip PhysicalStrip;

        [XmlAttribute(AttributeName = "from")]
        public int From { get; set; }

        [XmlAttribute(AttributeName = "to")]
        public int To { get; set; }

        [XmlAttribute(AttributeName = "HardwareStripId", DataType = "string")]
        public string HexStringHardwareStripId
        {
            get => "0x" + HardwareStripId.ToString("x");
            set => HardwareStripId = Convert.ToInt32(value, 16);
        }

        [XmlIgnore]
        public int HardwareStripId { get; set; } = int.MaxValue;

        [XmlIgnore]
        private bool IsForward => From < To;

        private int StartLedIndex => Math.Min(To, From);

        [XmlIgnore]
        public int LedCount => Math.Abs(From - To) + 1;

        [XmlIgnore]
        public LedColorBuffer ColorBuffer => PhysicalStrip.ColorBuffer.GetSegment(StartLedIndex, LedCount, !IsForward);

        public void SetColors(LedColorBuffer buffer, int sourceColorIndex, int ledCount, int destinationLedIndex)
        {
            var ledCountToSet = Math.Min(ledCount, LedCount);
            var segment = buffer.GetSegment(sourceColorIndex, ledCountToSet, !IsForward);
            PhysicalStrip.SetColors(segment, 0, ledCountToSet, StartLedIndex);
        }
    }
}