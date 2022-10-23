namespace Aristocrat.Monaco.G2S.Common.PackageManager.CommandHandlers
{
    using System;
    using Application.Contracts.Localization;
    using Data.Model;
    using Localization.Properties;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Exceptions;
    using Monaco.Common.Storage;
    using Protocol.Common.Installer;
    using Storage;
    using Transfer;

    /// <summary>
    ///     Upload package command handler implementation.
    /// </summary>
    public class UploadPackageHandler : BasePackageCommandHandler, IParametersActionHandler<TransferPackageArgs>
    {
        private readonly IPackageRepository _packageRepository;
        private readonly ITransferRepository _transferRepository;
        private readonly ITransferService _transferService;
        private readonly IInstallerService _installerService;
        private readonly IFileSystemProvider _fileSystemProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UploadPackageHandler" /> class.
        /// </summary>
        /// <param name="contextFactory">An <see cref="IMonacoContextFactory" /> instance.</param>
        /// <param name="packageRepository">Package repository instance.</param>
        /// <param name="transferRepository">Transfer repository instance.</param>
        /// <param name="transferService">Transfer service instance.</param>
        /// <param name="packageErrorRepository">Package errors repository.</param>
        /// <param name="installerService">Software installer Service.</param>
        /// <param name="fileSystemProvider">File system provider.</param>
        public UploadPackageHandler(
            IMonacoContextFactory contextFactory,
            IPackageRepository packageRepository,
            ITransferRepository transferRepository,
            ITransferService transferService,
            IPackageErrorRepository packageErrorRepository,
            IInstallerService installerService,
            IFileSystemProvider fileSystemProvider)
            : base(contextFactory, packageErrorRepository)
        {
            _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
            _transferRepository = transferRepository ?? throw new ArgumentNullException(nameof(transferRepository));
            _transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
            _installerService = installerService ?? throw new ArgumentNullException(nameof(installerService));
            _fileSystemProvider = fileSystemProvider ?? throw new ArgumentNullException(nameof(fileSystemProvider));
        }

        /// <inheritdoc />
        public void Execute(TransferPackageArgs parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (string.IsNullOrEmpty(parameter.PackageId))
            {
                throw new ArgumentNullException(nameof(parameter.PackageId));
            }

            if (parameter.TransferEntity == null)
            {
                throw new ArgumentNullException(nameof(parameter.TransferEntity));
            }

            if (parameter.ChangeStatusCallback == null)
            {
                throw new ArgumentNullException(nameof(parameter.ChangeStatusCallback));
            }

            using (var context = ContextFactory.CreateDbContext())
            {
                // 1. Check if package already exists.
                var found = _packageRepository.GetPackageByPackageId(context, parameter.PackageId);
                if (found == null)
                {
                    throw new CommandException(
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PackageNotFoundErrorMessage));
                }

                // 2. Create the package.
                var package = _installerService.BundleSoftwarePackage(parameter.PackageId, false);

                var fileName = package.name;
                found.Size = package.size;
                _packageRepository.Update(context, found);

                _transferRepository.Add(context, parameter.TransferEntity);
                FireCallBack(
                    parameter.TransferEntity,
                    parameter.PackageId,
                    parameter.ChangeStatusCallback);

                try
                {
                    // 3. Change Transfer state and notify.
                    parameter.TransferEntity.State = TransferState.InProgress;
                    FireCallBack(
                        parameter.TransferEntity,
                        parameter.PackageId,
                        parameter.ChangeStatusCallback);

                    // 4. Upload package
                    using (var stream = _fileSystemProvider.GetFileReadStream(fileName))
                    {
                        _transferService.Upload(
                            parameter.PackageId,
                            parameter.TransferEntity.Location,
                            parameter.TransferEntity.Parameters,
                            stream,
                            parameter.CancellationToken);
                    }

                    // 5. Change Transfer state and notify.
                    parameter.TransferEntity.State = TransferState.Completed;
                    parameter.TransferEntity.TransferCompletedDateTime = DateTime.UtcNow;
                    _transferRepository.Update(context, parameter.TransferEntity);
                    FireCallBack(
                        parameter.TransferEntity,
                        parameter.PackageId,
                        parameter.ChangeStatusCallback);
                }
                catch (Exception exc)
                {
                    HandleException(
                        parameter.TransferEntity,
                        parameter.PackageId,
                        parameter.ChangeStatusCallback,
                        exc);
                }
            }
        }

        private void HandleException(
            TransferEntity transferEntity,
            string packageId,
            Action<PackageTransferEventArgs> changeStatusCallback,
            Exception exc)
        {
            transferEntity.State = TransferState.Failed;
            var errorCode = transferEntity.Exception == 0 ? 7 : transferEntity.Exception;
            transferEntity.Exception = errorCode;
            using (var context = ContextFactory.CreateDbContext())
            {
                _transferRepository.Update(context, transferEntity);
            }

            HandleException(changeStatusCallback, packageId, transferEntity, exc);
        }

        private void FireCallBack(
            TransferEntity transferEntity,
            string packageId,
            Action<PackageTransferEventArgs> changeStatusCallback,
            PackageState? packageState = null)
        {
            InvokeCallBack(changeStatusCallback, packageId, transferEntity, packageState);
        }
    }
}