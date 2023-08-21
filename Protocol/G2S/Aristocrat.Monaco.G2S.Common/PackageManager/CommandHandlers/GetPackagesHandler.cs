namespace Aristocrat.Monaco.G2S.Common.PackageManager.CommandHandlers
{
    using System;
    using System.Collections.Generic;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Storage;
    using Storage;

    /// <summary>
    ///     Get packages handler
    /// </summary>
    public class GetPackagesHandler : IFuncHandler<IEnumerable<Package>>
    {
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IPackageRepository _packageRepository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetPackagesHandler" /> class.
        /// </summary>
        /// <param name="packageRepository">The package repository.</param>
        /// <param name="contextFactory">The context factory.</param>
        public GetPackagesHandler(IPackageRepository packageRepository, IMonacoContextFactory contextFactory)
        {
            _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <summary>
        ///     Executes this instance.
        /// </summary>
        /// <returns>A collection of package entities</returns>
        public IEnumerable<Package> Execute()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return _packageRepository.GetAll(context);
            }
        }
    }
}