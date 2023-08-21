namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;
    using BinarySerialization;

    [Serializable]
    public class Nudge : HarkeySerializableMessage, ISequencedCommand
    {
        public Nudge()
            : base(MessageMaskType.Command, HarkeyCommandId.Nudge, 11, false)
        {
        }

        [FieldOrder(0)]
        [FieldBitLength(8)]
        public byte Direction { get; set; }

        [FieldOrder(1)]
        [FieldBitLength(8)]
        public byte Nudge1 { get; set; }

        [FieldOrder(2)]
        [FieldBitLength(8)]
        public byte Nudge2 { get; set; }

        [FieldOrder(3)]
        [FieldBitLength(8)]
        public byte Nudge3 { get; set; }

        [FieldOrder(4)]
        [FieldBitLength(8)]
        public byte Nudge4 { get; set; }

        [FieldOrder(5)]
        [FieldBitLength(8)]
        public byte Nudge5 { get; set; }

        [FieldOrder(6)]
        [FieldBitLength(8)]
        public byte Nudge6 { get; set; }

        [FieldOrder(7)]
        [FieldBitLength(8)]
        public byte Delay { get; set; }
    }
}
