namespace Aristocrat.Monaco.Sas.Tests.ChangeRequests
{
    using System;
    using System.Threading;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.ChangeRequests;

    public class TestWaiter
    {
        private AutoResetEvent _waiter;

        public TestWaiter(MeterChangeExceptionHandler handler)
        {
            _waiter = new AutoResetEvent(false);
            CommitEventReceived = 0;
            CancelEventReceived = 0;
            if (handler != null)
            {
                handler.OnChangeCancel += OnChangeCancel;
                handler.OnChangeCommit += OnChangeCommit;
            }
        }

        public int CommitEventReceived { get; private set; }
        public int CancelEventReceived { get; private set; }

        public bool Wait(int timeout)
        {
            return _waiter.WaitOne(timeout);
        }

        private void OnChangeCommit(object sender, EventArgs e)
        {
            ++CommitEventReceived;
            _waiter.Set();
        }

        private void OnChangeCancel(object sender, EventArgs e)
        {
            ++CancelEventReceived;
            _waiter.Set();
        }
    }

    /// <summary>
    ///     Contains tests for MeterChangeExceptionHandlerTest
    /// </summary>
    [DoNotParallelize]
    [TestClass]
    public class MeterChangeExceptionHandlerTest
    {
        private const double CancelTimeout = 100;  // 100 ms
        private const double ThirtySecondTimeout = 30000;  // 30 seconds
        private const int TimeoutWait = 1500;  // one and half seconds
        private const int MaxAcknowledgements = 5;
        private MeterChangeExceptionHandler _target;
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private Mock<IPropertiesManager> _propertiesManager;
        private TestWaiter _waiter;
        private GeneralExceptionCode _actualSasExceptionType = GeneralExceptionCode.EgmPowerLost;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Strict);
            _exceptionHandler.Setup(x => x.ReportException(It.IsAny<ISasExceptionCollection>()))
                .Callback((ISasExceptionCollection ex) => _actualSasExceptionType = ex.ExceptionCode).Verifiable();
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(p => p.SetProperty(It.IsAny<string>(), It.IsAny<byte>()));

            _target = new MeterChangeExceptionHandler(_exceptionHandler.Object, _propertiesManager.Object);
            _waiter = new TestWaiter(_target);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullPropertiesManagerTest()
        {
            _target = new MeterChangeExceptionHandler(_exceptionHandler.Object, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullExceptionHandlerTest()
        {
            _target = new MeterChangeExceptionHandler(null, _propertiesManager.Object);
        }

        [TestMethod]
        public void TimerActiveTest()
        {
            Assert.IsFalse(_target.TimerActive());
            _target.StartPendingChange(MeterCollectStatus.LifetimeMeterChange, CancelTimeout);
            _exceptionHandler.Verify(m => m.ReportException(It.IsAny<ISasExceptionCollection>()), Times.Once);
            Assert.AreEqual(GeneralExceptionCode.MeterChangePending, _actualSasExceptionType);
            Assert.IsTrue(_target.TimerActive());

            // wait for the async call to finish
            Assert.IsTrue(_waiter.Wait(TimeoutWait));
            Assert.AreEqual(_waiter.CommitEventReceived, 0);
            Assert.AreEqual(_waiter.CancelEventReceived, 1);
            _exceptionHandler.Verify(m => m.ReportException(It.IsAny<ISasExceptionCollection>()), Times.Exactly(2));
            Assert.AreEqual(GeneralExceptionCode.MeterChangeCanceled, _actualSasExceptionType);
            Assert.IsFalse(_target.TimerActive());
        }

        [DataTestMethod]
        [DataRow(MeterCollectStatus.GameDenomPaytableChange, 2, DisplayName = "Test for state change for GameDenomPaytableChange")]
        [DataRow(MeterCollectStatus.LifetimeMeterChange, 2, DisplayName = "Test for state change for LifetimeMeterChange")]
        [DataRow(MeterCollectStatus.NotInPendingChange, 0, DisplayName = "Test for state change for NotInPendingChange")]
        public void StartPendingChangeTest(MeterCollectStatus meterChangeStatus, int numberOfExceptionsReported)
        {
            Assert.AreEqual(MeterCollectStatus.NotInPendingChange, _target.MeterChangeStatus);
            _target.StartPendingChange(meterChangeStatus, CancelTimeout);
            if (numberOfExceptionsReported > 0)
            {
                Assert.AreEqual(GeneralExceptionCode.MeterChangePending, _actualSasExceptionType);
            }
            Assert.AreEqual(meterChangeStatus, _target.MeterChangeStatus);

            // wait for the async call to finish
            Assert.IsTrue(_waiter.Wait(TimeoutWait) || (numberOfExceptionsReported == 0));
            Assert.AreEqual(_waiter.CommitEventReceived, 0);
            Assert.AreEqual(_waiter.CancelEventReceived, numberOfExceptionsReported > 0 ? 1 : 0);
            _exceptionHandler.Verify(m => m.ReportException(It.IsAny<ISasExceptionCollection>()), Times.Exactly(numberOfExceptionsReported));
            if (numberOfExceptionsReported > 0)
            {
                Assert.AreEqual(GeneralExceptionCode.MeterChangeCanceled, _actualSasExceptionType);
            }

            Assert.AreEqual(MeterCollectStatus.NotInPendingChange, _target.MeterChangeStatus);
        }

        [TestMethod]
        public void MultipleStartPendingChangeTest()
        {
            Assert.AreEqual(MeterCollectStatus.NotInPendingChange, _target.MeterChangeStatus);
            _target.StartPendingChange(MeterCollectStatus.LifetimeMeterChange, CancelTimeout);
            Assert.AreEqual(MeterCollectStatus.LifetimeMeterChange, _target.MeterChangeStatus);
            _target.StartPendingChange(MeterCollectStatus.GameDenomPaytableChange, CancelTimeout);
            Assert.AreEqual(MeterCollectStatus.LifetimeMeterChange, _target.MeterChangeStatus);
            _exceptionHandler.Verify(m => m.ReportException(It.IsAny<ISasExceptionCollection>()), Times.Once);
            Assert.AreEqual(GeneralExceptionCode.MeterChangePending, _actualSasExceptionType);

            // wait for the async call to finish
            _waiter.Wait(TimeoutWait);
            _exceptionHandler.Verify(m => m.ReportException(It.IsAny<ISasExceptionCollection>()), Times.Exactly(2));
            Assert.AreEqual(GeneralExceptionCode.MeterChangeCanceled, _actualSasExceptionType);
            Assert.AreEqual(_waiter.CommitEventReceived, 0);
            Assert.AreEqual(_waiter.CancelEventReceived, 1);
            Assert.AreEqual(MeterCollectStatus.NotInPendingChange, _target.MeterChangeStatus);
        }

        [DataTestMethod]
        [DataRow(MeterCollectStatus.GameDenomPaytableChange, DisplayName = "Test for host acknowledgement of pending change for GameDenomPaytableChange")]
        [DataRow(MeterCollectStatus.LifetimeMeterChange, DisplayName = "Test for host acknowledgement of pending change for LifetimeMeterChange")]
        public void AcknowledgePendingChangeTest(MeterCollectStatus meterChangeStatus)
        {
            Assert.AreEqual(MeterCollectStatus.NotInPendingChange, _target.MeterChangeStatus);
            _target.StartPendingChange(meterChangeStatus, ThirtySecondTimeout);
            _exceptionHandler.Verify(m => m.ReportException(It.IsAny<ISasExceptionCollection>()), Times.Once);
            Assert.AreEqual(GeneralExceptionCode.MeterChangePending, _actualSasExceptionType);

            for (int i = 0; i < MaxAcknowledgements; ++i)
            {
                Assert.AreEqual(meterChangeStatus, _target.MeterChangeStatus);
                _target.AcknowledgePendingChange();
            }

            Assert.AreEqual(MeterCollectStatus.NotInPendingChange, _target.MeterChangeStatus);
            Assert.AreEqual(_waiter.CommitEventReceived, 0);
            Assert.AreEqual(_waiter.CancelEventReceived, 1);
            _exceptionHandler.Verify(m => m.ReportException(It.IsAny<ISasExceptionCollection>()), Times.Exactly(2));
            Assert.AreEqual(GeneralExceptionCode.MeterChangeCanceled, _actualSasExceptionType);
        }

        [DataTestMethod]
        [DataRow(MeterCollectStatus.GameDenomPaytableChange, GeneralExceptionCode.EnabledGamesDenomsChanged, DisplayName = "Test for host notification for GameDenomPaytableChange to be commited with SasExceptionType.EnabledGamesDenomsChanged being sent.")]
        [DataRow(MeterCollectStatus.LifetimeMeterChange, GeneralExceptionCode.GamingMachineSoftMetersReset, DisplayName = "Test for host notification for LifetimeMeterChange to be commited with SasExceptionType.LifetimeMetersReset being sent.")]
        public void ReadyForPendingChangeTest(MeterCollectStatus meterChangeStatus, GeneralExceptionCode sasExceptionType)
        {
            Assert.AreEqual(MeterCollectStatus.NotInPendingChange, _target.MeterChangeStatus);
            _target.StartPendingChange(meterChangeStatus, ThirtySecondTimeout);
            Assert.AreEqual(meterChangeStatus, _target.MeterChangeStatus);
            _exceptionHandler.Verify(m => m.ReportException(It.IsAny<ISasExceptionCollection>()), Times.Once);
            Assert.AreEqual(GeneralExceptionCode.MeterChangePending, _actualSasExceptionType);
            _target.ReadyForPendingChange();
            Assert.AreEqual(MeterCollectStatus.NotInPendingChange, _target.MeterChangeStatus);
            Assert.AreEqual(_waiter.CommitEventReceived, 1);
            Assert.AreEqual(_waiter.CancelEventReceived, 0);
            _exceptionHandler.Verify(m => m.ReportException(It.IsAny<ISasExceptionCollection>()), Times.Exactly(2));
            Assert.AreEqual(sasExceptionType, _actualSasExceptionType);
        }

        [DataTestMethod]
        [DataRow(MeterCollectStatus.GameDenomPaytableChange, DisplayName = "Test for cancellation of pending change for GameDenomPaytableChange")]
        [DataRow(MeterCollectStatus.LifetimeMeterChange, DisplayName = "Test for cancellation of pending change for LifetimeMeterChange")]
        public void CancelPendingChange(MeterCollectStatus meterChangeStatus)
        {
            Assert.AreEqual(MeterCollectStatus.NotInPendingChange, _target.MeterChangeStatus);
            _target.StartPendingChange(meterChangeStatus, -1);
            Assert.AreEqual(meterChangeStatus, _target.MeterChangeStatus);
            _exceptionHandler.Verify(m => m.ReportException(It.IsAny<ISasExceptionCollection>()), Times.Once);
            Assert.AreEqual(GeneralExceptionCode.MeterChangePending, _actualSasExceptionType);
            _target.CancelPendingChange();
            Assert.AreEqual(MeterCollectStatus.NotInPendingChange, _target.MeterChangeStatus);
            Assert.AreEqual(_waiter.CommitEventReceived, 0);
            Assert.AreEqual(_waiter.CancelEventReceived, 1);
            _exceptionHandler.Verify(m => m.ReportException(It.IsAny<ISasExceptionCollection>()), Times.Exactly(2));
            Assert.AreEqual(GeneralExceptionCode.MeterChangeCanceled, _actualSasExceptionType);
        }
    }
}
