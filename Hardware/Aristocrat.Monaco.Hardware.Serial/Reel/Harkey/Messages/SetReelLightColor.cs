namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;
    using BinarySerialization;

    [Serializable]
    public class SetReelLightColor : HarkeySerializableMessage, ISequencedCommand
    {
        public SetReelLightColor()
            : base(MessageMaskType.Command, HarkeyCommandId.SetReelLightColor, 10, false)
        {
        }

        [FieldOrder(0)]
        [FieldBitLength(8)]
        public int ReelId { get; set; }

        [FieldOrder(1)]
        [FieldBitLength(16)]
        public ushort TopColor { get; set; }

        [FieldOrder(2)]
        [FieldBitLength(16)]
        public ushort MiddleColor { get; set; }

        [FieldOrder(3)]
        [FieldBitLength(16)]
        public ushort BottomColor { get; set; }
    }
}