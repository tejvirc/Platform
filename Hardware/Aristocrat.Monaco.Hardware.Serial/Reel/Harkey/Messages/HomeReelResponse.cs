namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    [Serializable]
    public class HomeReelResponse : HarkeySerializableMessage, ISequencedCommand
    {
        public HomeReelResponse()
            : base(MessageMaskType.CommandResponse, HarkeyCommandId.HomeReel, 4, false)
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