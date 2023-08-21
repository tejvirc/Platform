namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Reel
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>
    ///     The status for a reel spinning
    /// </summary>
    [Serializable]
    public class ReelSpinningStatus : GdsSerializableMessage, ITransactionSource
    {
        /// <summary>
        ///     Creates an instance of <see cref="ReelSpinningStatus"/>
        /// </summary>
        public ReelSpinningStatus()
            : base(GdsConstants.ReportId.ReelControllerSpinStatus)
        {
        }

        /// <summary> Gets or sets the transaction Id for this event </summary>
        [FieldOrder(0)]
        public byte TransactionId { get; set; }

        /// <summary>Gets or sets the reel id for the status</summary>
        [FieldOrder(1)]
        [FieldBitLength(8)]
        public int ReelId { get; set; }

        /// <summary>Gets or sets the reserved 1.</summary>
        /// <value>The reserved 1.</value>
        [FieldOrder(2)]
        [FieldBitLength(3)]
        public byte Reserved1 { get; set; }

        /// <summary> A reel has started slow spinning </summary>
        [FieldOrder(3)]
        [FieldBitLength(1)]
        public bool SlowSpinning { get; set; }

        /// <summary> A reel has started spinning </summary>
        [FieldOrder(4)]
        [FieldBitLength(1)]
        public bool Spinning { get; set; }

        /// <summary> A reel is accelerating </summary>
        [FieldOrder(5)]
        [FieldBitLength(1)]
        public bool Accelerating { get; set; }

        /// <summary> A reel is slowing down </summary>
        [FieldOrder(6)]
        [FieldBitLength(1)]
        public bool Decelerating { get; set; }

        /// <summary> A reel is idle at a known stop </summary>
        [FieldOrder(7)]
        [FieldBitLength(1)]
        public bool IdleAtStop { get; set; }

        /// <summary> The stop position of a reel if at a known stop </summary>
        [FieldOrder(9)]
        [FieldBitLength(8)]
        public int Stop { get; set; }

        /// <summary> The step position of a reel if at a known stop </summary>
        [FieldOrder(10)]
        [FieldBitLength(8)]
        public int Step { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant(
                $"{GetType()} [ReelId={ReelId},Reserved1={Reserved1},SlowSpinning={SlowSpinning},Spinning={Spinning},Accelerating={Accelerating},Decelerating={Decelerating},IdleAtStop={IdleAtStop},Stop={Stop},Step={Step}]");
        }
    }
}