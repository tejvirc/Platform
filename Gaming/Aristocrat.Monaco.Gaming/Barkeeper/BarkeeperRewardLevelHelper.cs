namespace Aristocrat.Monaco.Gaming.Barkeeper
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Xml.Serialization;
    using Contracts.Barkeeper;
    using Hardware.Contracts.EdgeLighting;

    public static class BarkeeperRewardLevelHelper
    {
        private const int SlowFlashTime = 500;
        private const int MediumFlashTime = 250;
        private const int FastFlashTime = 100;

        public static StripIDs ToStripIds(this BarkeeperLed @this)
        {
            switch (@this)
            {
                case BarkeeperLed.Button:
                    return StripIDs.BarkeeperStrip1Led;
                case BarkeeperLed.Halo:
                    return StripIDs.BarkeeperStrip4Led;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Color ToColor(this ColorOptions @this)
        {
            switch (@this)
            {
                case ColorOptions.Blue:
                    return Color.Blue;
                case ColorOptions.Green:
                    return Color.Green;
                case ColorOptions.Orange:
                    return Color.Orange;
                case ColorOptions.Purple:
                    return Color.Purple;
                case ColorOptions.Yellow:
                    return Color.Yellow;
                case ColorOptions.Black:
                    return Color.Black;
                case ColorOptions.White:
                    return Color.White;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static PatternParameters GetPattern(ColorOptions color, BarkeeperLed led, BarkeeperAlertOptions alert)
        {
            switch (alert)
            {
                case BarkeeperAlertOptions.LightOn:
                    return new SolidColorPatternParameters
                    {
                        Color = color.ToColor(),
                        Strips = new List<int> { (int)led.ToStripIds() },
                        Priority = StripPriority.PlatformControlled
                    };
                case BarkeeperAlertOptions.SlowFlash:
                    return new BlinkPatternParameters
                    {
                        OnColor = color.ToColor(),
                        OffColor = Color.Black,
                        Strips = new List<int> { (int)led.ToStripIds() },
                        Priority = StripPriority.PlatformControlled,
                        OnTime = SlowFlashTime,
                        OffTime = SlowFlashTime
                    };
                case BarkeeperAlertOptions.MediumFlash:
                    return new BlinkPatternParameters
                    {
                        OnColor = color.ToColor(),
                        OffColor = Color.Black,
                        Strips = new List<int> { (int)led.ToStripIds() },
                        Priority = StripPriority.PlatformControlled,
                        OnTime = MediumFlashTime,
                        OffTime = MediumFlashTime
                    };
                case BarkeeperAlertOptions.RapidFlash:
                    return new BlinkPatternParameters
                    {
                        OnColor = color.ToColor(),
                        OffColor = Color.Black,
                        Strips = new List<int> { (int)led.ToStripIds() },
                        Priority = StripPriority.PlatformControlled,
                        OnTime = FastFlashTime,
                        OffTime = FastFlashTime
                    };
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static BarkeeperRewardLevels ToRewards(string xml)
        {
            var theXmlRootAttribute = Attribute.GetCustomAttributes(typeof(BarkeeperRewardLevels))
                .FirstOrDefault(x => x is XmlRootAttribute) as XmlRootAttribute;
            var xmlSerializer = new XmlSerializer(typeof(BarkeeperRewardLevels), theXmlRootAttribute ?? new XmlRootAttribute(nameof(BarkeeperRewardLevels)));
            using (var stream = new StringReader(xml))
            {
                return (BarkeeperRewardLevels)xmlSerializer.Deserialize(stream);
            }
        }

        public static string ToXml(this BarkeeperRewardLevels rewardLevels)
        {
            var theXmlRootAttribute = Attribute.GetCustomAttributes(typeof(BarkeeperRewardLevels))
                .FirstOrDefault(x => x is XmlRootAttribute) as XmlRootAttribute;
            var xmlSerializer = new XmlSerializer(typeof(BarkeeperRewardLevels), theXmlRootAttribute ?? new XmlRootAttribute(nameof(BarkeeperRewardLevels)));
            using (var stream = new StringWriter())
            {
                xmlSerializer.Serialize(stream, rewardLevels);
                return stream.ToString();
            }
        }
    }
}