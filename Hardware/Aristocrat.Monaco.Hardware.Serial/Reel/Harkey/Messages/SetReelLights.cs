namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;
    using BinarySerialization;

    [Serializable]
    public class SetReelLights : HarkeySerializableMessage
    {
        public SetReelLights()
            : base(MessageMaskType.Command, HarkeyCommandId.SetReelLights, 7, true)
        {
        }

        [FieldOrder(0)]
        [FieldBitLength(8)]
        public int Reel1Lights { get; set; }

        [FieldOrder(1)]
        [FieldBitLength(8)]
        public int Reel2Lights { get; set; }

        [FieldOrder(2)]
        [FieldBitLength(8)]
        public int Reel3Lights { get; set; }

        [FieldOrder(3)]
        [FieldBitLength(8)]
        public int Reel4Lights { get; set; }

        [FieldOrder(4)]
        [FieldBitLength(8)]
        public int Reel5Lights { get; set; }

        [FieldOrder(5)]
        [FieldBitLength(8)]
        public int Reel6Lights { get; set; }
    }
}