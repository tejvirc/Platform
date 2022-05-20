namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Reel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using BinarySerialization;

    /// <summary>
    ///     The tilt reels command
    /// </summary>
    [Serializable]
    public class TiltReels : GdsSerializableMessage, IEquatable<TiltReels>, ITransactionSource
    {
        /// <summary>
        ///     Creates an instance of <see cref="TiltReels"/>
        /// </summary>
        public TiltReels()
            : base(GdsConstants.ReportId.ReelControllerTiltReels)
        {
        }

        /// <summary> Gets or sets the transaction Id for this event</summary>
        [FieldOrder(0)]
        public byte TransactionId { get; set; }

        /// <inheritdoc />
        public bool Equals(TiltReels other)
        {
            return !ReferenceEquals(null, other) &&
                   (ReferenceEquals(this, other) ||
                    TransactionId == other.TransactionId);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) &&
                   (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((TiltReels)obj));
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            return TransactionId.GetHashCode();
        }
    }
}
