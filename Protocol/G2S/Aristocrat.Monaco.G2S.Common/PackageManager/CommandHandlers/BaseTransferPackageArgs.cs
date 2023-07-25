namespace Aristocrat.Monaco.G2S.Common.PackageManager.CommandHandlers
{
    using System;
    using System.Threading;
    using G2S.Data.Model;

    /// <summary>
    ///     Base transfer package arguments
    /// </summary>
    public class BaseTransferPackageArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseTransferPackageArgs" /> class.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="changeStatusCallback">The change status callback.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <param name="packageLog">The package log.</param>
        public BaseTransferPackageArgs(
            string packageId,
            Action<PackageTransferEventArgs> changeStatusCallback,
            CancellationToken ct,
            PackageLog packageLog)
        {
            PackageId = packageId ?? throw new ArgumentNullException(nameof(packageId));
            ChangeStatusCallback =
                changeStatusCallback ?? throw new ArgumentNullException(nameof(changeStatusCallback));
            CancellationToken = ct;
            PackageLogEntity = packageLog ?? throw new ArgumentNullException(nameof(packageLog));
        }

        /// <summary>
        ///     Gets the package identifier.
        /// </summary>
        /// <value>
        ///     The package identifier.
        /// </value>
        public string PackageId { get; }

        /// <summary>
        ///     Gets the change status callback.
        /// </summary>
        /// <value>
        ///     The change status callback.
        /// </value>
        public Action<PackageTransferEventArgs> ChangeStatusCallback { get; }

        /// <summary>
        ///     Gets the cancellation token.
        /// </summary>
        public CancellationToken CancellationToken { get; }
        
        /// <summary>
        ///     Gets the package log.
        /// </summary>
        /// <value>
        ///     The package log entity.
        /// </value>
        public PackageLog PackageLogEntity { get; }
    }
}