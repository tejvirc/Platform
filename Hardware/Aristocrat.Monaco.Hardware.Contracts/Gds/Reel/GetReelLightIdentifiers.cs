namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Reel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using BinarySerialization;

    /// <summary>
    ///     The get reel light identifiers command
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Data will be used once wired up to the game")]
    [Serializable]
    public class GetReelLightIdentifiers : GdsSerializableMessage, IEquatable<GetReelLightIdentifiers>, ITransactionSource
    {
        /// <summary>
        ///     Creates an instance of <see cref="GetReelLightIdentifiers"/>
        /// </summary>
        public GetReelLightIdentifiers()
            : base(GdsConstants.ReportId.ReelControllerGetLightIds)
        {
        }

        /// <summary> Gets or sets the transaction Id for this event </summary>
        [FieldOrder(0)]
        public byte TransactionId { get; set; }

        /// <inheritdoc />
        public bool Equals(GetReelLightIdentifiers other)
        {
            return !ReferenceEquals(null, other) &&
                   (ReferenceEquals(this, other) ||
                       TransactionId == other.TransactionId);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) &&
                   (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((GetReelLightIdentifiers)obj));
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            var hashCode = TransactionId.GetHashCode();
            return hashCode;
        }
    }
}