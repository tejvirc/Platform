namespace Aristocrat.Monaco.Sas.Tests.Eft
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Timers;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.Eft;
    using Common;
    using Contracts.Eft;
    using Gaming.Contracts;
    using Hardware.Contracts.Door;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Eft;
    using Test.Common;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     Contains unit tests for the EftStateController class.
    /// </summary>
    [TestClass]
    public class EftStateControllerTests
    {
        private Mock<ISystemDisableManager> _disableProvider;
        private Mock<IDoorService> _doorService;
        private Mock<IGamePlayState> _gamePlayState;
        private Mock<IDisableByOperatorManager> _disableByOperator;
        private Mock<IOperatorMenuLauncher> _operatorMenu;
        private Mock<IEftTransferProvider> _eftTransferProvider;
        private Mock<IEftHistoryLogProvider> _historyLogProvider;

        private Mock<ISystemTimerWrapper> _timer;
        private EftStateController _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            _disableProvider = new Mock<ISystemDisableManager>(MockBehavior.Strict);
            _doorService = new Mock<IDoorService>(MockBehavior.Strict);
            _gamePlayState = new Mock<IGamePlayState>(MockBehavior.Strict);
            _disableByOperator = new Mock<IDisableByOperatorManager>(MockBehavior.Strict);
            _operatorMenu = new Mock<IOperatorMenuLauncher>(MockBehavior.Strict);
            _eftTransferProvider = new Mock<IEftTransferProvider>(MockBehavior.Strict);
            _historyLogProvider = new Mock<IEftHistoryLogProvider>(MockBehavior.Strict);
            _timer = new Mock<ISystemTimerWrapper>(MockBehavior.Strict);
            _target = ConstructController(false, false, false, false, false, false, false);
        }

        [DataRow(true, false, false, false, false, false, false, DisplayName = "Null ISystemDisableManager")]
        [DataRow(false, true, false, false, false, false, false, DisplayName = "Null IDoorService")]
        [DataRow(false, false, true, false, false, false, false, DisplayName = "Null IGamePlayState")]
        [DataRow(false, false, false, true, false, false, false, DisplayName = "Null IOperatorManager")]
        [DataRow(false, false, false, false, true, false, false, DisplayName = "Null IOperatorMenu")]
        [DataRow(false, false, false, false, false, true, false, DisplayName = "Null TransferProvider")]
        [DataRow(false, false, false, false, false, false, true, DisplayName = "Null HistoryLog")]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void InitializeWithNullArgumentExpectException(
            bool nullDisableManager,
            bool nullDoorService,
            bool nullGamePlayState,
            bool nullDisableByOperatorManager,
            bool nullOperatorMenuLauncher,
            bool nullEftTransferProvider,
            bool nullEftHistoryLogProvider)
        {
            _target = ConstructController(
                nullDisableManager,
                nullDoorService,
                nullGamePlayState,
                nullDisableByOperatorManager,
                nullOperatorMenuLauncher,
                nullEftTransferProvider,
                nullEftHistoryLogProvider);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _timer.Setup(m => m.Dispose());
            _target.Dispose();

            //testing disposing again
            _target.Dispose();
        }

        [TestMethod]
        public void TestCorrectPhaseOneActionTypeDLongPoll()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            var initialTransferData = ComposeTransferData(
                LongPoll.EftTransferPromotionalCreditsToMachine,
                "01",
                false,
                50);

            var testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Once);
            _timer.Verify(m => m.Stop(), Times.Never);
            handler.Verify(m => m.CheckTransferAmount(It.IsAny<ulong>()), Times.Once());
            _disableProvider.Verify(
                m => m.Disable(
                    It.IsAny<Guid>(),
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    false,
                    null),
                Times.Once());
            var expectedResponse = ComposeResponseData();
            AssertIfResponseIsEqual(testResponse, expectedResponse);
        }

        [TestMethod]
        public void TestCheckTransferAmountWithAmountExceeded()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(20);
            var initialTransferData = ComposeTransferData(
                LongPoll.EftTransferPromotionalCreditsToMachine,
                "01",
                false,
                50);
            handler.Setup(m => m.CheckTransferAmount(It.IsAny<ulong>()))
                .Returns((20, true));
            var testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Once);
            _timer.Verify(m => m.Stop(), Times.Never);
            _disableProvider.Verify(
                m => m.Disable(
                    It.IsAny<Guid>(),
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    false,
                    null),
                Times.Once());
            var expectedResponse = ComposeResponseData("01", false, 20, TransactionStatus.TransferAmountExceeded);
            AssertIfResponseIsEqual(testResponse, expectedResponse);
        }

        [TestMethod]
        public void TestCorrectPhaseTwoActionTypeDLongPoll()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            var initialTransferData = ComposeTransferData(
                LongPoll.EftTransferPromotionalCreditsToMachine,
                "01",
                false,
                50);

            var testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Once);
            _timer.Verify(m => m.Stop(), Times.Never);
            handler.Verify(m => m.CheckTransferAmount(It.IsAny<ulong>()), Times.Once());
            _disableProvider.Verify(
                m => m.Disable(
                    It.IsAny<Guid>(),
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    false,
                    null),
                Times.Once());
            handler.Verify(m => m.ProcessTransfer(It.IsAny<ulong>(), It.IsAny<int>()), Times.Never());
            var expectedResponse = ComposeResponseData();
            AssertIfResponseIsEqual(testResponse, expectedResponse);

            MockHistoryProviderToReturnLog(
                LongPoll.EftTransferCashableCreditsToMachine,
                initialTransferData,
                testResponse);
            var ackTransferData = ComposeTransferData(LongPoll.EftTransferCashableCreditsToMachine, "01", true, 50);

            testResponse = eftStateController.Handle(ackTransferData, handler.Object);
            testResponse.Handlers.ImpliedAckHandler.Invoke();

            _timer.Verify(m => m.Start(), Times.Exactly(2));
            _timer.Verify(m => m.Stop(), Times.Exactly(3));
            handler.Verify(m => m.ProcessTransfer(It.IsAny<ulong>(), It.IsAny<int>()), Times.Once());
            _disableProvider.Verify(m => m.Enable(It.IsAny<Guid>()), Times.Once());
            expectedResponse = ComposeResponseData("01", true);
            AssertIfResponseIsEqual(testResponse, expectedResponse);
        }

        [TestMethod]
        public void TestCorrectPhaseOneActionTypeULongPoll()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            var initialTransferData = ComposeTransferData(
                LongPoll.EftTransferCashAndNonCashableCreditsToHost,
                "01",
                false,
                0);

            var testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Once);
            _timer.Verify(m => m.Stop(), Times.Never);
            handler.Verify(m => m.CheckTransferAmount(It.IsAny<ulong>()), Times.Once());
            _disableProvider.Verify(
                m => m.Disable(
                    It.IsAny<Guid>(),
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    false,
                    null),
                Times.Once());
            var expectedResponse = ComposeResponseData();
            AssertIfResponseIsEqual(testResponse, expectedResponse);
        }

        [TestMethod]
        public void TestCorrectPhaseTwoActionTypeULongPoll()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            var initialTransferData = ComposeTransferData(
                LongPoll.EftTransferCashAndNonCashableCreditsToHost,
                "01",
                false,
                0);

            var testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Once);
            _timer.Verify(m => m.Stop(), Times.Never);
            handler.Verify(m => m.CheckTransferAmount(It.IsAny<ulong>()), Times.Once());
            _disableProvider.Verify(
                m => m.Disable(
                    It.IsAny<Guid>(),
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    false,
                    null),
                Times.Once());
            var expectedResponse = ComposeResponseData();
            AssertIfResponseIsEqual(testResponse, expectedResponse);

            MockHistoryProviderToReturnLog(
                LongPoll.EftTransferCashAndNonCashableCreditsToHost,
                initialTransferData,
                testResponse);
            var ackTransferData = ComposeTransferData(
                LongPoll.EftTransferCashAndNonCashableCreditsToHost,
                "01",
                true,
                0);

            testResponse = eftStateController.Handle(ackTransferData, handler.Object);
            testResponse.Handlers.ImpliedAckHandler.Invoke();

            _timer.Verify(m => m.Start(), Times.Exactly(2));
            _timer.Verify(m => m.Stop(), Times.Exactly(3));
            handler.Verify(m => m.ProcessTransfer(It.IsAny<ulong>(), It.IsAny<int>()), Times.Once());
            _disableProvider.Verify(m => m.Enable(It.IsAny<Guid>()), Times.Once());
            expectedResponse = ComposeResponseData("01", true);
            AssertIfResponseIsEqual(testResponse, expectedResponse);
        }

        [TestMethod]
        public void TestTenConsecutiveTypeDLongPollRequests()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            EftTransferData initialTransferData, ackTransferData;
            EftTransactionResponse expectedResponse;
            EftTransactionResponse testResponse;

            for (var i = 1; i < 11; i++)
            {
                initialTransferData = ComposeTransferData(
                    LongPoll.EftTransferPromotionalCreditsToMachine,
                    i.ToString(),
                    false,
                    50);

                testResponse = eftStateController.Handle(initialTransferData, handler.Object);

                _disableProvider.Verify(
                    m => m.Disable(
                        It.IsAny<Guid>(),
                        SystemDisablePriority.Immediate,
                        It.IsAny<Func<string>>(),
                        false,
                        null),
                    Times.Exactly(i));
                expectedResponse = ComposeResponseData(i.ToString());
                AssertIfResponseIsEqual(testResponse, expectedResponse);

                MockHistoryProviderToReturnLog(
                    LongPoll.EftTransferCashableCreditsToMachine,
                    initialTransferData,
                    testResponse);
                ackTransferData = ComposeTransferData(
                    LongPoll.EftTransferCashableCreditsToMachine,
                    i.ToString(),
                    true,
                    50);

                testResponse = eftStateController.Handle(ackTransferData, handler.Object);
                testResponse.Handlers.ImpliedAckHandler.Invoke();

                _timer.Verify(m => m.Start(), Times.Exactly(i * 2));
                _timer.Verify(m => m.Stop(), Times.Exactly(i * 3));
                handler.Verify(m => m.ProcessTransfer(It.IsAny<ulong>(), It.IsAny<int>()), Times.Exactly(i));
                _disableProvider.Verify(m => m.Enable(It.IsAny<Guid>()), Times.Exactly(i));
                expectedResponse = ComposeResponseData(i.ToString(), true);
                AssertIfResponseIsEqual(testResponse, expectedResponse);
            }
        }

        [TestMethod]
        public void TestTenConsecutiveTypeULongPollRequests()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            EftTransferData initialTransferData, ackTransferData;
            EftTransactionResponse expectedResponse;
            EftTransactionResponse testResponse;

            for (var i = 1; i < 11; i++)
            {
                initialTransferData = ComposeTransferData(
                    LongPoll.EftTransferCashAndNonCashableCreditsToHost,
                    i.ToString(),
                    false,
                    50);

                testResponse = eftStateController.Handle(initialTransferData, handler.Object);
                _disableProvider.Verify(
                    m => m.Disable(
                        It.IsAny<Guid>(),
                        SystemDisablePriority.Immediate,
                        It.IsAny<Func<string>>(),
                        false,
                        null),
                    Times.Exactly(i));
                expectedResponse = ComposeResponseData(i.ToString());
                AssertIfResponseIsEqual(testResponse, expectedResponse);

                MockHistoryProviderToReturnLog(
                    LongPoll.EftTransferCashAndNonCashableCreditsToHost,
                    initialTransferData,
                    testResponse);
                ackTransferData = ComposeTransferData(
                    LongPoll.EftTransferCashAndNonCashableCreditsToHost,
                    i.ToString(),
                    true,
                    50);

                testResponse = eftStateController.Handle(ackTransferData, handler.Object);
                testResponse.Handlers.ImpliedAckHandler.Invoke();

                _timer.Verify(m => m.Start(), Times.Exactly(i * 2));
                _timer.Verify(m => m.Stop(), Times.Exactly(i * 3));
                handler.Verify(m => m.ProcessTransfer(It.IsAny<ulong>(), It.IsAny<int>()), Times.Exactly(i));
                _disableProvider.Verify(m => m.Enable(It.IsAny<Guid>()), Times.Exactly(i));
                expectedResponse = ComposeResponseData(i.ToString(), true);
                AssertIfResponseIsEqual(testResponse, expectedResponse);
            }
        }

        [TestMethod]
        public void TestCorrectPhaseOneTimeOutReset()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            var initialTransferData = ComposeTransferData(
                LongPoll.EftTransferPromotionalCreditsToMachine,
                "01",
                false,
                50);

            var testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            handler.Verify(m => m.CheckTransferAmount(It.IsAny<ulong>()), Times.Once());
            _disableProvider.Verify(
                m => m.Disable(
                    It.IsAny<Guid>(),
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    false,
                    null),
                Times.Once());
            var expectedResponse = ComposeResponseData();
            AssertIfResponseIsEqual(testResponse, expectedResponse);

            _timer.Verify(m => m.Start(), Times.Once);
            _timer.Verify(m => m.Stop(), Times.Never);
            _timer.Raise(x => x.Elapsed += null, new EventArgs() as ElapsedEventArgs);

            _disableProvider.Verify(m => m.Enable(It.IsAny<Guid>()), Times.Once());
            var ackTransferData = ComposeTransferData(LongPoll.EftTransferPromotionalCreditsToMachine, "01", true, 50);
            testResponse = eftStateController.Handle(ackTransferData, handler.Object);
            expectedResponse = ComposeResponseData("01", false, 50, TransactionStatus.InvalidAck);
            AssertIfResponseIsEqual(testResponse, expectedResponse);
        }

        [TestMethod]
        public void TestCorrectPhaseTwoTimeOutExecution()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            var initialTransferData = ComposeTransferData(
                LongPoll.EftTransferPromotionalCreditsToMachine,
                "01",
                false,
                50);

            var testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Once);
            _timer.Verify(m => m.Stop(), Times.Never);
            handler.Verify(m => m.CheckTransferAmount(It.IsAny<ulong>()), Times.Once());
            _disableProvider.Verify(
                m => m.Disable(
                    It.IsAny<Guid>(),
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    false,
                    null),
                Times.Once());
            var expectedResponse = ComposeResponseData();
            AssertIfResponseIsEqual(testResponse, expectedResponse);

            MockHistoryProviderToReturnLog(
                LongPoll.EftTransferCashableCreditsToMachine,
                initialTransferData,
                testResponse);
            var ackTransferData = ComposeTransferData(LongPoll.EftTransferCashableCreditsToMachine, "01", true, 50);

            testResponse = eftStateController.Handle(ackTransferData, handler.Object);
            _timer.Raise(x => x.Elapsed += null, new EventArgs() as ElapsedEventArgs);

            _timer.Verify(m => m.Start(), Times.Exactly(2));
            _timer.Verify(m => m.Stop(), Times.AtLeast(2));
            handler.Verify(m => m.ProcessTransfer(It.IsAny<ulong>(), It.IsAny<int>()), Times.Once());
            _disableProvider.Verify(m => m.Enable(It.IsAny<Guid>()), Times.Once());
            expectedResponse = ComposeResponseData("01", true);
            AssertIfResponseIsEqual(testResponse, expectedResponse);
        }

        [TestMethod]
        public void TestWrongAckInIdle()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            var initialTransferData = ComposeTransferData(
                LongPoll.EftTransferPromotionalCreditsToMachine,
                "01",
                true,
                50);

            var testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Never);
            _timer.Verify(m => m.Stop(), Times.Never);
            var expectedResponse = ComposeResponseData("01", false, 50, TransactionStatus.InvalidAck);
            AssertIfResponseIsEqual(testResponse, expectedResponse);
        }

        [TestMethod]
        public void TestWrongAckInPhaseOne()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            var initialTransferData = ComposeTransferData(
                LongPoll.EftTransferPromotionalCreditsToMachine,
                "01",
                false,
                50);

            var testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Once);
            _timer.Verify(m => m.Stop(), Times.Never);
            handler.Verify(m => m.CheckTransferAmount(It.IsAny<ulong>()), Times.Once());
            _disableProvider.Verify(
                m => m.Disable(
                    It.IsAny<Guid>(),
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    false,
                    null),
                Times.Once());
            var expectedResponse = ComposeResponseData();
            AssertIfResponseIsEqual(testResponse, expectedResponse);

            var ackTransferData = ComposeTransferData(LongPoll.EftTransferPromotionalCreditsToMachine, "01", false, 50);

            testResponse = eftStateController.Handle(ackTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Once);
            _timer.Verify(m => m.Stop(), Times.Once);
            Assert.IsNull(testResponse.Handlers);
            handler.Verify(m => m.ProcessTransfer(It.IsAny<ulong>(), It.IsAny<int>()), Times.Never());
            _disableProvider.Verify(m => m.Enable(It.IsAny<Guid>()), Times.Once());
            expectedResponse = ComposeResponseData("01", false, 50, TransactionStatus.InvalidAck);
            AssertIfResponseIsEqual(testResponse, expectedResponse);
        }

        [TestMethod]
        public void TestWrongAckInPhaseTwo()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            var initialTransferData = ComposeTransferData(
                LongPoll.EftTransferPromotionalCreditsToMachine,
                "01",
                false,
                50);

            var testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Once);
            _timer.Verify(m => m.Stop(), Times.Never);
            //Send Phase 2 command with wrong Ack
            initialTransferData = ComposeTransferData(LongPoll.EftTransferPromotionalCreditsToMachine, "01", false, 50);
            testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Once);
            _timer.Verify(m => m.Stop(), Times.Once);
            Assert.IsNull(testResponse.Handlers);
            handler.Verify(m => m.ProcessTransfer(It.IsAny<ulong>(), It.IsAny<int>()), Times.Never());
            _disableProvider.Verify(m => m.Enable(It.IsAny<Guid>()), Times.Once());
            var expectedResponse = ComposeResponseData("01", false, 50, TransactionStatus.InvalidAck);
            AssertIfResponseIsEqual(testResponse, expectedResponse);
        }

        [TestMethod]
        public void TestRaceConditionOfNAckAndTimerInPhaseTwo()
        {
            var times = 0;
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            handler.Setup(m => m.ProcessTransfer(It.IsAny<ulong>(), It.IsAny<int>()))
                .Callback(() => ++times)
                .Returns(true);
            _historyLogProvider.Setup(h => h.UpdateLogEntryForNackedLP(It.IsAny<LongPoll>(), It.IsAny<int>(), It.IsAny<ulong>()))
                .Callback(() => ++times);

            const int iterations = 1000;
            for (int i = 0; i < iterations; i++)
            {
                var initialTransferData = ComposeTransferData(LongPoll.EftTransferPromotionalCreditsToMachine, "01", false, 50);
                var testResponse = eftStateController.Handle(initialTransferData, handler.Object);
                MockHistoryProviderToReturnLog(LongPoll.EftTransferPromotionalCreditsToMachine, initialTransferData, testResponse);
                initialTransferData = ComposeTransferData(LongPoll.EftTransferPromotionalCreditsToMachine, "01", true, 50);
                testResponse = eftStateController.Handle(initialTransferData, handler.Object);
                Parallel.Invoke(() => testResponse.Handlers.ImpliedNackHandler.Invoke(),
                    () => _timer.Raise(x => x.Elapsed += null, default(ElapsedEventArgs)));
                Assert.AreEqual(times, i + 1);
            }
        }

        [TestMethod]
        public void TestRaceConditionOfAckAndTimerInPhaseTwo()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);

            const int iterations = 1000;
            for (int i = 0; i < iterations; i++)
            {
                var initialTransferData = ComposeTransferData(LongPoll.EftTransferPromotionalCreditsToMachine, "01", false, 50);
                var testResponse = eftStateController.Handle(initialTransferData, handler.Object);
                MockHistoryProviderToReturnLog(LongPoll.EftTransferPromotionalCreditsToMachine, initialTransferData, testResponse);
                initialTransferData = ComposeTransferData(LongPoll.EftTransferPromotionalCreditsToMachine, "01", true, 50);
                testResponse = eftStateController.Handle(initialTransferData, handler.Object);
                Parallel.Invoke(() => testResponse.Handlers.ImpliedAckHandler.Invoke(),
                    () => _timer.Raise(x => x.Elapsed += null, default(ElapsedEventArgs)));
                //Expect ProcessTransfer to run once in one for loop.
                handler.Verify(m => m.ProcessTransfer(It.IsAny<ulong>(), It.IsAny<int>()), () => Times.Exactly(i + 1));
            }
        }

        [TestMethod]
        public void TestWrongTransactionNumberInPhaseTwo()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            var initialTransferData = ComposeTransferData(
                LongPoll.EftTransferPromotionalCreditsToMachine,
                "01",
                false,
                50);

            var testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Once);
            _timer.Verify(m => m.Stop(), Times.Never);

            //Send Phase 2 command with wrong Invalid Transaction Number
            initialTransferData = ComposeTransferData(LongPoll.EftTransferPromotionalCreditsToMachine, "02", true, 50);

            testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Once);
            _timer.Verify(m => m.Stop(), Times.Once);
            Assert.IsNull(testResponse.Handlers);
            handler.Verify(m => m.ProcessTransfer(It.IsAny<ulong>(), It.IsAny<int>()), Times.Never());
            _disableProvider.Verify(m => m.Enable(It.IsAny<Guid>()), Times.Once());
            var expectedResponse = ComposeResponseData("02", true, 50, TransactionStatus.InvalidTransactionNumber);
            AssertIfResponseIsEqual(testResponse, expectedResponse);
        }

        [TestMethod]
        public void TestCheckAndSetTransactionStatusWhileInPhase2()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            var initialTransferData = ComposeTransferData(
                LongPoll.EftTransferPromotionalCreditsToMachine,
                "01",
                false,
                50);

            var testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            //Mock history provider to return log for first request
            _historyLogProvider.Setup(
                    m => m.GetLastTransaction())
                .Returns(
                    new EftHistoryLogEntry
                    {
                        Command = LongPoll.EftTransferCashableCreditsToMachine,
                        RequestedTransactionAmount = initialTransferData.TransferAmount,
                        TransactionNumber = initialTransferData.TransactionNumber,
                        Acknowledgement = initialTransferData.Acknowledgement,
                        ReportedTransactionAmount = testResponse.TransferAmount,
                        ReportedTransactionStatus = testResponse.Status,
                        ToBeProcessed = false
                    });

            _timer.Verify(m => m.Start(), Times.Once);
            _timer.Verify(m => m.Stop(), Times.Never);
            handler.Verify(m => m.CheckTransferAmount(It.IsAny<ulong>()), Times.Once());
            _disableProvider.Verify(
                m => m.Disable(
                    It.IsAny<Guid>(),
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    false,
                    null),
                Times.Once());
            handler.Verify(m => m.ProcessTransfer(It.IsAny<ulong>(), It.IsAny<int>()), Times.Never());

            //Send Phase 2 command
            initialTransferData = ComposeTransferData(LongPoll.EftTransferPromotionalCreditsToMachine, "01", true, 50);
            testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            //Send Phase 2 command again
            var doors = new Dictionary<int, LogicalDoor> { { 5, new LogicalDoor(49, "Main Door", "Main Door") } };
            _doorService.Setup(m => m.LogicalDoors).Returns(doors);
            _doorService.Setup(m => m.GetDoorOpen(It.IsAny<int>())).Returns(true);
            initialTransferData = ComposeTransferData(LongPoll.EftTransferPromotionalCreditsToMachine, "01", true, 50);
            testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.AtLeast(1));
            _timer.Verify(m => m.Stop(), Times.AtLeast(1));
            Assert.IsNull(testResponse.Handlers);
            handler.Verify(m => m.ProcessTransfer(It.IsAny<ulong>(), It.IsAny<int>()), Times.Never());
            _disableProvider.Verify(m => m.Enable(It.IsAny<Guid>()), Times.Once());
            var expectedResponse = ComposeResponseData("01", false, 50, TransactionStatus.EgmDoorOpen);
            AssertIfResponseIsEqual(testResponse, expectedResponse);
        }

        [TestMethod]
        public void TestNewCommandWhileInInPhaseOne()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            var initialTransferData = ComposeTransferData(
                LongPoll.EftTransferPromotionalCreditsToMachine,
                "01",
                false,
                50);

            var testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Once);
            _timer.Verify(m => m.Stop(), Times.Never);
            handler.Verify(m => m.CheckTransferAmount(It.IsAny<ulong>()), Times.Once());
            _disableProvider.Verify(
                m => m.Disable(
                    It.IsAny<Guid>(),
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    false,
                    null),
                Times.Once());

            var expectedResponse = ComposeResponseData();
            AssertIfResponseIsEqual(testResponse, expectedResponse);
            var ackTransferData = ComposeTransferData(LongPoll.EftTransferCashableCreditsToMachine, "01", true, 50);
            MockHistoryProviderToReturnLog(
                LongPoll.EftTransferPromotionalCreditsToMachine,
                ackTransferData,
                expectedResponse);

            testResponse = eftStateController.Handle(ackTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Once);
            _timer.Verify(m => m.Stop(), Times.Once);
            handler.Verify(m => m.ProcessTransfer(It.IsAny<ulong>(), It.IsAny<int>()), Times.Never());
            expectedResponse = ComposeResponseData("01", true, 50, TransactionStatus.EgmBusy);
            AssertIfResponseIsEqual(testResponse, expectedResponse);
        }

        [TestMethod]
        public void TestPreviouslyCompletedThroughLogProvider()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            var initialTransferData = ComposeTransferData(
                LongPoll.EftTransferCashableCreditsToMachine,
                "01",
                false,
                50);

            var testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Once);
            _timer.Verify(m => m.Stop(), Times.Never);
            handler.Verify(m => m.CheckTransferAmount(It.IsAny<ulong>()), Times.Once());
            _disableProvider.Verify(
                m => m.Disable(
                    It.IsAny<Guid>(),
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    false,
                    null),
                Times.Once());
            handler.Verify(m => m.ProcessTransfer(It.IsAny<ulong>(), It.IsAny<int>()), Times.Never());
            var expectedResponse = ComposeResponseData();
            AssertIfResponseIsEqual(testResponse, expectedResponse);

            //Set Ack flag to true as the previously completed transaction is identified by this flag
            //along with the transactionId, command and amount.
            initialTransferData.Acknowledgement = true;
            MockHistoryProviderToReturnLog(
                LongPoll.EftTransferCashableCreditsToMachine,
                initialTransferData,
                testResponse);
            var ackTransferData = ComposeTransferData(LongPoll.EftTransferCashableCreditsToMachine, "01", true, 50);

            testResponse = eftStateController.Handle(ackTransferData, handler.Object);
            testResponse.Handlers.ImpliedAckHandler.Invoke();

            _timer.Verify(m => m.Start(), Times.Exactly(2));
            _timer.Verify(m => m.Stop(), Times.Exactly(3));
            handler.Verify(m => m.ProcessTransfer(It.IsAny<ulong>(), It.IsAny<int>()), Times.Once());
            _disableProvider.Verify(m => m.Enable(It.IsAny<Guid>()), Times.Once());
            expectedResponse = ComposeResponseData("01", true);
            AssertIfResponseIsEqual(testResponse, expectedResponse);

            //Reset Ack flag back before trying same Transfer data
            initialTransferData.Acknowledgement = false;
            testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Exactly(2));
            _timer.Verify(m => m.Stop(), Times.Exactly(3));
            _disableProvider.Verify(
                m => m.Disable(
                    It.IsAny<Guid>(),
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    false,
                    null),
                Times.Once());
            expectedResponse = ComposeResponseData("01", false, 0, TransactionStatus.PreviouslyCompleted);
            AssertIfResponseIsEqual(testResponse, expectedResponse);
        }

        [TestMethod]
        public void TestDoorOpen()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            var doors = new Dictionary<int, LogicalDoor> { { 5, new LogicalDoor(49, "Main Door", "Main Door") } };
            _doorService.Setup(m => m.LogicalDoors).Returns(doors);
            _doorService.Setup(m => m.GetDoorOpen(It.IsAny<int>())).Returns(true);
            var initialTransferData = ComposeTransferData(
                LongPoll.EftTransferPromotionalCreditsToMachine,
                "01",
                false,
                50);

            var testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Never);
            _timer.Verify(m => m.Stop(), Times.Never);
            handler.Verify(m => m.CheckTransferAmount(It.IsAny<ulong>()), Times.Never());
            var expectedResponse = ComposeResponseData("01", false, 50, TransactionStatus.EgmDoorOpen);
            AssertIfResponseIsEqual(testResponse, expectedResponse);
        }

        [TestMethod]
        public void TestInGamePlay()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            _gamePlayState.Setup(m => m.CurrentState).Returns(PlayState.Initiated);
            var initialTransferData = ComposeTransferData(
                LongPoll.EftTransferPromotionalCreditsToMachine,
                "01",
                false,
                50);

            var testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Never);
            _timer.Verify(m => m.Stop(), Times.Never);
            handler.Verify(m => m.CheckTransferAmount(It.IsAny<ulong>()), Times.Never());
            var expectedResponse = ComposeResponseData("01", false, 50, TransactionStatus.InGamePlayMode);
            AssertIfResponseIsEqual(testResponse, expectedResponse);
        }

        [TestMethod]
        public void TestInGamePlayOperatorMenu()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            _operatorMenu.Setup(m => m.IsShowing).Returns(true);
            var initialTransferData = ComposeTransferData(
                LongPoll.EftTransferPromotionalCreditsToMachine,
                "01",
                false,
                50);

            var testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Never);
            _timer.Verify(m => m.Stop(), Times.Never);
            handler.Verify(m => m.CheckTransferAmount(It.IsAny<ulong>()), Times.Never());
            var expectedResponse = ComposeResponseData("01", false, 50, TransactionStatus.InGamePlayMode);
            AssertIfResponseIsEqual(testResponse, expectedResponse);
        }

        [TestMethod]
        public void TestEgmDisabledInTypeDLongPoll()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            handler.Setup(m => m.CanContinueTransferIfDisabledByHost()).Returns(true);
            _disableProvider.Setup(m => m.IsDisabled).Returns(true);
            _disableProvider.Setup(m => m.CurrentDisableKeys).Returns(
                new List<Guid>
                {
                    ApplicationConstants.DisabledByHost0Key,
                    ApplicationConstants.DisabledByHost1Key,
                    ApplicationConstants.Host0CommunicationsOfflineDisableKey,
                    ApplicationConstants.Host1CommunicationsOfflineDisableKey,
                    SasConstants.EftTransactionLockUpGuid
                });
            var initialTransferData = ComposeTransferData(
                LongPoll.EftTransferPromotionalCreditsToMachine,
                "01",
                false,
                50);

            var testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Never);
            _timer.Verify(m => m.Stop(), Times.Never);
            handler.Verify(m => m.CheckTransferAmount(It.IsAny<ulong>()), Times.Never());
            var expectedResponse = ComposeResponseData("01", false, 50, TransactionStatus.EgmDisabled);
            AssertIfResponseIsEqual(testResponse, expectedResponse);
        }

        [TestMethod]
        public void TestEgmDisabledInTypeULongPoll()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            _disableProvider.Setup(m => m.IsDisabled).Returns(true);
            _disableProvider.Setup(m => m.CurrentDisableKeys).Returns(
                new List<Guid>
                {
                    ApplicationConstants.DisabledByHost0Key,
                    ApplicationConstants.DisabledByHost1Key,
                    ApplicationConstants.Host0CommunicationsOfflineDisableKey,
                    ApplicationConstants.Host1CommunicationsOfflineDisableKey,
                    SasConstants.EftTransactionLockUpGuid
                });
            handler.Setup(m => m.CanContinueTransferIfDisabledByHost()).Returns(false);
            var initialTransferData = ComposeTransferData(
                LongPoll.EftTransferCashAndNonCashableCreditsToHost,
                "01",
                false,
                0);

            var testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Once);
            _timer.Verify(m => m.Stop(), Times.Never);
            handler.Verify(m => m.CheckTransferAmount(It.IsAny<ulong>()), Times.Once());
            _disableProvider.Verify(
                m => m.Disable(
                    It.IsAny<Guid>(),
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    false,
                    null),
                Times.Once());
            var expectedResponse = ComposeResponseData("01", false, 50, TransactionStatus.EgmDisabled);
            AssertIfResponseIsEqual(testResponse, expectedResponse);

            MockHistoryProviderToReturnLog(
                LongPoll.EftTransferCashAndNonCashableCreditsToHost,
                initialTransferData,
                testResponse);
            var ackTransferData = ComposeTransferData(
                LongPoll.EftTransferCashAndNonCashableCreditsToHost,
                "01",
                true,
                0);

            testResponse = eftStateController.Handle(ackTransferData, handler.Object);
            testResponse.Handlers.ImpliedAckHandler.Invoke();

            _timer.Verify(m => m.Start(), Times.Exactly(2));
            _timer.Verify(m => m.Stop(), Times.Exactly(3));
            handler.Verify(m => m.ProcessTransfer(It.IsAny<ulong>(), It.IsAny<int>()), Times.Once());

            expectedResponse = ComposeResponseData("01", true, 50, TransactionStatus.EgmDisabled);
            AssertIfResponseIsEqual(testResponse, expectedResponse);
        }

        [TestMethod]
        public void TestEgmInTiltCondition()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            _disableProvider.Setup(m => m.IsDisabled).Returns(true);
            _disableProvider.Setup(m => m.CurrentDisableKeys).Returns(
                new List<Guid>
                {
                    ApplicationConstants.ExcessiveDocumentRejectGuid,
                    ApplicationConstants.DisabledByHost1Key,
                    ApplicationConstants.Host0CommunicationsOfflineDisableKey,
                    ApplicationConstants.Host1CommunicationsOfflineDisableKey,
                    SasConstants.EftTransactionLockUpGuid
                });
            var initialTransferData = ComposeTransferData(
                LongPoll.EftTransferPromotionalCreditsToMachine,
                "01",
                false,
                50);

            var testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Never);
            _timer.Verify(m => m.Stop(), Times.Never);
            handler.Verify(m => m.CheckTransferAmount(It.IsAny<ulong>()), Times.Never());
            var expectedResponse = ComposeResponseData("01", false, 50, TransactionStatus.EgmInTiltCondition);
            AssertIfResponseIsEqual(testResponse, expectedResponse);
        }

        [TestMethod]
        public void TestEgmInOutOfService()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            _disableByOperator.Setup(m => m.DisabledByOperator).Returns(true);
            var initialTransferData = ComposeTransferData(
                LongPoll.EftTransferPromotionalCreditsToMachine,
                "01",
                false,
                50);

            var testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Never);
            _timer.Verify(m => m.Stop(), Times.Never);
            handler.Verify(m => m.CheckTransferAmount(It.IsAny<ulong>()), Times.Never());
            var expectedResponse = ComposeResponseData("01", false, 50, TransactionStatus.EgmOutOfService);
            AssertIfResponseIsEqual(testResponse, expectedResponse);
        }

        [TestMethod]
        public void TestImpliedNack()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            var initialTransferData = ComposeTransferData(
                LongPoll.EftTransferPromotionalCreditsToMachine,
                "01",
                false,
                50);
            //First Command
            var testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            MockHistoryProviderToReturnLog(
                LongPoll.EftTransferCashableCreditsToMachine,
                initialTransferData,
                testResponse);
            var ackTransferData = ComposeTransferData(LongPoll.EftTransferCashableCreditsToMachine, "01", true, 50);

            //Second Command
            testResponse = eftStateController.Handle(ackTransferData, handler.Object);
            testResponse.Handlers.ImpliedNackHandler.Invoke();

            _timer.Verify(m => m.Start(), Times.Exactly(2));
            _timer.Verify(m => m.Stop(), Times.Exactly(3));
            handler.Verify(m => m.ProcessTransfer(It.IsAny<ulong>(), It.IsAny<int>()), Times.Never());
            _disableProvider.Verify(m => m.Enable(It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        public void TestRecoverIfRequiredForLastLogNull()
        {
            var controller = InitializeController();
            var handler = InitializeHandler(50);
            controller.RecoverIfRequired(handler.Object);
            handler.Verify(m => m.ProcessTransfer(It.IsAny<ulong>(), It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public void TestRecoverIfRequiredForLastLogCommandDifferentFromHandler()
        {
            var controller = InitializeController();
            var handler = InitializeHandler(50);
            MockHistoryProviderToReturnLog(
                LongPoll.EftTransferPromotionalCreditsToHost,
                new EftTransferData { Acknowledgement = true, TransactionNumber = 10, TransferAmount = 100 },
                new EftTransactionResponse
                {
                    Acknowledgement = true,
                    TransferAmount = 80,
                    TransactionNumber = 10,
                    Status = TransactionStatus.OperationSuccessful
                },
                true);

            controller.RecoverIfRequired(handler.Object);
            handler.Verify(m => m.ProcessTransfer(It.IsAny<ulong>(), It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public void TestRecoverIfRequiredForLastLogCommandSameAsHandler()
        {
            var controller = InitializeController();
            var handler = InitializeHandler(50);
            MockHistoryProviderToReturnLog(
                LongPoll.EftTransferCashableCreditsToMachine,
                new EftTransferData { Acknowledgement = true, TransactionNumber = 10, TransferAmount = 100 },
                new EftTransactionResponse
                {
                    Acknowledgement = true,
                    TransferAmount = 80,
                    TransactionNumber = 10,
                    Status = TransactionStatus.OperationSuccessful
                },
                true);

            controller.RecoverIfRequired(handler.Object);
            handler.Verify(m => m.ProcessTransfer(It.IsAny<ulong>(), It.IsAny<int>()), Times.Once);
            _historyLogProvider.Verify(
                m => m.UpdateLogEntryForRequestCompleted(
                    It.IsAny<LongPoll>(),
                    It.IsAny<int>(),
                    It.IsAny<ulong>()),
                Times.Once);
        }

        [TestMethod]
        public void TestReAckForIntermediaryNack()
        {
            var eftStateController = InitializeController();
            var handler = InitializeHandler(50);
            _eftTransferProvider.Setup(m => m.RestartCashoutTimer());
            var initialTransferData = ComposeTransferData(
                LongPoll.EftTransferPromotionalCreditsToMachine,
                "01",
                false,
                50);

            var testResponse = eftStateController.Handle(initialTransferData, handler.Object);

            _timer.Verify(m => m.Start(), Times.Once);
            _timer.Verify(m => m.Stop(), Times.Never);
            handler.Verify(m => m.CheckTransferAmount(It.IsAny<ulong>()), Times.Once());
            _disableProvider.Verify(
                m => m.Disable(
                    It.IsAny<Guid>(),
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    false,
                    null),
                Times.Once());
            var expectedResponse = ComposeResponseData();

            AssertIfResponseIsEqual(testResponse, expectedResponse);

            MockHistoryProviderToReturnLog(
                LongPoll.EftTransferCashableCreditsToMachine,
                initialTransferData,
                testResponse);
            var ackTransferData = ComposeTransferData(LongPoll.EftTransferCashableCreditsToMachine, "01", true, 50);

            testResponse = eftStateController.Handle(ackTransferData, handler.Object);
            testResponse.Handlers.IntermediateNackHandler.Invoke();

            _timer.Verify(m => m.Start(), Times.Exactly(3));
            _timer.Verify(m => m.Stop(), Times.AtLeast(2));

            testResponse.Handlers.ImpliedAckHandler.Invoke();

            handler.Verify(m => m.ProcessTransfer(It.IsAny<ulong>(), It.IsAny<int>()), Times.Once());
            _disableProvider.Verify(m => m.Enable(It.IsAny<Guid>()), Times.Once());

            expectedResponse = ComposeResponseData("01", true);
            AssertIfResponseIsEqual(testResponse, expectedResponse);
        }

        private static void AssertIfResponseIsEqual(
            EftTransactionResponse testResponse,
            EftTransactionResponse expectedResponse)
        {
            //Assert.AreEqual(expectedResponse.Acknowledgement, testResponse.Acknowledgement);
            Assert.AreEqual(expectedResponse.TransferAmount, testResponse.TransferAmount);
            Assert.AreEqual(expectedResponse.Status, testResponse.Status);
        }

        private EftStateController ConstructController(
            bool nullDisableManager,
            bool nullDoorService,
            bool nullGamePlayState,
            bool nullDisableByOperatorManager,
            bool nullOperatorMenuLauncher,
            bool nullEftTransferProvider,
            bool nullEftHistoryLogProvider)
        {
            return _target = new EftStateController(
                nullDisableManager ? null : _disableProvider.Object,
                nullDoorService ? null : _doorService.Object,
                nullGamePlayState ? null : _gamePlayState.Object,
                nullDisableByOperatorManager ? null : _disableByOperator.Object,
                nullOperatorMenuLauncher ? null : _operatorMenu.Object,
                nullEftTransferProvider ? null : _eftTransferProvider.Object,
                nullEftHistoryLogProvider ? null : _historyLogProvider.Object
            );
        }

        private EftStateController InitializeController()
        {
            _doorService.Setup(m => m.LogicalDoors).Returns(new Dictionary<int, LogicalDoor>());
            _operatorMenu.Setup(m => m.IsShowing).Returns(false);
            _gamePlayState.Setup(m => m.CurrentState).Returns(PlayState.Idle);
            var allowEft = new List<Guid>
            {
                ApplicationConstants.LiveAuthenticationDisableKey, SasConstants.EftTransactionLockUpGuid
            };

            _disableProvider.Setup(m => m.Enable(It.IsAny<Guid>()))
                .Verifiable();
            _disableProvider.Setup(
                    m => m.Disable(
                        It.IsAny<Guid>(),
                        SystemDisablePriority.Immediate,
                        It.IsAny<Func<string>>(),
                        false,
                        null))
                .Verifiable();
            _disableProvider.Setup(m => m.CurrentDisableKeys).Returns(allowEft);
            _disableProvider.Setup(m => m.IsDisabled).Returns(false);
            _disableByOperator.Setup(m => m.DisabledByOperator).Returns(false);

            _timer.Setup(m => m.Start());
            _timer.Setup(m => m.Stop());

            _eftTransferProvider.Setup(
                    m => m.CheckIfProcessed(
                        It.IsAny<string>(),
                        It.IsAny<EftTransferType>()))
                .Returns(false);

            _historyLogProvider.Setup(
                h => h.AddOrUpdateEntry(
                    It.IsAny<EftTransferData>(),
                    It.IsAny<EftTransactionResponse>()));
            _historyLogProvider.Setup(
                h => h.UpdateLogEntryForRequestCompleted(
                    It.IsAny<LongPoll>(),
                    It.IsAny<int>(),
                    It.IsAny<ulong>()));
            _historyLogProvider.Setup(
                h => h.UpdateLogEntryForNackedLP(
                    It.IsAny<LongPoll>(),
                    It.IsAny<int>(),
                    It.IsAny<ulong>()));
            _historyLogProvider.Setup(
                    h => h.GetLastTransaction())
                .Returns<IEftHistoryLogEntry>(null);

            _target = new EftStateController(
                _disableProvider.Object,
                _doorService.Object,
                _gamePlayState.Object,
                _disableByOperator.Object,
                _operatorMenu.Object,
                _eftTransferProvider.Object,
                _historyLogProvider.Object);

            _target.SetTimer(_timer.Object);

            return _target;
        }

        private Mock<IEftTransferHandler> InitializeHandler(ulong amountToReturn)
        {
            var transferHandler = new Mock<IEftTransferHandler>(MockBehavior.Strict);
            (ulong, bool) returnVariable = (amountToReturn, false);
            transferHandler.Setup(m => m.Commands)
                .Returns(new List<LongPoll> { LongPoll.EftTransferCashableCreditsToMachine });
            transferHandler.Setup(m => m.CheckTransferAmount(It.IsAny<ulong>()))
                .Returns(returnVariable);
            transferHandler.Setup(m => m.ProcessTransfer(It.IsAny<ulong>(), It.IsAny<int>())).Returns(true);
            transferHandler.Setup(m => m.GetDisableString()).Returns("");
            transferHandler.Setup(m => m.CanContinueTransferIfDisabledByHost()).Returns(true);
            return transferHandler;
        }

        private EftTransferData ComposeTransferData(LongPoll command, string transactionNumber, bool ack, ulong amount)
        {
            return new EftTransferData
            {
                Command = command,
                TransactionNumber = int.Parse(transactionNumber),
                Acknowledgement = ack,
                TransferAmount = amount
            };
        }

        private EftTransactionResponse ComposeResponseData(
            string transactionNumber = "01",
            bool ack = false,
            ulong amount = 50,
            TransactionStatus status = TransactionStatus.OperationSuccessful)
        {
            return new EftTransactionResponse
            {
                TransactionNumber = int.Parse(transactionNumber),
                Acknowledgement = ack,
                TransferAmount = amount,
                Status = status
            };
        }

        private void MockHistoryProviderToReturnLog(
            LongPoll command,
            EftTransferData initialTransferData,
            EftTransactionResponse testResponse,
            bool toBeProcessed = false)
        {
            //Mock history provider to return log for first request
            _historyLogProvider.Setup(
                    m => m.GetLastTransaction())
                .Returns(
                    new EftHistoryLogEntry
                    {
                        Command = command,
                        RequestedTransactionAmount = initialTransferData.TransferAmount,
                        TransactionNumber = initialTransferData.TransactionNumber,
                        Acknowledgement = initialTransferData.Acknowledgement,
                        ReportedTransactionAmount = testResponse.TransferAmount,
                        ReportedTransactionStatus = testResponse.Status,
                        ToBeProcessed = toBeProcessed
                    });
        }
    }
}