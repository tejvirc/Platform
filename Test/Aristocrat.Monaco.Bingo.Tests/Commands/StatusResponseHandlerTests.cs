namespace Aristocrat.Monaco.Bingo.Commands.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Bingo.Client.Messages;
    using Common;
    using Gaming.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Services.Reporting;

    [TestClass]
    public class StatusResponseHandlerTests
    {
        private readonly Mock<ICommandService> _statusResponseService = new(MockBehavior.Default);
        private readonly Mock<IMeterManager> _meterManager = new(MockBehavior.Strict);
        private readonly Mock<IMeter> _meterCashOut = new(MockBehavior.Loose);
        private readonly Mock<IMeter> _meterCashIn = new(MockBehavior.Loose);
        private readonly Mock<IMeter> _meterCashWon = new(MockBehavior.Loose);
        private readonly Mock<IMeter> _meterCashPlayed = new(MockBehavior.Loose);
        private readonly Mock<IEgmStatusService> _statusService = new(MockBehavior.Default);
        private StatusResponseHandler _target;

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
            const long cashOutAmount = 100_00;
            const long cashInAmount = 120_00;
            const long cashWonAmount = 150_00;
            const long cashPlayedAmount = 123_00;
            const EgmStatusFlag egmStatusFlag = EgmStatusFlag.OnLine | EgmStatusFlag.DisabledByProgCntrl;

            _meterCashOut.Setup(x => x.Lifetime).Returns(cashOutAmount.CentsToMillicents());
            _meterCashIn.Setup(x => x.Lifetime).Returns(cashInAmount.CentsToMillicents());
            _meterCashWon.Setup(x => x.Lifetime).Returns(cashWonAmount.CentsToMillicents());
            _meterCashPlayed.Setup(x => x.Lifetime).Returns(cashPlayedAmount.CentsToMillicents());
            _statusService.Setup(x => x.GetCurrentEgmStatus()).Returns(egmStatusFlag);

            var message = new StatusResponseMessage("123");
            _statusResponseService
                .Setup(m => m.ReportStatus(It.IsAny<StatusResponseMessage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(1)).Verifiable();
            await _target.Handle(message, CancellationToken.None);
            _statusResponseService.Verify(
                s => s.ReportStatus(
                    It.Is<StatusResponseMessage>(
                        m => ValidMessage(
                            m,
                            egmStatusFlag,
                            cashInAmount,
                            cashOutAmount,
                            cashPlayedAmount,
                            cashWonAmount)),
                    It.IsAny<CancellationToken>()),
                Times.Once());
        }

        private static bool ValidMessage(
            StatusResponseMessage message,
            EgmStatusFlag egmStatusFlag,
            long cashInAmount,
            long cashOutAmount,
            long cashPlayedAmount,
            long cashWonAmount)
        {
            return message.EgmStatusFlags == (int)egmStatusFlag && message.CashInMeterValue == cashInAmount &&
                   message.CashOutMeterValue == cashOutAmount && message.CashPlayedMeterValue == cashPlayedAmount &&
                   message.CashWonMeterValue == cashWonAmount;
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