namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    [Serializable]
    public class UnsolicitedErrorTamperResponse : HarkeySerializableMessage, ISimpleAckResponse
    {
        public static readonly byte Length = 2;

        public UnsolicitedErrorTamperResponse()
            : base(MessageMaskType.UnsolicitedErrorResponse, HarkeyCommandId.UnsolicitedErrorTamperResponse, Length, false)
        {
        }

        [FieldOrder(0)]
        [FieldBitLength(8)]
        public int ResponseCode { get; set; }

        public override string ToString()
        {
            return Invariant($"{GetType()} [ResponseCode={ResponseCode}]");
        }
    }
}