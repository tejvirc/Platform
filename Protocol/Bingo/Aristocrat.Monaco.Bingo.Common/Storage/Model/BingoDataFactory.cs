namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System;
    using System.Collections.Generic;
    using Kernel;
    using Monaco.Common.Storage;

    public class BingoDataFactory : IBingoDataFactory, IService
    {
        private readonly IMonacoContextFactory _contextFactory;

        public BingoDataFactory(IMonacoContextFactory contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        public BingoDataFactory()
            : this(
                new DbContextFactory(
                    new DefaultConnectionStringResolver(ServiceManager.GetInstance().GetService<IPathMapper>())))
        {
        }

        public ICollection<Type> ServiceTypes => new List<Type> { typeof(IBingoDataFactory) };

        public string Name => nameof(BingoDataFactory);

        public IServerConfigurationProvider GetConfigurationProvider()
        {
            return new ServerConfigurationProvider(
                _contextFactory,
                new Repository<BingoServerSettingsModel>()
            );
        }

        public IHostService GetHostService()
        {
            return new BingoHostService(_contextFactory, new Repository<Host>());
        }

        public void Initialize()
        {
        }
    }
}