namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Accounting.Contracts;
    using Hardware.Contracts.EdgeLighting;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using DisabledEvent = Hardware.Contracts.NoteAcceptor.DisabledEvent;
    using EnabledEvent = Hardware.Contracts.NoteAcceptor.EnabledEvent;

    [TestClass]
    public class LandingStripControllerTests
    {
        private Mock<IEventBus> _moqEventBus;
        private Mock<IBank> _moqBank;
        private Mock<IEdgeLightingController> _moqEdgeLightingController;
        private Mock<ISystemDisableManager> _moqDisableManager;
        private Mock<IPrinter> _moqPrinter;
        private Mock<INoteAcceptor> _moqNoteAcceptor;

        private Action<BankBalanceChangedEvent> _bankBalanceChangedCallback;
        private Action<HardwareWarningEvent> _hardwareWarningEventCallback;
        private Action<HardwareWarningClearEvent> _hardwareWarningClearEventCallback;
        private Action<MissedStartupEvent> _missedStartupEventHandler;
        private Action<SystemDisabledEvent> _systemDisabledEventHandler;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);

            _moqEventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _moqEventBus.Setup(
                    x => x.Subscribe(It.IsAny<LandingStripController>(), It.IsAny<Action<BankBalanceChangedEvent>>()))
                .Callback<object, Action<BankBalanceChangedEvent>>
                    ((subscriber, callback) => _bankBalanceChangedCallback = callback);
            _moqEventBus.Setup(
                    x => x.Subscribe(It.IsAny<LandingStripController>(), It.IsAny<Action<HardwareWarningEvent>>()))
                .Callback<object, Action<HardwareWarningEvent>>
                    ((subscriber, callback) => _hardwareWarningEventCallback = callback);
            _moqEventBus.Setup(
                    x => x.Subscribe(It.IsAny<LandingStripController>(), It.IsAny<Action<HardwareWarningClearEvent>>()))
                .Callback<object, Action<HardwareWarningClearEvent>>
                    ((subscriber, callback) => _hardwareWarningClearEventCallback = callback);
            _moqEventBus.Setup(
                    x => x.Subscribe(It.IsAny<LandingStripController>(), It.IsAny<Action<MissedStartupEvent>>()))
                .Callback<object, Action<MissedStartupEvent>>
                    ((subscriber, callback) => _missedStartupEventHandler = callback);
            _moqEventBus.Setup(
                    x => x.Subscribe(It.IsAny<LandingStripController>(), It.IsAny<Action<SystemDisabledEvent>>()))
                .Callback<object, Action<SystemDisabledEvent>>
                    ((subscriber, callback) => _systemDisabledEventHandler = callback);
            _moqEventBus.Setup(x => x.Subscribe(It.IsAny<LandingStripController>(), It.IsAny<Action<SystemDisableAddedEvent>>()));
            _moqEventBus.Setup(x => x.Subscribe(It.IsAny<LandingStripController>(), It.IsAny<Action<SystemDisableUpdatedEvent>>()));
            _moqEventBus.Setup(x => x.Subscribe(It.IsAny<LandingStripController>(), It.IsAny<Action<SystemDisableRemovedEvent>>()));
            _moqEventBus.Setup(x => x.Subscribe(It.IsAny<LandingStripController>(), It.IsAny<Action<SystemEnabledEvent>>()));
            _moqEventBus.Setup(x => x.Subscribe(It.IsAny<LandingStripController>(), It.IsAny<Action<SystemEnabledEvent>>()));
            _moqEventBus.Setup(x => x.Subscribe(It.IsAny<LandingStripController>(), It.IsAny<Action<EnabledEvent>>()));
            _moqEventBus.Setup(x => x.Subscribe(It.IsAny<LandingStripController>(), It.IsAny<Action<DisabledEvent>>()));

            _moqBank = MoqServiceManager.CreateAndAddService<IBank>(MockBehavior.Default);
            _moqBank.Setup(x => x.QueryBalance()).Returns(100);
            _moqPrinter = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Default);
            _moqNoteAcceptor = MoqServiceManager.CreateAndAddService<INoteAcceptor>(MockBehavior.Default);
            _moqNoteAcceptor.Setup(n => n.Enabled).Returns(true);

            _moqEdgeLightingController =
                MoqServiceManager.CreateAndAddService<IEdgeLightingController>(MockBehavior.Default);
            _moqEdgeLightingController.Setup(x => x.AddEdgeLightRenderer(It.IsAny<IndividualLedPatternParameters>()))
                .Returns(new EdgeLightToken(50));

            _moqDisableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Default);
        }

        [DataRow(false, true, true, true, DisplayName = "Null Event Bus Object")]
        [DataRow(true, false, true, true, DisplayName = "Null Edge Lighting Controller Object")]
        [DataRow(true, true, false, true, DisplayName = "Null Bank Object")]
        [DataRow(true, true, true, false, DisplayName = "Null System Disable Manager Object")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullParameterTest(
            bool eventBus,
            bool edgeLighting,
            bool bank,
            bool disable)
        {
            _ = new LandingStripController(
                eventBus ? _moqEventBus.Object : null,
                edgeLighting ? _moqEdgeLightingController.Object : null,
                bank ? _moqBank.Object : null,
                disable ? _moqDisableManager.Object : null);
        }

        [TestMethod]
        public void CreateLandingStripManagerObject()
        {
            try
            {
                CreateTestObject();
            }
            catch (Exception e)
            {
                Assert.Fail($"Exception occurred while creating LandingStripController - {e.Message}");
            }
        }

        [TestMethod]
        public void ExpectBnaRunAwayStripToGoFromIdleToActive()
        {
            try
            {
                CreateTestObject();

                Assert.IsNotNull(_bankBalanceChangedCallback);
                // Balance 100 - Should be Idle color i.e. blue
                _moqEdgeLightingController.Verify(
                    edgeLightingController => edgeLightingController.AddEdgeLightRenderer(
                        It.Is<SolidColorPatternParameters>(
                            solidColorPatternParameters =>
                                solidColorPatternParameters.Priority == StripPriority.LobbyView
                                && solidColorPatternParameters.Strips.SequenceEqual(
                                    new List<int> { (int)StripIDs.LandingStripRight })
                                && solidColorPatternParameters.Color == Color.Blue)),
                    Times.Once);

                // Balance zero hence verify that we call for IndividualLedPatternParameters
                _moqBank.Setup(b => b.QueryBalance()).Returns(0);
                _bankBalanceChangedCallback(new BankBalanceChangedEvent(100, 0, Guid.Empty));
                _moqEdgeLightingController.Verify(
                    edgeLightingController => edgeLightingController.AddEdgeLightRenderer(
                        It.Is<IndividualLedPatternParameters>(
                            individualLedPatternParameters =>
                                individualLedPatternParameters.Priority == StripPriority.LobbyView
                                && individualLedPatternParameters.Strips.SequenceEqual(
                                    new List<int> { (int)StripIDs.LandingStripRight }))),
                    Times.Once);
            }
            catch (Exception e)
            {
                Assert.Fail($"Exception occurred while creating LandingStripController - {e.Message}");
            }
        }


        [TestMethod]
        public void ExpectBnaRunAwayStripToGoFromActiveToIdle()
        {
            try
            {
                _moqBank.Setup(b => b.QueryBalance()).Returns(0);
                CreateTestObject();

                Assert.IsNotNull(_bankBalanceChangedCallback);
                // Balance zero hence verify that we call for IndividualLedPatternParameters
                _moqEdgeLightingController.Verify(
                    edgeLightingController => edgeLightingController.AddEdgeLightRenderer(
                        It.Is<IndividualLedPatternParameters>(
                            individualLedPatternParameters =>
                                individualLedPatternParameters.Priority == StripPriority.LobbyView
                                && individualLedPatternParameters.Strips.SequenceEqual(
                                    new List<int> { (int)StripIDs.LandingStripRight }))),
                    Times.Once);

                // Balance nonzero, expect to be Idle again
                _moqBank.Setup(b => b.QueryBalance()).Returns(100);
                _bankBalanceChangedCallback(new BankBalanceChangedEvent(0, 100, Guid.Empty));
                _moqEdgeLightingController.Verify(
                    edgeLightingController => edgeLightingController.AddEdgeLightRenderer(
                        It.Is<SolidColorPatternParameters>(
                            solidColorPatternParameters =>
                                solidColorPatternParameters.Priority == StripPriority.LobbyView
                                && solidColorPatternParameters.Strips.SequenceEqual(
                                    new List<int> { (int)StripIDs.LandingStripRight })
                                && solidColorPatternParameters.Color == Color.Blue)),
                    Times.Exactly(2));
            }
            catch (Exception e)
            {
                Assert.Fail($"Exception occurred while creating LandingStripController - {e.Message}");
            }
        }

        [TestMethod]
        public void ExpectPrinterRunAwayStripToGoFromIdleToActive()
        {
            try
            {
                CreateTestObject();

                _moqEdgeLightingController.Verify(
                    edgeLightingController => edgeLightingController.AddEdgeLightRenderer(
                        It.Is<SolidColorPatternParameters>(
                            solidColorPatternParameters =>
                                solidColorPatternParameters.Priority == StripPriority.LobbyView
                                && solidColorPatternParameters.Strips.SequenceEqual(
                                    new List<int> { (int)StripIDs.LandingStripLeft })
                                && solidColorPatternParameters.Color == Color.Blue)),
                    Times.Once);

                Assert.IsNotNull(_hardwareWarningEventCallback);
                // Paper in chute
                _moqPrinter.Setup(p => p.Warnings).Returns(PrinterWarningTypes.PaperInChute);
                _hardwareWarningEventCallback(new HardwareWarningEvent(PrinterWarningTypes.PaperInChute));
                _moqEdgeLightingController.Verify(
                    edgeLightingController => edgeLightingController.AddEdgeLightRenderer(
                        It.Is<IndividualLedPatternParameters>(
                            individualLedPatternParameters =>
                                individualLedPatternParameters.Priority == StripPriority.LobbyView
                                && individualLedPatternParameters.Strips.SequenceEqual(
                                    new List<int> { (int)StripIDs.LandingStripLeft }))),
                    Times.Once);

                // Paper removed
                _moqPrinter.Setup(p => p.Warnings).Returns(PrinterWarningTypes.None|PrinterWarningTypes.PaperLow);
                _hardwareWarningClearEventCallback(new HardwareWarningClearEvent(PrinterWarningTypes.PaperInChute));
                _moqEdgeLightingController.Verify(
                    edgeLightingController => edgeLightingController.AddEdgeLightRenderer(
                        It.Is<SolidColorPatternParameters>(
                            solidColorPatternParameters =>
                                solidColorPatternParameters.Priority == StripPriority.LobbyView
                                && solidColorPatternParameters.Strips.SequenceEqual(
                                    new List<int> { (int)StripIDs.LandingStripLeft })
                                && solidColorPatternParameters.Color == Color.Blue)),
                    Times.Exactly(2));
            }
            catch (Exception e)
            {
                Assert.Fail($"Exception occurred while creating LandingStripController - {e.Message}");
            }
        }

        [TestMethod]
        public void ExpectPrinterStripsToActiveDuringStartupIfPaperIsInChute()
        {
            try
            {
                CreateTestObject();

                _moqEdgeLightingController.Verify(
                    edgeLightingController => edgeLightingController.AddEdgeLightRenderer(
                        It.Is<SolidColorPatternParameters>(
                            solidColorPatternParameters =>
                                solidColorPatternParameters.Priority == StripPriority.LobbyView
                                && solidColorPatternParameters.Strips.SequenceEqual(
                                    new List<int> { (int)StripIDs.LandingStripLeft })
                                && solidColorPatternParameters.Color == Color.Blue)),
                    Times.Once);

                Assert.IsNotNull(_missedStartupEventHandler);
                // Paper in chute, missed event
                _moqPrinter.Setup(p => p.Warnings).Returns(PrinterWarningTypes.PaperInChute);
                _missedStartupEventHandler(new MissedStartupEvent(new HardwareWarningEvent(PrinterWarningTypes.PaperInChute)));
                _moqEdgeLightingController.Verify(
                    edgeLightingController => edgeLightingController.AddEdgeLightRenderer(
                        It.Is<IndividualLedPatternParameters>(
                            individualLedPatternParameters =>
                                individualLedPatternParameters.Priority == StripPriority.LobbyView
                                && individualLedPatternParameters.Strips.SequenceEqual(
                                    new List<int> { (int)StripIDs.LandingStripLeft }))),
                    Times.Once);
            }
            catch (Exception e)
            {
                Assert.Fail($"Exception occurred while creating LandingStripController - {e.Message}");
            }
        }

        [TestMethod]
        public void ExpectBothStripsIdleIfSystemDisabled()
        {
            try
            {
                _moqBank.Setup(b => b.QueryBalance()).Returns(0);
                _moqPrinter.Setup(p => p.Warnings).Returns(PrinterWarningTypes.PaperInChute);
                CreateTestObject();

                // Balance 0 - BNA should be Active
                _moqEdgeLightingController.Verify(
                    edgeLightingController => edgeLightingController.AddEdgeLightRenderer(
                        It.Is<IndividualLedPatternParameters>(
                            individualLedPatternParameters =>
                                individualLedPatternParameters.Priority == StripPriority.LobbyView
                                && individualLedPatternParameters.Strips.SequenceEqual(
                                    new List<int> { (int)StripIDs.LandingStripRight }))),
                    Times.Once);

                // Paper in chute - Printer should be Active
                _moqEdgeLightingController.Verify(
                    edgeLightingController => edgeLightingController.AddEdgeLightRenderer(
                        It.Is<IndividualLedPatternParameters>(
                            individualLedPatternParameters =>
                                individualLedPatternParameters.Priority == StripPriority.LobbyView
                                && individualLedPatternParameters.Strips.SequenceEqual(
                                    new List<int> { (int)StripIDs.LandingStripLeft }))),
                    Times.Once);

                // System Disabled
                _moqDisableManager.Setup(d => d.IsDisabled).Returns(true);
                _systemDisabledEventHandler(new SystemDisabledEvent(SystemDisablePriority.Normal));
                _moqEdgeLightingController.Verify(
                    edgeLightingController => edgeLightingController.AddEdgeLightRenderer(
                        It.Is<IndividualLedPatternParameters>(
                            individualLedPatternParameters =>
                                individualLedPatternParameters.Priority == StripPriority.LobbyView
                                && individualLedPatternParameters.Strips.SequenceEqual(
                                    new List<int> { (int)StripIDs.LandingStripRight }))),
                    Times.Once);
                _moqEdgeLightingController.Verify(
                    edgeLightingController => edgeLightingController.AddEdgeLightRenderer(
                        It.Is<IndividualLedPatternParameters>(
                            individualLedPatternParameters =>
                                individualLedPatternParameters.Priority == StripPriority.LobbyView
                                && individualLedPatternParameters.Strips.SequenceEqual(
                                    new List<int> { (int)StripIDs.LandingStripLeft }))),
                    Times.Once);
            }
            catch (Exception e)
            {
                Assert.Fail($"Exception occurred while creating LandingStripController - {e.Message}");
            }
        }

        [DataRow(false, false, DisplayName = "Both BNA and Printer not connected. Expect no event subscription")]
        [DataRow(false, true, DisplayName = "BNA connected and Printer not connected. Expect Bank related events subscription")]
        [DataRow(true, false, DisplayName = "BNA disconnected and Printer connected. Expect Printer related events subscription")]
        [DataRow(true, true, DisplayName = "Both BNA and Printer connected. Expect all event subscriptions")]
        [DataTestMethod]
        public void PrinterAndBnaConnectionRelatedTests(bool printerConnected, bool bnaConnected)
        {
            if (!printerConnected)
            {
                MoqServiceManager.RemoveService<IPrinter>();
            }

            if (!bnaConnected)
            {
                MoqServiceManager.RemoveService<INoteAcceptor>();
            }

            CreateTestObject();

            if (printerConnected)
            {
                _moqEventBus.Verify(x => x.Subscribe<HardwareWarningEvent>(It.IsAny<object>(), It.IsAny<Action<HardwareWarningEvent>>()));
                _moqEventBus.Verify(x => x.Subscribe<HardwareWarningClearEvent>(It.IsAny<object>(), It.IsAny<Action<HardwareWarningClearEvent>>()));
                _moqEventBus.Verify(x => x.Subscribe<MissedStartupEvent>(It.IsAny<object>(), It.IsAny<Action<MissedStartupEvent>>()));
            }

            if (bnaConnected)
            {
                _moqEventBus.Verify(x => x.Subscribe<BankBalanceChangedEvent>(It.IsAny<object>(), It.IsAny<Action<BankBalanceChangedEvent>>()));
            }
        }

        private void CreateTestObject()
        {
            _ = new LandingStripController(
                    _moqEventBus.Object,
                    _moqEdgeLightingController.Object,
                    _moqBank.Object,
                    _moqDisableManager.Object);
        }

        private class EdgeLightToken : IEdgeLightToken
        {
            public EdgeLightToken(int id = 100)
            {
                Id = id;
            }

            public int Id { get; }
        }
    }
}