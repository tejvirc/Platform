namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;
    using BinarySerialization;

    [Serializable]
    public class ChangeSpeed : HarkeySerializableMessage, ISequencedCommand
    {
        public ChangeSpeed()
            : base(MessageMaskType.Command, HarkeyCommandId.ChangeSpeed, 4, false)
        {
        }

        [FieldOrder(0)]
        [FieldBitLength(8)]
        public int Speed { get; set; }
    }
}