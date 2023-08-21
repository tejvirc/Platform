namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    [Serializable]
    public class SetFaultsResponse : HarkeySerializableMessage, ISequencedCommand
    {
        public static readonly byte Length = 5;

        public SetFaultsResponse()
            : base(MessageMaskType.CommandResponse, HarkeyCommandId.SetFaults, Length, false)
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