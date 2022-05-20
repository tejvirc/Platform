namespace Aristocrat.Monaco.Bingo.Tests
{
    using System;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Bingo.Client;
    using Aristocrat.Bingo.Client.Configuration;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Monaco.Bingo.Commands;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Common.Events;
    using Aristocrat.Monaco.Bingo.Common.Exceptions;
    using Aristocrat.Monaco.Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BingoClientConnectionStateTests
    {
        private BingoClientConnectionState _target;
        private Mock<IEventBus> _eventBus = new(MockBehavior.Strict);
        private Mock<IClient> _client = new(MockBehavior.Strict);
        private Mock<ICommandHandlerFactory> _commandFactory = new(MockBehavior.Strict);
        private Mock<ICommandService> _commandService = new(MockBehavior.Strict);
        private Mock<IPropertiesManager> _propertiesManager = new(MockBehavior.Strict);
        private Mock<ISystemDisableManager> _systemDisableManager = new(MockBehavior.Strict);

        // This will point to the HandleEvent function
        private Func<ProtocolInitialized, CancellationToken, Task> _handleProtocolInitializedEvent = null;

        // This will point to the HandleRestartingEvent function due to reconnection
        private Func<ForceReconnectionEvent, CancellationToken, Task> _handleForceReconnectionEvent = null;

        [TestInitialize]
        public void Initialize()
        {
            var uriBuilder = new UriBuilder() { Host = "localhost", Port = 5000 };
            _client.Setup(m => m.Start()).Returns(Task.FromResult(true));
            _client.SetupGet(m => m.Configuration).Returns(new ClientConfigurationOptions(uriBuilder.Uri, TimeSpan.FromSeconds(2), Enumerable.Empty<X509Certificate2>()));

            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<object>(),
                        It.IsAny<Func<ProtocolInitialized, CancellationToken, Task>>()))
                .Callback<object, Func<ProtocolInitialized, CancellationToken, Task>>((_, func) => _handleProtocolInitializedEvent = func);
            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<object>(),
                        It.IsAny<Func<ForceReconnectionEvent, CancellationToken, Task>>()))
                .Callback<object, Func<ForceReconnectionEvent, CancellationToken, Task>>((_, func) => _handleForceReconnectionEvent = func);
            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<object>(),
                        It.IsAny<Func<PropertyChangedEvent, CancellationToken, Task>>(),
                        It.IsAny<Predicate<PropertyChangedEvent>>()));

            _target = new BingoClientConnectionState(
                _eventBus.Object,
                _client.Object,
                _commandFactory.Object,
                _commandService.Object,
                _propertiesManager.Object,
                _systemDisableManager.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _eventBus.Setup(m => m.UnsubscribeAll(It.IsAny<BingoClientConnectionState>()));
            _target.Dispose();
        }

        [DataRow(true, false, false, false, false, false, DisplayName = "EventBus null")]
        [DataRow(false, true, false, false, false, false, DisplayName = "Client null")]
        [DataRow(false, false, true, false, false, false, DisplayName = "CommandFactory null")]
        [DataRow(false, false, false, true, false, false, DisplayName = "CommandService null")]
        [DataRow(false, false, false, false, true, false, DisplayName = "PropertiesManager null")]
        [DataRow(false, false, false, false, false, true, DisplayName = "SystemDisableManager null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest(
            bool eventBusNull,
            bool clientNull,
            bool commandFactoryNull,
            bool commandServiceNull,
            bool propertiesManagerNull,
            bool systemDisableManagerNull)
        {
            _target = new BingoClientConnectionState(
                eventBusNull ? null : _eventBus.Object,
                clientNull ? null : _client.Object,
                commandFactoryNull ? null : _commandFactory.Object,
                commandServiceNull ? null : _commandService.Object,
                propertiesManagerNull ? null : _propertiesManager.Object,
                systemDisableManagerNull ? null : _systemDisableManager.Object);
        }

        [TestMethod]
        public void WhenDisposeExpectSuccess()
        {
            _eventBus.Setup(m => m.UnsubscribeAll(It.IsAny<BingoClientConnectionState>()));

            Assert.IsNotNull(_target);

            _target.Dispose();

            _eventBus.Verify(b => b.UnsubscribeAll(_target));
        }

        [TestMethod]
        public async Task IdleToConnectedHappyPathTest()
        {
            // this tests going from
            // Idle -> Disconnected -> Connecting -> Registering -> Configuring -> Connected states

            // make sure the event was set up in the RegisterEventListeners method
            Assert.IsNotNull(_handleProtocolInitializedEvent);

            // set up mocks for the OnDisconnected method
            _eventBus.Setup(m => m.Publish(It.IsAny<HostDisconnectedEvent>())).Verifiable();
            _systemDisableManager.Setup(m => m.Disable(
                BingoConstants.BingoHostDisconnectedKey,
                SystemDisablePriority.Normal,
                It.IsAny<Func<string>>(),
                true,
                It.IsAny<Func<string>>(),
                null));

            // set up mocks for OnConnecting method
            _client.SetupSequence(m => m.Start())
                .Returns(Task.FromResult(false))
                .Returns(Task.FromResult(true));

            // this should trigger going from Idle -> Disconnected and
            // call the OnDisconnected method which triggers going to
            // Connecting state and calling OnConnecting method
            await _handleProtocolInitializedEvent(new ProtocolInitialized(), new CancellationToken(false));

            // we should now be in state Connecting

            // set up mocks for OnRegistering
            _commandFactory.Setup(m => m.Execute(It.IsAny<RegistrationCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            // set up mocks for OnRegisteringExit
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostRegistrationFailedKey));

            // set up mocks for OnConfiguring
            _commandFactory.Setup(m => m.Execute(It.IsAny<ConfigureCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            // set up mocks for OnConfiguringExit
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationInvalidKey));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationMismatchKey));

            // set up mocks for OnConnected
            _eventBus.Setup(m => m.Publish(It.IsAny<HostConnectedEvent>()));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostDisconnectedKey));
            _commandService.Setup(m => m.HandleCommands(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.SerialNumber, string.Empty)).Returns("1");

            // raise the Connected event which will call OnClientConnected
            // which will send the Trigger.Connected to the state machine
            // and put us in state Registering, which will call OnRegistering
            // which will move to state Configuring which will call OnConfiguring which
            // would move to state Connected.
            _client.Raise(m => m.Connected += null, new ConnectedEventArgs());

            _eventBus.Verify(m => m.Publish(It.IsAny<HostDisconnectedEvent>()));
        }

        [TestMethod]
        public async Task ReconfigureWhileRegisteringTest()
        {
            Assert.IsNotNull(_handleProtocolInitializedEvent);
            Assert.IsNotNull(_handleForceReconnectionEvent);

            _eventBus.Setup(m => m.Publish(It.IsAny<HostDisconnectedEvent>())).Verifiable();
            _systemDisableManager.Setup(m => m.Disable(
                BingoConstants.BingoHostDisconnectedKey,
                SystemDisablePriority.Normal,
                It.IsAny<Func<string>>(),
                true,
                It.IsAny<Func<string>>(),
                null));

            await _handleProtocolInitializedEvent(new ProtocolInitialized(), new CancellationToken(false));

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostRegistrationFailedKey));

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationInvalidKey));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationMismatchKey));

            _eventBus.Setup(m => m.Publish(It.IsAny<HostConnectedEvent>()));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostDisconnectedKey));
            _commandService.Setup(m => m.HandleCommands(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.SerialNumber, string.Empty)).Returns("1");

            _client.Raise(m => m.Connected += null, new ConnectedEventArgs());

            await _handleForceReconnectionEvent(new ForceReconnectionEvent(), new CancellationToken(false));

            _eventBus.Verify(m => m.Publish(It.IsAny<HostDisconnectedEvent>()));

        }

        [TestMethod]
        public async Task DisconnectedWhileRegisteringTest()
        {
            Assert.IsNotNull(_handleProtocolInitializedEvent);
            Assert.IsNotNull(_handleForceReconnectionEvent);

            _eventBus.Setup(m => m.Publish(It.IsAny<HostDisconnectedEvent>())).Verifiable();
            _systemDisableManager.Setup(m => m.Disable(
                BingoConstants.BingoHostDisconnectedKey,
                SystemDisablePriority.Normal,
                It.IsAny<Func<string>>(),
                true,
                It.IsAny<Func<string>>(),
                null));

            await _handleProtocolInitializedEvent(new ProtocolInitialized(), new CancellationToken(false));

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostRegistrationFailedKey));

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationInvalidKey));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationMismatchKey));

            _eventBus.Setup(m => m.Publish(It.IsAny<HostConnectedEvent>()));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostDisconnectedKey));
            _commandService.Setup(m => m.HandleCommands(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.SerialNumber, string.Empty)).Returns("1");

            _client.Raise(m => m.Connected += null, new ConnectedEventArgs());

            _client.Raise(m => m.Disconnected += null, new DisconnectedEventArgs());

            _eventBus.Verify(m => m.Publish(It.IsAny<HostDisconnectedEvent>()));
        }

        [TestMethod]
        public async Task DisconnectedWhileConnectedTest()
        {
            Assert.IsNotNull(_handleProtocolInitializedEvent);

            _eventBus.Setup(m => m.Publish(It.IsAny<HostDisconnectedEvent>())).Verifiable();
            _systemDisableManager.Setup(m => m.Disable(
                BingoConstants.BingoHostDisconnectedKey,
                SystemDisablePriority.Normal,
                It.IsAny<Func<string>>(),
                true,
                It.IsAny<Func<string>>(),
                null));

            _client.SetupSequence(m => m.Start())
                .Returns(Task.FromResult(false))
                .Returns(Task.FromResult(true));

            await _handleProtocolInitializedEvent(new ProtocolInitialized(), new CancellationToken(false));

            _commandFactory.Setup(m => m.Execute(It.IsAny<RegistrationCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostRegistrationFailedKey));

            _commandFactory.Setup(m => m.Execute(It.IsAny<ConfigureCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationInvalidKey));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationMismatchKey));

            _eventBus.Setup(m => m.Publish(It.IsAny<HostConnectedEvent>()));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostDisconnectedKey));
            _commandService.Setup(m => m.HandleCommands(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.SerialNumber, string.Empty)).Returns("1");

            _client.Raise(m => m.Connected += null, new ConnectedEventArgs());

            _client.Raise(m => m.Disconnected += null, new DisconnectedEventArgs());

            _eventBus.Verify(m => m.Publish(It.IsAny<HostDisconnectedEvent>()));
        }

        [TestMethod]
        public async Task ReconfigureWhileConnectedTest()
        {
            Assert.IsNotNull(_handleProtocolInitializedEvent);
            Assert.IsNotNull(_handleForceReconnectionEvent);

            _eventBus.Setup(m => m.Publish(It.IsAny<HostDisconnectedEvent>())).Verifiable();
            _systemDisableManager.Setup(m => m.Disable(
                BingoConstants.BingoHostDisconnectedKey,
                SystemDisablePriority.Normal,
                It.IsAny<Func<string>>(),
                true,
                It.IsAny<Func<string>>(),
                null));

            _client.SetupSequence(m => m.Start())
                .Returns(Task.FromResult(false))
                .Returns(Task.FromResult(true));

            await _handleProtocolInitializedEvent(new ProtocolInitialized(), new CancellationToken(false));

            _commandFactory.Setup(m => m.Execute(It.IsAny<RegistrationCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostRegistrationFailedKey));

            _commandFactory.Setup(m => m.Execute(It.IsAny<ConfigureCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationInvalidKey));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationMismatchKey));

            _eventBus.Setup(m => m.Publish(It.IsAny<HostConnectedEvent>()));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostDisconnectedKey));
            _commandService.Setup(m => m.HandleCommands(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.SerialNumber, string.Empty)).Returns("1");

            _client.Raise(m => m.Connected += null, new ConnectedEventArgs());

            await _handleForceReconnectionEvent(new ForceReconnectionEvent(), new CancellationToken(false));

            _eventBus.Verify(m => m.Publish(It.IsAny<HostDisconnectedEvent>()));
        }

        [TestMethod]
        public async Task RegistrationFailureTest()
        {
            Assert.IsNotNull(_handleProtocolInitializedEvent);

            _eventBus.Setup(m => m.Publish(It.IsAny<HostDisconnectedEvent>())).Verifiable();
            _systemDisableManager.Setup(m => m.Disable(
                BingoConstants.BingoHostDisconnectedKey,
                SystemDisablePriority.Normal,
                It.IsAny<Func<string>>(),
                true,
                It.IsAny<Func<string>>(),
                null));

            _client.SetupSequence(m => m.Start())
                .Returns(Task.FromResult(false))
                .Returns(Task.FromResult(true));

            _eventBus.Setup(m => m.Publish(It.IsAny<RegistrationFailedEvent>())).Verifiable();

            await _handleProtocolInitializedEvent(new ProtocolInitialized(), new CancellationToken(false));

            _commandFactory.Setup(m => m.Execute(It.IsAny<RegistrationCommand>(), It.IsAny<CancellationToken>()))
                .Throws(new RegistrationException("registration failed", RegistrationFailureReason.Rejected));

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostRegistrationFailedKey));

            _commandFactory.Setup(m => m.Execute(It.IsAny<ConfigureCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationInvalidKey));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationMismatchKey));

            _eventBus.Setup(m => m.Publish(It.IsAny<HostConnectedEvent>()));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostDisconnectedKey));
            _commandService.Setup(m => m.HandleCommands(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.SerialNumber, string.Empty)).Returns("1");

            _client.Raise(m => m.Connected += null, new ConnectedEventArgs());

            _eventBus.Verify(m => m.Publish(It.IsAny<HostDisconnectedEvent>()));

            _eventBus.Verify(m => m.Publish(It.IsAny<RegistrationFailedEvent>()));
        }

        [DataRow(ConfigurationFailureReason.ConfigurationMismatch, DisplayName = "Configuration Mismatch")]
        [DataRow(ConfigurationFailureReason.InvalidGameConfiguration, DisplayName = "Invalid Game Configureation")]
        [DataTestMethod]
        public async Task ConfigurationFailureTest(ConfigurationFailureReason reason)
        {
            Assert.IsNotNull(_handleProtocolInitializedEvent);

            // set up mocks for the OnDisconnected method
            _eventBus.Setup(m => m.Publish(It.IsAny<HostDisconnectedEvent>())).Verifiable();
            _systemDisableManager.Setup(m => m.Disable(
                BingoConstants.BingoHostDisconnectedKey,
                SystemDisablePriority.Normal,
                It.IsAny<Func<string>>(),
                true,
                It.IsAny<Func<string>>(),
                null));

            _eventBus.Setup(m => m.Publish(It.IsAny<InvalidConfigurationReceivedEvent>())).Verifiable();
            if (ConfigurationFailureReason.ConfigurationMismatch == reason)
            {
                _eventBus.Setup(m => m.Publish(It.IsAny<ConfigurationMismatchReceivedEvent>())).Verifiable();
            }

            // set up mocks for OnConnecting method
            _client.SetupSequence(m => m.Start())
                .Returns(Task.FromResult(false))
                .Returns(Task.FromResult(true));

            _commandFactory.Setup(m => m.Execute(It.IsAny<RegistrationCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostRegistrationFailedKey));

            _commandFactory.Setup(m => m.Execute(It.IsAny<ConfigureCommand>(), It.IsAny<CancellationToken>()))
                .Throws(new ConfigurationException("Configuration failed", reason));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationInvalidKey));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationMismatchKey));

            await _handleProtocolInitializedEvent(new ProtocolInitialized(), new CancellationToken(false));

            _eventBus.Setup(m => m.Publish(It.IsAny<HostConnectedEvent>()));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostDisconnectedKey));
            _commandService.Setup(m => m.HandleCommands(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.SerialNumber, string.Empty)).Returns("1");

            _client.Raise(m => m.Connected += null, new ConnectedEventArgs());

            if (ConfigurationFailureReason.ConfigurationMismatch == reason)
            {
                _eventBus.Verify(m => m.Publish(It.IsAny<ConfigurationMismatchReceivedEvent>()));
            }
            else
            {
                _eventBus.Verify(m => m.Publish(It.IsAny<InvalidConfigurationReceivedEvent>()));
            }
        }

        [TestMethod]
        public async Task MessageReceivedTest()
        {
            Assert.IsNotNull(_handleProtocolInitializedEvent);

            _eventBus.Setup(m => m.Publish(It.IsAny<HostDisconnectedEvent>())).Verifiable();
            _systemDisableManager.Setup(m => m.Disable(
                BingoConstants.BingoHostDisconnectedKey,
                SystemDisablePriority.Normal,
                It.IsAny<Func<string>>(),
                true,
                It.IsAny<Func<string>>(),
                null));

            _client.SetupSequence(m => m.Start())
                .Returns(Task.FromResult(false))
                .Returns(Task.FromResult(true));

            await _handleProtocolInitializedEvent(new ProtocolInitialized(), new CancellationToken(false));

            _commandFactory.Setup(m => m.Execute(It.IsAny<RegistrationCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostRegistrationFailedKey));

            _commandFactory.Setup(m => m.Execute(It.IsAny<ConfigureCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationInvalidKey));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationMismatchKey));

            _eventBus.Setup(m => m.Publish(It.IsAny<HostConnectedEvent>()));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostDisconnectedKey));
            _commandService.Setup(m => m.HandleCommands(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.SerialNumber, string.Empty)).Returns("1");

            _client.Raise(m => m.Connected += null, new ConnectedEventArgs());

            _client.Raise(m => m.MessageReceived += null, new EventArgs());

            _eventBus.Verify(m => m.Publish(It.IsAny<HostDisconnectedEvent>()));
        }
    }
}