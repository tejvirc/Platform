namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;
    using BinarySerialization;

    [Serializable]
    public class SetReelLightBrightness : HarkeySerializableMessage, ISequencedCommand
    {
        public SetReelLightBrightness()
            : base(MessageMaskType.Command, HarkeyCommandId.SetReelBrightness, 5, false)
        {
        }

        [FieldOrder(0)]
        [FieldBitLength(8)]
        public int ReelId { get; set; }

        [FieldOrder(1)]
        [FieldBitLength(8)]
        public int Brightness { get; set; }
    }
}