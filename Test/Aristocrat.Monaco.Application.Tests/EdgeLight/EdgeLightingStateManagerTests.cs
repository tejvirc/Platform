namespace Aristocrat.Monaco.Application.Tests.EdgeLight
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Drawing;
    using Application.EdgeLight;
    using Contracts;
    using Contracts.EdgeLight;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class EdgeLightingStateManagerTests
    {
        private static readonly List<int> AllStripIds = Enum.GetValues(typeof(StripIDs)).Cast<int>().ToList();

        private static readonly List<int> LobbyStripIds = AllStripIds.Except(new[] {
            (int)StripIDs.BarkeeperStrip1Led,
            (int)StripIDs.BarkeeperStrip4Led,
            (int)StripIDs.LandingStripLeft,
            (int)StripIDs.LandingStripRight,
            (int)StripIDs.StepperReel1,
            (int)StripIDs.StepperReel2,
            (int)StripIDs.StepperReel3,
            (int)StripIDs.StepperReel4,
            (int)StripIDs.StepperReel5
        }).ToList();

        private static readonly List<int> AuditStripIds = AllStripIds.Except(new[] {
            (int)StripIDs.BarkeeperStrip1Led,
            (int)StripIDs.BarkeeperStrip4Led,
            (int)StripIDs.StepperReel1,
            (int)StripIDs.StepperReel2,
            (int)StripIDs.StepperReel3,
            (int)StripIDs.StepperReel4,
            (int)StripIDs.StepperReel5
        }).ToList();

        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IEdgeLightingController> _edgeLightController;
        private EdgeLightingStateManager _edgeLightingStateManager;

        private class EdgeLightToken : IEdgeLightToken
        {
            public EdgeLightToken(int id = 100)
            {
                Id = id;
            }

            public int Id { get; }
        }

        private void SetUpDefault()
        {
            _propertiesManager
                .Setup(
                    n => n.GetProperty(ApplicationConstants.EdgeLightingBrightnessControlDefault, 100))
                .Returns(100);
            _propertiesManager
                .Setup(
                    m => m.GetProperty(
                        ApplicationConstants.MaximumAllowedEdgeLightingBrightnessKey,
                        100))
                .Returns(90);

            _propertiesManager.Setup(
                x => x.GetProperty(
                    ApplicationConstants.EdgeLightingDefaultStateColorOverrideSelectionKey,
                    It.IsAny<string>())).Returns("Transparent");
            _edgeLightController.Setup(x => x.StripIds).Returns(AllStripIds);
            _edgeLightController.Setup(
                x => x.AddEdgeLightRenderer(
                    It.IsAny<SolidColorPatternParameters>())).Returns(new EdgeLightToken(70));
        }

        [TestInitialize]
        public void Initialize()
        {
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);
            _edgeLightController = new Mock<IEdgeLightingController>(MockBehavior.Strict);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEdgeLightingControllerIsNullExpectException()
        {
            _edgeLightingStateManager = new EdgeLightingStateManager(
                null,
                _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPropertiesManagerIsNullExpectException()
        {
            _edgeLightingStateManager = new EdgeLightingStateManager(
                _edgeLightController.Object,
                null);
        }

        [TestMethod]
        public void EdgeLightStateManagerDefaultSetup()
        {
            SetUpDefault();
            _edgeLightingStateManager = new EdgeLightingStateManager(
                _edgeLightController.Object,
                _propertiesManager.Object);
            _edgeLightController.VerifyAll();
        }

        [TestMethod]
        public void SetStateAndClearStateTestForCashOut()
        {
            SetUpDefault();
            _edgeLightingStateManager = new EdgeLightingStateManager(
                _edgeLightController.Object,
                _propertiesManager.Object);
            _edgeLightController.Setup(
                x => x.AddEdgeLightRenderer(
                    It.IsAny<RainbowPatternParameters>())).Callback<PatternParameters>(
                rainbowPattern =>
                {
                    Assert.IsTrue(rainbowPattern.Priority == StripPriority.CashOut);
                    Assert.IsTrue(
                        rainbowPattern.Strips.SequenceEqual(
                            new List<int>
                            {
                                (int)StripIDs.MainCabinetLeft,
                                (int)StripIDs.MainCabinetRight,
                                (int)StripIDs.MainCabinetBottom,
                                (int)StripIDs.MainCabinetTop
                            }
                        ));
                }).Returns(new EdgeLightToken(50)).Verifiable();
            var tokenCashOut = _edgeLightingStateManager.SetState(EdgeLightState.Cashout);
            Assert.IsTrue(tokenCashOut.Id == 50);
            _edgeLightController.Verify(x => x.SetBrightnessForPriority(It.IsAny<int>(), It.IsAny<StripPriority>()) , Times.Never);
            _edgeLightController.Verify(x => x.ClearBrightnessForPriority(It.IsAny<StripPriority>()), Times.Never);
            _edgeLightController.Setup(x => x.RemoveEdgeLightRenderer(tokenCashOut));
            _edgeLightingStateManager.ClearState(tokenCashOut);
            _edgeLightController.Verify(x => x.SetBrightnessForPriority(It.IsAny<int>(), It.IsAny<StripPriority>()), Times.Never);
            _edgeLightController.Verify(x => x.ClearBrightnessForPriority(It.IsAny<StripPriority>()), Times.Never);
            _edgeLightController.VerifyAll();
        }

        [TestMethod]
        public void SetStateAndClearStateTestForDoorOpen()
        {
            SetUpDefault();
            _edgeLightingStateManager = new EdgeLightingStateManager(
                _edgeLightController.Object,
                _propertiesManager.Object);
            var brightness = 100;
#if (!RETAIL)
            brightness = 25;
#endif
            _edgeLightController.Setup(
                x => x.SetBrightnessForPriority(brightness, StripPriority.DoorOpen)).Verifiable();
            _edgeLightController.Setup(
                x => x.AddEdgeLightRenderer(
                    It.IsAny<SolidColorPatternParameters>())).Callback<PatternParameters>(
                pattern =>
                {
                    Assert.IsTrue(pattern is SolidColorPatternParameters);
                    Assert.IsTrue(((SolidColorPatternParameters)pattern).Priority == StripPriority.DoorOpen);
                    Assert.IsTrue(((SolidColorPatternParameters)pattern).Color == Color.White);
                    Assert.IsTrue(((SolidColorPatternParameters)pattern).Strips.SequenceEqual(new List<int>()));
                }).Returns(new EdgeLightToken(60)).Verifiable();
            _edgeLightController.Setup(x => x.ClearBrightnessForPriority(StripPriority.DoorOpen)).Verifiable();
            var tokenDoorOpen = _edgeLightingStateManager.SetState(EdgeLightState.DoorOpen);
            Assert.IsTrue(tokenDoorOpen.Id == 60);
            _edgeLightController.Verify(x => x.SetBrightnessForPriority(brightness, StripPriority.DoorOpen), Times.Once);
            _edgeLightController.Verify(x => x.ClearBrightnessForPriority(StripPriority.DoorOpen), Times.Never);
            _edgeLightController.Setup(x => x.RemoveEdgeLightRenderer(tokenDoorOpen));
            _edgeLightingStateManager.ClearState(tokenDoorOpen);
            _edgeLightController.Verify(x => x.SetBrightnessForPriority(brightness, StripPriority.DoorOpen), Times.Once);
            _edgeLightController.Verify(x => x.ClearBrightnessForPriority(StripPriority.DoorOpen), Times.Once);
            _edgeLightController.VerifyAll();
        }

        [TestMethod]
        public void SetStateAndClearStateTestForLobby()
        {
            SetUpDefault();
            _edgeLightingStateManager = new EdgeLightingStateManager(
                _edgeLightController.Object,
                _propertiesManager.Object);

            _edgeLightController.Setup(x => x.StripIds).Returns(AllStripIds);
            _edgeLightController.Setup(
                x => x.AddEdgeLightRenderer(
                    It.IsAny<SolidColorPatternParameters>())).Callback<PatternParameters>(
                pattern =>
                {
                    Assert.IsTrue(pattern is SolidColorPatternParameters);
                    Assert.IsTrue(((SolidColorPatternParameters)pattern).Priority == StripPriority.LobbyView);
                    Assert.IsTrue(((SolidColorPatternParameters)pattern).Color == Color.Blue);
                    CollectionAssert.AreEquivalent(LobbyStripIds, pattern.Strips.ToList());
                }).Returns(new EdgeLightToken(70)).Verifiable();
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.EdgeLightingLobbyModeColorOverrideSelectionKey, It.IsAny<string>())).Returns("Blue");
            var tokenLobby = _edgeLightingStateManager.SetState(EdgeLightState.Lobby);
            Assert.IsTrue(tokenLobby.Id == 70);
            _edgeLightController.Verify(x => x.SetBrightnessForPriority(It.IsAny<int>(), It.IsAny<StripPriority>()), Times.Never);
            _edgeLightController.Verify(x => x.ClearBrightnessForPriority(It.IsAny<StripPriority>()), Times.Never);
            _edgeLightController.Setup(x => x.RemoveEdgeLightRenderer(tokenLobby));
            _edgeLightingStateManager.ClearState(tokenLobby);
            _edgeLightController.Verify(x => x.SetBrightnessForPriority(It.IsAny<int>(), It.IsAny<StripPriority>()), Times.Never);
            _edgeLightController.Verify(x => x.ClearBrightnessForPriority(It.IsAny<StripPriority>()), Times.Never);
            _edgeLightController.VerifyAll();
        }

        [TestMethod]
        public void SetStateAndClearStateTestForOperatorMode()
        {
            SetUpDefault();
            _edgeLightingStateManager = new EdgeLightingStateManager(
                _edgeLightController.Object,
                _propertiesManager.Object);
            _edgeLightController.Setup(
                x => x.SetBrightnessForPriority(100, StripPriority.AuditMenu)).Verifiable();
            _edgeLightController.Setup(x => x.StripIds).Returns(AllStripIds);
            _edgeLightController.Setup(
                x => x.AddEdgeLightRenderer(
                    It.IsAny<SolidColorPatternParameters>())).Callback<PatternParameters>(
                pattern =>
                {
                    Assert.IsTrue(pattern is SolidColorPatternParameters);
                    Assert.IsTrue(((SolidColorPatternParameters)pattern).Priority == StripPriority.AuditMenu);
                    Assert.IsTrue(((SolidColorPatternParameters)pattern).Color == Color.FromArgb(255, 0, 0, 204));
                    Assert.IsTrue(((SolidColorPatternParameters)pattern).Strips.SequenceEqual(AuditStripIds));
                }).Returns(new EdgeLightToken(80)).Verifiable();
            _edgeLightController.Setup(x => x.ClearBrightnessForPriority(StripPriority.AuditMenu)).Verifiable();
            var tokenOperatorMode = _edgeLightingStateManager.SetState(EdgeLightState.OperatorMode);
            Assert.IsTrue(tokenOperatorMode.Id == 80);
            _edgeLightController.Verify(x => x.SetBrightnessForPriority(100, StripPriority.AuditMenu), Times.Once);
            _edgeLightController.Verify(x => x.ClearBrightnessForPriority(StripPriority.AuditMenu), Times.Never);
            _edgeLightController.Setup(x => x.RemoveEdgeLightRenderer(tokenOperatorMode));
            _edgeLightingStateManager.ClearState(tokenOperatorMode);
            _edgeLightController.Verify(x => x.SetBrightnessForPriority(100, StripPriority.AuditMenu), Times.Once);
            _edgeLightController.Verify(x => x.ClearBrightnessForPriority(StripPriority.AuditMenu), Times.Once);
            _edgeLightController.VerifyAll();
        }

        [TestMethod]
        public void SetStateAndClearStateTestForTowerLightDisabled()
        {
            SetUpDefault();
            _propertiesManager
                .Setup(
                    n => n.GetProperty(ApplicationConstants.EdgeLightAsTowerLightEnabled, false))
                .Returns(false);
            _edgeLightingStateManager = new EdgeLightingStateManager(
                _edgeLightController.Object,
                _propertiesManager.Object);
            var tokenTowerLight = _edgeLightingStateManager.SetState(EdgeLightState.TowerLightMode);
            Assert.IsTrue(tokenTowerLight == null);
            _edgeLightController.Verify(x => x.SetBrightnessForPriority(100, StripPriority.BarTopTowerLight), Times.Never);
            _edgeLightController.Verify(x => x.ClearBrightnessForPriority(StripPriority.BarTopTowerLight), Times.Never);
            _edgeLightController.VerifyAll();
        }

        [TestMethod]
        public void SetStateAndClearStateTestForTowerLightEnabled()
        {
            SetUpDefault();
            _propertiesManager
                .Setup(
                    n => n.GetProperty(ApplicationConstants.EdgeLightAsTowerLightEnabled, false))
                .Returns(true);
            _edgeLightingStateManager = new EdgeLightingStateManager(
                _edgeLightController.Object,
                _propertiesManager.Object);
            _edgeLightController.Setup(
                x => x.SetBrightnessForPriority(100, StripPriority.BarTopTowerLight)).Verifiable();
            _edgeLightController.Setup(
                x => x.AddEdgeLightRenderer(
                    It.IsAny<BlinkPatternParameters>())).Callback<PatternParameters>(
                pattern =>
                {
                    Assert.IsTrue(pattern is BlinkPatternParameters);
                    Assert.IsTrue(((BlinkPatternParameters)pattern).Priority == StripPriority.BarTopTowerLight);
                    Assert.IsTrue(((BlinkPatternParameters)pattern).OnColor == Color.Red);
                    Assert.IsTrue(((BlinkPatternParameters)pattern).OffColor == Color.Black);
                    Assert.IsTrue(((BlinkPatternParameters)pattern).Strips.SequenceEqual(new List<int>()));
                }).Returns(new EdgeLightToken(90)).Verifiable();
            _edgeLightController.Setup(x => x.ClearBrightnessForPriority(StripPriority.BarTopTowerLight)).Verifiable();
            var tokenTowerLight = _edgeLightingStateManager.SetState(EdgeLightState.TowerLightMode);
            Assert.IsTrue(tokenTowerLight.Id == 90);
            _edgeLightController.Verify(x => x.SetBrightnessForPriority(100, StripPriority.BarTopTowerLight), Times.Once);
            _edgeLightController.Verify(x => x.ClearBrightnessForPriority(StripPriority.BarTopTowerLight), Times.Never);
            _edgeLightController.Setup(x => x.RemoveEdgeLightRenderer(tokenTowerLight));
            _edgeLightingStateManager.ClearState(tokenTowerLight);
            _edgeLightController.Verify(x => x.SetBrightnessForPriority(100, StripPriority.BarTopTowerLight), Times.Once);
            _edgeLightController.Verify(x => x.ClearBrightnessForPriority(StripPriority.BarTopTowerLight), Times.Once);
            _edgeLightController.VerifyAll();
        }

        [TestMethod]
        public void ClearStateOnInvalidToken()
        {
            SetUpDefault();
            _edgeLightingStateManager = new EdgeLightingStateManager(
                _edgeLightController.Object,
                _propertiesManager.Object);
            _edgeLightController.VerifyAll();
            var token = new EdgeLightToken();
            _edgeLightController.Setup(x => x.RemoveEdgeLightRenderer(token)).Verifiable();
            _edgeLightingStateManager.ClearState(token);
            _edgeLightController.Verify(x => x.ClearBrightnessForPriority(It.IsAny<StripPriority>()), Times.Never);
            _edgeLightController.Verify(x => x.RemoveEdgeLightRenderer(token), Times.Once);
            _edgeLightController.VerifyAll();
        }

        [TestMethod]
        public void SetStateForInvalidState()
        {
            SetUpDefault();
            _edgeLightingStateManager = new EdgeLightingStateManager(
                _edgeLightController.Object,
                _propertiesManager.Object);
            _edgeLightController.VerifyAll();
            var token = _edgeLightingStateManager.SetState((EdgeLightState)20);
            Assert.IsTrue(token == null);
            _edgeLightController.VerifyAll();
        }

        [TestMethod]
        public void SetMoreStatesThanClear()
        {
            SetUpDefault();
            _edgeLightingStateManager = new EdgeLightingStateManager(
                _edgeLightController.Object,
                _propertiesManager.Object);

            _edgeLightController.VerifyAll();
            _edgeLightController.Setup(x => x.StripIds).Returns(AllStripIds);
            _edgeLightController.Setup(
                x => x.AddEdgeLightRenderer(
                    It.IsAny<RainbowPatternParameters>())).Callback<PatternParameters>(
                rainbowPattern =>
                {
                    Assert.IsTrue(rainbowPattern.Priority == StripPriority.CashOut);
                    CollectionAssert.AreEquivalent(
                        new List<int>
                        {
                            (int)StripIDs.MainCabinetLeft,
                            (int)StripIDs.MainCabinetRight,
                            (int)StripIDs.MainCabinetBottom,
                            (int)StripIDs.MainCabinetTop
                        },
                        rainbowPattern.Strips.ToList());
                }).Returns(new EdgeLightToken(50)).Verifiable();
            var tokenCashOut = _edgeLightingStateManager.SetState(EdgeLightState.Cashout);
            Assert.IsTrue(tokenCashOut.Id == 50);
            var brightness = 100;
#if (!RETAIL)
            brightness = 25;
#endif
            _edgeLightController.Setup(
                x => x.SetBrightnessForPriority(brightness, StripPriority.DoorOpen)).Verifiable();
            _edgeLightController.Setup(
                x => x.AddEdgeLightRenderer(
                    It.IsAny<SolidColorPatternParameters>())).Callback<PatternParameters>(
                pattern =>
                {
                    Assert.IsTrue(pattern is SolidColorPatternParameters);
                    Assert.IsTrue(((SolidColorPatternParameters)pattern).Priority == StripPriority.DoorOpen);
                    Assert.IsTrue(((SolidColorPatternParameters)pattern).Color == Color.White);
                    Assert.IsTrue(((SolidColorPatternParameters)pattern).Strips.SequenceEqual(new List<int>()));
                }).Returns(new EdgeLightToken(60)).Verifiable();
            _edgeLightController.Setup(x => x.ClearBrightnessForPriority(StripPriority.DoorOpen)).Verifiable();
            var tokenDoorOpen = _edgeLightingStateManager.SetState(EdgeLightState.DoorOpen);
            Assert.IsTrue(tokenDoorOpen.Id == 60);
            _edgeLightController.Setup(
                x => x.AddEdgeLightRenderer(
                    It.IsAny<SolidColorPatternParameters>())).Callback<PatternParameters>(
                pattern =>
                {
                    Assert.IsTrue(pattern is SolidColorPatternParameters);
                    Assert.IsTrue(((SolidColorPatternParameters)pattern).Priority == StripPriority.LobbyView);
                    Assert.IsTrue(((SolidColorPatternParameters)pattern).Color == Color.Blue);
                    CollectionAssert.AreEquivalent(LobbyStripIds, pattern.Strips.ToList());
                }).Returns(new EdgeLightToken(70)).Verifiable();
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.EdgeLightingLobbyModeColorOverrideSelectionKey, It.IsAny<string>())).Returns("Blue");
            var tokenLobby = _edgeLightingStateManager.SetState(EdgeLightState.Lobby);
            Assert.IsTrue(tokenLobby.Id == 70);
            _edgeLightController.Setup(x => x.RemoveEdgeLightRenderer(null));
            _edgeLightingStateManager.ClearState(null);
            _edgeLightController.Setup(x => x.RemoveEdgeLightRenderer(tokenDoorOpen));
            _edgeLightingStateManager.ClearState(tokenDoorOpen);
            _edgeLightController.VerifyAll();
        }
        [TestMethod]
        public void AttractModeNonCabinetStrips()
        {
            SetUpDefault();
            _edgeLightingStateManager = new EdgeLightingStateManager(
                _edgeLightController.Object,
                _propertiesManager.Object);
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.EdgeLightingAttractModeColorOverrideSelectionKey, It.IsAny<string>())).Returns("Green");
            _edgeLightController.Setup(x => x.StripIds).Returns(AllStripIds);
            _edgeLightController.Setup(x=>x.ClearBrightnessForPriority(StripPriority.LobbyView));
            _edgeLightController.Setup(
                x => x.AddEdgeLightRenderer(
                    It.IsAny<SolidColorPatternParameters>())).Callback<PatternParameters>(
                pattern =>
                {
                    Assert.IsTrue(pattern is SolidColorPatternParameters);
                    Assert.IsTrue(((SolidColorPatternParameters)pattern).Priority == StripPriority.LobbyView);
                    Assert.IsTrue(((SolidColorPatternParameters)pattern).Color == Color.Green);
                    CollectionAssert.AreEquivalent(LobbyStripIds, pattern.Strips.ToList());
                }).Returns(new EdgeLightToken(70)).Verifiable();
            var tokenLobby = _edgeLightingStateManager.SetState(EdgeLightState.AttractMode);
            Assert.IsTrue(tokenLobby.Id == 70);
            _edgeLightController.Verify(x => x.SetBrightnessForPriority(It.IsAny<int>(), It.IsAny<StripPriority>()), Times.Never);
            _edgeLightController.Setup(x => x.RemoveEdgeLightRenderer(tokenLobby));
            _edgeLightingStateManager.ClearState(tokenLobby);
            _edgeLightController.Verify(x => x.SetBrightnessForPriority(It.IsAny<int>(), It.IsAny<StripPriority>()), Times.Never);
            _edgeLightController.Verify(x => x.ClearBrightnessForPriority(StripPriority.LobbyView), Times.Once);
            _edgeLightController.VerifyAll();
        }

        [TestMethod]
        public void DefaultModeNonCabinetStrips()
        {
            SetUpDefault();
            _edgeLightingStateManager = new EdgeLightingStateManager(
                _edgeLightController.Object,
                _propertiesManager.Object);
            _edgeLightController.Setup(x => x.StripIds).Returns(AllStripIds);
            _edgeLightController.Setup(x => x.ClearBrightnessForPriority(StripPriority.LowPriority));
            _edgeLightController.Setup(
                x => x.AddEdgeLightRenderer(
                    It.IsAny<SolidColorPatternParameters>())).Callback<PatternParameters>(
                pattern =>
                {
                    Assert.IsTrue(pattern is SolidColorPatternParameters);
                    Assert.IsTrue(((SolidColorPatternParameters)pattern).Priority == StripPriority.LowPriority);
                    Assert.IsTrue(((SolidColorPatternParameters)pattern).Color == Color.Transparent);
                    CollectionAssert.AreEquivalent(LobbyStripIds, pattern.Strips.ToList());
                }).Returns(new EdgeLightToken(90)).Verifiable();
            var tokenDefault = _edgeLightingStateManager.SetState(EdgeLightState.DefaultMode);
            Assert.IsTrue(tokenDefault.Id == 90);
            _edgeLightController.Verify(x => x.SetBrightnessForPriority(It.IsAny<int>(), It.IsAny<StripPriority>()), Times.Never);
            _edgeLightController.Setup(x => x.RemoveEdgeLightRenderer(tokenDefault));
            _edgeLightingStateManager.ClearState(tokenDefault);
            _edgeLightController.Verify(x => x.SetBrightnessForPriority(It.IsAny<int>(), It.IsAny<StripPriority>()), Times.Never);
            _edgeLightController.Verify(x => x.ClearBrightnessForPriority(StripPriority.LowPriority), Times.Once);
            _edgeLightController.VerifyAll();
        }
    }
}
