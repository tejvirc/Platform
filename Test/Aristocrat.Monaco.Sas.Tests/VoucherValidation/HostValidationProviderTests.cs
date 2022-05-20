namespace Aristocrat.Monaco.Sas.Tests.VoucherValidation
{
    using System;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.VoucherValidation;

    [TestClass]
    public class HostValidationProviderTests
    {
        private const byte ValidationId = 0x01;
        private const string ValidationNumber = "1234567890123456";
        private const int WaitTime = 1000;

        private Mock<ISasHost> _sasHost;
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private HostValidationProvider _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _sasHost = new Mock<ISasHost>(MockBehavior.Default);
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Default);
            _target = new HostValidationProvider(_exceptionHandler.Object, _sasHost.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullSasHostTest()
        {
            _target = new HostValidationProvider(null, _sasHost.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullExceptionHandlerTest()
        {
            _target = new HostValidationProvider(null, _sasHost.Object);
        }

        [TestMethod]
        public void GettingValidationDataWithoutRequestTest()
        {
            Assert.AreEqual(ValidationState.NoValidationPending, _target.CurrentState);
            Assert.IsNull(_target.GetPendingValidationData());
        }

        [TestMethod]
        public void SettingValidationResultsWithoutRequestTest()
        {
            Assert.AreEqual(ValidationState.NoValidationPending, _target.CurrentState);
            Assert.IsFalse(_target.SetHostValidationResult(new HostValidationResults(ValidationId, ValidationNumber)));
        }

        [TestMethod]
        public void RequestingValidationWhenAnotherIsInprogressTest()
        {
            _exceptionHandler.Setup(
                x => x.ReportException(
                    It.Is<ISasExceptionCollection>(
                        ex => ex.ExceptionCode == GeneralExceptionCode.SystemValidationRequest))).Verifiable();
            _target.GetValidationResults(100, TicketType.CashOut); // Start the first request
            var results = _target.GetValidationResults(100, TicketType.CashOut);

            Assert.IsTrue(results.Wait(WaitTime));
            Assert.IsNull(results.Result);
            _sasHost.Verify();
        }

        [TestMethod]
        public void SettingValidationResultsWhileCashoutInformationIsPendingTest()
        {
            _exceptionHandler.Setup(
                x => x.ReportException(
                    It.Is<ISasExceptionCollection>(
                        ex => ex.ExceptionCode == GeneralExceptionCode.SystemValidationRequest))).Verifiable();
            var results = _target.GetValidationResults(100, TicketType.CashOut);
            _target.SetHostValidationResult(null);
            Assert.IsTrue(results.Wait(WaitTime));
            Assert.IsNull(results.Result);
            _sasHost.Verify();
        }

        [TestMethod]
        public void SettingValidationResultsAfterReadingCashoutData()
        {
            const ulong expectedAmount = 100;
            const TicketType expectedTicketType = TicketType.CashOut;

            _exceptionHandler.Setup(
                x => x.ReportException(
                    It.Is<ISasExceptionCollection>(
                        ex => ex.ExceptionCode == GeneralExceptionCode.SystemValidationRequest))).Verifiable();
            var results = _target.GetValidationResults(expectedAmount, expectedTicketType);
            var hostValidationData = _target.GetPendingValidationData();
            Assert.AreEqual(expectedTicketType, hostValidationData.TicketType);
            Assert.AreEqual(expectedAmount, hostValidationData.Amount);

            _target.SetHostValidationResult(new HostValidationResults(ValidationId, ValidationNumber));
            Assert.IsTrue(results.Wait(WaitTime));
            var hostValidationResults = results.Result;
            Assert.AreEqual(ValidationNumber, hostValidationResults.ValidationNumber);
            Assert.AreEqual(ValidationId, hostValidationResults.SystemId);
            _sasHost.Verify();
        }

        [TestMethod]
        public void ReadingCashoutInformationWhileWaitingForValidationData()
        {
            const ulong expectedAmount = 100;
            const TicketType expectedTicketType = TicketType.CashOut;

            _exceptionHandler.Setup(
                x => x.ReportException(
                    It.Is<ISasExceptionCollection>(
                        ex => ex.ExceptionCode == GeneralExceptionCode.SystemValidationRequest))).Verifiable();
            var results = _target.GetValidationResults(expectedAmount, expectedTicketType);
            var hostValidationData = _target.GetPendingValidationData();
            Assert.AreEqual(expectedTicketType, hostValidationData.TicketType);
            Assert.AreEqual(expectedAmount, hostValidationData.Amount);

            hostValidationData = _target.GetPendingValidationData();
            Assert.AreEqual(expectedTicketType, hostValidationData.TicketType);
            Assert.AreEqual(expectedAmount, hostValidationData.Amount);

            _target.SetHostValidationResult(new HostValidationResults(ValidationId, ValidationNumber));
            Assert.IsTrue(results.Wait(WaitTime));
            var hostValidationResults = results.Result;
            Assert.AreEqual(ValidationNumber, hostValidationResults.ValidationNumber);
            Assert.AreEqual(ValidationId, hostValidationResults.SystemId);
            _sasHost.Verify();
        }

        [TestMethod]
        public void HostOfflineWhileCashoutIsPendingTest()
        {
            _exceptionHandler.Setup(
                x => x.ReportException(
                    It.Is<ISasExceptionCollection>(
                        ex => ex.ExceptionCode == GeneralExceptionCode.SystemValidationRequest))).Verifiable();
            var results = _target.GetValidationResults(100, TicketType.CashOut);
            _sasHost.Setup(x => x.IsHostOnline(SasGroup.Validation)).Returns(false).Verifiable();
            _target.OnSystemDisabled();
            Assert.IsTrue(results.Wait(WaitTime));
            Assert.IsNull(results.Result);
            _sasHost.Verify();
        }

        [TestMethod]
        public void HostOfflineAfterReadingCashoutDataTest()
        {
            const ulong expectedAmount = 100;
            const TicketType expectedTicketType = TicketType.CashOut;

            _exceptionHandler.Setup(
                x => x.ReportException(
                    It.Is<ISasExceptionCollection>(
                        ex => ex.ExceptionCode == GeneralExceptionCode.SystemValidationRequest))).Verifiable();
            var results = _target.GetValidationResults(expectedAmount, expectedTicketType);
            var hostValidationData = _target.GetPendingValidationData();
            Assert.AreEqual(expectedTicketType, hostValidationData.TicketType);
            Assert.AreEqual(expectedAmount, hostValidationData.Amount);

            _sasHost.Setup(x => x.IsHostOnline(SasGroup.Validation)).Returns(false).Verifiable();
            _target.OnSystemDisabled();
            Assert.IsTrue(results.Wait(WaitTime));
            Assert.IsNull(results.Result);
            _sasHost.Verify();
        }
    }
}