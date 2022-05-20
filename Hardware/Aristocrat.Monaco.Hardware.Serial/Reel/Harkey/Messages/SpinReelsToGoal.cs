namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;
    using BinarySerialization;

    [Serializable]
    public class SpinReelsToGoal : HarkeySerializableMessage, ISequencedCommand
    {
        public SpinReelsToGoal()
            : base(MessageMaskType.Command, HarkeyCommandId.SpinToGoal, 12, false)
        {
        }

        [FieldOrder(0)]
        [FieldBitLength(8)]
        public byte Direction { get; set; }

        [FieldOrder(1)]
        [FieldBitLength(8)]
        public byte Goal1 { get; set; }

        [FieldOrder(2)]
        [FieldBitLength(8)]
        public byte Goal2 { get; set; }

        [FieldOrder(3)]
        [FieldBitLength(8)]
        public byte Goal3 { get; set; }

        [FieldOrder(4)]
        [FieldBitLength(8)]
        public byte Goal4 { get; set; }

        [FieldOrder(5)]
        [FieldBitLength(8)]
        public byte Goal5 { get; set; }

        [FieldOrder(6)]
        [FieldBitLength(8)]
        public byte Goal6 { get; set; }

        [FieldOrder(7)]
        [FieldBitLength(8)]
        public byte SelectedReels { get; set; }

        [FieldOrder(8)]
        [FieldBitLength(8)]
        public byte RampTable { get; set; }
    }
}
