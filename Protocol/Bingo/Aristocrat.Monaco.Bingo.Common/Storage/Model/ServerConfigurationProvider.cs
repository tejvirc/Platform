namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System;
    using Monaco.Common.Storage;

    public class ServerConfigurationProvider : IServerConfigurationProvider
    {
        private readonly IMonacoContextFactory _factory;
        private readonly IRepository<BingoServerSettingsModel> _model;

        public ServerConfigurationProvider(IMonacoContextFactory factory, IRepository<BingoServerSettingsModel> model)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public BingoServerSettingsModel GetServerConfiguration()
        {
            using (var context = _factory.CreateDbContext())
            {
                return _model.GetSingle(context) ?? new BingoServerSettingsModel();
            }
        }
    }
}