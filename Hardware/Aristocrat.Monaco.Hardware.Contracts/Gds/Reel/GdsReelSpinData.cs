namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Reel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using BinarySerialization;
    using Contracts.Reel;

    /// <summary>
    ///     The reel spin data
    /// </summary>
    [Serializable]
    public class GdsReelSpinData : IEquatable<GdsReelSpinData>
    {
        /// <summary>
        ///     Creates the reel spin data
        /// </summary>
        public GdsReelSpinData()
        {
        }

        /// <summary>
        ///     Creates the reel spin data
        /// </summary>
        /// <param name="data">The Contract.Reel.ReelSpinData</param>
        public GdsReelSpinData(ReelSpinData data)
        {
            ReelId = data.ReelId;
            Direction = data.Direction;
            Rpm = data.Rpm;
            Step = data.Step;
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

        /// <inheritdoc />
        public bool Equals(GdsReelSpinData other)
        {
            return !ReferenceEquals(null, other) &&
                   (ReferenceEquals(this, other) ||
                    ReelId == other.ReelId &&
                    Direction == other.Direction &&
                    Rpm == other.Rpm &&
                    Step == other.Step);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) &&
                   (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((GdsReelSpinData)obj));
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
                return hashCode;
            }
        }
    }
}
