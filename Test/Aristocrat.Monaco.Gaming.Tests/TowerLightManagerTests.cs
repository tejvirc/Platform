namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Xml.Serialization;
    using Accounting.Contracts.Handpay;
    using Application.Contracts;
    using Application.Contracts.EdgeLight;
    using Application.Contracts.OperatorMenu;
    using Kernel.Contracts.MessageDisplay;
    using Contracts;
    using Contracts.TowerLight;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.TowerLight;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using TowerLight;
    using Vgt.Client12.Application.OperatorMenu;
    using Kernel.MessageDisplay;
    using Aristocrat.Monaco.Localization.Properties;

    [TestClass]
    public class TowerLightManagerTests
    {
        private Mock<IConfigurationUtility> _configUtility;
        private Mock<IDoorService> _doorService;
        private Mock<IEventBus> _eventBus;
        private Mock<Dictionary<int, LogicalDoor>> _logicalDoors;
        private Mock<ISystemDisableManager> _systemDisableManager;
        private Mock<ITowerLight> _towerLight;
        private Mock<IMessageDisplay> _messageDisplay;
        private Mock<IEdgeLightingStateManager> _edgeLightingStateManager;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IOperatorMenuLauncher> _operatorMenuLauncher;
        private Action<OpenEvent> _handlerOpenEvent;
        private Action<ClosedEvent> _handlerCloseEvent;
        private Action<GameEndedEvent> _handlerGameEndedEvent;
        private Action<PrimaryGameStartedEvent> _handlerPrimaryGameStartedEvent;
        private Action<CallAttendantButtonOnEvent> _handlerCallAttendantButtonOnEvent;
        private Action<CallAttendantButtonOffEvent> _handlerCallAttendantButtonOffEvent;
        private Action<SystemDisableAddedEvent> _handlerSystemDisableAddedEvent;
        private Action<SystemDisableRemovedEvent> _handlerSystemDisableRemovedEvent;
        private Action<HandpayStartedEvent> _handlerHandpayStartedEvent;
        private Action<HandpayCompletedEvent> _handlerHandpayCompletedEvent;
        private Action<OperatorMenuEnteredEvent> _handlerOperatorMenuEnteredEvent;
        private Action<OperatorMenuExitedEvent> _handlerOperatorMenuExitedEvent;

        private TowerLightManager _target;

        [TestInitialize]
        public void Initialize()
        {
            _eventBus = new Mock<IEventBus>();
            _systemDisableManager = new Mock<ISystemDisableManager>();
            _configUtility = new Mock<IConfigurationUtility>();
            _doorService = new Mock<IDoorService>();
            _towerLight = new Mock<ITowerLight>();
            _messageDisplay = new Mock<IMessageDisplay>();
            _logicalDoors = new Mock<Dictionary<int, LogicalDoor>>();
            _edgeLightingStateManager = new Mock<IEdgeLightingStateManager>();
            _propertiesManager = new Mock<IPropertiesManager>();
            _operatorMenuLauncher = new Mock<IOperatorMenuLauncher>();

            _systemDisableManager.SetupGet(m => m.CurrentDisableKeys).Returns(new List<Guid>());

            SetupLogicDoors();
            SetupGetConfigurationMethod();

            _eventBus.Setup(x => x.Subscribe(It.IsAny<TowerLightManager>(), It.IsAny<Action<PrimaryGameStartedEvent>>()))
                .Callback((object x, Action<PrimaryGameStartedEvent> y) => _handlerPrimaryGameStartedEvent = y);

            _eventBus.Setup(x => x.Subscribe(It.IsAny<TowerLightManager>(), It.IsAny<Action<GameEndedEvent>>()))
                .Callback((object x, Action<GameEndedEvent> y) => _handlerGameEndedEvent = y);

            _eventBus.Setup(x => x.Subscribe(It.IsAny<TowerLightManager>(), It.IsAny<Action<OpenEvent>>()))
                .Callback((object x, Action<OpenEvent> y) => _handlerOpenEvent = y);

            _eventBus.Setup(x => x.Subscribe(It.IsAny<TowerLightManager>(), It.IsAny<Action<ClosedEvent>>()))
                .Callback((object x, Action<ClosedEvent> y) => _handlerCloseEvent = y);

            _eventBus.Setup(x => x.Subscribe(It.IsAny<TowerLightManager>(), It.IsAny<Action<CallAttendantButtonOnEvent>>()))
                .Callback((object x, Action<CallAttendantButtonOnEvent> y) => _handlerCallAttendantButtonOnEvent = y);

            _eventBus.Setup(x => x.Subscribe(It.IsAny<TowerLightManager>(), It.IsAny<Action<CallAttendantButtonOffEvent>>()))
                .Callback((object x, Action<CallAttendantButtonOffEvent> y) => _handlerCallAttendantButtonOffEvent = y);

            _eventBus.Setup(x => x.Subscribe(It.IsAny<TowerLightManager>(), It.IsAny<Action<SystemDisableAddedEvent>>()))
                .Callback((object x, Action<SystemDisableAddedEvent> y) => _handlerSystemDisableAddedEvent = y);

            _eventBus.Setup(x => x.Subscribe(It.IsAny<TowerLightManager>(), It.IsAny<Action<SystemDisableRemovedEvent>>()))
                .Callback((object x, Action<SystemDisableRemovedEvent> y) => _handlerSystemDisableRemovedEvent = y);

            _eventBus.Setup(x => x.Subscribe(It.IsAny<TowerLightManager>(), It.IsAny<Action<HandpayStartedEvent>>()))
                .Callback((object x, Action<HandpayStartedEvent> y) => _handlerHandpayStartedEvent = y);

            _eventBus.Setup(x => x.Subscribe(It.IsAny<TowerLightManager>(), It.IsAny<Action<HandpayCompletedEvent>>()))
                .Callback((object x, Action<HandpayCompletedEvent> y) => _handlerHandpayCompletedEvent = y);

            _eventBus.Setup(x => x.Subscribe(It.IsAny<TowerLightManager>(), It.IsAny<Action<OperatorMenuEnteredEvent>>()))
                .Callback((object x, Action<OperatorMenuEnteredEvent> y) => _handlerOperatorMenuEnteredEvent = y);

            _eventBus.Setup(x => x.Subscribe(It.IsAny<TowerLightManager>(), It.IsAny<Action<OperatorMenuExitedEvent>>()))
                .Callback((object x, Action<OperatorMenuExitedEvent> y) => _handlerOperatorMenuExitedEvent = y);


            _target = new TowerLightManager(
                _eventBus.Object,
                _systemDisableManager.Object,
                _configUtility.Object,
                _doorService.Object,
                _towerLight.Object,
                _messageDisplay.Object,
                _edgeLightingStateManager.Object,
                _propertiesManager.Object,
                _operatorMenuLauncher.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
        }

        private void SetupGetConfigurationMethod()
        {
            var towerLightConfig = new TowerLightConfiguration
            {
                SignalDefinitions = new[]
                {
                    new SignalDefinitionsType()
                    {
                        OperationalCondition = new []
                        {
                            GetAuditMenuConfiguration(),
                            GetHandpayConfiguration(),
                            GetTiltConfiguration(),
                            GetSoftErrorConfiguration(),
                            GetIdleConfiguration()
                        }
                    }
                }
            };

            _configUtility.Setup(c => c.GetConfiguration(It.IsAny<string>(), It.IsAny<Func<TowerLightConfiguration>>()))
                .Returns(towerLightConfig);
        }

        private static OperationalConditionType GetIdleConfiguration()
        {
            return new OperationalConditionType
            {
                condition = new[] { "Idle" },
                DoorCondition = new[]
                {
                    new DoorConditionType
                    {
                        condition = "DoorOpen",
                        Set = new []
                        {
                            new SetType
                            {
                                lightTier = "Tier1",
                                flashState = "MediumFlash"
                            },
                            new SetType
                            {
                                lightTier = "Tier2",
                                flashState = "Off"
                            }
                        }
                    },
                    new DoorConditionType
                    {
                        condition = "DoorWasOpenBefore",
                        Set = new []
                        {
                            new SetType
                            {
                                lightTier = "Tier1",
                                flashState = "On"
                            },
                            new SetType
                            {
                                lightTier = "Tier2",
                                flashState = "Off"
                            }
                        }
                    },
                    new DoorConditionType
                    {
                        condition = "AllClosed",
                        Set = new []
                        {
                            new SetType
                            {
                                lightTier = "Tier1",
                                flashState = "Off"
                            },
                            new SetType
                            {
                                lightTier = "Tier2",
                                flashState = "Off"
                            }
                        }
                    }
                }
            };
        }

        private static OperationalConditionType GetSoftErrorConfiguration()
        {
            return new OperationalConditionType
            {
                condition = new[] { "SoftError" },
                DoorCondition = new[]
                {
                    new DoorConditionType
                    {
                        condition = "DoorOpen",
                        Set = new []
                        {
                            new SetType
                            {
                                lightTier = "Tier1",
                                flashState = "MediumFlash"
                            },
                            new SetType
                            {
                                lightTier = "Tier2",
                                flashState = "On"
                            }
                        }
                    },
                    new DoorConditionType
                    {
                        condition = "DoorWasOpenBefore",
                        Set = new []
                        {
                            new SetType
                            {
                                lightTier = "Tier1",
                                flashState = "On"
                            },
                            new SetType
                            {
                                lightTier = "Tier2",
                                flashState = "On"
                            }
                        }
                    },
                    new DoorConditionType
                    {
                        condition = "AllClosed",
                        Set = new []
                        {
                            new SetType
                            {
                                lightTier = "Tier1",
                                flashState = "Off"
                            },
                            new SetType
                            {
                                lightTier = "Tier2",
                                flashState = "On"
                            }
                        }
                    }
                }
            };
        }

        private static OperationalConditionType GetTiltConfiguration()
        {
            return new OperationalConditionType
            {
                condition = new[] { "Tilt" },
                DoorCondition = new[]
                {
                    new DoorConditionType
                    {
                        condition = "DoorOpen",
                        Set = new []
                        {
                            new SetType
                            {
                                lightTier = "Tier1",
                                flashState = "MediumFlash"
                            },
                            new SetType
                            {
                                lightTier = "Tier2",
                                flashState = "SlowFlash"
                            }
                        }
                    },
                    new DoorConditionType
                    {
                        condition = "DoorWasOpenBefore",
                        Set = new []
                        {
                            new SetType
                            {
                                lightTier = "Tier1",
                                flashState = "On"
                            },
                            new SetType
                            {
                                lightTier = "Tier2",
                                flashState = "SlowFlash"
                            }
                        }
                    },
                    new DoorConditionType
                    {
                        condition = "AllClosed",
                        Set = new []
                        {
                            new SetType
                            {
                                lightTier = "Tier1",
                                flashState = "Off"
                            },
                            new SetType
                            {
                                lightTier = "Tier2",
                                flashState = "SlowFlash"
                            }
                        }
                    }
                }
            };
        }

        private static OperationalConditionType GetHandpayConfiguration()
        {
            return new OperationalConditionType
            {
                condition = new[] { "Handpay" },
                DoorCondition = new[]
                {
                    new DoorConditionType
                    {
                        condition = "DoorOpen",
                        Set = new []
                        {
                            new SetType
                            {
                                lightTier = "Tier1",
                                flashState = "MediumFlash"
                            },
                            new SetType
                            {
                                lightTier = "Tier2",
                                flashState = "SlowFlash"
                            }
                        }
                    },
                    new DoorConditionType
                    {
                        condition = "DoorWasOpenBefore",
                        Set = new []
                        {
                            new SetType
                            {
                                lightTier = "Tier1",
                                flashState = "SlowFlash"
                            },
                            new SetType
                            {
                                lightTier = "Tier2",
                                flashState = "SlowFlash"
                            }
                        }
                    },
                    new DoorConditionType
                    {
                        condition = "AllClosed",
                        Set = new []
                        {
                            new SetType
                            {
                                lightTier = "Tier1",
                                flashState = "SlowFlash"
                            },
                            new SetType
                            {
                                lightTier = "Tier2",
                                flashState = "SlowFlash"
                            }
                        }
                    }
                }
            };
        }

        private static OperationalConditionType GetAuditMenuConfiguration()
        {
            return new OperationalConditionType
            {
                condition = new[] { "AuditMenu" },
                DoorCondition = new[]
                {
                    new DoorConditionType
                    {
                        condition = "DoorOpen",
                        Set = new[]
                        {
                            new SetType
                            {
                                lightTier = "Tier1",
                                flashState = "MediumFlash"
                            },
                            new SetType
                            {
                                lightTier = "Tier2",
                                flashState = "SlowFlash"
                            }
                        }
                    },
                    new DoorConditionType
                    {
                        condition = "DoorWasOpenBefore",
                        Set = new []
                        {
                            new SetType
                            {
                                lightTier = "Tier1",
                                flashState = "On"
                            },
                            new SetType
                            {
                                lightTier = "Tier2",
                                flashState = "SlowFlash"
                            }
                        }
                    },
                    new DoorConditionType
                    {
                        condition = "AllClosed",
                        Set = new []
                        {
                            new SetType
                            {
                                lightTier = "Tier1",
                                flashState = "Off"
                            },
                            new SetType
                            {
                                lightTier = "Tier2",
                                flashState = "SlowFlash"
                            }
                        }
                    }
                }
            };
        }

        private void SetupLogicDoors()
        {
            foreach (DoorLogicalId entry in Enum.GetValues(typeof(DoorLogicalId)))
            {
                _logicalDoors.Object.Add((int)entry, new LogicalDoor { Closed = true });
            }

            _doorService.SetupGet(d => d.LogicalDoors).Returns(_logicalDoors.Object);
        }

        private void SetupMainDoorOpen()
        {
            _doorService.Setup(d => d.GetDoorClosed(It.IsAny<int>())).Returns(true);
            _doorService.Setup(d => d.GetDoorClosed((int)DoorLogicalId.Main)).Returns(false);
        }

        private void SetupAllDoorClosed()
        {
            _doorService.Setup(d => d.GetDoorClosed(It.IsAny<int>())).Returns(true);
        }

        [TestMethod]
        [DataRow("TwoTier", 2)]
        [DataRow("FourTier", 4)]
        [DataRow("Bla", 2)] //TwoTier should be selected if no defined
        [DataRow(null, 2)] //TwoTier should be selected if no defined
        [DataRow("", 2)] //TwoTier should be selected if no defined
        public void GivenTierTypeWhenInitializeThenSignalDefinitionSelected(string selectedTier, int count)
        {
            var towerLightConfig = new TowerLightConfiguration
            {
                SignalDefinitions = new[]
                {
                        new SignalDefinitionsType()
                        {
                            Tier = "TwoTier",
                            OperationalCondition = new[]
                            {
                                new OperationalConditionType
                                {
                                    condition = new[] { "AuditMenu" },
                                    DoorCondition = new[]
                                    {
                                        new DoorConditionType
                                        {
                                            condition = "DoorOpen",
                                            Set = new[]
                                            {
                                                new SetType
                                                {
                                                    lightTier = "Tier1",
                                                    flashState = "MediumFlash"
                                                },
                                                new SetType
                                                {
                                                    lightTier = "Tier2",
                                                    flashState = "SlowFlash"
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new SignalDefinitionsType()
                        {
                            Tier = "FourTier",
                            OperationalCondition = new[]
                            {
                                new OperationalConditionType
                                {
                                    condition = new[] { "AuditMenu" },
                                    DoorCondition = new[]
                                    {
                                        new DoorConditionType
                                        {
                                            condition = "DoorOpen",
                                            Set = new[]
                                            {
                                                new SetType
                                                {
                                                    lightTier = "Tier1",
                                                    flashState = "MediumFlash"
                                                },
                                                new SetType
                                                {
                                                    lightTier = "Tier2",
                                                    flashState = "SlowFlash"
                                                },
                                                new SetType
                                                {
                                                    lightTier = "Tier3",
                                                    flashState = "SlowFlash"
                                                },
                                                new SetType
                                                {
                                                    lightTier = "Tier4",
                                                    flashState = "SlowFlash"
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
            };

            _configUtility.Setup(c => c.GetConfiguration(It.IsAny<string>(), It.IsAny<Func<TowerLightConfiguration>>()))
                .Returns(towerLightConfig);

            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.TowerLightTierTypeSelection
                    , Application.Contracts.TowerLight.TowerLightTierTypes.Undefined.ToString()))
                .Returns(selectedTier)
                .Verifiable();

            using (var underTest = new TowerLightManager(
                _eventBus.Object,
                _systemDisableManager.Object,
                _configUtility.Object,
                _doorService.Object,
                _towerLight.Object,
                _messageDisplay.Object,
                _edgeLightingStateManager.Object,
                _propertiesManager.Object,
                _operatorMenuLauncher.Object))
            {
                var result = underTest.ConfiguredLightTiers.ToArray();
                Assert.AreEqual(count, result.Length);
            }
        }

        [TestMethod]
        public void GivenTierTypeWhenInitializeNotConfiguredThenSignalDefinitionEmpty()
        {
            var towerLightConfig = new TowerLightConfiguration
            {
                SignalDefinitions = new[]
                {
                        new SignalDefinitionsType()
                        {
                            Tier = "TwoTier",
                            OperationalCondition = new[]
                            {
                                new OperationalConditionType
                                {
                                    condition = new[] { "AuditMenu" },
                                    DoorCondition = new[]
                                    {
                                        new DoorConditionType
                                        {
                                            condition = "DoorOpen",
                                            Set = new[]
                                            {
                                                new SetType
                                                {
                                                    lightTier = "Tier1",
                                                    flashState = "MediumFlash"
                                                },
                                                new SetType
                                                {
                                                    lightTier = "Tier2",
                                                    flashState = "SlowFlash"
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
            };

            _configUtility.Setup(c => c.GetConfiguration(It.IsAny<string>(), It.IsAny<Func<TowerLightConfiguration>>()))
                .Returns(towerLightConfig);

            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.TowerLightTierTypeSelection
                    , Application.Contracts.TowerLight.TowerLightTierTypes.Undefined.ToString()))
                .Returns(Application.Contracts.TowerLight.TowerLightTierTypes.FourTier.ToString())
                .Verifiable();

            using (var underTest = new TowerLightManager(
                _eventBus.Object,
                _systemDisableManager.Object,
                _configUtility.Object,
                _doorService.Object,
                _towerLight.Object,
                _messageDisplay.Object,
                _edgeLightingStateManager.Object,
                _propertiesManager.Object,
                _operatorMenuLauncher.Object))
            {
                var result = underTest.ConfiguredLightTiers;
                Assert.IsFalse(result.Any());
            }
        }

        [TestMethod]
        public void GivenDoorWasOpenBeforeResetEndGameConditionWhenAllClosedThenSignalIsNotResetAndResetAfterGameEnded()
        {
            var doorWasOpenBeforeResetEndGame = new DoorConditionType
            {
                condition = "DoorWasOpenBeforeResetEndGame",
                Set = new[]
                {
                    new SetType { lightTier = "Tier1", flashState = "MediumFlash" },
                    new SetType { lightTier = "Tier2", flashState = "SlowFlash" }
                }
            };
            var allClosed = new DoorConditionType
            {
                condition = "AllClosed",
                Set = new[]
                {
                    new SetType { lightTier = "Tier1", flashState = "Off" },
                    new SetType { lightTier = "Tier2", flashState = "Off" }
                }
            };

            var towerLightConfig = new TowerLightConfiguration
            {
                SignalDefinitions = new[]
                {
                    new SignalDefinitionsType()
                    {
                        Tier = "TwoTier",
                        OperationalCondition = new[]
                        {
                            new OperationalConditionType
                            {
                                condition = new[] { "Idle" },
                                DoorCondition = new []{ doorWasOpenBeforeResetEndGame, allClosed }
                            }
                        }
                    }
                }
            };

            _configUtility.Setup(c => c.GetConfiguration(It.IsAny<string>(), It.IsAny<Func<TowerLightConfiguration>>()))
                .Returns(towerLightConfig);

            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.TowerLightTierTypeSelection
                    , Application.Contracts.TowerLight.TowerLightTierTypes.Undefined.ToString()))
                .Returns(Application.Contracts.TowerLight.TowerLightTierTypes.TwoTier.ToString())
                .Verifiable();

            using (new TowerLightManager(
                _eventBus.Object,
                _systemDisableManager.Object,
                _configUtility.Object,
                _doorService.Object,
                _towerLight.Object,
                _messageDisplay.Object,
                _edgeLightingStateManager.Object,
                _propertiesManager.Object,
                _operatorMenuLauncher.Object))
            {
                _towerLight.ResetCalls();
                SetupMainDoorOpen();
                _handlerOpenEvent?.Invoke(new OpenEvent(1, "Bla"));
                _towerLight.Verify(
                    m => m.SetFlashState(
                        It.Is<LightTier>(x => x == LightTier.Tier1),
                        It.Is<FlashState>(x => x == FlashState.MediumFlash), Timeout.InfiniteTimeSpan, false));

                _towerLight.Verify(
                    m => m.SetFlashState(
                        It.Is<LightTier>(x => x == LightTier.Tier2),
                        It.Is<FlashState>(x => x == FlashState.SlowFlash), Timeout.InfiniteTimeSpan, false));

                _towerLight.ResetCalls();
                SetupAllDoorClosed();
                _handlerCloseEvent?.Invoke(new ClosedEvent(1, "Bla"));
                _towerLight.Verify(
                    m => m.SetFlashState(
                        It.Is<LightTier>(x => x == LightTier.Tier1),
                        It.Is<FlashState>(x => x == FlashState.MediumFlash), Timeout.InfiniteTimeSpan, false));

                _towerLight.Verify(
                    m => m.SetFlashState(
                        It.Is<LightTier>(x => x == LightTier.Tier2),
                        It.Is<FlashState>(x => x == FlashState.SlowFlash), Timeout.InfiniteTimeSpan, false));

                _towerLight.ResetCalls();
                _handlerGameEndedEvent?.Invoke(new GameEndedEvent(1, 1, "bla", Mock.Of<IGameHistoryLog>()));
                _towerLight.Verify(
                    m => m.SetFlashState(
                        It.Is<LightTier>(x => x == LightTier.Tier1),
                        It.Is<FlashState>(x => x == FlashState.Off), Timeout.InfiniteTimeSpan, false));

                _towerLight.Verify(
                    m => m.SetFlashState(
                        It.Is<LightTier>(x => x == LightTier.Tier2),
                        It.Is<FlashState>(x => x == FlashState.Off), Timeout.InfiniteTimeSpan, false));
            }
        }

        [TestMethod]
        public void GivenMacau4TierWhenConditionChangedThenSignalChanged()
        {
            var towerLightConfig = GetConfig("Macau");

            _configUtility.Setup(c => c.GetConfiguration(It.IsAny<string>(), It.IsAny<Func<TowerLightConfiguration>>()))
                .Returns(towerLightConfig);

            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.TowerLightTierTypeSelection
                    , Application.Contracts.TowerLight.TowerLightTierTypes.Undefined.ToString()))
                .Returns(Application.Contracts.TowerLight.TowerLightTierTypes.FourTier.ToString())
                .Verifiable();
            _towerLight.ResetCalls();
            var flashStateDic = new Dictionary<LightTier, FlashState>()
            {
                { LightTier.Tier1, FlashState.Off },
                { LightTier.Tier2, FlashState.Off },
                { LightTier.Tier3, FlashState.Off },
                { LightTier.Tier4, FlashState.Off },
            };
            _towerLight.Setup(
                    m => m.SetFlashState(
                        It.IsAny<LightTier>(),
                        It.IsAny<FlashState>(),
                        Timeout.InfiniteTimeSpan, false))
                .Callback<LightTier, FlashState, TimeSpan, bool>(
                    (lightTier, flashState, duration, test) =>
                    {
                        flashStateDic[lightTier] = flashState;
                    });
            using (new TowerLightManager(
                _eventBus.Object,
                _systemDisableManager.Object,
                _configUtility.Object,
                _doorService.Object,
                _towerLight.Object,
                _messageDisplay.Object,
                _edgeLightingStateManager.Object,
                _propertiesManager.Object,
                _operatorMenuLauncher.Object))
            {
                // open door, idle: DoorOpen
                SetupMainDoorOpen();
                _handlerOpenEvent?.Invoke(new OpenEvent(1, "Bla"));
                Assert.AreEqual(FlashState.SlowFlash, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier3]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier4]);


                // door closed, idle: AllClosed | DoorWasOpenBefore | DoorWasOpenBeforeResetEndGame
                SetupAllDoorClosed();
                _handlerCloseEvent?.Invoke(new ClosedEvent(1, "Bla"));
                Assert.AreEqual(FlashState.SlowFlash, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier3]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier4]);

                // service button pressed. idle | service: AllClosed | DoorWasOpenBefore | DoorWasOpenBeforeResetEndGame
                _handlerCallAttendantButtonOnEvent?.Invoke(new CallAttendantButtonOnEvent());
                Assert.AreEqual(FlashState.SlowFlash, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.SlowFlash, flashStateDic[LightTier.Tier3]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier4]);


                // service button reset. idle: AllClosed | DoorWasOpenBefore | DoorWasOpenBeforeResetEndGame
                _handlerCallAttendantButtonOffEvent?.Invoke(new CallAttendantButtonOffEvent());
                Assert.AreEqual(FlashState.SlowFlash, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier3]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier4]);

                // game started. idle: AllClosed | DoorWasOpenBeforeResetEndGame
                _handlerPrimaryGameStartedEvent?.Invoke(new PrimaryGameStartedEvent(1, 1, "bla", Mock.Of<IGameHistoryLog>()));
                Assert.AreEqual(FlashState.SlowFlash, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier3]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier4]);

                // game ended. idle: AllClosed
                _handlerGameEndedEvent?.Invoke(new GameEndedEvent(1, 1, "bla", Mock.Of<IGameHistoryLog>()));
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier3]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier4]);

                var disableId1 = Guid.NewGuid();
                var disableId2 = Guid.NewGuid();
                // tilt. tilt: AllClosed
                _handlerSystemDisableAddedEvent?.Invoke(new SystemDisableAddedEvent(SystemDisablePriority.Normal, disableId1, "bla", true));
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier3]);
                Assert.AreEqual(FlashState.SlowFlash, flashStateDic[LightTier.Tier4]);

                // tilt. tilt: DoorOpen | DoorWasOpenBefore | DoorWasOpenBeforeResetEndGame
                // need to disable second one to keep tilt on
                _handlerSystemDisableAddedEvent?.Invoke(new SystemDisableAddedEvent(SystemDisablePriority.Normal, disableId2, "bla", true));
                SetupMainDoorOpen();
                _handlerOpenEvent?.Invoke(new OpenEvent(1, "Bla"));
                Assert.AreEqual(FlashState.SlowFlash, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier3]);
                Assert.AreEqual(FlashState.SlowFlash, flashStateDic[LightTier.Tier4]);

                // service button pressed. tilt | service: DoorOpen | DoorWasOpenBefore | DoorWasOpenBeforeResetEndGame
                _handlerCallAttendantButtonOnEvent?.Invoke(new CallAttendantButtonOnEvent());
                Assert.AreEqual(FlashState.SlowFlash, flashStateDic[LightTier.Tier1]); // door open
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.SlowFlash, flashStateDic[LightTier.Tier3]); // service
                Assert.AreEqual(FlashState.SlowFlash, flashStateDic[LightTier.Tier4]); // tilt

                // service button reset. tilt: DoorOpen | DoorWasOpenBefore | DoorWasOpenBeforeResetEndGame
                _handlerCallAttendantButtonOffEvent?.Invoke(new CallAttendantButtonOffEvent());
                Assert.AreEqual(FlashState.SlowFlash, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier3]);
                Assert.AreEqual(FlashState.SlowFlash, flashStateDic[LightTier.Tier4]);

                // remove tilt: idle: DoorOpen | DoorWasOpenBefore | DoorWasOpenBeforeResetEndGame
                _handlerSystemDisableRemovedEvent?.Invoke(new SystemDisableRemovedEvent(SystemDisablePriority.Normal, disableId1, "bla", true, true));
                _handlerSystemDisableRemovedEvent?.Invoke(new SystemDisableRemovedEvent(SystemDisablePriority.Normal, disableId2, "bla", true, true));
                Assert.AreEqual(FlashState.SlowFlash, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier3]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier4]);

                SetupAllDoorClosed();
                _handlerCloseEvent?.Invoke(new ClosedEvent(1, "Bla"));
                // start game
                _handlerPrimaryGameStartedEvent?.Invoke(new PrimaryGameStartedEvent(1, 1, "bla", Mock.Of<IGameHistoryLog>()));
                //end game
                _handlerGameEndedEvent?.Invoke(new GameEndedEvent(1, 1, "bla", Mock.Of<IGameHistoryLog>()));
                // idle: AllClosed
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier3]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier4]);

                // add hand pay first and tilt second. remove tilt first and hand pay second
                // hand pay: hand pay : AllClosed
                _handlerHandpayStartedEvent?.Invoke(new HandpayStartedEvent(HandpayType.GameWin, 1, 1, 1, 1, false));
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.SlowFlash, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier3]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier4]);

                // tilt. tilt | hand pay: AllClosed
                _handlerSystemDisableAddedEvent?.Invoke(new SystemDisableAddedEvent(SystemDisablePriority.Normal, disableId1, "bla", true));
                // need to disable second one to keep tilt on for hand pay
                _handlerSystemDisableAddedEvent?.Invoke(new SystemDisableAddedEvent(SystemDisablePriority.Normal, disableId2, "bla", true));
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.SlowFlash, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier3]);
                Assert.AreEqual(FlashState.SlowFlash, flashStateDic[LightTier.Tier4]);

                // remove tilt: hand pay : AllClosed
                _handlerSystemDisableRemovedEvent?.Invoke(new SystemDisableRemovedEvent(SystemDisablePriority.Normal, disableId1, "bla", true, true));
                _handlerSystemDisableRemovedEvent?.Invoke(new SystemDisableRemovedEvent(SystemDisablePriority.Normal, disableId2, "bla", true, true));
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.SlowFlash, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier3]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier4]);

                // remove hand pay: idle : AllClosed
                _handlerHandpayCompletedEvent?.Invoke(new HandpayCompletedEvent(new HandpayTransaction()));
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier3]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier4]);
                // add hand pay first and tilt second. remove tilt first and hand pay second

                // add hand pay first and tilt second. remove tilt second and hand pay first
                // hand pay: hand pay : AllClosed
                _handlerHandpayStartedEvent?.Invoke(new HandpayStartedEvent(HandpayType.GameWin, 1, 1, 1, 1, false));
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.SlowFlash, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier3]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier4]);

                // tilt. tilt | hand pay: AllClosed
                _handlerSystemDisableAddedEvent?.Invoke(new SystemDisableAddedEvent(SystemDisablePriority.Normal, disableId1, "bla", true));
                // need to disable second one to keep tilt on for hand pay
                _handlerSystemDisableAddedEvent?.Invoke(new SystemDisableAddedEvent(SystemDisablePriority.Normal, disableId2, "bla", true));
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.SlowFlash, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier3]);
                Assert.AreEqual(FlashState.SlowFlash, flashStateDic[LightTier.Tier4]);

                // remove hand pay: tilt : AllClosed
                _handlerHandpayCompletedEvent?.Invoke(new HandpayCompletedEvent(new HandpayTransaction()));
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier3]);
                Assert.AreEqual(FlashState.SlowFlash, flashStateDic[LightTier.Tier4]);


                // remove tilt: idle : AllClosed
                _handlerSystemDisableRemovedEvent?.Invoke(new SystemDisableRemovedEvent(SystemDisablePriority.Normal, disableId1, "bla", true, true));
                _handlerSystemDisableRemovedEvent?.Invoke(new SystemDisableRemovedEvent(SystemDisablePriority.Normal, disableId2, "bla", true, true));
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier3]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier4]);
                // add hand pay first and tilt second. remove tilt second and hand pay first

            }
        }

        private static TowerLightConfiguration GetConfig(string jurisdiction)
        {
            var xmlPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, $@"..\..\..\..\bin\Debug\Platform\bin\jurisdiction\{jurisdiction}\TowerLight.config.xml"));
            var serializer = new XmlSerializer(typeof(TowerLightConfiguration));
            using (var stream = new FileStream(xmlPath, FileMode.Open))
            {
                return (TowerLightConfiguration)serializer.Deserialize(stream);
            }
        }

        [TestMethod]
        public void GivenMacau2TierWhenConditionChangedThenSignalChanged()
        {
            var towerLightConfig = GetConfig("Macau");
            _configUtility.Setup(c => c.GetConfiguration(It.IsAny<string>(), It.IsAny<Func<TowerLightConfiguration>>()))
                .Returns(towerLightConfig);

            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.TowerLightTierTypeSelection
                    , Application.Contracts.TowerLight.TowerLightTierTypes.Undefined.ToString()))
                .Returns(Application.Contracts.TowerLight.TowerLightTierTypes.TwoTier.ToString())
                .Verifiable();
            _towerLight.ResetCalls();
            var flashStateDic = new Dictionary<LightTier, FlashState>()
            {
                { LightTier.Tier1, FlashState.Off },
                { LightTier.Tier2, FlashState.Off },
            };
            _towerLight.Setup(
                    m => m.SetFlashState(
                        It.IsAny<LightTier>(),
                        It.IsAny<FlashState>(),
                        Timeout.InfiniteTimeSpan, false))
                .Callback<LightTier, FlashState, TimeSpan, bool>(
                    (lightTier, flashState, duration, test) =>
                    {
                        flashStateDic[lightTier] = flashState;
                    });
            using (new TowerLightManager(
                _eventBus.Object,
                _systemDisableManager.Object,
                _configUtility.Object,
                _doorService.Object,
                _towerLight.Object,
                _messageDisplay.Object,
                _edgeLightingStateManager.Object,
                _propertiesManager.Object,
                _operatorMenuLauncher.Object))
            {
                // open door, idle: DoorOpen
                SetupMainDoorOpen();
                _handlerOpenEvent?.Invoke(new OpenEvent(1, "Bla"));
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.MediumFlash, flashStateDic[LightTier.Tier1]);


                // door closed, idle: AllClosed | DoorWasOpenBefore | DoorWasOpenBeforeResetEndGame
                SetupAllDoorClosed();
                _handlerCloseEvent?.Invoke(new ClosedEvent(1, "Bla"));
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);

                // service button pressed. idle | service: AllClosed | DoorWasOpenBefore | DoorWasOpenBeforeResetEndGame
                _handlerCallAttendantButtonOnEvent?.Invoke(new CallAttendantButtonOnEvent());
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.On, flashStateDic[LightTier.Tier1]);

                // service button reset. idle: AllClosed | DoorWasOpenBefore | DoorWasOpenBeforeResetEndGame
                _handlerCallAttendantButtonOffEvent?.Invoke(new CallAttendantButtonOffEvent());
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);

                // game started. idle: AllClosed | DoorWasOpenBeforeResetEndGame
                _handlerPrimaryGameStartedEvent?.Invoke(new PrimaryGameStartedEvent(1, 1, "bla", Mock.Of<IGameHistoryLog>()));
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);

                // game ended. idle: AllClosed
                _handlerGameEndedEvent?.Invoke(new GameEndedEvent(1, 1, "bla", Mock.Of<IGameHistoryLog>()));
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);

                var disableId1 = Guid.NewGuid();
                var disableId2 = Guid.NewGuid();
                // tilt. tilt: AllClosed
                _handlerSystemDisableAddedEvent?.Invoke(new SystemDisableAddedEvent(SystemDisablePriority.Normal, disableId1, "bla", true));
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.MediumFlash, flashStateDic[LightTier.Tier1]);

                // tilt. tilt: DoorOpen | DoorWasOpenBefore | DoorWasOpenBeforeResetEndGame
                // need to disable second one to keep tilt on
                _handlerSystemDisableAddedEvent?.Invoke(new SystemDisableAddedEvent(SystemDisablePriority.Normal, disableId2, "bla", true));
                SetupMainDoorOpen();
                _handlerOpenEvent?.Invoke(new OpenEvent(1, "Bla"));
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.MediumFlash, flashStateDic[LightTier.Tier1]);

                // service button pressed. tilt | service: DoorOpen | DoorWasOpenBefore | DoorWasOpenBeforeResetEndGame
                _handlerCallAttendantButtonOnEvent?.Invoke(new CallAttendantButtonOnEvent());
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.MediumFlash, flashStateDic[LightTier.Tier1]); // ignore service due to tilt

                // service button reset. tilt: DoorOpen | DoorWasOpenBefore | DoorWasOpenBeforeResetEndGame
                _handlerCallAttendantButtonOffEvent?.Invoke(new CallAttendantButtonOffEvent());
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.MediumFlash, flashStateDic[LightTier.Tier1]);

                // remove tilt: idle: DoorOpen | DoorWasOpenBefore | DoorWasOpenBeforeResetEndGame
                _handlerSystemDisableRemovedEvent?.Invoke(new SystemDisableRemovedEvent(SystemDisablePriority.Normal, disableId1, "bla", true, true));
                _handlerSystemDisableRemovedEvent?.Invoke(new SystemDisableRemovedEvent(SystemDisablePriority.Normal, disableId2, "bla", true, true));
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.MediumFlash, flashStateDic[LightTier.Tier1]);

                SetupAllDoorClosed();
                _handlerCloseEvent?.Invoke(new ClosedEvent(1, "Bla"));
                // start game
                _handlerPrimaryGameStartedEvent?.Invoke(new PrimaryGameStartedEvent(1, 1, "bla", Mock.Of<IGameHistoryLog>()));
                //end game
                _handlerGameEndedEvent?.Invoke(new GameEndedEvent(1, 1, "bla", Mock.Of<IGameHistoryLog>()));
                // idle: AllClosed
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);

                //START add handpay first and tilt second. remove tilt first and hand pay second
                // hand pay: hand pay : AllClosed
                _handlerHandpayStartedEvent?.Invoke(new HandpayStartedEvent(HandpayType.GameWin, 1, 1, 1, 1, false));
                Assert.AreEqual(FlashState.MediumFlashReversed, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.MediumFlash, flashStateDic[LightTier.Tier1]);

                // tilt. tilt | hand pay: AllClosed
                _handlerSystemDisableAddedEvent?.Invoke(new SystemDisableAddedEvent(SystemDisablePriority.Normal, disableId1, "bla", true));
                // need to disable second one to keep tilt on for hand pay
                _handlerSystemDisableAddedEvent?.Invoke(new SystemDisableAddedEvent(SystemDisablePriority.Normal, disableId2, "bla", true));
                Assert.AreEqual(FlashState.MediumFlashReversed, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.MediumFlash, flashStateDic[LightTier.Tier1]);

                // remove tilt: hand pay : AllClosed
                _handlerSystemDisableRemovedEvent?.Invoke(new SystemDisableRemovedEvent(SystemDisablePriority.Normal, disableId1, "bla", true, true));
                _handlerSystemDisableRemovedEvent?.Invoke(new SystemDisableRemovedEvent(SystemDisablePriority.Normal, disableId2, "bla", true, true));
                Assert.AreEqual(FlashState.MediumFlashReversed, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.MediumFlash, flashStateDic[LightTier.Tier1]);

                // remove hand pay: idle : AllClosed
                _handlerHandpayCompletedEvent?.Invoke(new HandpayCompletedEvent(new HandpayTransaction()));
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                //END add hand pay first and tilt second. remove tilt first and hand pay second

                //START add hand pay first and tilt second. remove tilt second and hand pay first
                // hand pay: hand pay : AllClosed
                _handlerHandpayStartedEvent?.Invoke(new HandpayStartedEvent(HandpayType.GameWin, 1, 1, 1, 1, false));
                Assert.AreEqual(FlashState.MediumFlashReversed, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.MediumFlash, flashStateDic[LightTier.Tier1]);

                // tilt. tilt | hand pay: AllClosed
                _handlerSystemDisableAddedEvent?.Invoke(new SystemDisableAddedEvent(SystemDisablePriority.Normal, disableId1, "bla", true));
                // need to disable second one to keep tilt on for hand pay
                _handlerSystemDisableAddedEvent?.Invoke(new SystemDisableAddedEvent(SystemDisablePriority.Normal, disableId2, "bla", true));
                Assert.AreEqual(FlashState.MediumFlashReversed, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.MediumFlash, flashStateDic[LightTier.Tier1]);

                // remove hand pay: tilt : AllClosed
                _handlerHandpayCompletedEvent?.Invoke(new HandpayCompletedEvent(new HandpayTransaction()));
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.MediumFlash, flashStateDic[LightTier.Tier1]);

                // remove tilt: idle : AllClosed
                _handlerSystemDisableRemovedEvent?.Invoke(new SystemDisableRemovedEvent(SystemDisablePriority.Normal, disableId1, "bla", true, true));
                _handlerSystemDisableRemovedEvent?.Invoke(new SystemDisableRemovedEvent(SystemDisablePriority.Normal, disableId2, "bla", true, true));
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                //END add hand pay first and tilt second. remove tilt second and hand pay first

                //START add hand pay first and audit second. remove audit first and hand pay second
                // hand pay: handpay : AllClosed
                _handlerHandpayStartedEvent?.Invoke(new HandpayStartedEvent(HandpayType.GameWin, 1, 1, 1, 1, false));
                Assert.AreEqual(FlashState.MediumFlashReversed, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.MediumFlash, flashStateDic[LightTier.Tier1]);

                // audit menu: handpay | audit : AllClosed
                _handlerOperatorMenuEnteredEvent?.Invoke(new OperatorMenuEnteredEvent());
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);

                // exit audit: handpay
                _handlerOperatorMenuExitedEvent?.Invoke(new OperatorMenuExitedEvent());
                Assert.AreEqual(FlashState.MediumFlashReversed, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.MediumFlash, flashStateDic[LightTier.Tier1]);

                // remove hand pay: Idle : AllClosed
                _handlerHandpayCompletedEvent?.Invoke(new HandpayCompletedEvent(new HandpayTransaction()));
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier1]);
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                //END add hand pay first and audit second. remove audit first and hand pay second

                // 'cancel credit': cancel credit : AllClosed
                _handlerHandpayStartedEvent?.Invoke(new HandpayStartedEvent(HandpayType.CancelCredit, 1, 1, 1, 1, false));
                Assert.AreEqual(FlashState.Off, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.MediumFlash, flashStateDic[LightTier.Tier1]);

                // hand pay (should exclude 'cancel credit'): hand pay : AllClosed
                _handlerHandpayStartedEvent?.Invoke(new HandpayStartedEvent(HandpayType.GameWin, 1, 1, 1, 1, false));
                Assert.AreEqual(FlashState.MediumFlashReversed, flashStateDic[LightTier.Tier2]);
                Assert.AreEqual(FlashState.MediumFlash, flashStateDic[LightTier.Tier1]);
            }
        }

        [TestMethod]
        public void GivenDoorWasOpenBeforeConditionWhenAllClosedThenSignalIsNotResetAndResetAfterGameStarted()
        {
            var doorWasOpenBeforeResetStartGame = new DoorConditionType
            {
                condition = "DoorWasOpenBefore",
                Set = new[]
                {
                    new SetType { lightTier = "Tier1", flashState = "MediumFlash" },
                    new SetType { lightTier = "Tier2", flashState = "SlowFlash" }
                }
            };
            var allClosed = new DoorConditionType
            {
                condition = "AllClosed",
                Set = new[]
                {
                    new SetType { lightTier = "Tier1", flashState = "Off" },
                    new SetType { lightTier = "Tier2", flashState = "Off" }
                }
            };

            var towerLightConfig = new TowerLightConfiguration
            {
                SignalDefinitions = new[]
                {
                    new SignalDefinitionsType()
                    {
                        Tier = "TwoTier",
                        OperationalCondition = new[]
                        {
                            new OperationalConditionType
                            {
                                condition = new[] { "Idle" },
                                DoorCondition = new []{ doorWasOpenBeforeResetStartGame, allClosed }
                            }
                        }
                    }
                }
            };

            _configUtility.Setup(c => c.GetConfiguration(It.IsAny<string>(), It.IsAny<Func<TowerLightConfiguration>>()))
                .Returns(towerLightConfig);

            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.TowerLightTierTypeSelection
                    , Application.Contracts.TowerLight.TowerLightTierTypes.Undefined.ToString()))
                .Returns(Application.Contracts.TowerLight.TowerLightTierTypes.TwoTier.ToString())
                .Verifiable();

            using (new TowerLightManager(
                _eventBus.Object,
                _systemDisableManager.Object,
                _configUtility.Object,
                _doorService.Object,
                _towerLight.Object,
                _messageDisplay.Object,
                _edgeLightingStateManager.Object,
                _propertiesManager.Object,
                _operatorMenuLauncher.Object))
            {
                _towerLight.ResetCalls();
                SetupMainDoorOpen();
                _handlerOpenEvent?.Invoke(new OpenEvent(1, "Bla"));
                _towerLight.Verify(
                    m => m.SetFlashState(
                        It.Is<LightTier>(x => x == LightTier.Tier1),
                        It.Is<FlashState>(x => x == FlashState.MediumFlash), Timeout.InfiniteTimeSpan, false));

                _towerLight.Verify(
                    m => m.SetFlashState(
                        It.Is<LightTier>(x => x == LightTier.Tier2),
                        It.Is<FlashState>(x => x == FlashState.SlowFlash), Timeout.InfiniteTimeSpan, false));

                _towerLight.ResetCalls();
                SetupAllDoorClosed();
                _handlerCloseEvent?.Invoke(new ClosedEvent(1, "Bla"));
                _towerLight.Verify(
                    m => m.SetFlashState(
                        It.Is<LightTier>(x => x == LightTier.Tier1),
                        It.Is<FlashState>(x => x == FlashState.MediumFlash), Timeout.InfiniteTimeSpan, false));

                _towerLight.Verify(
                    m => m.SetFlashState(
                        It.Is<LightTier>(x => x == LightTier.Tier2),
                        It.Is<FlashState>(x => x == FlashState.SlowFlash), Timeout.InfiniteTimeSpan, false));

                _towerLight.ResetCalls();
                _handlerPrimaryGameStartedEvent?.Invoke(new PrimaryGameStartedEvent(1, 2, "bla", Mock.Of<IGameHistoryLog>()));
                _towerLight.Verify(
                    m => m.SetFlashState(
                        It.Is<LightTier>(x => x == LightTier.Tier1),
                        It.Is<FlashState>(x => x == FlashState.Off), Timeout.InfiniteTimeSpan, false));

                _towerLight.Verify(
                    m => m.SetFlashState(
                        It.Is<LightTier>(x => x == LightTier.Tier2),
                        It.Is<FlashState>(x => x == FlashState.Off), Timeout.InfiniteTimeSpan, false));
            }
        }

        [TestMethod]
        public void GivenNotDoorWasOpenBeforeConditionWhenAllClosedThenSignalIsReset()
        {
            var doorOpen = new DoorConditionType
            {
                condition = "DoorOpen",
                Set = new[]
                {
                    new SetType { lightTier = "Tier1", flashState = "MediumFlash" },
                    new SetType { lightTier = "Tier2", flashState = "SlowFlash" }
                }
            };
            var allClosed = new DoorConditionType
            {
                condition = "AllClosed",
                Set = new[]
                {
                    new SetType { lightTier = "Tier1", flashState = "Off" },
                    new SetType { lightTier = "Tier2", flashState = "Off" }
                }
            };

            var towerLightConfig = new TowerLightConfiguration
            {
                SignalDefinitions = new[]
                {
                    new SignalDefinitionsType()
                    {
                        Tier = "TwoTier",
                        OperationalCondition = new[]
                        {
                            new OperationalConditionType
                            {
                                condition = new[] { "Idle" },
                                DoorCondition = new []{ doorOpen, allClosed }
                            }
                        }
                    }
                }
            };

            _configUtility.Setup(c => c.GetConfiguration(It.IsAny<string>(), It.IsAny<Func<TowerLightConfiguration>>()))
                .Returns(towerLightConfig);

            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.TowerLightTierTypeSelection
                    , Application.Contracts.TowerLight.TowerLightTierTypes.Undefined.ToString()))
                .Returns(Application.Contracts.TowerLight.TowerLightTierTypes.TwoTier.ToString())
                .Verifiable();

            using (new TowerLightManager(
                _eventBus.Object,
                _systemDisableManager.Object,
                _configUtility.Object,
                _doorService.Object,
                _towerLight.Object,
                _messageDisplay.Object,
                _edgeLightingStateManager.Object,
                _propertiesManager.Object,
                _operatorMenuLauncher.Object))
            {
                _towerLight.ResetCalls();
                SetupMainDoorOpen();
                _handlerOpenEvent?.Invoke(new OpenEvent(1, "Bla"));
                _towerLight.Verify(
                    m => m.SetFlashState(
                        It.Is<LightTier>(x => x == LightTier.Tier1),
                        It.Is<FlashState>(x => x == FlashState.MediumFlash), Timeout.InfiniteTimeSpan, false));

                _towerLight.Verify(
                    m => m.SetFlashState(
                        It.Is<LightTier>(x => x == LightTier.Tier2),
                        It.Is<FlashState>(x => x == FlashState.SlowFlash), Timeout.InfiniteTimeSpan, false));

                _towerLight.ResetCalls();
                SetupAllDoorClosed();
                _handlerCloseEvent?.Invoke(new ClosedEvent(1, "Bla"));
                _towerLight.Verify(
                    m => m.SetFlashState(
                        It.Is<LightTier>(x => x == LightTier.Tier1),
                        It.Is<FlashState>(x => x == FlashState.Off), Timeout.InfiniteTimeSpan, false));

                _towerLight.Verify(
                    m => m.SetFlashState(
                        It.Is<LightTier>(x => x == LightTier.Tier2),
                        It.Is<FlashState>(x => x == FlashState.Off), Timeout.InfiniteTimeSpan, false));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventBusIsNullExpectException()
        {
            _target = new TowerLightManager(
                null,
                _systemDisableManager.Object,
                _configUtility.Object,
                _doorService.Object,
                _towerLight.Object,
                _messageDisplay.Object,
                _edgeLightingStateManager.Object,
                _propertiesManager.Object,
                _operatorMenuLauncher.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenSystemDisableManagerIsNullExpectException()
        {
            _target = new TowerLightManager(
                _eventBus.Object,
                null,
                _configUtility.Object,
                _doorService.Object,
                _towerLight.Object,
                _messageDisplay.Object,
                _edgeLightingStateManager.Object,
                _propertiesManager.Object,
                _operatorMenuLauncher.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConfigUtilityIsNullExpectException()
        {
            _target = new TowerLightManager(
                _eventBus.Object,
                _systemDisableManager.Object,
                null,
                _doorService.Object,
                _towerLight.Object,
                _messageDisplay.Object,
                _edgeLightingStateManager.Object,
                _propertiesManager.Object,
                _operatorMenuLauncher.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenDoorServiceIsNullExpectException()
        {
            _target = new TowerLightManager(
                _eventBus.Object,
                _systemDisableManager.Object,
                _configUtility.Object,
                null,
                _towerLight.Object,
                _messageDisplay.Object,
                _edgeLightingStateManager.Object,
                _propertiesManager.Object,
                _operatorMenuLauncher.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenTowerLightIsNullExpectException()
        {
            _target = new TowerLightManager(
                _eventBus.Object,
                _systemDisableManager.Object,
                _configUtility.Object,
                _doorService.Object,
                null,
                _messageDisplay.Object,
                _edgeLightingStateManager.Object,
                _propertiesManager.Object,
                _operatorMenuLauncher.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenMessageDisplayIsNullExpectException()
        {
            _target = new TowerLightManager(
                _eventBus.Object,
                _systemDisableManager.Object,
                _configUtility.Object,
                _doorService.Object,
                _towerLight.Object,
                null,
                _edgeLightingStateManager.Object,
                _propertiesManager.Object,
                _operatorMenuLauncher.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEdgeLightStateManagerIsNullExpectException()
        {
            _target = new TowerLightManager(
                _eventBus.Object,
                _systemDisableManager.Object,
                _configUtility.Object,
                _doorService.Object,
                _towerLight.Object,
                _messageDisplay.Object,
                null,
                _propertiesManager.Object,
                _operatorMenuLauncher.Object);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            _target = new TowerLightManager(
                _eventBus.Object,
                _systemDisableManager.Object,
                _configUtility.Object,
                _doorService.Object,
                _towerLight.Object,
                _messageDisplay.Object,
                _edgeLightingStateManager.Object,
                _propertiesManager.Object,
                _operatorMenuLauncher.Object);
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void ConfiguredLightTierTest()
        {
            var configuredLightTiers = _target.ConfiguredLightTiers.ToList();
            Assert.AreEqual(2, configuredLightTiers.Count);
            Assert.IsTrue(configuredLightTiers.Contains(LightTier.Tier1));
            Assert.IsTrue(configuredLightTiers.Contains(LightTier.Tier2));
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            Assert.AreEqual(1, _target.ServiceTypes.Count);
            Assert.IsTrue(_target.ServiceTypes.Contains(typeof(ITowerLightManager)));
        }

        [TestMethod]
        public void ServiceNameTest()
        {
            Assert.AreEqual("TowerLightManager", _target.Name);
        }

        [TestMethod]
        public void TestTowerLightInAuditMenuWithDoorOpen()
        {
            SetupMainDoorOpen();

            Action<OperatorMenuEnteredEvent> callback = null;
            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<OperatorMenuEnteredEvent>>()))
                .Callback(
                    (object subscriber, Action<OperatorMenuEnteredEvent> eventCallback) =>
                    {
                        callback = eventCallback;
                    });

            _target.Initialize();

            callback.Invoke(new OperatorMenuEnteredEvent());
            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier1),
                    It.Is<FlashState>(x => x == FlashState.MediumFlash), Timeout.InfiniteTimeSpan, false));
            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier2),
                    It.Is<FlashState>(x => x == FlashState.SlowFlash), Timeout.InfiniteTimeSpan, false));
        }

        [TestMethod]
        public void TestTowerLightInAuditMenuWithDoorClosed()
        {
            SetupAllDoorClosed();

            Action<OperatorMenuEnteredEvent> callback = null;
            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<OperatorMenuEnteredEvent>>()))
                .Callback(
                    (object subscriber, Action<OperatorMenuEnteredEvent> eventCallback) =>
                    {
                        callback = eventCallback;
                    });

            _target.Initialize();

            callback.Invoke(new OperatorMenuEnteredEvent());
            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier1),
                    It.Is<FlashState>(x => x == FlashState.Off), Timeout.InfiniteTimeSpan, false));
            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier2),
                    It.Is<FlashState>(x => x == FlashState.SlowFlash), Timeout.InfiniteTimeSpan, false));
        }

        [TestMethod]
        public void TestTowerLightInHandpayWithDoorOpen()
        {
            SetupMainDoorOpen();

            Action<HandpayStartedEvent> callback = null;
            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<HandpayStartedEvent>>()))
                .Callback(
                    (object subscriber, Action<HandpayStartedEvent> eventCallback) => { callback = eventCallback; });

            _target.Initialize();
            callback.Invoke(new HandpayStartedEvent(HandpayType.GameWin, 0, 0, 0, 0, false));

            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier1),
                    It.Is<FlashState>(x => x == FlashState.MediumFlash), Timeout.InfiniteTimeSpan, false));
            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier2),
                    It.Is<FlashState>(x => x == FlashState.SlowFlash), Timeout.InfiniteTimeSpan, false));
        }

        [TestMethod]
        public void TestTowerLightInHandpayWithDoorClosed()
        {
            SetupAllDoorClosed();

            Action<HandpayStartedEvent> callback = null;
            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<HandpayStartedEvent>>()))
                .Callback(
                    (object subscriber, Action<HandpayStartedEvent> eventCallback) => { callback = eventCallback; });

            _target.Initialize();
            callback.Invoke(new HandpayStartedEvent(HandpayType.GameWin, 0, 0, 0, 0, false));

            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier1),
                    It.Is<FlashState>(x => x == FlashState.SlowFlash), Timeout.InfiniteTimeSpan, false));
            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier2),
                    It.Is<FlashState>(x => x == FlashState.SlowFlash), Timeout.InfiniteTimeSpan, false));
        }

        [TestMethod]
        public void TestTowerLightWithOnlyLiveAuthentication()
        {
            SetupAllDoorClosed();

            Action<SystemDisableAddedEvent> callback = null;
            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<SystemDisableAddedEvent>>()))
                .Callback(
                    (object subscriber, Action<SystemDisableAddedEvent> eventCallback) =>
                    {
                        callback = eventCallback;
                    });

            _target.Initialize();
            callback.Invoke(new SystemDisableAddedEvent(SystemDisablePriority.Immediate, Guid.Empty, string.Empty, false));

            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier1),
                    It.Is<FlashState>(x => x == FlashState.Off), Timeout.InfiniteTimeSpan, false));
            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier2),
                    It.Is<FlashState>(x => x == FlashState.SlowFlash), Timeout.InfiniteTimeSpan, false));
        }

        [TestMethod]
        public void TestTowerLightWithLiveAuthenticationWithDoorOpen()
        {
            SetupMainDoorOpen();

            Action<SystemDisableAddedEvent> callback = null;
            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<SystemDisableAddedEvent>>()))
                .Callback(
                    (object subscriber, Action<SystemDisableAddedEvent> eventCallback) =>
                    {
                        callback = eventCallback;
                    });

            _target.Initialize();
            callback.Invoke(new SystemDisableAddedEvent(SystemDisablePriority.Immediate, Guid.NewGuid(), string.Empty, false));
            callback.Invoke(new SystemDisableAddedEvent(SystemDisablePriority.Immediate, ApplicationConstants.LiveAuthenticationDisableKey, string.Empty, false));

            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier1),
                    It.Is<FlashState>(x => x == FlashState.MediumFlash), Timeout.InfiniteTimeSpan, false));
            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier2),
                    It.Is<FlashState>(x => x == FlashState.Off), Timeout.InfiniteTimeSpan, false));
        }

        [TestMethod]
        public void TestTowerLightInLockupWithDoorOpen()
        {
            SetupMainDoorOpen();

            Action<SystemDisableAddedEvent> callback = null;
            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<SystemDisableAddedEvent>>()))
                .Callback(
                    (object subscriber, Action<SystemDisableAddedEvent> eventCallback) =>
                    {
                        callback = eventCallback;
                    });

            _target.Initialize();
            // Add two door open events
            callback.Invoke(new SystemDisableAddedEvent(SystemDisablePriority.Immediate, Guid.NewGuid(), string.Empty, false));
            callback.Invoke(new SystemDisableAddedEvent(SystemDisablePriority.Immediate, Guid.NewGuid(), string.Empty, false));

            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier1),
                    It.Is<FlashState>(x => x == FlashState.MediumFlash), Timeout.InfiniteTimeSpan, false));
            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier2),
                    It.Is<FlashState>(x => x == FlashState.SlowFlash), Timeout.InfiniteTimeSpan, false));
        }

        [TestMethod]
        public void TestTowerLightInLockupWithDoorClosed()
        {
            SetupAllDoorClosed();

            Action<SystemDisableAddedEvent> callback = null;
            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<SystemDisableAddedEvent>>()))
                .Callback(
                    (object subscriber, Action<SystemDisableAddedEvent> eventCallback) =>
                    {
                        callback = eventCallback;
                    });

            _target.Initialize();
            callback.Invoke(new SystemDisableAddedEvent(SystemDisablePriority.Immediate, Guid.Empty, string.Empty, false));

            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier1),
                    It.Is<FlashState>(x => x == FlashState.Off), Timeout.InfiniteTimeSpan, false));
            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier2),
                    It.Is<FlashState>(x => x == FlashState.SlowFlash), Timeout.InfiniteTimeSpan, false));
        }

        [TestMethod]
        public void TestTowerLightInIdleWithDoorOpen()
        {
            SetupMainDoorOpen();

            _target.Initialize();
            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier1),
                    It.Is<FlashState>(x => x == FlashState.MediumFlash), Timeout.InfiniteTimeSpan, false));
            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier2),
                    It.Is<FlashState>(x => x == FlashState.Off), Timeout.InfiniteTimeSpan, false));
        }

        [TestMethod]
        public void TestTowerLightInIdleWithDoorClosed()
        {
            SetupAllDoorClosed();

            _target.Initialize();
            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier1),
                    It.Is<FlashState>(x => x == FlashState.Off), Timeout.InfiniteTimeSpan, false));
            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier2),
                    It.Is<FlashState>(x => x == FlashState.Off), Timeout.InfiniteTimeSpan, false));
        }

        [DataRow(false, FlashState.Off, FlashState.On, FlashState.Off, FlashState.Off)]
        [DataRow(true, FlashState.MediumFlash, FlashState.On, FlashState.MediumFlash, FlashState.Off)]
        [DataTestMethod]
        public void TestTowerLightWithSoftErrors(
            bool doorsOpen,
            FlashState tier1WithError,
            FlashState tier2WithError,
            FlashState tier1WithoutError,
            FlashState tier2WithoutError)
        {
            if (doorsOpen)
            {
                SetupMainDoorOpen();
            }
            else
            {
                SetupAllDoorClosed();
            }

            var softError = new DisplayableMessage(
               () => string.Empty,
               DisplayableMessageClassification.SoftError,
               DisplayableMessagePriority.Normal);
            _messageDisplay.Setup(x => x.AddMessageDisplayHandler(_target, It.IsAny<CultureProviderType>(), true))
                .Callback(() => _target.DisplayMessage(softError));

            //var softError = new Mock<IDisplayableMessage>();

            //softError.SetupGet(x => x.Message).Returns("Test");
            //softError.SetupGet(x => x.MessageResourceKey).Returns("Test");
            //softError.SetupGet(x => x.CultureProvider).Returns(CultureProviderType.Operator);
            //softError.SetupGet(x => x.Classification).Returns(DisplayableMessageClassification.SoftError);
            //softError.SetupGet(x => x.Priority).Returns(DisplayableMessagePriority.Normal);

            //_messageDisplay.Setup(x => x.AddMessageDisplayHandler(_target, It.IsAny<CultureProviderType>(), true))
            //    .Callback(() => _target.DisplayMessage(softError.Object));

            _target.Initialize();
            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier1),
                    It.Is<FlashState>(x => x == tier1WithError),
                    Timeout.InfiniteTimeSpan,
                    false));
            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier2),
                    It.Is<FlashState>(x => x == tier2WithError),
                    Timeout.InfiniteTimeSpan,
                    false));

            _target.RemoveMessage(softError);
            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier1),
                    It.Is<FlashState>(x => x == tier1WithoutError),
                    Timeout.InfiniteTimeSpan,
                    false));
            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier2),
                    It.Is<FlashState>(x => x == tier2WithoutError),
                    Timeout.InfiniteTimeSpan,
                    false));
        }

        [DataRow(false, FlashState.Off, FlashState.On, FlashState.Off, FlashState.Off)]
        [DataRow(true, FlashState.MediumFlash, FlashState.On, FlashState.MediumFlash, FlashState.Off)]
        [DataTestMethod]
        public void TestTowerLightWithSoftErrorsCleared(
            bool doorsOpen,
            FlashState tier1WithError,
            FlashState tier2WithError,
            FlashState tier1WithoutError,
            FlashState tier2WithoutError)
        {
            if (doorsOpen)
            {
                SetupMainDoorOpen();
            }
            else
            {
                SetupAllDoorClosed();
            }

            //var softError = new DisplayableMessage(
            //    () => string.Empty,
            //    DisplayableMessageClassification.SoftError,
            //    DisplayableMessagePriority.Normal);
            //_messageDisplay.Setup(x => x.AddMessageDisplayHandler(_target, true))
            //    .Callback(() => _target.DisplayMessage(softError));

            var softError = new Mock<IDisplayableMessage>();

            softError.SetupGet(x => x.Message).Returns("Test");
            softError.SetupGet(x => x.Classification).Returns(DisplayableMessageClassification.SoftError);

            _messageDisplay.Setup(x => x.AddMessageDisplayHandler(_target, It.IsAny<CultureProviderType>(), true))
                .Callback(() => _target.DisplayMessage(softError.Object));
            _target.Initialize();
            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier1),
                    It.Is<FlashState>(x => x == tier1WithError),
                    Timeout.InfiniteTimeSpan,
                    false));
            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier2),
                    It.Is<FlashState>(x => x == tier2WithError),
                    Timeout.InfiniteTimeSpan,
                    false));

            _target.ClearMessages();
            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier1),
                    It.Is<FlashState>(x => x == tier1WithoutError),
                    Timeout.InfiniteTimeSpan,
                    false));
            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier2),
                    It.Is<FlashState>(x => x == tier2WithoutError),
                    Timeout.InfiniteTimeSpan,
                    false));
        }

        [TestMethod]
        public void TowerLightPriorityTest()
        {
            SetupMainDoorOpen();

            var softError = new Mock<IDisplayableMessage>();

            softError.SetupGet(x => x.Message).Returns("Test");
            softError.SetupGet(x => x.MessageResourceKey).Returns("Test");
            softError.SetupGet(x => x.CultureProvider).Returns(CultureProviderType.Operator);
            softError.SetupGet(x => x.Classification).Returns(DisplayableMessageClassification.SoftError);
            softError.SetupGet(x => x.Priority).Returns(DisplayableMessagePriority.Normal);

            _messageDisplay.Setup(x => x.AddMessageDisplayHandler(_target, It.IsAny<CultureProviderType>(), true))
                .Callback(() => _target.DisplayMessage(softError.Object));

            Action<SystemDisableAddedEvent> disableCallback = null;
            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<SystemDisableAddedEvent>>()))
                .Callback(
                    (object subscriber, Action<SystemDisableAddedEvent> eventCallback) =>
                    {
                        disableCallback = eventCallback;
                    });
            Action<ClosedEvent> doorCallback = null;
            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<ClosedEvent>>()))
                .Callback(
                    (object subscriber, Action<ClosedEvent> eventCallback) =>
                    {
                        doorCallback = eventCallback;
                    });

            _target.Initialize();

            _doorService.Setup(d => d.GetDoorClosed((int)DoorLogicalId.Main)).Returns(true);
            disableCallback.Invoke(new SystemDisableAddedEvent(SystemDisablePriority.Immediate, Guid.Empty, string.Empty, false));
            doorCallback.Invoke(new ClosedEvent());

            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier1),
                    It.Is<FlashState>(x => x == FlashState.On),
                    Timeout.InfiniteTimeSpan,
                    false));
            _towerLight.Verify(
                m => m.SetFlashState(
                    It.Is<LightTier>(x => x == LightTier.Tier2),
                    It.Is<FlashState>(x => x == FlashState.SlowFlash),
                    Timeout.InfiniteTimeSpan,
                    false));
        }
    }
}