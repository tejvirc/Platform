namespace Aristocrat.Monaco.Sas.Storage
{
    using System;
    using System.Collections.Generic;
    using Common.Storage;
    using Contracts.SASProperties;
    using Kernel;
    using Models;

    /// <summary>
    ///     The SAS persistence data factory service
    /// </summary>
    public class SasDataFactory : ISasDataFactory, IService
    {
        private readonly IMonacoContextFactory _contextFactory;

        /// <summary>
        ///     Creates an instance of <see cref="SasDataFactory"/>
        /// </summary>
        public SasDataFactory()
            : this(
                new DbContextFactory(
                    new DefaultConnectionStringResolver(ServiceManager.GetInstance().GetService<IPathMapper>())))
        {
        }

        /// <summary>
        ///     Creates an instance of <see cref="SasDataFactory"/>
        /// </summary>
        /// <param name="contextFactory">An instance of <see cref="IMonacoContextFactory"/></param>
        public SasDataFactory(IMonacoContextFactory contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <inheritdoc />
        public string Name => nameof(SasDataFactory);

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new List<Type> { typeof(ISasDataFactory) };

        /// <inheritdoc />
        public ISasConfigurationService GetConfigurationService()
        {
            return new SasConfigurationProvider(
                _contextFactory,
                new Repository<Host>(),
                new Repository<PortAssignment>(),
                new Repository<SasFeatures>());
        }

        /// <inheritdoc />
        public void Initialize()
        {
        }
    }
}