namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    [Serializable]
    public class NudgeResponse : HarkeySerializableMessage, ISequencedCommand, ISpinResponse
    {
        public NudgeResponse()
            : base(MessageMaskType.CommandResponse, HarkeyCommandId.Nudge, 7, false)
        {
        }

        [FieldOrder(0)]
        [FieldBitLength(8)]
        public int ResponseCode1 { get; set; }

        [FieldOrder(1)]
        [FieldBitLength(8)]
        public int ResponseCode2 { get; set; }

        public override string ToString()
        {
            return Invariant($"{GetType()} [ResponseCode={ResponseCode1},{ResponseCode2}]");
        }
    }
}
