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

        [Ignore]
        public int[] AllReelLights { get; } = new int[HarkeyConstants.MaxReelId];

        [FieldOrder(0)]
        [FieldBitLength(8)]
        public int Reel1Lights
        {
            get => AllReelLights[0];
            set => AllReelLights[0] = value;
        }

        [FieldOrder(1)]
        [FieldBitLength(8)]
        public int Reel2Lights
        {
            get => AllReelLights[1];
            set => AllReelLights[1] = value;
        }

        [FieldOrder(2)]
        [FieldBitLength(8)]
        public int Reel3Lights
        {
            get => AllReelLights[2];
            set => AllReelLights[2] = value;
        }

        [FieldOrder(3)]
        [FieldBitLength(8)]
        public int Reel4Lights
        {
            get => AllReelLights[3];
            set => AllReelLights[3] = value;
        }

        [FieldOrder(4)]
        [FieldBitLength(8)]
        public int Reel5Lights
        {
            get => AllReelLights[4];
            set => AllReelLights[4] = value;
        }

        [FieldOrder(5)]
        [FieldBitLength(8)]
        public int Reel6Lights
        {
            get => AllReelLights[5];
            set => AllReelLights[5] = value;
        }
    }
}