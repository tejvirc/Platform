namespace Aristocrat.Monaco.G2S.Common.PackageManager.CommandHandlers
{
    using Application.Contracts.Authentication;
    using Kernel;
    using Data.Model;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Exceptions;
    using Monaco.Common.Storage;
    using Protocol.Common.Installer;
    using Transfer;
    using Storage;
    using System;
    using System.Data.Entity;
    using Application.Contracts.Localization;
    using Localization.Properties;

    /// <summary>
    ///     Retry upload or download command handler implementation.
    /// </summary>
    public class RetryTransferHandler : IParametersActionHandler<BaseTransferPackageArgs>
    {
        private readonly IAuthenticationService _componentHash;
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IPackageErrorRepository _packageErrorRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly ITransferRepository _transferRepository;
        private readonly ITransferService _transferService;
        private readonly IInstallerService _installerService;
        private readonly IPathMapper _pathMapper;
        private readonly IFileSystemProvider _fileSystemProvider;
        private readonly Action<PackageLog, DbContext> _updatePackageLogCallback;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RetryTransferHandler" /> class.
        /// </summary>
        /// <param name="contextFactory">An <see cref="IMonacoContextFactory" /> instance.</param>
        /// <param name="packageRepository">Package repository instance.</param>
        /// <param name="transferRepository">Transfer repository instance.</param>
        /// <param name="transferService">Transfer service instance.</param>
        /// <param name="packageErrorRepository">Package errors repository.</param>
        /// <param name="componentHash">Component hash calculator</param>
        /// <param name="installerService">Software installer service.</param>
        /// <param name="pathMapper">Path Mapper.</param>
        /// <param name="fileSystemProvider">File system provider.</param>
        /// <param name="updatePackageLogCallback">Update package log callback.</param>
        public RetryTransferHandler(
            IMonacoContextFactory contextFactory,
            IPackageRepository packageRepository,
            ITransferRepository transferRepository,
            ITransferService transferService,
            IPackageErrorRepository packageErrorRepository,
            IAuthenticationService componentHash,
            IInstallerService installerService,
            IPathMapper pathMapper,
            IFileSystemProvider fileSystemProvider,
            Action<PackageLog, DbContext> updatePackageLogCallback)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
            _transferRepository = transferRepository ?? throw new ArgumentNullException(nameof(transferRepository));
            _transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
            _packageErrorRepository = packageErrorRepository ?? throw new ArgumentNullException(nameof(packageErrorRepository));
            _componentHash = componentHash ?? throw new ArgumentNullException(nameof(componentHash));
            _installerService = installerService ?? throw new ArgumentNullException(nameof(installerService));
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));
            _fileSystemProvider = fileSystemProvider ?? throw new ArgumentNullException(nameof(fileSystemProvider));
            _updatePackageLogCallback = updatePackageLogCallback ?? throw new ArgumentNullException(nameof(updatePackageLogCallback));
        }

        /// <inheritdoc />
        public void Execute(BaseTransferPackageArgs parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (string.IsNullOrEmpty(parameter.PackageId))
            {
                throw new ArgumentNullException(nameof(parameter.PackageId));
            }

            if (parameter.ChangeStatusCallback == null)
            {
                throw new ArgumentNullException(nameof(parameter.ChangeStatusCallback));
            }

            using (var context = _contextFactory.Create())
            {
                // 1. Check if package transfer exists.
                var foundTransfer = _transferRepository.GetByPackageId(context, parameter.PackageId);
                if (foundTransfer == null)
                {
                    throw new CommandException(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PackageTransferNotFoundErrorMessage));
                }

                // 2. Check if package exists in case it should be uploaded.
                var found = _packageRepository.GetPackageByPackageId(context, parameter.PackageId);
                if (found == null && foundTransfer.TransferType == TransferType.Upload)
                {
                    throw new CommandException(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PackageNotFoundButTransferUploadTypeErrorMessage));
                }

                // 3. Delete all previous package errors.
                _packageErrorRepository.DeleteByPackageId(context, parameter.PackageId);

                // 4. Retry Upload or Download with already implemented handlers.
                if (foundTransfer.TransferType == TransferType.Upload)
                {
                    var handler = new UploadPackageHandler(
                        _contextFactory,
                        _packageRepository,
                        _transferRepository,
                        _transferService,
                        _packageErrorRepository,
                        _installerService,
                        _fileSystemProvider);

                    handler.Execute(
                        new TransferPackageArgs(
                            parameter.PackageId,
                            foundTransfer,
                            parameter.ChangeStatusCallback,
                            parameter.CancellationToken,
                            parameter.PackageLogEntity));
                }
                else
                {
                    var handler = new DownloadPackageHandler(
                        _contextFactory,
                        _transferRepository,
                        _transferService,
                        _packageErrorRepository,
                        _componentHash,
                        _pathMapper,
                        _fileSystemProvider,
                        _updatePackageLogCallback);

                    handler.Execute(
                        new TransferPackageArgs(
                            parameter.PackageId,
                            foundTransfer,
                            parameter.ChangeStatusCallback,
                            parameter.CancellationToken,
                            parameter.PackageLogEntity));
                }
            }
        }
    }
}
