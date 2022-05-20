namespace Aristocrat.Monaco.G2S.Common.PackageManager.CommandHandlers
{
    using System;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Storage;
    using Protocol.Common.Installer;
    using Storage;

    /// <summary>
    ///     Uninstalls a module
    /// </summary>
    public class UninstallModuleHandler : BasePackageCommandHandler, IParametersActionHandler<UninstallModuleArgs>
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly IInstallerService _installerService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UninstallModuleHandler" /> class.
        /// </summary>
        /// <param name="contextFactory">An <see cref="IMonacoContextFactory" /> instance.</param>
        /// <param name="moduleRepository">Module repository instance.</param>
        /// <param name="packageErrorRepository">Package errors repository.</param>
        /// <param name="installerService">Software Installer service.</param>
        public UninstallModuleHandler(
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
        public void Execute(UninstallModuleArgs parameter)
        {
            var result = _installerService.UninstallSoftwarePackage(parameter.ModuleEntity.PackageId);

            using (var context = ContextFactory.Create())
            {
                var module = _moduleRepository.GetModuleByModuleId(context, parameter.ModuleEntity.ModuleId);
                _moduleRepository.Delete(context, module);
            }

            parameter.DeviceChanged = result.deviceChanged;

            parameter.UninstallModuleCallback(parameter);
        }
    }
}