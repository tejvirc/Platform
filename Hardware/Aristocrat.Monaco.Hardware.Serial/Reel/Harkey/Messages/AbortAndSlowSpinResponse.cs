namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    [Serializable]
    public class AbortAndSlowSpinResponse : HarkeySerializableMessage, ISimpleAckResponse
    {
        public AbortAndSlowSpinResponse()
            : base(MessageMaskType.CommandResponse, HarkeyCommandId.AbortAndSlowSpin, 3, false)
        {
        }

        [FieldOrder(0)]
        [FieldBitLength(8)]
        public int ResponseCode { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant(
                $"{GetType()} [ResponseCode={ResponseCode}]");
        }
    }
}
