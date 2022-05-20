namespace Aristocrat.Monaco.G2S.Common.PackageManager.CommandHandlers
{
    using System;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Exceptions;
    using Monaco.Common.Storage;
    using Protocol.Common.Installer;
    using Storage;
    using Data.Model;
    using Data.Packages;
    using System.Data.Entity;

    /// <summary>
    ///     Delete package command handler implementation.
    /// </summary>
    public class DeletePackageHandler : BasePackageCommandHandler, IParametersActionHandler<DeletePackageArgs>
    {
        private readonly IPackageLogRepository _packageLogs;
        private readonly IPackageRepository _packageRepository;
        private readonly Action<PackageLog, DbContext> _updatePackageLogCallback;
        private readonly IInstallerService _installerService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DeletePackageHandler" /> class.
        /// </summary>
        /// <param name="contextFactory">An <see cref="IMonacoContextFactory" /> instance.</param>
        /// <param name="packageErrorRepository">Package errors repository.</param>
        /// <param name="packageLogs">Package logs.</param>
        /// <param name="packageRepository">Packages.</param>
        /// <param name="installerService">Software installer service.</param>
        /// <param name="updatePackageLogCallback">Update package log callback.</param>
        public DeletePackageHandler(
            IMonacoContextFactory contextFactory,
            IPackageErrorRepository packageErrorRepository,
            IPackageLogRepository packageLogs,
            IPackageRepository packageRepository,
            IInstallerService installerService,
            Action<PackageLog, DbContext> updatePackageLogCallback)
            : base(contextFactory, packageErrorRepository)
        {
            _packageLogs = packageLogs ?? throw new ArgumentNullException(nameof(packageLogs));
            _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
            _installerService = installerService ?? throw new ArgumentNullException(nameof(installerService));
            _updatePackageLogCallback = updatePackageLogCallback ?? throw new ArgumentNullException(nameof(updatePackageLogCallback));
        }

        /// <inheritdoc />
        public void Execute(DeletePackageArgs parameter)
        {
            using (var context = ContextFactory.Create())
            {
                if (parameter == null)
                {
                    throw new ArgumentNullException(nameof(parameter));
                }

                var package = _packageRepository.GetPackageByPackageId(context, parameter.PackageId);
                
                var packageLog = _packageLogs.GetLastPackageLogeByPackageId(context, parameter.PackageId);

                if (package?.State == PackageState.InUse)
                {
                    throw new CommandException("Package Entity InUse");
                }

                packageLog.State = PackageState.Deleted;

                _installerService.DeleteSoftwarePackage(parameter.PackageId);

                _updatePackageLogCallback(packageLog, context);

                parameter.PackageResult = packageLog;

                parameter.DeletePackageCallback?.Invoke(parameter);
            }
        }
    }
}
