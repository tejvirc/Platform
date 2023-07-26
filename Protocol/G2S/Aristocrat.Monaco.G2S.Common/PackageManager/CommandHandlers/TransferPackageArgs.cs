namespace Aristocrat.Monaco.G2S.Common.PackageManager.CommandHandlers
{
    using System;
    using System.Threading;
    using G2S.Data.Model;
    using Storage;

    /// <summary>
    ///     Transfer package arguments
    /// </summary>
    public class TransferPackageArgs : BaseTransferPackageArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransferPackageArgs" /> class.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="transferEntity">The transfer entity.</param>
        /// <param name="changeStatusCallback">The change status callback.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <param name="packageLog">The package log.</param>
        /// <param name="deviceId">Device Id.</param>
        public TransferPackageArgs(
            string packageId,
            TransferEntity transferEntity,
            Action<PackageTransferEventArgs> changeStatusCallback,
            CancellationToken ct,
            PackageLog packageLog,
            int deviceId = 0)
            : base(packageId, changeStatusCallback, ct, packageLog)
        {
            TransferEntity = transferEntity ?? throw new ArgumentNullException(nameof(transferEntity));
            TransferEntity.State = TransferState.Pending;
            DeviceId = deviceId;
        }

        /// <summary>
        ///     Gets the device identifier.
        /// </summary>
        /// <value>
        ///     The device identifier.
        /// </value>
        public int DeviceId { get; }

        /// <summary>
        ///     Gets the transfer entity.
        /// </summary>
        /// <value>
        ///     The transfer entity.
        /// </value>
        public TransferEntity TransferEntity { get; }
    }
}