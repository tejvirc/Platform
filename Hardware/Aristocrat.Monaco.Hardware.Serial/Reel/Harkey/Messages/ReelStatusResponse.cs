namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    [Serializable]
    public class ReelStatusResponse : HarkeySerializableMessage, ISequencedCommand
    {
        public static readonly byte Length = 10;

        public ReelStatusResponse()
            : base(MessageMaskType.CommandResponse, HarkeyCommandId.GetStatus, Length, false)
        {
        }

        [FieldOrder(0)]
        [FieldBitLength(8)]
        public GlobalStatus GlobalStatus { get; set; }

        [FieldOrder(1)]
        [FieldBitLength(8)]
        public ReelStatus Reel1Status { get; set; }

        [FieldOrder(2)]
        [FieldBitLength(8)]
        public ReelStatus Reel2Status { get; set; }

        [FieldOrder(3)]
        [FieldBitLength(8)]
        public ReelStatus Reel3Status { get; set; }

        [FieldOrder(4)]
        [FieldBitLength(8)]
        public ReelStatus Reel4Status { get; set; }

        [FieldOrder(5)]
        [FieldBitLength(8)]
        public ReelStatus Reel5Status { get; set; }

        [FieldOrder(6)]
        [FieldBitLength(8)]
        public ReelStatus Reel6Status { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant(
                $"{GetType()} [GlobalStatus={GlobalStatus}, Reel1Status={Reel1Status}, Reel2Status={Reel2Status}, Reel3Status={Reel3Status}, Reel4Status={Reel4Status}, Reel5Status={Reel5Status}, Reel6Status={Reel6Status}]");
        }
    }
}