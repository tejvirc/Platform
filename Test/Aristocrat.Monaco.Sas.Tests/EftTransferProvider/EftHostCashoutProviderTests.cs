namespace Aristocrat.Monaco.Sas.Tests.EftTransferProvider
{
    using System;
    using System.Threading.Tasks;
    using System.Timers;
    using Aristocrat.Sas.Client;
    using Common;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.EftTransferProvider;
    using Test.Common;

    [TestClass]
    public class EftHostCashoutProviderTests
    {
        private readonly GeneralExceptionCode Exception66 = GeneralExceptionCode.CashOutButtonPressed;

        private Mock<ISasExceptionHandler> _exceptionHandler =
            new Mock<ISasExceptionHandler>(MockBehavior.Strict);

        private Mock<ISystemDisableManager> _disableProvider =
            new Mock<ISystemDisableManager>(MockBehavior.Strict);

        private Mock<IGamePlayState> _gamePlayState = new Mock<IGamePlayState>(MockBehavior.Strict);
        private Mock<ISystemTimerWrapper> _timer = new Mock<ISystemTimerWrapper>(MockBehavior.Strict);
        private EftHostCashoutProvider _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            _disableProvider = new Mock<ISystemDisableManager>(MockBehavior.Strict);
            _gamePlayState = new Mock<IGamePlayState>(MockBehavior.Strict);
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Strict);
            _timer = new Mock<ISystemTimerWrapper>(MockBehavior.Strict);
            _target = ConstructCashoutProvider(false, false, false);

            _disableProvider.Setup(m => m.IsDisabled).Returns(false);
            _gamePlayState.Setup(m => m.CurrentState).Returns(PlayState.Idle);
            _timer.Setup(m => m.Start());
            _timer.Setup(m => m.Stop());
            _timer.Setup(m => m.Enabled).Returns(false);
            _exceptionHandler.Setup(m => m.ReportException(new GenericExceptionBuilder(Exception66)));

            _target.SetTimer(_timer.Object);
        }

        [DataRow(true, false, false, DisplayName = "Null ISasExceptionHandler")]
        [DataRow(false, true, false, DisplayName = "Null ISystemDisableManager")]
        [DataRow(false, false, true, DisplayName = "Null IGamePlayState")]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void InitializeWithNullArgumentExpectException(
            bool nullISasExceptionHandler,
            bool nullISystemDisableManager,
            bool nullIGamePlayState)
        {
            _target = ConstructCashoutProvider(
                nullISasExceptionHandler,
                nullISystemDisableManager,
                nullIGamePlayState);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _timer.Setup(m => m.Dispose());
            _target.Dispose();

            //testing disposing twice to ensure all is disposed and works properly
            _target.Dispose();
        }

        [TestMethod]
        public void DisposeTest()
        {
            _timer.Setup(m => m.Dispose());
            _target.Dispose();

            _timer.Verify(m => m.Dispose(), Times.Once);
        }

        [TestMethod]
        public void HandleHostCashOutTest()
        {
            var handleHostCashOutTask = new Task<CashOutReason>(() => _target.HandleHostCashOut());
            handleHostCashOutTask.Start();
            _timer.Raise(x => x.Elapsed += null, new EventArgs() as ElapsedEventArgs);

            Assert.AreEqual(CashOutReason.TimedOut, handleHostCashOutTask.Result);
            _timer.Verify(m => m.Start(), Times.Once);
            _exceptionHandler.Verify(m => m.ReportException(new GenericExceptionBuilder(Exception66)), Times.Once);
        }

        [TestMethod]
        public void HandleHostCashOutWhenInTiltTest()
        {
            _disableProvider.Setup(m => m.IsDisabled).Returns(true);

            var handleHostCashOutTask = new Task<CashOutReason>(() => _target.HandleHostCashOut());
            handleHostCashOutTask.Start();
            _timer.Raise(x => x.Elapsed += null, new EventArgs() as ElapsedEventArgs);

            Assert.AreEqual(CashOutReason.None, handleHostCashOutTask.Result);
            _timer.Verify(m => m.Start(), Times.Never);
            _exceptionHandler.Verify(m => m.ReportException(new GenericExceptionBuilder(Exception66)), Times.Once);
        }

        [TestMethod]
        public void HandleHostCashOutWhenInGamePlayTest()
        {
            _gamePlayState.Setup(m => m.CurrentState).Returns(PlayState.Initiated);

            var handleHostCashOutTask = new Task<CashOutReason>(() => _target.HandleHostCashOut());
            handleHostCashOutTask.Start();
            _timer.Raise(x => x.Elapsed += null, new EventArgs() as ElapsedEventArgs);

            Assert.AreEqual(CashOutReason.None, handleHostCashOutTask.Result);
            _timer.Verify(m => m.Start(), Times.Never);
            _exceptionHandler.Verify(m => m.ReportException(new GenericExceptionBuilder(Exception66)), Times.Once);
        }

        [TestMethod]
        public void CashOutAcceptedIfTimerEnabledTest()
        {
            _timer.Setup(m => m.Enabled).Returns(true);

            var actualResult = _target.CashOutAccepted();

            _timer.Verify(m => m.Stop(), Times.Once);
            Assert.IsTrue(actualResult);
        }

        [TestMethod]
        public void CashOutAcceptedIfTimerNotEnabledTest()
        {
            _timer.Setup(m => m.Enabled).Returns(false);

            var actualResult = _target.CashOutAccepted();

            _timer.Verify(m => m.Stop(), Times.Never);
            Assert.IsFalse(actualResult);
        }

        [TestMethod]
        public async Task CallHandleHostCashOutThenCallCashOutAcceptedTest()
        {
            _timer.Setup(m => m.Enabled).Returns(true);
            var handleHostCashOutTask = new Task<CashOutReason>(_target.HandleHostCashOut);
            handleHostCashOutTask.Start();
            var actualResult = _target.CashOutAccepted();

            Assert.AreEqual(CashOutReason.CashOutAccepted, await handleHostCashOutTask);
            _exceptionHandler.Verify(m => m.ReportException(new GenericExceptionBuilder(Exception66)), Times.Once);
            _timer.Verify(m => m.Start(), Times.Once);
            _timer.Verify(m => m.Stop(), Times.Once);
            Assert.IsTrue(actualResult);
        }

        [TestMethod]
        public void CallHandleHostCashOutThenCallCashOutAcceptedAfterTimeOutTest()
        {
            _timer.Setup(m => m.Enabled).Returns(false);
            var handleHostCashOutTask = new Task<CashOutReason>(_target.HandleHostCashOut);
            handleHostCashOutTask.Start();
            _timer.Raise(x => x.Elapsed += null, new EventArgs() as ElapsedEventArgs);

            var actualResult = _target.CashOutAccepted();
            Assert.AreEqual(CashOutReason.TimedOut, handleHostCashOutTask.Result);
            _exceptionHandler.Verify(m => m.ReportException(new GenericExceptionBuilder(Exception66)), Times.Once);
            _timer.Verify(m => m.Start(), Times.Once);
            _timer.Verify(m => m.Stop(), Times.Never);
            Assert.IsFalse(actualResult);
        }

        [TestMethod]
        public void RestartTimerIfTimerStartedTest()
        {
            _timer.Setup(m => m.Enabled).Returns(true);

            _target.RestartTimerIfPendingCallbackFromHost();

            _timer.Verify(m => m.Start(), Times.Once);
        }

        [TestMethod]
        public void RestartTimerIfTimerNotStartedTest()
        {
            _timer.Setup(m => m.Enabled).Returns(false);

            _target.RestartTimerIfPendingCallbackFromHost();

            _timer.Verify(m => m.Start(), Times.Never);
        }

        [TestMethod]
        public void SetTimerTest()
        {
            _timer.Raise(x => x.Elapsed += null, new EventArgs() as ElapsedEventArgs);
        }

        private EftHostCashoutProvider ConstructCashoutProvider(
            bool nullISasExceptionHandler,
            bool nullISystemDisableManager,
            bool nullIGamePlayState)
        {
            return _target = new EftHostCashoutProvider(
                nullISasExceptionHandler ? null : _exceptionHandler.Object,
                nullISystemDisableManager ? null : _disableProvider.Object,
                nullIGamePlayState ? null : _gamePlayState.Object
            );
        }
    }
}