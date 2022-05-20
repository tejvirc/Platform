namespace Aristocrat.Monaco.Hardware.EdgeLight.Strips
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;
    using Contracts;

    public class LogicalStrip : HexId, IStrip
    {
        [XmlElement(ElementName = "LEDSegment")]
        public List<LedSegment> LedSegments = new List<LedSegment>();

        [XmlIgnore]
        public int StripId => Id;

        [XmlIgnore]
        public int Brightness
        {
            get => LedSegments.FirstOrDefault()?.PhysicalStrip.Brightness ?? 100;
            set
            {
                LedSegments.ForEach(x => x.PhysicalStrip.Brightness = value);
                BrightnessChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [XmlIgnore]
        public int LedCount => LedSegments.Sum(x => x.LedCount);

        [XmlIgnore]
        public LedColorBuffer ColorBuffer
        {
            get
            {
                var colorBuffer = new LedColorBuffer();
                colorBuffer = LedSegments.Aggregate(
                    colorBuffer,
                    (current, ledSegment) => current.Append(ledSegment.ColorBuffer));
                return colorBuffer;
            }
        }

        public void SetColors(LedColorBuffer segment, int sourceColorIndex, int ledCount, int destinationLedIndex)
        {
            foreach (var ledSegment in LedSegments)
            {
                ledSegment.SetColors(segment, sourceColorIndex, ledCount, destinationLedIndex);
                sourceColorIndex += ledSegment.LedCount;
                ledCount -= ledSegment.LedCount;
                destinationLedIndex = Math.Max(0, destinationLedIndex - ledSegment.LedCount);
            }
        }

        public event EventHandler<EventArgs> BrightnessChanged;
    }
}