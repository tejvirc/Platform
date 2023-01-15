namespace Aristocrat.Monaco.Sas.Tests.Base
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Kernel.Contracts.MessageDisplay;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Monaco.Sas.Storage.Repository;
    using Contracts.Client;
    using Contracts.Events;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Base;
    using Test.Common;
    using Localization.Properties;

    [TestClass]
    public class SasDisableProviderTests
    {
        private const int WaitTime = 1000;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IStorageDataProvider<SasDisableInformation>> _disableDataProvider;
        private Mock<ISystemDisableManager> _systemDisableManager;
        private Mock<IPlayerBank> _playerBank;
        private Mock<IEventBus> _eventBus;
        private Mock<IMessageDisplay> _messageDisplay;
        private Mock<IGamePlayState> _gamePlayState;
        private SasDisableProvider _target;

        private static IEnumerable<object[]> DisableTestData => new List<object[]>
        {
            new object[]
            {
                SystemDisablePriority.Normal,
                new List<(Guid, DisableState)>
                {
                    (ApplicationConstants.DisabledByHost0Key, DisableState.DisabledByHost0)
                }
            },
            new object[]
            {
                SystemDisablePriority.Normal,
                new List<(Guid, DisableState)>
                {
                    (ApplicationConstants.DisabledByHost1Key, DisableState.DisabledByHost1)
                }
            },
            new object[]
            {
                SystemDisablePriority.Normal,
                new List<(Guid, DisableState)>
                {
                    (ApplicationConstants.Host0CommunicationsOfflineDisableKey,
                        DisableState.Host0CommunicationsOffline)
                }
            },
            new object[]
            {
                SystemDisablePriority.Normal,
                new List<(Guid, DisableState)>
                {
                    (ApplicationConstants.Host1CommunicationsOfflineDisableKey,
                        DisableState.Host1CommunicationsOffline)
                }
            },
            new object[]
            {
                SystemDisablePriority.Normal,
                new List<(Guid, DisableState)>
                {
                    (new Guid("{D45C666A-C4AA-43b8-8E02-F6B5EB5C7B25}"), DisableState.MaintenanceMode)
                }
            },
            new object[]
            {
                SystemDisablePriority.Normal,
                new List<(Guid, DisableState)>
                {
                    (new Guid("{F72C5936-9133-41C6-B8D1-F8F94E84D990}"),
                        DisableState.PowerUpDisabledByHost0)
                }
            },
            new object[]
            {
                SystemDisablePriority.Normal,
                new List<(Guid, DisableState)>
                {
                    (new Guid("{67D8BC13-F777-49F4-A59B-5F20C4084D6B}"),
                        DisableState.PowerUpDisabledByHost1)
                }
            },
            new object[]
            {
                SystemDisablePriority.Normal,
                new List<(Guid, DisableState)>
                {
                    (new Guid("{6AFB8906-4F6B-449d-A50A-0FB0337AE3C7}"),
                        DisableState.ProgressivesNotSupported)
                }
            },
            new object[]
            {
                SystemDisablePriority.Normal,
                new List<(Guid, DisableState)>
                {
                    (new Guid("{13CAD5EC-F655-4049-B9B4-E9017BFA79F7}"), DisableState.ValidationIdNeeded)
                }
            },
            new object[]
            {
                SystemDisablePriority.Normal,
                new List<(Guid, DisableState)>
                {
                    (new Guid("{581CEE73-F905-4BF5-B18B-DC044B88163F}"), DisableState.ValidationQueueFull)
                }
            },
            new object[]
            {
                SystemDisablePriority.Normal,
                new List<(Guid, DisableState)>
                {
                    (ApplicationConstants.DisabledByHost0Key, DisableState.DisabledByHost0),
                    (ApplicationConstants.DisabledByHost1Key, DisableState.DisabledByHost1),
                    (ApplicationConstants.Host0CommunicationsOfflineDisableKey,
                        DisableState.Host0CommunicationsOffline),
                    (ApplicationConstants.Host1CommunicationsOfflineDisableKey,
                        DisableState.Host1CommunicationsOffline)
                }
            }
        };

        private static IEnumerable<object[]> EnableTestData => new List<object[]>
        {
            new object[]
            {
                DisableState.DisabledByHost0, new[] { DisableState.DisabledByHost0 }, DisableState.None,
                new List<Guid> { ApplicationConstants.DisabledByHost0Key }
            },
            new object[]
            {
                DisableState.DisabledByHost1, new[] { DisableState.DisabledByHost1 }, DisableState.None,
                new List<Guid> { ApplicationConstants.DisabledByHost1Key }
            },
            new object[]
            {
                DisableState.Host0CommunicationsOffline, new[] { DisableState.Host0CommunicationsOffline },
                DisableState.None, new List<Guid> { ApplicationConstants.Host0CommunicationsOfflineDisableKey }
            },
            new object[]
            {
                DisableState.Host1CommunicationsOffline, new[] { DisableState.Host1CommunicationsOffline },
                DisableState.None, new List<Guid> { ApplicationConstants.Host1CommunicationsOfflineDisableKey }
            },
            new object[]
            {
                DisableState.PowerUpDisabledByHost0, new[] { DisableState.PowerUpDisabledByHost0 },
                DisableState.None, new List<Guid> { new Guid("{F72C5936-9133-41C6-B8D1-F8F94E84D990}") }
            },
            new object[]
            {
                DisableState.PowerUpDisabledByHost1, new[] { DisableState.PowerUpDisabledByHost1 },
                DisableState.None, new List<Guid> { new Guid("{67D8BC13-F777-49F4-A59B-5F20C4084D6B}") }
            },
            new object[]
            {
                DisableState.ProgressivesNotSupported, new[] { DisableState.ProgressivesNotSupported },
                DisableState.None, new List<Guid> { new Guid("{6AFB8906-4F6B-449d-A50A-0FB0337AE3C7}") }
            },
            new object[]
            {
                DisableState.ValidationIdNeeded, new[] { DisableState.ValidationIdNeeded }, DisableState.None,
                new List<Guid> { new Guid("{13CAD5EC-F655-4049-B9B4-E9017BFA79F7}") }
            },
            new object[]
            {
                DisableState.ValidationQueueFull, new[] { DisableState.ValidationQueueFull }, DisableState.None,
                new List<Guid> { new Guid("{581CEE73-F905-4BF5-B18B-DC044B88163F}") }
            },
            new object[]
            {
                DisableState.DisabledByHost0 | DisableState.DisabledByHost1 |
                DisableState.Host0CommunicationsOffline | DisableState.Host1CommunicationsOffline,
                new[] { DisableState.DisabledByHost0, DisableState.Host0CommunicationsOffline },
                DisableState.DisabledByHost1 | DisableState.Host1CommunicationsOffline,
                new List<Guid>
                {
                    ApplicationConstants.DisabledByHost0Key,
                    ApplicationConstants.Host0CommunicationsOfflineDisableKey
                }
            }
        };

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            var locale = MoqServiceManager.CreateAndAddService<ILocalizerFactory>(MockBehavior.Strict);
            locale.Setup(x => x.For(It.IsAny<string>())).Returns(new Mock<ILocalizer>().Object);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _systemDisableManager = new Mock<ISystemDisableManager>(MockBehavior.Default);
            _playerBank = new Mock<IPlayerBank>(MockBehavior.Default);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _messageDisplay = new Mock<IMessageDisplay>(MockBehavior.Default);
            _disableDataProvider = new Mock<IStorageDataProvider<SasDisableInformation>>(MockBehavior.Default);
            _disableDataProvider.Setup(x => x.GetData()).Returns(new SasDisableInformation { DisableStates = DisableState.None });
            _propertiesManager.Setup(
                    x => x.GetProperty(GamingConstants.LockupBehavior, It.IsAny<CashableLockupStrategy>()))
                .Returns(CashableLockupStrategy.ForceCashout);
            _gamePlayState = new Mock<IGamePlayState>(MockBehavior.Default);
            _propertiesManager
                .Setup(x => x.GetProperty(GamingConstants.LockupBehavior, It.IsAny<CashableLockupStrategy>()))
                .Returns(CashableLockupStrategy.ForceCashout);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<GamePlayStateChangedEvent>>()));
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<SystemDisableAddedEvent>>()));
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<SystemDisableRemovedEvent>>()));
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<BankBalanceChangedEvent>>(), It.IsAny<Predicate<BankBalanceChangedEvent>>()));
            _eventBus.Setup(x => x.Unsubscribe<GamePlayStateChangedEvent>(It.IsAny<object>()));
            _eventBus.Setup(x => x.Unsubscribe<SystemDisableAddedEvent>(It.IsAny<object>()));
            _eventBus.Setup(x => x.Unsubscribe<SystemDisableRemovedEvent>(It.IsAny<object>()));
            _eventBus.Setup(x => x.Unsubscribe<BankBalanceChangedEvent>(It.IsAny<object>()));
            _target = CreateDisableProvider();
        }

        [DataRow(true, false, false, false, false, false, false, DisplayName = "Null Properties Manager")]
        [DataRow(false, true, false, false, false, false, false, DisplayName = "Null Disable Data Provider")]
        [DataRow(false, false, true, false, false, false, false, DisplayName = "Null System Disable Manager")]
        [DataRow(false, false, false, true, false, false, false, DisplayName = "Null Message Display")]
        [DataRow(false, false, false, false, true, false, false, DisplayName = "Null Game Play State")]
        [DataRow(false, false, false, false, false, true, false, DisplayName = "Null Event Bus")]
        [DataRow(false, false, false, false, false, false, true, DisplayName = "Null Player Bank")]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorTest(
            bool nullPropertiesManger,
            bool nullDisableDataProvider,
            bool nullSystemDisableManager,
            bool nullMessageDisplay,
            bool nullGamePlayState,
            bool nullEventBus,
            bool nullPlayerBank)
        {
            _target = CreateDisableProvider(
                nullPropertiesManger,
                nullDisableDataProvider,
                nullSystemDisableManager,
                nullMessageDisplay,
                nullGamePlayState,
                nullEventBus,
                nullPlayerBank);
        }

        [DynamicData(nameof(DisableTestData))]
        [DataTestMethod]
        public void DisableTest(SystemDisablePriority priority, IList<(Guid guid, DisableState state)> disableGuids)
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<HostOfflineEvent>()));
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { DisableOnDisconnect = false });

            foreach (var (guid, _) in disableGuids)
            {
                _systemDisableManager.Setup(x => x.Disable(guid, priority, It.IsAny<string>(), It.IsAny<CultureProviderType>()))
                    .Verifiable();
            }

            var disableStates = disableGuids.Select(x => x.state).ToArray();
            var savedStates = disableStates.Aggregate((current, state) => current | state);
            _disableDataProvider.Setup(x => x.Save(It.Is<SasDisableInformation>(d => d.DisableStates == savedStates)))
                .Returns(Task.CompletedTask)
                .Verifiable();
            Assert.IsTrue(_target.Disable(priority, disableStates).Wait(WaitTime));

            _systemDisableManager.Verify();
        }

        [DynamicData(nameof(DisableTestData))]
        [DataTestMethod]
        public void SoftDisableTest(SystemDisablePriority priority, IList<(Guid guid, DisableState state)> disableGuids)
        {
            foreach (var (guid, state) in disableGuids)
            {
                Assert.IsTrue(_target.Disable(priority, state, false).Wait(WaitTime));
                _messageDisplay.Verify(
                    x => x.DisplayMessage(
                        It.Is<IDisplayableMessage>(
                            message => message.Id == guid && message.Priority == DisplayableMessagePriority.Normal &&
                                       message.Classification == DisplayableMessageClassification.SoftError)));

                Assert.IsTrue(_target.IsSoftErrorStateActive(state));
            }
        }

        [TestMethod]
        public void DisableAfterSoftErrorTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<HostOfflineEvent>()));
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { DisableOnDisconnect = false });

            Assert.IsTrue(
                _target.Disable(SystemDisablePriority.Normal, DisableState.DisabledByHost0, false).Wait(WaitTime));
            _messageDisplay.Verify(
                x => x.DisplayMessage(
                    It.Is<IDisplayableMessage>(
                        message => message.Id == ApplicationConstants.DisabledByHost0Key &&
                                   message.Priority == DisplayableMessagePriority.Normal &&
                                   message.Classification == DisplayableMessageClassification.SoftError)));
            Assert.IsTrue(_target.IsSoftErrorStateActive(DisableState.DisabledByHost0));
            Assert.IsFalse(_target.IsDisableStateActive(DisableState.DisabledByHost0));

            Assert.IsTrue(
                _target.Disable(SystemDisablePriority.Normal, DisableState.DisabledByHost0, true).Wait(WaitTime));
            _systemDisableManager.Verify(
                x => x.Disable(
                    ApplicationConstants.DisabledByHost0Key,
                    SystemDisablePriority.Normal,
                    "DisabledByHost0",
                    CultureProviderType.Player));
            Assert.IsFalse(_target.IsSoftErrorStateActive(DisableState.DisabledByHost0));
            Assert.IsTrue(_target.IsDisableStateActive(DisableState.DisabledByHost0));
        }

        [DynamicData(nameof(EnableTestData))]
        [DataTestMethod]
        public void EnableTest(
            DisableState startingState,
            DisableState[] clearingState,
            DisableState expectedState,
            IEnumerable<Guid> disableGuids)
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<HostOfflineEvent>()));

            foreach (var guid in disableGuids)
            {
                _systemDisableManager.Setup(x => x.Enable(guid)).Verifiable();
            }

            _disableDataProvider.Setup(x => x.GetData()).Returns(new SasDisableInformation { DisableStates = startingState });
            _disableDataProvider.Setup(x => x.Save(It.Is<SasDisableInformation>(d => d.DisableStates == expectedState)))
                .Returns(Task.CompletedTask)
                .Verifiable();
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { DisableOnDisconnect = false });

            _target = CreateDisableProvider();
            Assert.IsTrue(_target.Enable(clearingState).Wait(WaitTime));

            _disableDataProvider.Verify();
            _systemDisableManager.Verify();
        }

        [TestMethod]
        public void IsDisableStateActiveTest()
        {
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { DisableOnDisconnect = false });
            Assert.IsTrue(
                _target.Disable(SystemDisablePriority.Immediate, DisableState.DisabledByHost0).Wait(WaitTime));

            Assert.IsTrue(_target.IsDisableStateActive(DisableState.DisabledByHost0));
            Assert.IsFalse(_target.IsDisableStateActive(DisableState.Host0CommunicationsOffline));
        }

        [TestMethod]
        public void OnSasReconfiguredTest()
        {
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { DisableOnDisconnect = false });
            Assert.IsTrue(
                _target.Disable(SystemDisablePriority.Immediate, DisableState.DisabledByHost0).Wait(WaitTime));

            Assert.IsTrue(_target.OnSasReconfigured().Wait(WaitTime));
            Assert.IsFalse(_target.IsDisableStateActive(DisableState.DisabledByHost0));
        }

        [DataRow(true, false, true, DisplayName = "SAS disable with Game Idle, expect Force Cashout")]
        [DataRow(
            false,
            false,
            false,
            DisplayName = "SAS disable with Game not Idle, expect not to Force Cashout immediately")]
        [DataRow(
            true,
            true,
            false,
            DisplayName = "SAS disable with another hard lockup, expect not to Force Cashout immediately")]
        [DataTestMethod]
        public void ForceCashoutTests(bool gamePlayStateIdle, bool anyOtherLockup, bool forceCashout)
        {
            const DisableState state = DisableState.DisabledByHost0;
            _gamePlayState.Setup(x => x.Idle).Returns(gamePlayStateIdle);
            if (anyOtherLockup)
            {
                _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(
                    new List<Guid> { ApplicationConstants.LogicDoorGuid, ApplicationConstants.DisabledByHost0Key });
            }
            else
            {
                _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(
                    new List<Guid> { ApplicationConstants.DisabledByHost0Key });
            }

            _playerBank.Setup(x => x.Balance).Returns(100L);
            _ = _target.Disable(
                SystemDisablePriority.Normal,
                state);
            Assert.IsTrue(_target.IsDisableStateActive(state));

            if (forceCashout)
            {
                _playerBank.Verify(x => x.CashOut(true), Times.Once);
            }
            else
            {
                _playerBank.Verify(x => x.CashOut(true), Times.Never);
            }
        }

        [TestMethod]
        public void WaitForGameStateToIdleAndForceCashout()
        {
            Action<GamePlayStateChangedEvent> callback = null;

            const DisableState state = DisableState.DisabledByHost0;
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<GamePlayStateChangedEvent>>()))
                .Callback((object c, Action<GamePlayStateChangedEvent> eventCallBack) => { callback = eventCallBack; });
            _target = CreateDisableProvider();

            _gamePlayState.Setup(x => x.Idle).Returns(false);
            _playerBank.Setup(x => x.Balance).Returns(100L);
            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(
                new List<Guid> { ApplicationConstants.DisabledByHost0Key });
            _ = _target.Disable(
                SystemDisablePriority.Normal,
                state);
            Assert.IsTrue(_target.IsDisableStateActive(state));
            // Force cashout shouldn't happen since GamePlayState is not idle
            _playerBank.Verify(x => x.CashOut(true), Times.Never);

            _gamePlayState.Setup(x => x.Idle).Returns(true);
            // Previous states and new states doesnt matter as we are expecting State to be in Idle.
            callback.Invoke(new GamePlayStateChangedEvent(PlayState.PayGameResults, PlayState.Idle));
            _playerBank.Verify(x => x.CashOut(true), Times.Once);
        }

        [TestMethod]
        public void WaitForOtherLockupToClearAndForceCashout()
        {
            Action<SystemDisableRemovedEvent> callback = null;

            const DisableState state = DisableState.DisabledByHost0;
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<SystemDisableRemovedEvent>>()))
                .Callback((object c, Action<SystemDisableRemovedEvent> eventCallBack) => { callback = eventCallBack; });
            _target = CreateDisableProvider();
            
            _gamePlayState.Setup(x => x.Idle).Returns(true);
            _playerBank.Setup(x => x.Balance).Returns(100L);
            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(
                new List<Guid> { ApplicationConstants.DisabledByHost0Key, ApplicationConstants.LogicDoorGuid });
            _ = _target.Disable(
                SystemDisablePriority.Normal,
                state);
            Assert.IsTrue(_target.IsDisableStateActive(state));
            // Force cashout shouldn't happen since LogicDoor is open
            _playerBank.Verify(x => x.CashOut(true), Times.Never);

            // Clear LogicDoor
            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(
                new List<Guid> { ApplicationConstants.DisabledByHost0Key });
            // Events parameters doesnt matter since we are expecting EnabledKeys to return HostDisable lockup only
            callback.Invoke(
                new SystemDisableRemovedEvent(SystemDisablePriority.Normal, Guid.NewGuid(), "", false, false));
            _playerBank.Verify(x => x.CashOut(true), Times.Once);
        }

        private SasDisableProvider CreateDisableProvider(
            bool nullPropertiesManager = false,
            bool nullDisableDataProvider = false,
            bool nullSystemDisableManager = false,
            bool nullMessageDisplay = false,
            bool nullGamePlayState = false,
            bool nullEventBus = false,
            bool nullPlayerBank = false)
        {
            return new SasDisableProvider(
                nullPropertiesManager ? null : _propertiesManager.Object,
                nullDisableDataProvider ? null : _disableDataProvider.Object,
                nullSystemDisableManager ? null : _systemDisableManager.Object,
                nullMessageDisplay ? null : _messageDisplay.Object,
                nullEventBus ? null : _eventBus.Object,
                nullGamePlayState ? null : _gamePlayState.Object,
                nullPlayerBank ? null : _playerBank.Object
            );
        }
    }
}