namespace Aristocrat.Monaco.G2S.Common.PackageManager.CommandHandlers
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using Application.Contracts.Authentication;
    using Data.Model;
    using Kernel;
    using log4net;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Storage;
    using Storage;
    using Transfer;

    /// <summary>
    ///     Download package command handler implementation.
    /// </summary>
    public class DownloadPackageHandler : BasePackageCommandHandler, IParametersActionHandler<TransferPackageArgs>
    {
        private const string DownloadsDirectoryPath = "/Downloads";
        private const string HashAlgorithm = @"SHA1";
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IAuthenticationService _hashCalculator;
        private readonly ITransferRepository _transferRepository;
        private readonly ITransferService _transferService;
        private readonly IPathMapper _pathMapper;
        private readonly IFileSystemProvider _fileSystemProvider;
        private readonly Action<PackageLog, DbContext> _updatePackageLogCallback;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DownloadPackageHandler" /> class.
        /// </summary>
        /// <param name="contextFactory">An <see cref="IMonacoContextFactory" /> instance.</param>
        /// <param name="transferRepository">Transfer repository instance.</param>
        /// <param name="transferService">Transfer service instance.</param>
        /// <param name="packageErrorRepository">Package errors repository.</param>
        /// <param name="hashCalculator">Hash calculator</param>
        /// <param name="pathMapper">Path mapper.</param>
        /// <param name="fileSystemProvider">File system provider.</param>
        /// <param name="updatePackageLogCallback">Update package log callback.</param>
        public DownloadPackageHandler(
            IMonacoContextFactory contextFactory,
            ITransferRepository transferRepository,
            ITransferService transferService,
            IPackageErrorRepository packageErrorRepository,
            IAuthenticationService hashCalculator,
            IPathMapper pathMapper,
            IFileSystemProvider fileSystemProvider,
            Action<PackageLog, DbContext> updatePackageLogCallback)
            : base(contextFactory, packageErrorRepository)
        {
            _hashCalculator = hashCalculator ?? throw new ArgumentNullException(nameof(hashCalculator));
            _transferRepository = transferRepository ?? throw new ArgumentNullException(nameof(transferRepository));
            _transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));
            _fileSystemProvider = fileSystemProvider ?? throw new ArgumentNullException(nameof(fileSystemProvider));
            _updatePackageLogCallback = updatePackageLogCallback ?? throw new ArgumentNullException(nameof(updatePackageLogCallback));
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

            if (parameter.PackageLogEntity == null)
            {
                throw new ArgumentNullException(nameof(parameter.PackageLogEntity));
            }

            if (parameter.ChangeStatusCallback == null)
            {
                throw new ArgumentNullException(nameof(parameter.ChangeStatusCallback));
            }

            using (var context = ContextFactory.CreateDbContext())
            {
                var packageEntity = parameter.PackageLogEntity;

                // 1. Add Transfer Entity record to database.
                var addedToDbTransferEntity = _transferRepository.GetByPackageId(context,parameter.TransferEntity.PackageId);
                FireCallBack(addedToDbTransferEntity, parameter.PackageId, parameter.ChangeStatusCallback);

                // 2. Change Transfer state and notify.
                addedToDbTransferEntity.State = TransferState.InProgress;
                FireCallBack(addedToDbTransferEntity, parameter.PackageId, parameter.ChangeStatusCallback);
                var fileName = string.Empty;
                try
                {
                    // 3. Download package.
                    var temporaryDirectory = _pathMapper.GetDirectory(DownloadsDirectoryPath);

                    fileName = Path.Combine(temporaryDirectory.FullName, parameter.PackageId);
                    fileName += Path.GetExtension(parameter.TransferEntity.Location);

                    var archiveSize = DownloadPackageToFile(addedToDbTransferEntity, parameter.PackageId, fileName, parameter.CancellationToken);

                    // 4. Change Transfer state and notify.
                    addedToDbTransferEntity.State = TransferState.Completed;
                    addedToDbTransferEntity.Size = archiveSize;
                    _transferRepository.Update(context, addedToDbTransferEntity);

                    CalculateHash(packageEntity, fileName);

                    // 5. update package
                    packageEntity.Size = archiveSize;
                    _updatePackageLogCallback(packageEntity, context);

                    FireCallBack(addedToDbTransferEntity, parameter.PackageId, parameter.ChangeStatusCallback, packageEntity.State, fileName);
                }
                catch(IOException io)
                {
                    addedToDbTransferEntity.Exception = 6;
                    ExceptionHandler(parameter, io, fileName, addedToDbTransferEntity);
                }
                catch (AggregateException ae)
                {
                    var exceptions = new StringBuilder();
                    var aggException = ae.Flatten();
                    foreach (var ex in aggException.InnerExceptions)
                    {
                        exceptions.Append($"{ex.Message} {ex} ");
                    }

                    addedToDbTransferEntity.Exception = 7;
                    ExceptionHandler(parameter, new Exception(exceptions.ToString()), fileName, addedToDbTransferEntity);
                }
                catch (OperationCanceledException oce)
                {
                    addedToDbTransferEntity.Exception = 4;
                    ExceptionHandler(parameter, oce, fileName, addedToDbTransferEntity);
                }
                catch (FtpServiceNotAvailableException ftpExc)
                {
                    addedToDbTransferEntity.Exception = 2;
                    ExceptionHandler(parameter, ftpExc, fileName, addedToDbTransferEntity);
                }
                catch (Exception exc)
                {
                    addedToDbTransferEntity.Exception = 7;
                    ExceptionHandler(parameter, exc, fileName, addedToDbTransferEntity);
                }
            }
        }

        private void ExceptionHandler(
            TransferPackageArgs parameter,
            Exception exc,
            string fileName,
            TransferEntity addedToDbTransferEntity)
        {
            Logger.Error($"Failed to download package: {exc}");
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                Logger.Error($"Deleting package file: {fileName}");
                File.Delete(fileName);
            }

            HandleException(addedToDbTransferEntity,parameter.PackageId,parameter.ChangeStatusCallback,exc);
        }

        private long DownloadPackageToFile(
            TransferEntity transferEntity,
            string packageId,
            string fileName,
            CancellationToken ct)
        {
            long archiveSize;

            using (var stream = _fileSystemProvider.GetFileWriteStream(fileName))
            {
                _transferService.Download(packageId, transferEntity.Location, transferEntity.Parameters, stream, ct);
                archiveSize = stream.Length;
            }

            return archiveSize;
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
            PackageState? packageState = null,
            string filePath = null)
        {
            InvokeCallBack(changeStatusCallback, packageId, transferEntity, packageState, filePath);
        }

        private void CalculateHash(PackageLog package, string fileName)
        {
            using (var fileStream = _fileSystemProvider.GetFileReadStream(fileName))
            {
                package.Hash = ConvertExtensions.ToPackedHexString(_hashCalculator.ComputeHash(fileStream, HashAlgorithm));
            }
        }
    }
}
