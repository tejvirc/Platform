namespace Aristocrat.Monaco.G2S.Common.PackageManager.CommandHandlers
{
    using System;
    using System.Reflection;
    using Data.Model;
    using log4net;
    using Monaco.Common.Storage;
    using Storage;

    /// <summary>
    ///     Base implementation of package command handler with some common functionality.
    /// </summary>
    public class BasePackageCommandHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        ///     Initializes a new instance of the <see cref="BasePackageCommandHandler" /> class.
        /// </summary>
        /// <param name="contextFactory">An <see cref="IMonacoContextFactory" /> instance.</param>
        /// <param name="packageErrorRepository">The package error repository.</param>
        public BasePackageCommandHandler(
            IMonacoContextFactory contextFactory,
            IPackageErrorRepository packageErrorRepository)
        {
            ContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            PackageErrorRepository =
                packageErrorRepository ?? throw new ArgumentNullException(nameof(packageErrorRepository));
        }

        /// <summary>
        ///     Gets package error repository instance.
        /// </summary>
        protected IPackageErrorRepository PackageErrorRepository { get; }

        /// <summary>
        ///     Gets db context factory.
        /// </summary>
        protected IMonacoContextFactory ContextFactory { get; }

        /// <summary>
        ///     Handles exception during command execution.
        /// </summary>
        /// <param name="changeStatusCallback">Status change callback.</param>
        /// <param name="packageId">The package identifier</param>
        /// <param name="transferEntity">The transfer entity</param>
        /// <param name="exception">Exception instance.</param>
        protected void HandleException(
            Action<PackageTransferEventArgs> changeStatusCallback,
            string packageId,
            TransferEntity transferEntity,
            Exception exception)
        {
            var errorCode = transferEntity.Exception == 0 ? 7 : transferEntity.Exception;
            var errorMessage = "Package Error: Package " + packageId + "  Message " + exception.Message + " : " +
                               exception.InnerException;
            Logger.Error(errorMessage);

            var errorEntity = new PackageError
            {
                PackageId = packageId, ErrorCode = errorCode, ErrorMessage = errorMessage
            };

            using (var context = ContextFactory.CreateDbContext())
            {
                PackageErrorRepository.Add(context, errorEntity);
            }

            InvokeCallBack(changeStatusCallback, packageId, transferEntity, PackageState.Error);
        }

        /// <summary>
        ///     Invokes the callback
        /// </summary>
        /// <param name="changeStatusCallback">Status change callback.</param>
        /// <param name="packageId">The package identifier</param>
        /// <param name="transferEntity">The transfer entity</param>
        /// <param name="packageState">The package state</param>
        /// <param name="filePath">The file path</param>
        protected void InvokeCallBack(
            Action<PackageTransferEventArgs> changeStatusCallback,
            string packageId,
            TransferEntity transferEntity,
            PackageState? packageState = null,
            string filePath = null)
        {
            var args = new PackageTransferEventArgs
            {
                PackageId = packageId,
                PackageState = packageState,
                TransferId = transferEntity.TransferId,
                TransferState = transferEntity.State,
                PackageFilePath = filePath,
                Size = transferEntity.Size
            };

            changeStatusCallback(args);
        }
    }
}