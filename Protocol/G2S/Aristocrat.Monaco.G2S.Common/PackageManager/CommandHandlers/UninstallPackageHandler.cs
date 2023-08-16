namespace Aristocrat.Monaco.G2S.Common.PackageManager.CommandHandlers
{
    using System;
    using System.IO;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Storage;
    using Protocol.Common.Installer;
    using Storage;

    /// <summary>
    ///     Uninstalls a package
    /// </summary>
    public class UninstallPackageHandler : BasePackageCommandHandler, IParametersActionHandler<InstallPackageArgs>
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly IInstallerService _installerService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UninstallPackageHandler" /> class.
        /// </summary>
        /// <param name="contextFactory">An <see cref="IMonacoContextFactory" /> instance.</param>
        /// <param name="moduleRepository">Module repository instance.</param>
        /// <param name="packageErrorRepository">Package errors repository.</param>
        /// <param name="installerService">Software Installer service.</param>
        public UninstallPackageHandler(
            IMonacoContextFactory contextFactory,
            IModuleRepository moduleRepository,
            IPackageErrorRepository packageErrorRepository,
            IInstallerService installerService)
            : base(contextFactory, packageErrorRepository)
        {
            _moduleRepository = moduleRepository ?? throw new ArgumentNullException(nameof(moduleRepository));
            _installerService = installerService ?? throw new ArgumentNullException(nameof(installerService));
        }

        /// <inheritdoc />
        public void Execute(InstallPackageArgs parameter)
        {
            var result = _installerService.UninstallSoftwarePackage(
                parameter.PackageEntity.PackageId,
                files =>
                {
                    using (var context = ContextFactory.CreateDbContext())
                    {
                        foreach (var file in files)
                        {
                            var fileInfo = new FileInfo(file);
                            var me = _moduleRepository.GetModuleByModuleId(context, fileInfo.Name);
                            if (me != null)
                            {
                                _moduleRepository.Delete(context, me);
                            }
                        }
                    }
                });

            parameter.DeviceChanged = result.deviceChanged;
            parameter.PackageManifest = result.manifest;
            parameter.InstallPackageCallback(parameter);
        }
    }
}