namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    [Serializable]
    public class UnsolicitedErrorStallResponse : HarkeySerializableMessage
    {
        public static readonly byte Length = 3;

        public UnsolicitedErrorStallResponse()
            : base(MessageMaskType.UnsolicitedErrorResponse, HarkeyCommandId.UnsolicitedErrorStallResponse, Length, false)
        {
        }

        [FieldOrder(0)]
        [FieldBitLength(8)]
        public int ConstantCode { get; set; }

        [FieldOrder(1)]
        [FieldBitLength(8)]
        public int ResponseCode { get; set; }

        public override string ToString()
        {
            return Invariant($"{GetType()} [ConstantCode={ConstantCode}, ResponseCode={ResponseCode}]");
        }
    }
}