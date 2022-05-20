namespace Aristocrat.Monaco.Bingo.Commands.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Metering;
    using Aristocrat.Monaco.Bingo.Commands;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class StatusResponseHandlerTests
    {
        private readonly Mock<ICommandService> _statusResponseService = new Mock<ICommandService>(MockBehavior.Default);
        private readonly Mock<IMeterManager> _meterManager = new Mock<IMeterManager>(MockBehavior.Strict);
        private readonly Mock<IMeter> _meterCashOut = new Mock<IMeter>(MockBehavior.Loose);
        private readonly Mock<IMeter> _meterCashIn = new Mock<IMeter>(MockBehavior.Loose);
        private readonly Mock<IMeter> _meterCashWon = new Mock<IMeter>(MockBehavior.Loose);
        private readonly Mock<IMeter> _meterCashPlayed = new Mock<IMeter>(MockBehavior.Loose);
        private readonly Mock<IEgmStatusService> _statusService = new Mock<IEgmStatusService>(MockBehavior.Default);
        public StatusResponseHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _statusService.Setup(x => x.GetCurrentEgmStatus()).Returns(0);
            _meterManager.Setup(x => x.GetMeter(ApplicationMeters.TotalOut)).Returns(_meterCashOut.Object);
            _meterManager.Setup(x => x.GetMeter(ApplicationMeters.TotalIn)).Returns(_meterCashIn.Object);
            _meterManager.Setup(x => x.GetMeter(GamingMeters.TotalPaidAmt)).Returns(_meterCashWon.Object);
            _meterManager.Setup(x => x.GetMeter(GamingMeters.WageredAmount)).Returns(_meterCashPlayed.Object);

            _target = CreateTarget();
        }

        [DataRow(true, false, false, DisplayName = "Response Service Null")]
        [DataRow(false, true, false, DisplayName = "Meter Manager Null")]
        [DataRow(false, false, true, DisplayName = "Status Service Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorArgumentsTest(bool nullResponse, bool nullMeter, bool nullStatus)
        {
            CreateTarget(nullResponse, nullMeter, nullStatus);
        }

        [TestMethod]
        public async Task StatusHandleTest()
        {
            var message = new StatusResponseMessage("123");
            CancellationToken token = CancellationToken.None;

            _statusResponseService.Setup(m => m.ReportStatus(message, token)).Returns(Task.FromResult(1)).Verifiable();

            await _target.Handle(message);

            _statusResponseService.Verify(m => m.ReportStatus(message, token), Times.Once());
        }

        private StatusResponseHandler CreateTarget(bool nullResponse = false, bool nullMeter = false, bool nullStatus = false)
        {
            return new StatusResponseHandler(
                nullResponse ? null : _statusResponseService.Object,
                nullMeter ? null : _meterManager.Object,
                nullStatus ? null : _statusService.Object);
        }
    }
}