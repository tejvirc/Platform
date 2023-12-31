﻿namespace Aristocrat.Monaco.Bingo.Tests
{
    using System;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client;
    using Aristocrat.Bingo.Client.Configuration;
    using Commands;
    using Common;
    using Common.Events;
    using Common.Exceptions;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BingoClientConnectionStateTests
    {
        private BingoClientConnectionState _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);
        private readonly Mock<IClient> _client = new(MockBehavior.Default);
        private readonly Mock<ICommandHandlerFactory> _commandFactory = new(MockBehavior.Default);
        private readonly Mock<ISystemDisableManager> _systemDisableManager = new(MockBehavior.Default);

        // This will point to the HandleRestartingEvent function due to reconnection
        private Func<ForceReconnectionEvent, CancellationToken, Task> _handleForceReconnectionEvent;

        [TestInitialize]
        public void Initialize()
        {
            var uriBuilder = new UriBuilder { Host = "localhost", Port = 5000 };
            _client.Setup(m => m.Start()).Returns(Task.FromResult(true));
            _client.SetupGet(m => m.Configuration).Returns(new ClientConfigurationOptions(uriBuilder.Uri, TimeSpan.FromSeconds(2), Enumerable.Empty<X509Certificate2>()));

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
                _systemDisableManager.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _eventBus.Setup(m => m.UnsubscribeAll(It.IsAny<BingoClientConnectionState>()));
            _target.Dispose();
        }

        [DataRow(true, false, false, false, DisplayName = "EventBus null")]
        [DataRow(false, true, false, false, DisplayName = "Client null")]
        [DataRow(false, false, true, false, DisplayName = "CommandFactory null")]
        [DataRow(false, false, false, true, DisplayName = "SystemDisableManager null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest(
            bool eventBusNull,
            bool clientNull,
            bool commandFactoryNull,
            bool systemDisableManagerNull)
        {
            _target = new BingoClientConnectionState(
                eventBusNull ? null : _eventBus.Object,
                clientNull ? null : _client.Object,
                commandFactoryNull ? null : _commandFactory.Object,
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

            await _target.Start();

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
            Assert.IsNotNull(_handleForceReconnectionEvent);

            _eventBus.Setup(m => m.Publish(It.IsAny<HostDisconnectedEvent>())).Verifiable();
            _systemDisableManager.Setup(m => m.Disable(
                BingoConstants.BingoHostDisconnectedKey,
                SystemDisablePriority.Normal,
                It.IsAny<Func<string>>(),
                true,
                It.IsAny<Func<string>>(),
                null));

            await _target.Start();

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostRegistrationFailedKey));

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationInvalidKey));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationMismatchKey));

            _eventBus.Setup(m => m.Publish(It.IsAny<HostConnectedEvent>()));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostDisconnectedKey));

            _client.Raise(m => m.Connected += null, new ConnectedEventArgs());

            await _handleForceReconnectionEvent(new ForceReconnectionEvent(), new CancellationToken(false));

            _eventBus.Verify(m => m.Publish(It.IsAny<HostDisconnectedEvent>()));
        }

        [TestMethod]
        public async Task DisconnectedWhileRegisteringTest()
        {
            Assert.IsNotNull(_handleForceReconnectionEvent);

            _eventBus.Setup(m => m.Publish(It.IsAny<HostDisconnectedEvent>())).Verifiable();
            _systemDisableManager.Setup(m => m.Disable(
                BingoConstants.BingoHostDisconnectedKey,
                SystemDisablePriority.Normal,
                It.IsAny<Func<string>>(),
                true,
                It.IsAny<Func<string>>(),
                null));

            await _target.Start();

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostRegistrationFailedKey));

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationInvalidKey));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationMismatchKey));

            _eventBus.Setup(m => m.Publish(It.IsAny<HostConnectedEvent>()));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostDisconnectedKey));

            _client.Raise(m => m.Connected += null, new ConnectedEventArgs());

            _client.Raise(m => m.Disconnected += null, new DisconnectedEventArgs());

            _eventBus.Verify(m => m.Publish(It.IsAny<HostDisconnectedEvent>()));
        }

        [TestMethod]
        public async Task DisconnectedWhileConnectedTest()
        {
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

            await _target.Start();

            _commandFactory.Setup(m => m.Execute(It.IsAny<RegistrationCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostRegistrationFailedKey));

            _commandFactory.Setup(m => m.Execute(It.IsAny<ConfigureCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationInvalidKey));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationMismatchKey));

            _eventBus.Setup(m => m.Publish(It.IsAny<HostConnectedEvent>()));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostDisconnectedKey));

            _client.Raise(m => m.Connected += null, new ConnectedEventArgs());

            _client.Raise(m => m.Disconnected += null, new DisconnectedEventArgs());

            _eventBus.Verify(m => m.Publish(It.IsAny<HostDisconnectedEvent>()));
        }

        [TestMethod]
        public async Task ReconfigureWhileConnectedTest()
        {
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

            await _target.Start();

            _commandFactory.Setup(m => m.Execute(It.IsAny<RegistrationCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostRegistrationFailedKey));

            _commandFactory.Setup(m => m.Execute(It.IsAny<ConfigureCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationInvalidKey));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationMismatchKey));

            _eventBus.Setup(m => m.Publish(It.IsAny<HostConnectedEvent>()));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostDisconnectedKey));

            _client.Raise(m => m.Connected += null, new ConnectedEventArgs());

            await _handleForceReconnectionEvent(new ForceReconnectionEvent(), new CancellationToken(false));

            _eventBus.Verify(m => m.Publish(It.IsAny<HostDisconnectedEvent>()));
        }

        [TestMethod]
        public async Task RegistrationFailureTest()
        {
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

            await _target.Start();

            _commandFactory.Setup(m => m.Execute(It.IsAny<RegistrationCommand>(), It.IsAny<CancellationToken>()))
                .Throws(new RegistrationException("registration failed", RegistrationFailureReason.Rejected));

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostRegistrationFailedKey));

            _commandFactory.Setup(m => m.Execute(It.IsAny<ConfigureCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationInvalidKey));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationMismatchKey));

            _eventBus.Setup(m => m.Publish(It.IsAny<HostConnectedEvent>()));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostDisconnectedKey));

            _client.Raise(m => m.Connected += null, new ConnectedEventArgs());

            _eventBus.Verify(m => m.Publish(It.IsAny<HostDisconnectedEvent>()));

            _eventBus.Verify(m => m.Publish(It.IsAny<RegistrationFailedEvent>()));
        }

        [DataRow(ConfigurationFailureReason.ConfigurationMismatch, DisplayName = "Configuration Mismatch")]
        [DataRow(ConfigurationFailureReason.InvalidGameConfiguration, DisplayName = "Invalid Game Configuration")]
        [DataTestMethod]
        public async Task ConfigurationFailureTest(ConfigurationFailureReason reason)
        {
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

            await _target.Start();

            _eventBus.Setup(m => m.Publish(It.IsAny<HostConnectedEvent>()));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostDisconnectedKey));

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

            await _target.Start();

            _commandFactory.Setup(m => m.Execute(It.IsAny<RegistrationCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostRegistrationFailedKey));

            _commandFactory.Setup(m => m.Execute(It.IsAny<ConfigureCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationInvalidKey));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostConfigurationMismatchKey));

            _eventBus.Setup(m => m.Publish(It.IsAny<HostConnectedEvent>()));
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.BingoHostDisconnectedKey));

            _client.Raise(m => m.Connected += null, new ConnectedEventArgs());

            _client.Raise(m => m.MessageReceived += null, EventArgs.Empty);

            _eventBus.Verify(m => m.Publish(It.IsAny<HostDisconnectedEvent>()));
        }

        [TestMethod]
        public async Task ShutdownTest()
        {
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

            await _target.Start();

            _client.Setup(x => x.Stop()).Returns(Task.FromResult(false));
            await _target.Stop();
            _client.Verify(x => x.Stop(), Times.Once);
        }
    }
}