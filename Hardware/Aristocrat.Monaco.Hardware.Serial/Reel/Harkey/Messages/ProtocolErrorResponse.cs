namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    [Serializable]
    public class ProtocolErrorResponse : HarkeySerializableMessage, ISimpleAckResponse
    {
        public static readonly byte Length = 2;

        public ProtocolErrorResponse()
            : base(MessageMaskType.ProtocolErrorResponse, HarkeyCommandId.ProtocolErrorResponse, Length, false)
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