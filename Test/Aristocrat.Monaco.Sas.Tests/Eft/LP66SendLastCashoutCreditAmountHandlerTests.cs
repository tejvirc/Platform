namespace Aristocrat.Monaco.Sas.Tests.Eft
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Test.Common;
    using Aristocrat.Monaco.Sas.Eft;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Sas.Client.Eft;
    using Aristocrat.Sas.Client;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Vgt.Client12.Application.OperatorMenu;
    using Aristocrat.Monaco.Common;
    using System.Timers;
    using Aristocrat.Monaco.Hardware.Contracts.Door;

    /// <summary>
    ///     Contains unit tests for the LP66SendLastCashoutCreditAmountHandler class.
    /// </summary>
    [TestClass]
    public class LP66SendLastCashoutCreditAmountHandlerTests
    {
        private Mock<IPlayerInitiatedCashoutProvider> _playerInitiatedCashoutMock;
        private Mock<ISystemDisableManager> _systemDisableManagerMock;
        private Mock<IGamePlayState> _gamePlayStateMock;
        private Mock<IOperatorMenuLauncher> _operatorMenuLauncherMock;
        private Mock<IDisableByOperatorManager> _disableByOperatorManagerMock;
        private Mock<IDoorService> _doorServiceMock;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            _playerInitiatedCashoutMock = new Mock<IPlayerInitiatedCashoutProvider>(MockBehavior.Strict);
            _systemDisableManagerMock = new Mock<ISystemDisableManager>(MockBehavior.Strict);
            _gamePlayStateMock = new Mock<IGamePlayState>(MockBehavior.Strict);
            _operatorMenuLauncherMock = new Mock<IOperatorMenuLauncher>(MockBehavior.Strict);
            _disableByOperatorManagerMock = new Mock<IDisableByOperatorManager>(MockBehavior.Strict);
            _doorServiceMock = new Mock<IDoorService>(MockBehavior.Strict);
        }

        [DataRow(true, false, false, false, false, false, DisplayName = "Null DoorService")]
        [DataRow(false, true, false, false, false, false, DisplayName = "Null PlayerInitiatedCashoutProvider")]
        [DataRow(false, false, true, false, false, false, DisplayName = "Null SystemDisableManager")]
        [DataRow(false, false, false, true, false, false, DisplayName = "Null GamePlayState")]
        [DataRow(false, false, false, false, true, false, DisplayName = "Null OperatorMenuLauncher")]
        [DataRow(false, false, false, false, false, true, DisplayName = "Null DisableByOperatorManager")]
        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullArgumentExpectExceptionTests(
            bool isNullDoorService,
            bool isNullPlayerInitiatedCashoutProvider,
            bool isNullSystemDisableManager,
            bool isNullGamePlayState,
            bool isNullOperatorMenuLauncher,
            bool isNullDisableByOperatorManager)
        {
            new LP66SendLastCashoutCreditAmountHandler(
                isNullDoorService ? null : _doorServiceMock.Object,
                isNullPlayerInitiatedCashoutProvider ? null : _playerInitiatedCashoutMock.Object,
                isNullSystemDisableManager ? null : _systemDisableManagerMock.Object,
                isNullGamePlayState ? null : _gamePlayStateMock.Object,
                isNullOperatorMenuLauncher ? null : _operatorMenuLauncherMock.Object,
                isNullDisableByOperatorManager ? null : _disableByOperatorManagerMock.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            var lp66Isntance = new LP66SendLastCashoutCreditAmountHandler(
                _doorServiceMock.Object,
                _playerInitiatedCashoutMock.Object,
                _systemDisableManagerMock.Object,
                _gamePlayStateMock.Object,
                _operatorMenuLauncherMock.Object,
                _disableByOperatorManagerMock.Object);
            Assert.AreEqual(1, lp66Isntance.Commands.Count);
            Assert.IsTrue(lp66Isntance.Commands.Contains(LongPoll.EftSendLastCashOutCreditAmount));
        }

        [TestMethod]
        public void ShouldReturnCorrectAmountAndStatusWhenIdleForFalseAckRequest()
        {
            var mockAmount = 10000ul;
            _playerInitiatedCashoutMock.Setup(em => em.GetCashoutAmount()).Returns(mockAmount);

            _doorServiceMock.Setup(ds => ds.GetDoorOpen(It.IsAny<int>())).Returns(false);
            _doorServiceMock.Setup(ds => ds.LogicalDoors).Returns(new Dictionary<int, LogicalDoor>());
            _systemDisableManagerMock.Setup(sdmm => sdmm.CurrentDisableKeys).Returns(new List<Guid>());
            _gamePlayStateMock.Setup(gps => gps.CurrentState).Returns(PlayState.Idle);
            _operatorMenuLauncherMock.Setup(gps => gps.IsShowing).Returns(false);
            _disableByOperatorManagerMock.Setup(a => a.DisabledByOperator).Returns(false);

            var systemTimerMock = new Mock<ISystemTimerWrapper>(MockBehavior.Strict);
            systemTimerMock.Setup(stm => stm.Start());
            systemTimerMock.Setup(stm => stm.Stop());
            systemTimerMock.Setup(stm => stm.Enabled).Returns(false);
            systemTimerMock.Setup(stm => stm.Interval).Returns(800);
            systemTimerMock.Setup(stm => stm.AutoReset).Returns(false);

            var lp66Instance = new LP66SendLastCashoutCreditAmountHandler(
                 _doorServiceMock.Object,
                _playerInitiatedCashoutMock.Object,
                _systemDisableManagerMock.Object,
                _gamePlayStateMock.Object,
                _operatorMenuLauncherMock.Object,
                _disableByOperatorManagerMock.Object);

            lp66Instance.SetupTimer(systemTimerMock.Object);

            var theResponse = lp66Instance.Handle(new EftSendLastCashoutData { Acknowledgement = false });
            Assert.IsNotNull(theResponse);
            Assert.IsNull(theResponse.Handlers);
            Assert.AreEqual(mockAmount, theResponse.LastCashoutAmount);
            Assert.IsFalse(theResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.OperationSuccessful, theResponse.Status);
            systemTimerMock.Verify(stm => stm.Start(), Times.Once); //will go to WaitingForHostAck state
            systemTimerMock.Verify(stm => stm.Stop(), Times.Never);

            //verify all services
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Once);
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Once);
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Once);
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Once);
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Once);
        }

        [TestMethod]
        public void ShouldReturnCorrectAmountAndInvalidAckStatusWhenIdleForTrueAckRequest()
        {
            var mockAmount = 9876ul;
            _doorServiceMock.Setup(ds => ds.GetDoorOpen(It.IsAny<int>())).Returns(false);
            _doorServiceMock.Setup(ds => ds.LogicalDoors).Returns(new Dictionary<int, LogicalDoor>());
            _playerInitiatedCashoutMock.Setup(em => em.GetCashoutAmount()).Returns(mockAmount);
            _systemDisableManagerMock.Setup(sdmm => sdmm.CurrentDisableKeys).Returns(new List<Guid>());
            _gamePlayStateMock.Setup(gps => gps.CurrentState).Returns(PlayState.Idle);
            _operatorMenuLauncherMock.Setup(gps => gps.IsShowing).Returns(false);
            _disableByOperatorManagerMock.Setup(a => a.DisabledByOperator).Returns(false);

            var systemTimerMock = new Mock<ISystemTimerWrapper>(MockBehavior.Strict);
            systemTimerMock.Setup(stm => stm.Start());
            systemTimerMock.Setup(stm => stm.Stop());
            systemTimerMock.Setup(stm => stm.Enabled).Returns(false);
            systemTimerMock.Setup(stm => stm.Interval).Returns(800);
            systemTimerMock.Setup(stm => stm.AutoReset).Returns(false);

            var lp66Instance = new LP66SendLastCashoutCreditAmountHandler(
                 _doorServiceMock.Object,
                _playerInitiatedCashoutMock.Object,
                _systemDisableManagerMock.Object,
                _gamePlayStateMock.Object,
                _operatorMenuLauncherMock.Object,
                _disableByOperatorManagerMock.Object);

            lp66Instance.SetupTimer(systemTimerMock.Object);

            var requestData = new EftSendLastCashoutData { Acknowledgement = true };
            var theResponse = lp66Instance.Handle(requestData);
            Assert.IsNotNull(theResponse);
            Assert.IsNull(theResponse.Handlers);
            Assert.AreEqual(mockAmount, theResponse.LastCashoutAmount);
            Assert.AreEqual(requestData.Acknowledgement, theResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.InvalidAck, theResponse.Status);
            systemTimerMock.Verify(stm => stm.Start(), Times.Never); //still stay as Idle
            systemTimerMock.Verify(stm => stm.Stop(), Times.Never);

            //verify all services
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Once);
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Never);
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Never);
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Never);
        }

        [TestMethod]
        public void ShouldReturnOperationSuccessfulIfDoorOpens()
        {
            var mockAmount = 0ul;
            _doorServiceMock.Setup(ds => ds.GetDoorOpen(It.IsAny<int>())).Returns(true);
            _doorServiceMock.Setup(ds => ds.LogicalDoors).Returns(new Dictionary<int, LogicalDoor>{ {1, new LogicalDoor()} });
            _playerInitiatedCashoutMock.Setup(em => em.GetCashoutAmount()).Returns(mockAmount);
            _systemDisableManagerMock.Setup(sdmm => sdmm.CurrentDisableKeys).Returns(new List<Guid>());
            _gamePlayStateMock.Setup(gps => gps.CurrentState).Returns(PlayState.Idle);
            _operatorMenuLauncherMock.Setup(gps => gps.IsShowing).Returns(false);
            _disableByOperatorManagerMock.Setup(a => a.DisabledByOperator).Returns(true);

            var systemTimerMock = new Mock<ISystemTimerWrapper>(MockBehavior.Strict);
            systemTimerMock.Setup(stm => stm.Start());
            systemTimerMock.Setup(stm => stm.Stop());
            systemTimerMock.Setup(stm => stm.Enabled).Returns(false);
            systemTimerMock.Setup(stm => stm.Interval).Returns(800);
            systemTimerMock.Setup(stm => stm.AutoReset).Returns(false);

            var lp66Instance = new LP66SendLastCashoutCreditAmountHandler(
                 _doorServiceMock.Object,
                _playerInitiatedCashoutMock.Object,
                _systemDisableManagerMock.Object,
                _gamePlayStateMock.Object,
                _operatorMenuLauncherMock.Object,
                _disableByOperatorManagerMock.Object);

            lp66Instance.SetupTimer(systemTimerMock.Object);

            var requestData = new EftSendLastCashoutData { Acknowledgement = false };
            var theResponse = lp66Instance.Handle(requestData);
            Assert.IsNotNull(theResponse);
            Assert.IsNull(theResponse.Handlers);
            Assert.AreEqual(mockAmount, theResponse.LastCashoutAmount);
            Assert.AreEqual(requestData.Acknowledgement, theResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.OperationSuccessful, theResponse.Status);
            systemTimerMock.Verify(stm => stm.Start(), Times.Once); //still stay as Idle
            systemTimerMock.Verify(stm => stm.Stop(), Times.Never);

            //verify all services
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Once);
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Never);
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Never);
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Never);
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Never);
        }

        [TestMethod]
        public void ShouldReturnEgmOutOfService()
        {
            var mockAmount = 0ul;
            _doorServiceMock.Setup(ds => ds.GetDoorOpen(It.IsAny<int>())).Returns(false);
            _doorServiceMock.Setup(ds => ds.LogicalDoors).Returns(new Dictionary<int, LogicalDoor>());
            _playerInitiatedCashoutMock.Setup(em => em.GetCashoutAmount()).Returns(mockAmount);
            _systemDisableManagerMock.Setup(sdmm => sdmm.CurrentDisableKeys).Returns(new List<Guid>());
            _gamePlayStateMock.Setup(gps => gps.CurrentState).Returns(PlayState.Idle);
            _operatorMenuLauncherMock.Setup(gps => gps.IsShowing).Returns(false);
            _disableByOperatorManagerMock.Setup(a => a.DisabledByOperator).Returns(true);

            var systemTimerMock = new Mock<ISystemTimerWrapper>(MockBehavior.Strict);
            systemTimerMock.Setup(stm => stm.Start());
            systemTimerMock.Setup(stm => stm.Stop());
            systemTimerMock.Setup(stm => stm.Enabled).Returns(false);
            systemTimerMock.Setup(stm => stm.Interval).Returns(800);
            systemTimerMock.Setup(stm => stm.AutoReset).Returns(false);

            var lp66Instance = new LP66SendLastCashoutCreditAmountHandler(
                 _doorServiceMock.Object,
                _playerInitiatedCashoutMock.Object,
                _systemDisableManagerMock.Object,
                _gamePlayStateMock.Object,
                _operatorMenuLauncherMock.Object,
                _disableByOperatorManagerMock.Object);

            lp66Instance.SetupTimer(systemTimerMock.Object);

            var requestData = new EftSendLastCashoutData { Acknowledgement = false };
            var theResponse = lp66Instance.Handle(requestData);
            Assert.IsNotNull(theResponse);
            Assert.IsNull(theResponse.Handlers);
            Assert.AreEqual(mockAmount, theResponse.LastCashoutAmount);
            Assert.AreEqual(requestData.Acknowledgement, theResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.EgmOutOfService, theResponse.Status);
            systemTimerMock.Verify(stm => stm.Start(), Times.Once); //still stay as Idle
            systemTimerMock.Verify(stm => stm.Stop(), Times.Never);

            //verify all services
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Once);
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Once);
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Once);
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Once);
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Once);
        }

        [TestMethod]
        public void ShouldReturnInGamePlayModeByShowingOperatorMenu()
        {
            var mockAmount = 0ul;
            _doorServiceMock.Setup(ds => ds.GetDoorOpen(It.IsAny<int>())).Returns(false);
            _doorServiceMock.Setup(ds => ds.LogicalDoors).Returns(new Dictionary<int, LogicalDoor>());
            _playerInitiatedCashoutMock.Setup(em => em.GetCashoutAmount()).Returns(mockAmount);
            _systemDisableManagerMock.Setup(sdmm => sdmm.CurrentDisableKeys).Returns(new List<Guid>());
            _gamePlayStateMock.Setup(gps => gps.CurrentState).Returns(PlayState.Idle);
            _operatorMenuLauncherMock.Setup(gps => gps.IsShowing).Returns(true);
            _disableByOperatorManagerMock.Setup(a => a.DisabledByOperator).Returns(false);

            var systemTimerMock = new Mock<ISystemTimerWrapper>(MockBehavior.Strict);
            systemTimerMock.Setup(stm => stm.Start());
            systemTimerMock.Setup(stm => stm.Stop());
            systemTimerMock.Setup(stm => stm.Enabled).Returns(false);
            systemTimerMock.Setup(stm => stm.Interval).Returns(800);
            systemTimerMock.Setup(stm => stm.AutoReset).Returns(false);

            var lp66Instance = new LP66SendLastCashoutCreditAmountHandler(
                _doorServiceMock.Object,
                _playerInitiatedCashoutMock.Object,
                _systemDisableManagerMock.Object,
                _gamePlayStateMock.Object,
                _operatorMenuLauncherMock.Object,
                _disableByOperatorManagerMock.Object);

            lp66Instance.SetupTimer(systemTimerMock.Object);

            var requestData = new EftSendLastCashoutData { Acknowledgement = false };
            var theResponse = lp66Instance.Handle(requestData);
            Assert.IsNotNull(theResponse);
            Assert.IsNull(theResponse.Handlers);
            Assert.AreEqual(mockAmount, theResponse.LastCashoutAmount);
            Assert.AreEqual(requestData.Acknowledgement, theResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.InGamePlayMode, theResponse.Status);
            systemTimerMock.Verify(stm => stm.Start(), Times.Once); //still stay as Idle
            systemTimerMock.Verify(stm => stm.Stop(), Times.Never);

            //verify all services
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Once);
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Once);
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Once);
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Once);
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Never);
        }

        [TestMethod]
        public void ShouldReturnInGamePlayMode()
        {
            var mockAmount = 0ul;
            _doorServiceMock.Setup(ds => ds.GetDoorOpen(It.IsAny<int>())).Returns(false);
            _doorServiceMock.Setup(ds => ds.LogicalDoors).Returns(new Dictionary<int, LogicalDoor>());
            _playerInitiatedCashoutMock.Setup(em => em.GetCashoutAmount()).Returns(mockAmount);
            _systemDisableManagerMock.Setup(sdmm => sdmm.CurrentDisableKeys).Returns(new List<Guid>());
            _gamePlayStateMock.Setup(gps => gps.CurrentState).Returns(PlayState.PrimaryGameStarted);
            _operatorMenuLauncherMock.Setup(gps => gps.IsShowing).Returns(false);
            _disableByOperatorManagerMock.Setup(a => a.DisabledByOperator).Returns(false);

            var systemTimerMock = new Mock<ISystemTimerWrapper>(MockBehavior.Strict);
            systemTimerMock.Setup(stm => stm.Start());
            systemTimerMock.Setup(stm => stm.Stop());
            systemTimerMock.Setup(stm => stm.Enabled).Returns(false);
            systemTimerMock.Setup(stm => stm.Interval).Returns(800);
            systemTimerMock.Setup(stm => stm.AutoReset).Returns(false);

            var lp66Instance = new LP66SendLastCashoutCreditAmountHandler(
                _doorServiceMock.Object,
                _playerInitiatedCashoutMock.Object,
                _systemDisableManagerMock.Object,
                _gamePlayStateMock.Object,
                _operatorMenuLauncherMock.Object,
                _disableByOperatorManagerMock.Object);

            lp66Instance.SetupTimer(systemTimerMock.Object);

            var requestData = new EftSendLastCashoutData { Acknowledgement = false };
            var theResponse = lp66Instance.Handle(requestData);
            Assert.IsNotNull(theResponse);
            Assert.IsNull(theResponse.Handlers);
            Assert.AreEqual(mockAmount, theResponse.LastCashoutAmount);
            Assert.AreEqual(requestData.Acknowledgement, theResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.InGamePlayMode, theResponse.Status);
            systemTimerMock.Verify(stm => stm.Start(), Times.Once); //still stay as Idle
            systemTimerMock.Verify(stm => stm.Stop(), Times.Never);

            //verify all services
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Once);
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Once);
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Once);
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Never);
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Never);
        }

        [TestMethod]
        public void ShouldReturnEgmDisabled()
        {
            var mockAmount = 0ul;
            _doorServiceMock.Setup(ds => ds.GetDoorOpen(It.IsAny<int>())).Returns(false);
            _doorServiceMock.Setup(ds => ds.LogicalDoors).Returns(new Dictionary<int, LogicalDoor>());
            _playerInitiatedCashoutMock.Setup(em => em.GetCashoutAmount()).Returns(mockAmount);
            _systemDisableManagerMock.Setup(sdmm => sdmm.CurrentDisableKeys).Returns(EftCommonGuids.DisabledByHostGuids);
            _gamePlayStateMock.Setup(gps => gps.CurrentState).Returns(PlayState.Idle);
            _operatorMenuLauncherMock.Setup(gps => gps.IsShowing).Returns(false);
            _disableByOperatorManagerMock.Setup(a => a.DisabledByOperator).Returns(false);

            var systemTimerMock = new Mock<ISystemTimerWrapper>(MockBehavior.Strict);
            systemTimerMock.Setup(stm => stm.Start());
            systemTimerMock.Setup(stm => stm.Stop());
            systemTimerMock.Setup(stm => stm.Enabled).Returns(false);
            systemTimerMock.Setup(stm => stm.Interval).Returns(800);
            systemTimerMock.Setup(stm => stm.AutoReset).Returns(false);

            var lp66Instance = new LP66SendLastCashoutCreditAmountHandler(
                _doorServiceMock.Object,
                _playerInitiatedCashoutMock.Object,
                _systemDisableManagerMock.Object,
                _gamePlayStateMock.Object,
                _operatorMenuLauncherMock.Object,
                _disableByOperatorManagerMock.Object);

            lp66Instance.SetupTimer(systemTimerMock.Object);

            var requestData = new EftSendLastCashoutData { Acknowledgement = false };
            var theResponse = lp66Instance.Handle(requestData);
            Assert.IsNotNull(theResponse);
            Assert.IsNull(theResponse.Handlers);
            Assert.AreEqual(mockAmount, theResponse.LastCashoutAmount);
            Assert.AreEqual(requestData.Acknowledgement, theResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.EgmDisabled, theResponse.Status);
            systemTimerMock.Verify(stm => stm.Start(), Times.Once); //still stay as Idle
            systemTimerMock.Verify(stm => stm.Stop(), Times.Never);

            //verify all services
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Once);
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Once);
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Never);
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Never);
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Never);
        }

        [TestMethod]
        public void ShouldReturnEgmInTiltCondition()
        {
            var mockAmount = 0ul;
            _doorServiceMock.Setup(ds => ds.GetDoorOpen(It.IsAny<int>())).Returns(false);
            _doorServiceMock.Setup(ds => ds.LogicalDoors).Returns(new Dictionary<int, LogicalDoor>());
            _playerInitiatedCashoutMock.Setup(em => em.GetCashoutAmount()).Returns(mockAmount);
            _systemDisableManagerMock.Setup(sdmm => sdmm.CurrentDisableKeys).Returns(new List<Guid> { Guid.NewGuid() });
            _gamePlayStateMock.Setup(gps => gps.CurrentState).Returns(PlayState.Idle);
            _operatorMenuLauncherMock.Setup(gps => gps.IsShowing).Returns(false);
            _disableByOperatorManagerMock.Setup(a => a.DisabledByOperator).Returns(false);

            var systemTimerMock = new Mock<ISystemTimerWrapper>(MockBehavior.Strict);
            systemTimerMock.Setup(stm => stm.Start());
            systemTimerMock.Setup(stm => stm.Stop());
            systemTimerMock.Setup(stm => stm.Enabled).Returns(false);
            systemTimerMock.Setup(stm => stm.Interval).Returns(800);
            systemTimerMock.Setup(stm => stm.AutoReset).Returns(false);

            var lp66Instance = new LP66SendLastCashoutCreditAmountHandler(
                _doorServiceMock.Object,
                _playerInitiatedCashoutMock.Object,
                _systemDisableManagerMock.Object,
                _gamePlayStateMock.Object,
                _operatorMenuLauncherMock.Object,
                _disableByOperatorManagerMock.Object);

            lp66Instance.SetupTimer(systemTimerMock.Object);

            var requestData = new EftSendLastCashoutData { Acknowledgement = false };
            var theResponse = lp66Instance.Handle(requestData);
            Assert.IsNotNull(theResponse);
            Assert.IsNull(theResponse.Handlers);
            Assert.AreEqual(mockAmount, theResponse.LastCashoutAmount);
            Assert.AreEqual(requestData.Acknowledgement, theResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.EgmInTiltCondition, theResponse.Status);
            systemTimerMock.Verify(stm => stm.Start(), Times.Once); //still stay as Idle
            systemTimerMock.Verify(stm => stm.Stop(), Times.Never);

            //verify all services
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Once);
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Once);
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Never);
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Never);
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Never);
        }

        [TestMethod]
        public void ShouldStillMoveToWaitingForHostAckIfLastCashoutAmountIsZero()
        {
            var mockAmount = 0ul;
            _doorServiceMock.Setup(ds => ds.GetDoorOpen(It.IsAny<int>())).Returns(false);
            _doorServiceMock.Setup(ds => ds.LogicalDoors).Returns(new Dictionary<int, LogicalDoor>());
            _playerInitiatedCashoutMock.Setup(em => em.GetCashoutAmount()).Returns(mockAmount);
            _systemDisableManagerMock.Setup(sdmm => sdmm.CurrentDisableKeys).Returns(new List<Guid>());
            _gamePlayStateMock.Setup(gps => gps.CurrentState).Returns(PlayState.Idle);
            _operatorMenuLauncherMock.Setup(gps => gps.IsShowing).Returns(false);
            _disableByOperatorManagerMock.Setup(a => a.DisabledByOperator).Returns(false);

            var systemTimerMock = new Mock<ISystemTimerWrapper>(MockBehavior.Strict);
            systemTimerMock.Setup(stm => stm.Start());
            systemTimerMock.Setup(stm => stm.Stop());
            systemTimerMock.Setup(stm => stm.Enabled).Returns(false);
            systemTimerMock.Setup(stm => stm.Interval).Returns(800);
            systemTimerMock.Setup(stm => stm.AutoReset).Returns(false);

            var lp66Instance = new LP66SendLastCashoutCreditAmountHandler(
                _doorServiceMock.Object,
                _playerInitiatedCashoutMock.Object,
                _systemDisableManagerMock.Object,
                _gamePlayStateMock.Object,
                _operatorMenuLauncherMock.Object,
                _disableByOperatorManagerMock.Object);

            lp66Instance.SetupTimer(systemTimerMock.Object);

            var theResponse = lp66Instance.Handle(new EftSendLastCashoutData { Acknowledgement = false });
            Assert.IsNotNull(theResponse);
            Assert.IsNull(theResponse.Handlers);
            Assert.AreEqual(mockAmount, theResponse.LastCashoutAmount);
            Assert.IsFalse(theResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.OperationSuccessful, theResponse.Status);
            systemTimerMock.Verify(stm => stm.Start(), Times.Once);
            systemTimerMock.Verify(stm => stm.Stop(), Times.Never);

            //verify all services
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Once);
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Once);
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Once);
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Once);
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Once);
        }

        [TestMethod]
        public void ShouldReturnInvalidAckWhenWaitingForHostAck()
        {
            //move state machine to Wiating for host ack state.
            var mockAmount = 10000ul;
            _doorServiceMock.Setup(ds => ds.GetDoorOpen(It.IsAny<int>())).Returns(false);
            _doorServiceMock.Setup(ds => ds.LogicalDoors).Returns(new Dictionary<int, LogicalDoor>());
            _playerInitiatedCashoutMock.Setup(em => em.GetCashoutAmount()).Returns(mockAmount);
            _systemDisableManagerMock.Setup(sdmm => sdmm.CurrentDisableKeys).Returns(new List<Guid>());
            _gamePlayStateMock.Setup(gps => gps.CurrentState).Returns(PlayState.Idle);
            _operatorMenuLauncherMock.Setup(gps => gps.IsShowing).Returns(false);
            _disableByOperatorManagerMock.Setup(a => a.DisabledByOperator).Returns(false);

            var systemTimerMock = new Mock<ISystemTimerWrapper>(MockBehavior.Strict);
            systemTimerMock.Setup(stm => stm.Start());
            systemTimerMock.Setup(stm => stm.Stop());
            systemTimerMock.Setup(stm => stm.Enabled).Returns(false);
            systemTimerMock.Setup(stm => stm.Interval).Returns(800);
            systemTimerMock.Setup(stm => stm.AutoReset).Returns(false);

            var lp66Instance = new LP66SendLastCashoutCreditAmountHandler(
                _doorServiceMock.Object,
                _playerInitiatedCashoutMock.Object,
                _systemDisableManagerMock.Object,
                _gamePlayStateMock.Object,
                _operatorMenuLauncherMock.Object,
                _disableByOperatorManagerMock.Object);

            lp66Instance.SetupTimer(systemTimerMock.Object);

            var firstResponse = lp66Instance.Handle(new EftSendLastCashoutData { Acknowledgement = false });

            Assert.IsNotNull(firstResponse);
            Assert.IsNull(firstResponse.Handlers);
            Assert.AreEqual(mockAmount, firstResponse.LastCashoutAmount);
            Assert.IsFalse(firstResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.OperationSuccessful, firstResponse.Status);

            systemTimerMock.Verify(stm => stm.Start(), Times.Once); //will stay as Idle state
            systemTimerMock.Verify(stm => stm.Stop(), Times.Never);

            //verify all services
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Once);
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Once);
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Once);
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Once);
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Once);

            //lp66Instance is in the Waiting For Host Ack state
            //should return the correct amount with Ack = false
            var secondResponse = lp66Instance.Handle(new EftSendLastCashoutData { Acknowledgement = false });
            Assert.IsNotNull(secondResponse);
            Assert.IsNull(secondResponse.Handlers);
            Assert.AreEqual(mockAmount, secondResponse.LastCashoutAmount);
            Assert.IsFalse(secondResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.InvalidAck, secondResponse.Status);

            //verify all services again
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Exactly(2));
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Once);
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Once);
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Once);
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Once);

            systemTimerMock.Verify(stm => stm.Start(), Times.Once);
            systemTimerMock.Verify(stm => stm.Stop(), Times.Once);
        }

        [TestMethod]
        public void ShouldGoBackToIdleWhenStateMachinesTimesOutInWaitingForHostAck()
        {
            //move state machine to Wiating for host ack state.
            var mockAmount = 10000ul;
            _doorServiceMock.Setup(ds => ds.GetDoorOpen(It.IsAny<int>())).Returns(false);
            _doorServiceMock.Setup(ds => ds.LogicalDoors).Returns(new Dictionary<int, LogicalDoor>());
            _playerInitiatedCashoutMock.Setup(em => em.GetCashoutAmount()).Returns(mockAmount);
            _systemDisableManagerMock.Setup(sdmm => sdmm.CurrentDisableKeys).Returns(new List<Guid>());
            _gamePlayStateMock.Setup(gps => gps.CurrentState).Returns(PlayState.Idle);
            _operatorMenuLauncherMock.Setup(gps => gps.IsShowing).Returns(false);
            _disableByOperatorManagerMock.Setup(a => a.DisabledByOperator).Returns(false);

            var systemTimerMock = new Mock<ISystemTimerWrapper>(MockBehavior.Strict);
            systemTimerMock.Setup(stm => stm.Start());
            systemTimerMock.Setup(stm => stm.Stop());
            systemTimerMock.Setup(stm => stm.Enabled).Returns(false);
            systemTimerMock.Setup(stm => stm.Interval).Returns(800);
            systemTimerMock.Setup(stm => stm.AutoReset).Returns(false);

            var lp66Instance = new LP66SendLastCashoutCreditAmountHandler(
                _doorServiceMock.Object,
                _playerInitiatedCashoutMock.Object,
                _systemDisableManagerMock.Object,
                _gamePlayStateMock.Object,
                _operatorMenuLauncherMock.Object,
                _disableByOperatorManagerMock.Object);

            lp66Instance.SetupTimer(systemTimerMock.Object);

            var firstResponse = lp66Instance.Handle(new EftSendLastCashoutData { Acknowledgement = false });

            Assert.IsNotNull(firstResponse);
            Assert.IsNull(firstResponse.Handlers);
            Assert.AreEqual(mockAmount, firstResponse.LastCashoutAmount);
            Assert.IsFalse(firstResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.OperationSuccessful, firstResponse.Status);

            //verify all services
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Once);
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Once);
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Once);
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Once);
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Once);

            systemTimerMock.Verify(stm => stm.Start(), Times.Once); //will stay as Idle state
            systemTimerMock.Verify(stm => stm.Stop(), Times.Never);

            systemTimerMock.Raise(x => x.Elapsed += null, new EventArgs() as ElapsedEventArgs);

            //lp66Instance is in Idle state,
            //it should not be able to process the message with Ack = true and get invalid ack
            var secondResponse = lp66Instance.Handle(new EftSendLastCashoutData { Acknowledgement = true });
            Assert.IsNotNull(secondResponse);
            Assert.IsNull(secondResponse.Handlers);
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Exactly(2));        //still Once
            Assert.AreEqual(mockAmount, secondResponse.LastCashoutAmount);
            Assert.IsTrue(secondResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.InvalidAck, secondResponse.Status);

            //verify all services again
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Exactly(2));
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Once);
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Once);
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Once);
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Once);

            systemTimerMock.Verify(stm => stm.Start(), Times.Once);
            systemTimerMock.Verify(stm => stm.Stop(), Times.Once);
        }

        [TestMethod]
        public void ShouldHaveImpliedAckWhenWaitingForImpliedAck()
        {
            //move state machine to Wiating for host ack state.
            var mockAmount = 98732ul;
            _doorServiceMock.Setup(ds => ds.GetDoorOpen(It.IsAny<int>())).Returns(false);
            _doorServiceMock.Setup(ds => ds.LogicalDoors).Returns(new Dictionary<int, LogicalDoor>());
            _playerInitiatedCashoutMock.Setup(em => em.GetCashoutAmount()).Returns(mockAmount);
            _systemDisableManagerMock.Setup(sdmm => sdmm.CurrentDisableKeys).Returns(new List<Guid>());
            _gamePlayStateMock.Setup(gps => gps.CurrentState).Returns(PlayState.Idle);
            _operatorMenuLauncherMock.Setup(gps => gps.IsShowing).Returns(false);
            _disableByOperatorManagerMock.Setup(a => a.DisabledByOperator).Returns(false);

            var systemTimerMock = new Mock<ISystemTimerWrapper>(MockBehavior.Strict);
            systemTimerMock.Setup(stm => stm.Start());
            systemTimerMock.Setup(stm => stm.Stop());
            systemTimerMock.Setup(stm => stm.Enabled).Returns(false);
            systemTimerMock.Setup(stm => stm.Interval).Returns(int.MaxValue);
            systemTimerMock.Setup(stm => stm.AutoReset).Returns(false);

            var lp66Instance = new LP66SendLastCashoutCreditAmountHandler(
                _doorServiceMock.Object,
                _playerInitiatedCashoutMock.Object,
                _systemDisableManagerMock.Object,
                _gamePlayStateMock.Object,
                _operatorMenuLauncherMock.Object,
                _disableByOperatorManagerMock.Object);

            lp66Instance.SetupTimer(systemTimerMock.Object);

            var firstResponse = lp66Instance.Handle(new EftSendLastCashoutData { Acknowledgement = false });
            Assert.IsNotNull(firstResponse);
            Assert.IsNull(firstResponse.Handlers);
            Assert.AreEqual(mockAmount, firstResponse.LastCashoutAmount);
            Assert.IsFalse(firstResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.OperationSuccessful, firstResponse.Status);

            //verify all service
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Once);
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Once);
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Once);
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Once);
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Once);

            systemTimerMock.Verify(stm => stm.Start(), Times.Once); //in WaitingForHostAck
            systemTimerMock.Verify(stm => stm.Stop(), Times.Never);

            var secondResponse = lp66Instance.Handle(new EftSendLastCashoutData { Acknowledgement = true });
            Assert.IsNotNull(secondResponse);
            Assert.AreEqual(mockAmount, secondResponse.LastCashoutAmount);
            Assert.IsTrue(secondResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.OperationSuccessful, firstResponse.Status);

            //verify ack and nack handler
            Assert.IsNotNull(secondResponse.Handlers);
            Assert.IsNotNull(secondResponse.Handlers.ImpliedAckHandler);
            Assert.IsNotNull(secondResponse.Handlers.ImpliedNackHandler);
            Assert.IsNotNull(secondResponse.Handlers.IntermediateNackHandler);

            //verify all service
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Exactly(2));
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Exactly(2));
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Exactly(2));
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Exactly(2));
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Exactly(2));

            systemTimerMock.Verify(stm => stm.Start(), Times.Exactly(2));   // in WaitingForImpliedAck
            systemTimerMock.Verify(stm => stm.Stop(), Times.Once);          // exit WaitingForHostAck
        }

        [TestMethod]
        public void ShouldCleaCashoutAmountByInvokingImpliedAckInWaitingForHostAck()
        {
            //move state machine to Wiating for host ack state.
            var mockAmount = 12345ul;
            _playerInitiatedCashoutMock.Setup(em => em.GetCashoutAmount()).Returns(mockAmount);
            _playerInitiatedCashoutMock.Setup(em => em.ClearCashoutAmount());

            _doorServiceMock.Setup(ds => ds.GetDoorOpen(It.IsAny<int>())).Returns(false);
            _doorServiceMock.Setup(ds => ds.LogicalDoors).Returns(new Dictionary<int, LogicalDoor>());
            _doorServiceMock.Setup(ds => ds.LogicalDoors).Returns(new Dictionary<int, LogicalDoor>());
            _systemDisableManagerMock.Setup(sdmm => sdmm.CurrentDisableKeys).Returns(new List<Guid>());
            _gamePlayStateMock.Setup(gps => gps.CurrentState).Returns(PlayState.Idle);
            _operatorMenuLauncherMock.Setup(gps => gps.IsShowing).Returns(false);
            _disableByOperatorManagerMock.Setup(a => a.DisabledByOperator).Returns(false);

            var systemTimerMock = new Mock<ISystemTimerWrapper>(MockBehavior.Strict);
            systemTimerMock.Setup(stm => stm.Start());
            systemTimerMock.Setup(stm => stm.Stop());
            systemTimerMock.Setup(stm => stm.Enabled).Returns(false);
            systemTimerMock.Setup(stm => stm.Interval).Returns(int.MaxValue);
            systemTimerMock.Setup(stm => stm.AutoReset).Returns(false);

            var lp66Instance = new LP66SendLastCashoutCreditAmountHandler(
                _doorServiceMock.Object,
                _playerInitiatedCashoutMock.Object,
                _systemDisableManagerMock.Object,
                _gamePlayStateMock.Object,
                _operatorMenuLauncherMock.Object,
                _disableByOperatorManagerMock.Object);

            lp66Instance.SetupTimer(systemTimerMock.Object);

            var firstResponse = lp66Instance.Handle(new EftSendLastCashoutData { Acknowledgement = false });
            Assert.IsNotNull(firstResponse);
            Assert.IsNull(firstResponse.Handlers);
            Assert.AreEqual(mockAmount, firstResponse.LastCashoutAmount);
            Assert.IsFalse(firstResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.OperationSuccessful, firstResponse.Status);

            _playerInitiatedCashoutMock.Verify(m => m.ClearCashoutAmount(), Times.Never);

            //verify all service
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Once);
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Once);
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Once);
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Once);
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Once);

            systemTimerMock.Verify(stm => stm.Start(), Times.Once); //in WaitingForHostAck
            systemTimerMock.Verify(stm => stm.Stop(), Times.Never);

            var secondResponse = lp66Instance.Handle(new EftSendLastCashoutData { Acknowledgement = true });
            Assert.IsNotNull(secondResponse);
            Assert.AreEqual(mockAmount, secondResponse.LastCashoutAmount);
            Assert.IsTrue(secondResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.OperationSuccessful, secondResponse.Status);

            //verify ack and nack handler
            Assert.IsNotNull(secondResponse.Handlers);
            Assert.IsNotNull(secondResponse.Handlers.ImpliedAckHandler);
            Assert.IsNotNull(secondResponse.Handlers.ImpliedNackHandler);
            Assert.IsNotNull(secondResponse.Handlers.IntermediateNackHandler);

            //verify all service
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Exactly(2));
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Exactly(2));
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Exactly(2));
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Exactly(2));
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Exactly(2));

            systemTimerMock.Verify(stm => stm.Start(), Times.Exactly(2));   // in WaitingForImpliedAck
            systemTimerMock.Verify(stm => stm.Stop(), Times.Once);          // exit WaitingForHostAck

            secondResponse.Handlers.ImpliedAckHandler();
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Exactly(2));
            _playerInitiatedCashoutMock.Verify(em => em.ClearCashoutAmount(), Times.Once);
        }

        [TestMethod]
        public void ShouldClearCashoutAmountByTimeoutInWaitingForHostAck()
        {
            //move state machine to Wiating for host ack state.
            var mockAmount = 4312ul;
            _playerInitiatedCashoutMock.Setup(em => em.GetCashoutAmount()).Returns(mockAmount);
            _playerInitiatedCashoutMock.Setup(em => em.ClearCashoutAmount());

            _doorServiceMock.Setup(ds => ds.GetDoorOpen(It.IsAny<int>())).Returns(false);
            _doorServiceMock.Setup(ds => ds.LogicalDoors).Returns(new Dictionary<int, LogicalDoor>());
            _systemDisableManagerMock.Setup(sdmm => sdmm.CurrentDisableKeys).Returns(new List<Guid>());
            _gamePlayStateMock.Setup(gps => gps.CurrentState).Returns(PlayState.Idle);
            _operatorMenuLauncherMock.Setup(gps => gps.IsShowing).Returns(false);
            _disableByOperatorManagerMock.Setup(a => a.DisabledByOperator).Returns(false);

            var systemTimerMock = new Mock<ISystemTimerWrapper>(MockBehavior.Strict);
            systemTimerMock.Setup(stm => stm.Start());
            systemTimerMock.Setup(stm => stm.Stop());
            systemTimerMock.Setup(stm => stm.Enabled).Returns(false);
            systemTimerMock.Setup(stm => stm.Interval).Returns(int.MaxValue);
            systemTimerMock.Setup(stm => stm.AutoReset).Returns(false);

            var lp66Instance = new LP66SendLastCashoutCreditAmountHandler(
                _doorServiceMock.Object,
                _playerInitiatedCashoutMock.Object,
                _systemDisableManagerMock.Object,
                _gamePlayStateMock.Object,
                _operatorMenuLauncherMock.Object,
                _disableByOperatorManagerMock.Object);

            lp66Instance.SetupTimer(systemTimerMock.Object);

            var firstResponse = lp66Instance.Handle(new EftSendLastCashoutData { Acknowledgement = false });
            Assert.IsNotNull(firstResponse);
            Assert.IsNull(firstResponse.Handlers);
            Assert.AreEqual(mockAmount, firstResponse.LastCashoutAmount);
            Assert.IsFalse(firstResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.OperationSuccessful, firstResponse.Status);

            _playerInitiatedCashoutMock.Verify(em => em.ClearCashoutAmount(), Times.Never);

            //verify all service
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Once);
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Once);
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Once);
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Once);
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Once);

            systemTimerMock.Verify(stm => stm.Start(), Times.Once); //in WaitingForHostAck
            systemTimerMock.Verify(stm => stm.Stop(), Times.Never);

            var secondResponse = lp66Instance.Handle(new EftSendLastCashoutData { Acknowledgement = true });
            Assert.IsNotNull(secondResponse);
            Assert.AreEqual(mockAmount, secondResponse.LastCashoutAmount);
            Assert.IsTrue(secondResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.OperationSuccessful, secondResponse.Status);

            //verify ack and nack handler
            Assert.IsNotNull(secondResponse.Handlers);
            Assert.IsNotNull(secondResponse.Handlers.ImpliedAckHandler);
            Assert.IsNotNull(secondResponse.Handlers.ImpliedNackHandler);
            Assert.IsNotNull(secondResponse.Handlers.IntermediateNackHandler);

            //verify all service
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Exactly(2));
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Exactly(2));
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Exactly(2));
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Exactly(2));
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Exactly(2));

            systemTimerMock.Verify(stm => stm.Start(), Times.Exactly(2));   // in WaitingForImpliedAck
            systemTimerMock.Verify(stm => stm.Stop(), Times.Once);          // exit WaitingForHostAck

            systemTimerMock.Raise(x => x.Elapsed += null, new EventArgs() as ElapsedEventArgs);
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Exactly(2));
            _playerInitiatedCashoutMock.Verify(em => em.ClearCashoutAmount(), Times.Once);
        }

        [TestMethod]
        public void ShouldRestartTimerForReceivingSameCommandInWaitingForHostAck()
        {
            //move state machine to Wiating for host ack state.
            var mockAmount = 7658ul;
            _playerInitiatedCashoutMock.Setup(em => em.GetCashoutAmount()).Returns(mockAmount);

            _doorServiceMock.Setup(ds => ds.GetDoorOpen(It.IsAny<int>())).Returns(false);
            _doorServiceMock.Setup(ds => ds.LogicalDoors).Returns(new Dictionary<int, LogicalDoor>());
            _systemDisableManagerMock.Setup(sdmm => sdmm.CurrentDisableKeys).Returns(new List<Guid>());
            _gamePlayStateMock.Setup(gps => gps.CurrentState).Returns(PlayState.Idle);
            _operatorMenuLauncherMock.Setup(gps => gps.IsShowing).Returns(false);
            _disableByOperatorManagerMock.Setup(a => a.DisabledByOperator).Returns(false);

            var systemTimerMock = new Mock<ISystemTimerWrapper>(MockBehavior.Strict);
            systemTimerMock.Setup(stm => stm.Start());
            systemTimerMock.Setup(stm => stm.Stop());
            systemTimerMock.Setup(stm => stm.Enabled).Returns(false);
            systemTimerMock.Setup(stm => stm.Interval).Returns(int.MaxValue);
            systemTimerMock.Setup(stm => stm.AutoReset).Returns(false);

            var lp66Instance = new LP66SendLastCashoutCreditAmountHandler(
                _doorServiceMock.Object,
                _playerInitiatedCashoutMock.Object,
                _systemDisableManagerMock.Object,
                _gamePlayStateMock.Object,
                _operatorMenuLauncherMock.Object,
                _disableByOperatorManagerMock.Object);

            lp66Instance.SetupTimer(systemTimerMock.Object);

            var firstResponse = lp66Instance.Handle(new EftSendLastCashoutData { Acknowledgement = false });
            Assert.IsNotNull(firstResponse);
            Assert.IsNull(firstResponse.Handlers);
            Assert.AreEqual(mockAmount, firstResponse.LastCashoutAmount);
            Assert.IsFalse(firstResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.OperationSuccessful, firstResponse.Status);

            //verify all service
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Once);
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Once);
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Once);
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Once);
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Once);

            systemTimerMock.Verify(stm => stm.Start(), Times.Once);
            systemTimerMock.Verify(stm => stm.Stop(), Times.Never);

            //LP66 Handler is in WaitingForHostAck now
            var secondResponse = lp66Instance.Handle(new EftSendLastCashoutData { Acknowledgement = true });
            Assert.IsNotNull(secondResponse);
            Assert.AreEqual(mockAmount, secondResponse.LastCashoutAmount);
            Assert.IsTrue(secondResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.OperationSuccessful, secondResponse.Status);

            //verify ack and nack handler
            Assert.IsNotNull(secondResponse.Handlers);
            Assert.IsNotNull(secondResponse.Handlers.ImpliedAckHandler);
            Assert.IsNotNull(secondResponse.Handlers.ImpliedNackHandler);
            Assert.IsNotNull(secondResponse.Handlers.IntermediateNackHandler);

            //verify all service
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Exactly(2));
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Exactly(2));
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Exactly(2));
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Exactly(2));
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Exactly(2));

            systemTimerMock.Verify(stm => stm.Start(), Times.Exactly(2));   // enter WaitingForImpliedAck
            systemTimerMock.Verify(stm => stm.Stop(), Times.Once);          // exit WaitingForHostAck

            //LP66 Handler is in WaitingForImpliedAck now
            var thirdResponse = lp66Instance.Handle(new EftSendLastCashoutData { Acknowledgement = true });
            Assert.IsNotNull(thirdResponse);
            Assert.AreEqual(mockAmount, thirdResponse.LastCashoutAmount);
            Assert.IsTrue(thirdResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.OperationSuccessful, thirdResponse.Status);

            //verify ack and nack handler
            Assert.IsNotNull(thirdResponse.Handlers);
            Assert.IsNotNull(thirdResponse.Handlers.ImpliedAckHandler);
            Assert.IsNotNull(thirdResponse.Handlers.ImpliedNackHandler);
            Assert.IsNotNull(thirdResponse.Handlers.IntermediateNackHandler);

            //verify all service
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Exactly(3));
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Exactly(3));
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Exactly(3));
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Exactly(3));
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Exactly(3));

            systemTimerMock.Verify(stm => stm.Start(), Times.Exactly(3));   // enter WaitingForImpliedAck
            systemTimerMock.Verify(stm => stm.Stop(), Times.Exactly(2));    // exit WaitingForImpliedAck

            //LP66 Handler is in WaitingForImpliedAck again
            var fourthResponse = lp66Instance.Handle(new EftSendLastCashoutData { Acknowledgement = true });
            Assert.IsNotNull(fourthResponse);
            Assert.AreEqual(mockAmount, fourthResponse.LastCashoutAmount);
            Assert.IsTrue(fourthResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.OperationSuccessful, fourthResponse.Status);

            //verify ack and nack handler
            Assert.IsNotNull(fourthResponse.Handlers);
            Assert.IsNotNull(fourthResponse.Handlers.ImpliedAckHandler);
            Assert.IsNotNull(fourthResponse.Handlers.ImpliedNackHandler);
            Assert.IsNotNull(fourthResponse.Handlers.IntermediateNackHandler);

            //verify all service
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Exactly(4));
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Exactly(4));
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Exactly(4));
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Exactly(4));
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Exactly(4));

            systemTimerMock.Verify(stm => stm.Start(), Times.Exactly(4));   // enter WaitingForImpliedAck
            systemTimerMock.Verify(stm => stm.Stop(), Times.Exactly(3));    // exit WaitingForImpliedAck
        }

        [TestMethod]
        public void ShouldRestartTimerIfReAckRequestedInWaitingForImpliedAck()
        {
            //move state machine to Wiating for host ack state.
            var mockAmount = 7658ul;
            _playerInitiatedCashoutMock.Setup(em => em.GetCashoutAmount()).Returns(mockAmount);

            _doorServiceMock.Setup(ds => ds.GetDoorOpen(It.IsAny<int>())).Returns(false);
            _doorServiceMock.Setup(ds => ds.LogicalDoors).Returns(new Dictionary<int, LogicalDoor>());
            _systemDisableManagerMock.Setup(sdmm => sdmm.CurrentDisableKeys).Returns(new List<Guid>());
            _gamePlayStateMock.Setup(gps => gps.CurrentState).Returns(PlayState.Idle);
            _operatorMenuLauncherMock.Setup(gps => gps.IsShowing).Returns(false);
            _disableByOperatorManagerMock.Setup(a => a.DisabledByOperator).Returns(false);

            var systemTimerMock = new Mock<ISystemTimerWrapper>(MockBehavior.Strict);
            systemTimerMock.Setup(stm => stm.Start());
            systemTimerMock.Setup(stm => stm.Stop());
            systemTimerMock.Setup(stm => stm.Enabled).Returns(false);
            systemTimerMock.Setup(stm => stm.Interval).Returns(int.MaxValue);
            systemTimerMock.Setup(stm => stm.AutoReset).Returns(false);

            var lp66Instance = new LP66SendLastCashoutCreditAmountHandler(
                _doorServiceMock.Object,
                _playerInitiatedCashoutMock.Object,
                _systemDisableManagerMock.Object,
                _gamePlayStateMock.Object,
                _operatorMenuLauncherMock.Object,
                _disableByOperatorManagerMock.Object);

            lp66Instance.SetupTimer(systemTimerMock.Object);

            var firstResponse = lp66Instance.Handle(new EftSendLastCashoutData { Acknowledgement = false });
            Assert.IsNotNull(firstResponse);
            Assert.IsNull(firstResponse.Handlers);
            Assert.AreEqual(mockAmount, firstResponse.LastCashoutAmount);
            Assert.IsFalse(firstResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.OperationSuccessful, firstResponse.Status);

            //verify all service
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Once);
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Once);
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Once);
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Once);
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Once);

            systemTimerMock.Verify(stm => stm.Start(), Times.Once);
            systemTimerMock.Verify(stm => stm.Stop(), Times.Never);

            //LP66 Handler is in WaitingForHostAck now
            var secondResponse = lp66Instance.Handle(new EftSendLastCashoutData { Acknowledgement = true });
            Assert.IsNotNull(secondResponse);
            Assert.AreEqual(mockAmount, secondResponse.LastCashoutAmount);
            Assert.IsTrue(secondResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.OperationSuccessful, secondResponse.Status);

            //verify ack and nack handler
            Assert.IsNotNull(secondResponse.Handlers);
            Assert.IsNotNull(secondResponse.Handlers.ImpliedAckHandler);
            Assert.IsNotNull(secondResponse.Handlers.ImpliedNackHandler);
            Assert.IsNotNull(secondResponse.Handlers.IntermediateNackHandler);

            //verify all service
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Exactly(2));
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Exactly(2));
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Exactly(2));
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Exactly(2));
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Exactly(2));

            systemTimerMock.Verify(stm => stm.Start(), Times.Exactly(2));   // enter WaitingForImpliedAck
            systemTimerMock.Verify(stm => stm.Stop(), Times.Once);          // exit WaitingForHostAck

            secondResponse.Handlers.IntermediateNackHandler.Invoke();

            //verify time start and stop again
            systemTimerMock.Verify(stm => stm.Start(), Times.Exactly(3));   // enter WaitingForImpliedAck
            systemTimerMock.Verify(stm => stm.Stop(), Times.Exactly(2));    // exit WaitingForImpliedAck
        }

        [TestMethod]
        public void ShouldRestartTimerIfReAckRequestedAfterMultipleAckMsgInWaitingForImpliedAck()
        {
            //move state machine to Wiating for host ack state.
            var mockAmount = 8558ul;
            _playerInitiatedCashoutMock.Setup(em => em.GetCashoutAmount()).Returns(mockAmount);

            _doorServiceMock.Setup(ds => ds.GetDoorOpen(It.IsAny<int>())).Returns(false);
            _doorServiceMock.Setup(ds => ds.LogicalDoors).Returns(new Dictionary<int, LogicalDoor>());
            _systemDisableManagerMock.Setup(sdmm => sdmm.CurrentDisableKeys).Returns(new List<Guid>());
            _gamePlayStateMock.Setup(gps => gps.CurrentState).Returns(PlayState.Idle);
            _operatorMenuLauncherMock.Setup(gps => gps.IsShowing).Returns(false);
            _disableByOperatorManagerMock.Setup(a => a.DisabledByOperator).Returns(false);

            var systemTimerMock = new Mock<ISystemTimerWrapper>(MockBehavior.Strict);
            systemTimerMock.Setup(stm => stm.Start());
            systemTimerMock.Setup(stm => stm.Stop());
            systemTimerMock.Setup(stm => stm.Enabled).Returns(false);
            systemTimerMock.Setup(stm => stm.Interval).Returns(int.MaxValue);
            systemTimerMock.Setup(stm => stm.AutoReset).Returns(false);

            var lp66Instance = new LP66SendLastCashoutCreditAmountHandler(
                _doorServiceMock.Object,
                _playerInitiatedCashoutMock.Object,
                _systemDisableManagerMock.Object,
                _gamePlayStateMock.Object,
                _operatorMenuLauncherMock.Object,
                _disableByOperatorManagerMock.Object);

            lp66Instance.SetupTimer(systemTimerMock.Object);

            var firstResponse = lp66Instance.Handle(new EftSendLastCashoutData { Acknowledgement = false });
            Assert.IsNotNull(firstResponse);
            Assert.IsNull(firstResponse.Handlers);
            Assert.AreEqual(mockAmount, firstResponse.LastCashoutAmount);
            Assert.IsFalse(firstResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.OperationSuccessful, firstResponse.Status);

            //verify all service
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Once);
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Once);
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Once);
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Once);
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Once);

            systemTimerMock.Verify(stm => stm.Start(), Times.Once);
            systemTimerMock.Verify(stm => stm.Stop(), Times.Never);

            //LP66 Handler is in WaitingForHostAck now
            var secondResponse = lp66Instance.Handle(new EftSendLastCashoutData { Acknowledgement = true });
            Assert.IsNotNull(secondResponse);
            Assert.AreEqual(mockAmount, secondResponse.LastCashoutAmount);
            Assert.IsTrue(secondResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.OperationSuccessful, secondResponse.Status);

            //verify ack and nack handler
            Assert.IsNotNull(secondResponse.Handlers);
            Assert.IsNotNull(secondResponse.Handlers.ImpliedAckHandler);
            Assert.IsNotNull(secondResponse.Handlers.ImpliedNackHandler);
            Assert.IsNotNull(secondResponse.Handlers.IntermediateNackHandler);

            //verify all service
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Exactly(2));
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Exactly(2));
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Exactly(2));
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Exactly(2));
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Exactly(2));

            systemTimerMock.Verify(stm => stm.Start(), Times.Exactly(2));   // enter WaitingForImpliedAck
            systemTimerMock.Verify(stm => stm.Stop(), Times.Once);          // exit WaitingForHostAck

            //LP66 Handler is in WaitingForImpliedAck now
            var thirdResponse = lp66Instance.Handle(new EftSendLastCashoutData { Acknowledgement = true });
            Assert.IsNotNull(thirdResponse);
            Assert.AreEqual(mockAmount, thirdResponse.LastCashoutAmount);
            Assert.IsTrue(thirdResponse.Acknowledgement);
            Assert.AreEqual(TransactionStatus.OperationSuccessful, thirdResponse.Status);

            //verify ack and nack handler
            Assert.IsNotNull(thirdResponse.Handlers);
            Assert.IsNotNull(thirdResponse.Handlers.ImpliedAckHandler);
            Assert.IsNotNull(thirdResponse.Handlers.ImpliedNackHandler);
            Assert.IsNotNull(thirdResponse.Handlers.IntermediateNackHandler);

            //verify all service
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Exactly(3));
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Exactly(3));
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Exactly(3));
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Exactly(3));
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Exactly(3));

            systemTimerMock.Verify(stm => stm.Start(), Times.Exactly(3));   // enter WaitingForImpliedAck
            systemTimerMock.Verify(stm => stm.Stop(), Times.Exactly(2));    // exit WaitingForImpliedAck

            thirdResponse.Handlers.IntermediateNackHandler.Invoke();

            //verify all service
            _playerInitiatedCashoutMock.Verify(em => em.GetCashoutAmount(), Times.Exactly(3));
            _systemDisableManagerMock.Verify(m => m.CurrentDisableKeys, Times.Exactly(3));
            _gamePlayStateMock.Verify(m => m.CurrentState, Times.Exactly(3));
            _operatorMenuLauncherMock.Verify(m => m.IsShowing, Times.Exactly(3));
            _disableByOperatorManagerMock.Verify(m => m.DisabledByOperator, Times.Exactly(3));

            systemTimerMock.Verify(stm => stm.Start(), Times.Exactly(4));   // enter WaitingForImpliedAck
            systemTimerMock.Verify(stm => stm.Stop(), Times.Exactly(3));    // exit WaitingForImpliedAck
        }
    }
}