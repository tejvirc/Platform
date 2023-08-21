namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Reel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using BinarySerialization;
    using Contracts.Reel;
    using Contracts.Reel.ControlData;

    /// <summary>
    ///     The nudge reel data
    /// </summary>
    [Serializable]
    public class GdsNudgeReelData : IEquatable<GdsNudgeReelData>
    {
        /// <summary>
        ///     Creates the nudge reel data
        /// </summary>
        public GdsNudgeReelData()
        {
        }

        /// <summary>
        ///     Creates the nudge reel data
        /// </summary>
        /// <param name="data">The Contract.Reel.NudgeReelData</param>
        public GdsNudgeReelData(NudgeReelData data)
        {
            ReelId = data.ReelId;
            Direction = data.Direction;
            Rpm = data.Rpm;
            Step = data.Step;
            Delay = data.Delay;
        }

        /// <summary>
        ///     Gets the reel id for this data
        /// </summary>
        [FieldOrder(0)]
        public int ReelId { get; set; }

        /// <summary>
        ///     Gets the spin direction to use
        /// </summary>
        [FieldOrder(1)]
        public SpinDirection Direction { get; set; }

        /// <summary>
        ///     The RPMs to spin the reels
        /// </summary>
        [FieldOrder(2)]
        public int Rpm { get; set; }

        /// <summary>
        ///     Gets the step for the reel
        /// </summary>
        [FieldOrder(3)]
        public int Step { get; set; }

        /// <summary>
        ///     Gets the delay for the reel
        /// </summary>
        [FieldOrder(4)]
        public int Delay { get; set; }

        /// <inheritdoc />
        public bool Equals(GdsNudgeReelData other)
        {
            return !ReferenceEquals(null, other) &&
                   (ReferenceEquals(this, other) ||
                    ReelId == other.ReelId &&
                    Direction == other.Direction &&
                    Rpm == other.Rpm &&
                    Step == other.Step &&
                    Delay == other.Delay);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) &&
                   (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((GdsNudgeReelData)obj));
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ReelId;
                hashCode = (hashCode * 397) ^ (int)Direction;
                hashCode = (hashCode * 397) ^ Rpm;
                hashCode = (hashCode * 397) ^ Step;
                hashCode = (hashCode * 397) ^ Delay;
                return hashCode;
            }
        }
    }
}
