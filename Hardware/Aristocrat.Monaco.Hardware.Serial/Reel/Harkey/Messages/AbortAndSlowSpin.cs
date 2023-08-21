namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;
    using BinarySerialization;
    
    [Serializable]
    public class AbortAndSlowSpin : HarkeySerializableMessage
    {
        public AbortAndSlowSpin()
            : base(MessageMaskType.Command, HarkeyCommandId.AbortAndSlowSpin, 2, true)
        {
        }

        [FieldOrder(0)]
        [FieldBitLength(8)]
        public int SelectedReels { get; set; }
    }
}
