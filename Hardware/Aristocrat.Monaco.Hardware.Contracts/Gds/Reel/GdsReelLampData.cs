namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Reel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using BinarySerialization;
    using Contracts.Reel.ControlData;

    /// <summary>
    ///     The reel lamp data
    /// </summary>
    [Serializable]
    public class GdsReelLampData : IEquatable<GdsReelLampData>
    {
        /// <summary>
        ///     Creates the reel lamp data
        /// </summary>
        public GdsReelLampData()
        {
        }

        /// <summary>
        ///     Creates the reel lamp data
        /// </summary>
        /// <param name="data">The Contract.Reel.ReelLampData</param>
        public GdsReelLampData(ReelLampData data)
        {
            LampId = data.Id;
            IsLampOn = data.IsLampOn;
            RedIntensity = data.Color.R;
            GreenIntensity = data.Color.G;
            BlueIntensity = data.Color.B;
        }

        /// <summary>
        ///     Gets the lamp id for this data
        /// </summary>
        [FieldOrder(0)]
        public int LampId { get; set; }

        /// <summary>
        ///     Whether the reel lamp is turned on or not
        /// </summary>
        [FieldOrder(1)]
        public bool IsLampOn { get; set; }

        /// <summary>
        ///     The red intensity
        /// </summary>
        public byte RedIntensity { get; set; }

        /// <summary>
        ///     The green intensity
        /// </summary>
        public byte GreenIntensity { get; set; }

        /// <summary>
        ///     The blue intensity
        /// </summary>
        public byte BlueIntensity { get; set; }

        /// <inheritdoc />
        public bool Equals(GdsReelLampData other)
        {
            return !ReferenceEquals(null, other) &&
                   (ReferenceEquals(this, other) ||
                    LampId == other.LampId &&
                    IsLampOn == other.IsLampOn &&
                    RedIntensity == other.RedIntensity &&
                    GreenIntensity == other.GreenIntensity &&
                    BlueIntensity == other.BlueIntensity);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) &&
                   (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((GdsReelLampData)obj));
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = LampId;
                hashCode = (hashCode * 397) ^ IsLampOn.GetHashCode();
                hashCode = (hashCode * 397) ^ RedIntensity.GetHashCode();
                hashCode = (hashCode * 397) ^ GreenIntensity.GetHashCode();
                hashCode = (hashCode * 397) ^ BlueIntensity.GetHashCode();
                return hashCode;
            }
        }
    }
}
