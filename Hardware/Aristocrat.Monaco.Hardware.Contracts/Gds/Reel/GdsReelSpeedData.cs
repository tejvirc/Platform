namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Reel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using BinarySerialization;
    using Contracts.Reel.ControlData;

    /// <summary>
    ///     The reel speed data
    /// </summary>
    [Serializable]
    public class GdsReelSpeedData : IEquatable<GdsReelSpeedData>
    {
        /// <summary>
        ///     Creates the reel speed data
        /// </summary>
        public GdsReelSpeedData()
        {
        }

        /// <summary>
        ///     Creates the reel speed data
        /// </summary>
        /// <param name="data">The Contract.Reel.ReelSpeedData</param>
        public GdsReelSpeedData(ReelSpeedData data)
        {
            ReelId = data.ReelId;
            Speed = data.Speed;
        }

        /// <summary>
        ///     Gets the reel id for this data
        /// </summary>
        [FieldOrder(0)]
        public int ReelId { get; set; }

        /// <summary>
        ///     The speed to set for the reel
        /// </summary>
        [FieldOrder(1)]
        public int Speed { get; set; }

        /// <inheritdoc />
        public bool Equals(GdsReelSpeedData other)
        {
            return !ReferenceEquals(null, other) &&
                   (ReferenceEquals(this, other) ||
                    ReelId == other.ReelId &&
                    Speed == other.Speed);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) &&
                   (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((GdsReelSpeedData)obj));
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ReelId;
                hashCode = (hashCode * 397) ^ Speed;
                return hashCode;
            }
        }
    }
}
