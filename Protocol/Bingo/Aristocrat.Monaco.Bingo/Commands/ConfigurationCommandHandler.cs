namespace Aristocrat.Monaco.Bingo.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Monaco.Protocol.Common.Storage.Entity;
    using Common.Exceptions;
    using Common.Storage.Model;
    using Gaming.Contracts;
    using Kernel;
    using ServerApiGateway;
    using Services.Configuration;
    using ConfigurationFactory = Common.IBingoStrategyFactory<Services.Configuration.IConfiguration, Services.Configuration.ConfigurationType>;

    public class ConfigurationCommandHandler : ICommandHandler<ConfigureCommand>
    {
        private readonly IConfigurationService _configurationService;
        private readonly IPropertiesManager _properties;
        private readonly IGameProvider _gameProvider;
        private readonly ConfigurationFactory _configurationFactory;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public ConfigurationCommandHandler(
            IConfigurationService configurationService,
            IPropertiesManager properties,
            IGameProvider gameProvider,
            ConfigurationFactory configurationFactory,
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _configurationFactory = configurationFactory ?? throw new ArgumentNullException(nameof(configurationFactory));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task Handle(ConfigureCommand command, CancellationToken token = default)
        {
            try
            {
                var serialNumber = _properties.GetValue(ApplicationConstants.SerialNumber, string.Empty);
                var allGameTitles = _gameProvider.GetAllGames();
                var gameTitles = string.Join(",", allGameTitles.Select(x => x.CdsThemeId ?? x.ThemeName).Distinct());
                var message = new ConfigurationMessage(serialNumber, gameTitles);
                var result = await _configurationService.ConfigureClient(message, token);

                var model = _unitOfWorkFactory.Invoke(x => x.Repository<BingoServerSettingsModel>().Queryable().FirstOrDefault()) ??
                    new BingoServerSettingsModel();

                var configSets = new (ConfigurationType type, IEnumerable<ConfigurationResponse.Types.ClientAttribute> fields)[]
                {
                    // *NOTE* We parse the compliance configuration first so we set any global property values first which may be
                    // overridden by EGM specific settings (IE. DisplayBingoCard) that are handled when parsing the remaining configurations.
                    (ConfigurationType.ComplianceConfiguration, result.ComplianceConfiguration.ClientAttribute),
                    (ConfigurationType.SystemConfiguration, result.SystemConfiguration.ClientAttribute),
                    (ConfigurationType.MachineAndGameConfiguration, result.MachineGameConfiguration.ClientAttribute),
                    (ConfigurationType.MessageConfiguration, result.MessageConfiguration.ClientAttribute)
                };

                foreach (var (type, fields) in configSets)
                {
                    var cfg = _configurationFactory.Create(type);
                    cfg.Configure(fields, model);
                }

                SaveBingoServerSettings(model);
            }
            catch (ConfigurationException)
            {
                // re-throw it so the BingoClientConnectionState machine gets it
                throw;
            }
            catch (Exception e)
            {
                throw new ConfigurationException(
                    "Configuration failed to communicate to the server",
                    e,
                    ConfigurationFailureReason.NoResponse);
            }
        }

        private void SaveBingoServerSettings(BingoServerSettingsModel model)
        {
            using var work = _unitOfWorkFactory.Create();
            var repository = work.Repository<BingoServerSettingsModel>();
            repository.AddOrUpdate(model);
            work.SaveChanges();
        }
    }
}