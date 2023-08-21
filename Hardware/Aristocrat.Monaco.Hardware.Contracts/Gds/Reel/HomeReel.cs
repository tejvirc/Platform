namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Reel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using BinarySerialization;

    /// <summary>
    ///     The home reels command
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Data will be used once wired up to the game")]
    [Serializable]
    public class HomeReel : GdsSerializableMessage, IEquatable<HomeReel>, ITransactionSource
    {
        /// <summary>
        ///     Creates an instance of <see cref="HomeReel"/>
        /// </summary>
        public HomeReel()
            : base(GdsConstants.ReportId.ReelControllerHomeReel)
        {
        }

        /// <summary> Gets or sets the transaction Id for this event </summary>
        [FieldOrder(0)]
        public byte TransactionId { get; set; }

        /// <summary>Gets or sets the reel id for the status</summary>
        [FieldOrder(1)]
        [FieldBitLength(8)]
        public int ReelId { get; set; }

        /// <summary>Gets or sets the stop to home the reel to</summary>
        [FieldOrder(2)]
        [FieldBitLength(16)]
        public int Stop { get; set; }

        /// <inheritdoc />
        public bool Equals(HomeReel other)
        {
            return !ReferenceEquals(null, other) &&
                   (ReferenceEquals(this, other) || ReelId == other.ReelId && Stop == other.Stop &&
                       TransactionId == other.TransactionId);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) &&
                   (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((HomeReel)obj));
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ReelId;
                hashCode = (hashCode * 397) ^ Stop;
                hashCode = (hashCode * 397) ^ TransactionId.GetHashCode();
                return hashCode;
            }
        }
    }
}