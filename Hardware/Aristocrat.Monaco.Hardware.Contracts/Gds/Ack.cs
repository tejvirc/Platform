namespace Aristocrat.Monaco.Hardware.Contracts.Gds
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>
    /// ACK command structure
    /// </summary>
    [Serializable]
    public class Ack : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public Ack() : base(GdsConstants.ReportId.Ack) {}

        /// <summary>Gets or sets the reserved bytes for future use 1.</summary>
        /// <value>The reserved bytes for future use 1.</value>
        [FieldOrder(0)]
        [FieldBitLength(7)]
        public byte Reserved1 { get; set; }

        /// <summary>
        /// Tells whether host must reset the current transaction id.
        /// </summary>
        /// <remarks>true - The device MUST re-sync its Transaction ID and resend the pending report.
        /// <para/>
        /// false - The device MUST NOT re-sync its Transaction ID. This is an acknowledgement.</remarks>
        [FieldOrder(1)]
        [FieldBitLength(1)]
        public bool Resync { get; set; }

        /// <summary>Gets or sets the identifier of the transaction.</summary>
        /// <value>The identifier of the transaction.</value>
        [FieldOrder(2)]
        public byte TransactionId { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [Resync={Resync}, TransactionId={TransactionId}]");
        }
    }
}