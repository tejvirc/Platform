namespace Aristocrat.Monaco.G2S.Common.PackageManager.CommandHandlers
{
    using System;
    using System.Reflection;
    using G2S.Data.Model;
    using log4net;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Storage;
    using Protocol.Common.Installer;
    using Storage;

    /// <summary>
    ///     Installs a package
    /// </summary>
    public class InstallPackageHandler : BasePackageCommandHandler, IParametersActionHandler<InstallPackageArgs>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IPackageRepository _packageRepository;
        private readonly IInstallerService _installerService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InstallPackageHandler" /> class.
        /// </summary>
        /// <param name="contextFactory">An <see cref="IMonacoContextFactory" /> instance.</param>
        /// <param name="packageRepository">Package repository instance.</param>
        /// <param name="packageErrorRepository">Package errors repository.</param>
        /// <param name="installerService">Software installer service.</param>
        public InstallPackageHandler(
            IMonacoContextFactory contextFactory,
            IPackageRepository packageRepository,
            IPackageErrorRepository packageErrorRepository,
            IInstallerService installerService)
            : base(contextFactory, packageErrorRepository)
        {
            _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
            _installerService = installerService ?? throw new ArgumentNullException(nameof(installerService));
        }

        /// <inheritdoc />
        public void Execute(InstallPackageArgs parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            try
            {
                var result = _installerService.InstallPackage(
                    parameter.PackageEntity.PackageId,
                    () =>
                    {
                        using (var context = ContextFactory.Create())
                        {
                            _packageRepository.Update(context, parameter.PackageEntity);
                        }
                    });

                parameter.PackageManifest = result.manifest;

                parameter.Path = result.path;
                parameter.FileSize = result.size;
                parameter.DeviceChanged = result.deviceChanged;
                parameter.ExitAction = result.action;

                parameter.InstallPackageCallback(parameter);
            }
            catch (Exception exception)
            {
                parameter.PackageEntity.State = PackageState.Error;
                parameter.InstallPackageCallback(parameter);

                Logger.Error(
                    $"Installation Error - Package {parameter.PackageEntity} State: {parameter.PackageEntity.State}",
                    exception);
            }
        }
    }
}