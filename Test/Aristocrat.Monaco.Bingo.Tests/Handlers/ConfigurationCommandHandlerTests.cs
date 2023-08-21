namespace Aristocrat.Monaco.Bingo.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Monaco.Bingo.Services.Configuration;
    using Aristocrat.Monaco.Protocol.Common.Storage.Entity;
    using Aristocrat.Monaco.Protocol.Common.Storage.Repositories;
    using Commands;
    using Common.Storage.Model;
    using Gaming.Contracts;
    using Google.Protobuf.Collections;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using ServerApiGateway;
    using ConfigurationFactory = Common.IBingoStrategyFactory<Bingo.Services.Configuration.IConfiguration, Bingo.Services.Configuration.ConfigurationType>;

    [TestClass]
    public class ConfigurationCommandHandlerTests
    {
        private readonly ConfigureCommand _configureCommand = new ConfigureCommand();
        private ConfigurationCommandHandler _target;
        private readonly Mock<IConfigurationService> _configurationService = new Mock<IConfigurationService>(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
        private readonly Mock<IGameProvider> _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);
        private readonly Mock<ConfigurationFactory> _configurationFactory = new Mock<ConfigurationFactory>(MockBehavior.Default);
        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new Mock<IUnitOfWorkFactory>(MockBehavior.Default);

        [TestInitialize]
        public void Initialize()
        {
            _target = new ConfigurationCommandHandler(
                _configurationService.Object,
                _propertiesManager.Object,
                _gameProvider.Object,
                _configurationFactory.Object,
                _unitOfWorkFactory.Object);
        }

        [DataRow(true, false, false, false, false, DisplayName = "ConfigurationService null")]
        [DataRow(false, true, false, false, false, DisplayName = "PropertiesManager null")]
        [DataRow(false, false, true, false, false, DisplayName = "GameProvider null")]
        [DataRow(false, false, false, true, false, DisplayName = "ConfigurationFactory null")]
        [DataRow(false, false, false, false, true, DisplayName = "UnitOfWorkFactory null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest(
            bool configurationServiceNull,
            bool propertiesManagerNull,
            bool gameProviderNull,
            bool configurationFactoryNull,
            bool unitOfWorkFactoryNull)
        {
            _target = new ConfigurationCommandHandler(
                configurationServiceNull ? null : _configurationService.Object,
                propertiesManagerNull ? null : _propertiesManager.Object,
                gameProviderNull ? null : _gameProvider.Object,
                configurationFactoryNull ? null : _configurationFactory.Object,
                unitOfWorkFactoryNull ? null : _unitOfWorkFactory.Object);
        }

        [TestMethod]
        public async Task HandleCommandTest()
        {
            var machineSerial = "123";
            var gameDetail = new Mock<IGameDetail>(MockBehavior.Default);
            var gameDetailCollection = new List<IGameDetail> { gameDetail.Object };
            var configurationResponse = new ConfigurationResponse {
                MessageConfiguration = new ConfigurationResponse.Types.MessageConfiguration(),
                ComplianceConfiguration = new ConfigurationResponse.Types.ComplianceConfiguration(),
                MachineGameConfiguration = new ConfigurationResponse.Types.MachineGameConfiguration(),
                SystemConfiguration = new ConfigurationResponse.Types.SystemConfiguration(),
            };

            var model = new BingoServerSettingsModel();

            var messageConfiguration = new Mock<IConfiguration>();
            var complianceConfiguration = new Mock<IConfiguration>();
            var machineAndGameConfiguration = new Mock<IConfiguration>();
            var systemConfiguration = new Mock<IConfiguration>();

            var work = new Mock<IUnitOfWork>();
            var repository = new Mock<IRepository<BingoServerSettingsModel>>();

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.SerialNumber, string.Empty)).Returns(machineSerial).Verifiable();
            _gameProvider.Setup(m => m.GetAllGames()).Returns(gameDetailCollection).Verifiable();
            _configurationService.Setup(m => m.ConfigureClient(It.IsAny<ConfigurationMessage>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(configurationResponse)).Verifiable();
            _configurationFactory.Setup(m => m.Create(ConfigurationType.MessageConfiguration)).Returns(messageConfiguration.Object).Verifiable();
            _configurationFactory.Setup(m => m.Create(ConfigurationType.ComplianceConfiguration)).Returns(complianceConfiguration.Object).Verifiable();
            _configurationFactory.Setup(m => m.Create(ConfigurationType.MachineAndGameConfiguration)).Returns(machineAndGameConfiguration.Object).Verifiable();
            _configurationFactory.Setup(m => m.Create(ConfigurationType.SystemConfiguration)).Returns(systemConfiguration.Object).Verifiable();
            messageConfiguration.Setup(m => m.Configure(It.IsAny<RepeatedField<ConfigurationResponse.Types.ClientAttribute>>(), model)).Verifiable();
            complianceConfiguration.Setup(m => m.Configure(It.IsAny<RepeatedField<ConfigurationResponse.Types.ClientAttribute>>(), model)).Verifiable();
            machineAndGameConfiguration.Setup(m => m.Configure(It.IsAny<RepeatedField<ConfigurationResponse.Types.ClientAttribute>>(), model)).Verifiable();
            systemConfiguration.Setup(m => m.Configure(It.IsAny<RepeatedField<ConfigurationResponse.Types.ClientAttribute>>(), model)).Verifiable();

            _unitOfWorkFactory.Setup(m => m.Create()).Returns(work.Object);
            work.Setup(m => m.Repository<BingoServerSettingsModel>()).Returns(repository.Object);

            await _target.Handle(_configureCommand, CancellationToken.None);

            _configurationService.Verify();
            _propertiesManager.Verify();
            _gameProvider.Verify();
            _configurationFactory.Verify();
            _unitOfWorkFactory.Verify();
        }
    }
}
