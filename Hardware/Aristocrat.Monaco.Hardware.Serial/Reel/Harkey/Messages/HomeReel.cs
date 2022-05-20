namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;
    using BinarySerialization;

    [Serializable]
    public class HomeReel : HarkeySerializableMessage, ISequencedCommand
    {
        public HomeReel()
            : base(MessageMaskType.Command, HarkeyCommandId.HomeReel, 4, false)
        {
        }

        [FieldOrder(0)]
        [FieldBitLength(8)]
        public int ReelId { get; set; }
    }
}