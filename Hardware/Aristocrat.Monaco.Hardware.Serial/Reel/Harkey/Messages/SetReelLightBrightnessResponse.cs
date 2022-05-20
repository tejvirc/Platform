namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    [Serializable]
    public class SetReelLightBrightnessResponse : HarkeySerializableMessage, ISequencedCommand, ISimpleAckResponse
    {
        public static readonly byte Length = 4;

        public SetReelLightBrightnessResponse()
            : base(MessageMaskType.CommandResponse, HarkeyCommandId.SetReelBrightness, Length, false)
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