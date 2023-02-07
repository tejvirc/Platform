namespace Aristocrat.Monaco.Bingo.Commands.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Bingo.Client.Messages;
    using Common;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Services.Reporting;

    [TestClass]
    public class ReportEgmStatusCommandHandlerTests
    {
        private const string SerialNumber = "123";

        private readonly Mock<IStatusReportingService> _statusResponseService = new(MockBehavior.Default);
        private readonly Mock<IMeterManager> _meterManager = new(MockBehavior.Strict);
        private readonly Mock<IMeter> _meterCashOut = new(MockBehavior.Loose);
        private readonly Mock<IMeter> _meterCashIn = new(MockBehavior.Loose);
        private readonly Mock<IMeter> _meterCashWon = new(MockBehavior.Loose);
        private readonly Mock<IMeter> _meterCashPlayed = new(MockBehavior.Loose);
        private readonly Mock<IPropertiesManager> _propertyManger = new(MockBehavior.Default);
        private readonly Mock<IEgmStatusService> _statusService = new(MockBehavior.Default);
        private ReportEgmStatusCommandHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _statusService.Setup(x => x.GetCurrentEgmStatus()).Returns(0);
            _meterManager.Setup(x => x.GetMeter(ApplicationMeters.TotalOut)).Returns(_meterCashOut.Object);
            _meterManager.Setup(x => x.GetMeter(ApplicationMeters.TotalIn)).Returns(_meterCashIn.Object);
            _meterManager.Setup(x => x.GetMeter(GamingMeters.TotalPaidAmt)).Returns(_meterCashWon.Object);
            _meterManager.Setup(x => x.GetMeter(GamingMeters.WageredAmount)).Returns(_meterCashPlayed.Object);
            _propertyManger.Setup(x => x.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<string>()))
                .Returns(SerialNumber);

            _target = CreateTarget();
        }

        [DataRow(true, false, false, false, DisplayName = "Response Service Null")]
        [DataRow(false, true, false, false, DisplayName = "Meter Manager Null")]
        [DataRow(false, false, true, false, DisplayName = "Properties Null")]
        [DataRow(false, false, false, true, DisplayName = "Status Service Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorArgumentsTest(bool nullResponse, bool nullMeter, bool nullProperties, bool nullStatus)
        {
            CreateTarget(nullResponse, nullMeter, nullProperties, nullStatus);
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
            var expected = new StatusMessage(
                SerialNumber,
                cashPlayedAmount,
                cashWonAmount,
                cashInAmount,
                cashOutAmount,
                (int)egmStatusFlag);
            var comparer = new StatusMessageComparer();

            _statusResponseService
                .Setup(m => m.ReportStatus(It.IsAny<StatusMessage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(1)).Verifiable();
            await _target.Handle(new ReportEgmStatusCommand(), CancellationToken.None);
            _statusResponseService.Verify(
                s => s.ReportStatus(
                    It.Is<StatusMessage>(m => comparer.Equals(m, expected)),
                    It.IsAny<CancellationToken>()),
                Times.Once());
        }

        private ReportEgmStatusCommandHandler CreateTarget(
            bool nullResponse = false,
            bool nullMeter = false,
            bool nullProperties = false,
            bool nullStatus = false)
        {
            return new ReportEgmStatusCommandHandler(
                nullResponse ? null : _statusResponseService.Object,
                nullMeter ? null : _meterManager.Object,
                nullProperties ? null : _propertyManger.Object,
                nullStatus ? null : _statusService.Object);
        }

        private class StatusMessageComparer : IEqualityComparer<StatusMessage>
        {
            public bool Equals(StatusMessage x, StatusMessage y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (ReferenceEquals(x, null))
                {
                    return false;
                }

                if (ReferenceEquals(y, null))
                {
                    return false;
                }

                if (x.GetType() != y.GetType())
                {
                    return false;
                }

                return x.MachineSerial == y.MachineSerial && x.CashPlayedMeterValue == y.CashPlayedMeterValue &&
                       x.CashWonMeterValue == y.CashWonMeterValue && x.CashInMeterValue == y.CashInMeterValue &&
                       x.CashOutMeterValue == y.CashOutMeterValue && x.EgmStatusFlags == y.EgmStatusFlags;
            }

            public int GetHashCode(StatusMessage obj)
            {
                unchecked
                {
                    var hashCode = (obj.MachineSerial != null ? obj.MachineSerial.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ obj.CashPlayedMeterValue.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.CashWonMeterValue.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.CashInMeterValue.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.CashOutMeterValue.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.EgmStatusFlags;
                    return hashCode;
                }
            }
        }
    }
}