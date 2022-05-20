namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    [Serializable]
    public class Rm6VersionResponse : HarkeySerializableMessage, ISequencedCommand, IVersionProvider
    {
        public static readonly byte Length = 6;

        public Rm6VersionResponse()
            : base(MessageMaskType.CommandResponse, HarkeyCommandId.GetRm6Version, Length, false)
        {
        }

        [FieldOrder(0)]
        [FieldBitLength(8)]
        public int MajorVersion { get; set; }

        [FieldOrder(1)]
        [FieldBitLength(8)]
        public int MinorVersion { get; set; }

        [FieldOrder(2)]
        [FieldBitLength(8)]
        public int MiniVersion { get; set; }

        public override string ToString()
        {
            return Invariant($"{GetType()} [Version=v{MajorVersion}.{MinorVersion}.{MiniVersion}]");
        }
    }
}