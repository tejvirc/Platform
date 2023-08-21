namespace Aristocrat.Monaco.G2S.Common.PackageManager.CommandHandlers
{
    using System;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Storage;
    using Protocol.Common.Installer;
    using Storage;

    /// <summary>
    ///     Validates a package
    /// </summary>
    public class ValidatePackageHandler : BasePackageCommandHandler, IParametersFuncHandler<string, bool>
    {
        private readonly IInstallerService _installerService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidatePackageHandler" /> class.
        /// </summary>
        /// <param name="contextFactory">An <see cref="IMonacoContextFactory" /> instance.</param>
        /// <param name="packageErrorRepository">The package error repository.</param>
        /// <param name="installerService">Software installer service.</param>
        public ValidatePackageHandler(
            IMonacoContextFactory contextFactory,
            IPackageErrorRepository packageErrorRepository,
            IInstallerService installerService)
            : base(contextFactory, packageErrorRepository)
        {
            _installerService = installerService ?? throw new ArgumentNullException(nameof(installerService));
        }

        /// <inheritdoc />
        public bool Execute(string filePath)
        {
            return _installerService.ValidateSoftwarePackage(filePath);
        }
    }
}