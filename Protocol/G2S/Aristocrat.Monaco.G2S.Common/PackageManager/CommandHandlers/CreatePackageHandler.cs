namespace Aristocrat.Monaco.G2S.Common.PackageManager.CommandHandlers
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Storage;
    using Protocol.Common.Installer;
    using Storage;
    using G2S.Data.Model;
    
    /// <summary>
    ///     Create package command handler implementation.
    /// </summary>
    public class CreatePackageHandler : BasePackageCommandHandler,
        IParametersFuncHandler<CreatePackageArgs, CreatePackageState>
    {
        private readonly Action<PackageLog, DbContext> _updatePackageCallback;
        private readonly IInstallerService _installerService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CreatePackageHandler" /> class.
        /// </summary>
        /// <param name="contextFactory">An <see cref="IMonacoContextFactory" /> instance.</param>
        /// <param name="packageErrorRepository">Package errors repository.</param>
        /// <param name="installerService">Software installer Service</param>
        /// <param name="updatePackageCallback">Update package callback.</param>
        public CreatePackageHandler(
            IMonacoContextFactory contextFactory,
            IPackageErrorRepository packageErrorRepository,
            IInstallerService installerService,
            Action<PackageLog, DbContext> updatePackageCallback)
            : base(contextFactory, packageErrorRepository)
        {
            _installerService = installerService ?? throw new ArgumentNullException(nameof(installerService));
            _updatePackageCallback = updatePackageCallback ?? throw new ArgumentNullException(nameof(updatePackageCallback));
        }

        /// <inheritdoc />
        public CreatePackageState Execute(CreatePackageArgs parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (parameter.PackageLogEntity == null)
            {
                throw new ArgumentNullException(nameof(parameter.PackageLogEntity));
            }

            using (var context = ContextFactory.CreateDbContext())
            {
                var result = _installerService.BundleSoftwarePackage(parameter.ModuleEntity.PackageId, parameter.Overwrite, parameter.Format);

                parameter.PackageLogEntity.State = PackageState.Available;
                parameter.PackageLogEntity.Size = result.size;

                _updatePackageCallback(parameter.PackageLogEntity, context);
            }

            return CreatePackageState.Created;
        }
    }
}
