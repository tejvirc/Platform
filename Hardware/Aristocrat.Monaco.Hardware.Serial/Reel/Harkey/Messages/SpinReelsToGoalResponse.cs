namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    [Serializable]
    public class SpinReelsToGoalResponse : HarkeySerializableMessage, ISequencedCommand, ISpinResponse
    {
        public SpinReelsToGoalResponse()
            : base(MessageMaskType.CommandResponse, HarkeyCommandId.SpinToGoal, 7, false)
        {
        }

        [FieldOrder(0)]
        [FieldBitLength(8)]
        public int ResponseCode1 { get; set; }

        [FieldOrder(1)]
        [FieldBitLength(8)]
        public int ResponseCode2 { get; set; }

        [FieldOrder(2)]
        [FieldBitLength(8)]
        public int ResponseCode3 { get; set; }

        [FieldOrder(3)]
        [FieldBitLength(8)]
        public int ResponseCode4 { get; set; }

        public override string ToString()
        {
            return Invariant($"{GetType()} [ResponseCode={ResponseCode1},{ResponseCode2},{ResponseCode3},{ResponseCode4}]");
        }
    }
}
