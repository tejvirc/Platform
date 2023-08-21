namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;
    using BinarySerialization;

    [Serializable]
    public class SetFaults : HarkeySerializableMessage, ISequencedCommand
    {
        public SetFaults()
            : base(MessageMaskType.Command, HarkeyCommandId.SetFaults, 10, false)
        {
        }

        [FieldOrder(0)]
        [FieldBitLength(8)]
        public byte Fault1 { get; set; }

        [FieldOrder(1)]
        [FieldBitLength(8)]
        public byte Fault2 { get; set; }

        [FieldOrder(2)]
        [FieldBitLength(8)]
        public byte Fault3 { get; set; }

        [FieldOrder(3)]
        [FieldBitLength(8)]
        public byte Fault4 { get; set; }

        [FieldOrder(4)]
        [FieldBitLength(8)]
        public byte Fault5 { get; set; }

        [FieldOrder(5)]
        [FieldBitLength(8)]
        public byte Fault6 { get; set; }
    }
}
