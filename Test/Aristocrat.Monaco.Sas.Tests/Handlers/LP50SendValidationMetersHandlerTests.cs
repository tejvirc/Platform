namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Aristocrat.Sas.Client.Metering;
    using Contracts.SASProperties;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    [TestClass]
    public class LP50SendValidationMetersHandlerTests
    {
        private static IEnumerable<object[]> ValidationHandlerTests => new List<object[]>
        {
            // Cash out Support
            new object[]
            {
                SasValidationType.SecureEnhanced,
                TicketValidationType.CashableTicketFromCashOutOrWin,
                50L,
                200L,
                new List<(string, long)>
                {
                    (AccountingMeters.VoucherOutCashableCount, 20),
                    (AccountingMeters.VoucherOutCashablePromoCount, 30),
                    (AccountingMeters.VoucherOutCashableAmount, 150000),
                    (AccountingMeters.VoucherOutCashablePromoAmount, 50000)
                }
            },
            new object[]
            {
                SasValidationType.SecureEnhanced,
                TicketValidationType.RestrictedPromotionalTicketFromCashOut,
                35L,
                320L,
                new List<(string, long)>
                {
                    (AccountingMeters.VoucherOutNonCashableCount, 35),
                    (AccountingMeters.VoucherOutNonCashableAmount, 320000),
                }
            },
            // Ticket in support
            new object[]
            {
                SasValidationType.SecureEnhanced,
                TicketValidationType.CashableTicketRedeemed,
                35L,
                320L,
                new List<(string, long)>
                {
                    (AccountingMeters.VoucherInCashableCount, 35),
                    (AccountingMeters.VoucherInCashableAmount, 320000),
                }
            },
            new object[]
            {
                SasValidationType.SecureEnhanced,
                TicketValidationType.RestrictedPromotionalTicketRedeemed,
                35L,
                320L,
                new List<(string, long)>
                {
                    (AccountingMeters.VoucherInNonCashableCount, 35),
                    (AccountingMeters.VoucherInNonCashableAmount, 320000),
                }
            },
            new object[]
            {
                SasValidationType.SecureEnhanced,
                TicketValidationType.NonRestrictedPromotionalTicketRedeemed,
                35L,
                320L,
                new List<(string, long)>
                {
                    (AccountingMeters.VoucherInCashablePromoCount, 35),
                    (AccountingMeters.VoucherInCashablePromoAmount, 320000),
                }
            },
            // Hand pay support
            new object[]
            {
                SasValidationType.SecureEnhanced,
                TicketValidationType.HandPayFromCashOutNoReceipt,
                3L,
                6000L,
                new List<(string, long)>
                {
                    (AccountingMeters.HandpaidValidatedCancelNoReceiptCount, 3),
                    (AccountingMeters.HandpaidValidatedCancelNoReceiptAmount, 6000000),
                }
            },
            new object[]
            {
                SasValidationType.SecureEnhanced,
                TicketValidationType.HandPayFromCashOutReceiptPrinted,
                3L,
                6000L,
                new List<(string, long)>
                {
                    (AccountingMeters.HandpaidValidatedCancelReceiptCount, 3),
                    (AccountingMeters.HandpaidValidatedCancelReceiptAmount, 6000000),
                }
            },
            new object[]
            {
                SasValidationType.SecureEnhanced,
                TicketValidationType.HandPayFromWinNoReceipt,
                3L,
                6000L,
                new List<(string, long)>
                {
                    (SasMeterCollection.SasMeterForCode(SasMeterId.ValidatedJackpotHandPayNoReceiptCount).MappedMeterName, 3),
                    (SasMeterCollection.SasMeterForCode(SasMeterId.ValidatedJackpotHandPayNoReceiptCents).MappedMeterName, 6000000),
                }
            },
            new object[]
            {
                SasValidationType.SecureEnhanced,
                TicketValidationType.HandPayFromWinReceiptPrinted,
                3L,
                6000L,
                new List<(string, long)>
                {
                    (SasMeterCollection.SasMeterForCode(SasMeterId.ValidatedJackpotHandPayReceiptCount).MappedMeterName, 3),
                    (SasMeterCollection.SasMeterForCode(SasMeterId.ValidatedJackpotHandPayReceiptCents).MappedMeterName, 6000000),
                }
            },
            // Unsupported meters tests
            new object[]
            {
                SasValidationType.SecureEnhanced,
                TicketValidationType.DebitTicketFromAftTransfer,
                0L,
                0L,
                new List<(string, long)>()
            },
            new object[]
            {
                SasValidationType.SecureEnhanced,
                TicketValidationType.CashableTicketFromAftTransfer,
                0L,
                0L,
                new List<(string, long)>()
            },
            new object[]
            {
                SasValidationType.SecureEnhanced,
                TicketValidationType.RestrictedTicketFromAftTransfer,
                0L,
                0L,
                new List<(string, long)>()
            },
            // Non Secure Enhanced Query Secure Enhanced only meters
            new object[]
            {
                SasValidationType.System,
                TicketValidationType.HandPayFromCashOutReceiptPrinted,
                0L,
                0L,
                new List<(string, long)>()
            },
            new object[]
            {
                SasValidationType.System,
                TicketValidationType.HandPayFromWinNoReceipt,
                0L,
                0L,
                new List<(string, long)>()
            },
            new object[]
            {
                SasValidationType.System,
                TicketValidationType.HandPayFromCashOutNoReceipt,
                0L,
                0L,
                new List<(string, long)>()
            },
            new object[]
            {
                SasValidationType.System,
                TicketValidationType.HandPayFromWinReceiptPrinted,
                0L,
                0L,
                new List<(string, long)>()
            },
            new object[]
            {
                SasValidationType.None,
                TicketValidationType.HandPayFromCashOutReceiptPrinted,
                0L,
                0L,
                new List<(string, long)>()
            },
            new object[]
            {
                SasValidationType.None,
                TicketValidationType.HandPayFromWinNoReceipt,
                0L,
                0L,
                new List<(string, long)>()
            },
            new object[]
            {
                SasValidationType.None,
                TicketValidationType.HandPayFromCashOutNoReceipt,
                0L,
                0L,
                new List<(string, long)>()
            },
            new object[]
            {
                SasValidationType.None,
                TicketValidationType.HandPayFromWinReceiptPrinted,
                0L,
                0L,
                new List<(string, long)>()
            }
        };

        private LP50SendValidationMetersHandler _target;
        private Mock<IMeterManager> _meterManager;
        private Mock<IPropertiesManager> _propertiesManager;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _meterManager = new Mock<IMeterManager>(MockBehavior.Default);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _target = new LP50SendValidationMetersHandler(_meterManager.Object, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullMeterManagerTest()
        {
            _target = new LP50SendValidationMetersHandler(null, _propertiesManager.Object);
            Assert.Fail("Should have thrown an ArgumentNullException");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPropertiesManagerTest()
        {
            _target = new LP50SendValidationMetersHandler(_meterManager.Object, null);
            Assert.Fail("Should have thrown an ArgumentNullException");
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendValidationMeters));
        }

        [DynamicData(nameof(ValidationHandlerTests))]
        [DataTestMethod]
        public void HandleTest(
            SasValidationType sasValidationType,
            TicketValidationType ticketValidationType,
            long expectedValidationCount,
            long expectedValidationAmount,
            List<(string meterName, long meterValue)> meters)
        {
            foreach (var (name, value) in meters)
            {
                var meter = new Mock<IMeter>(MockBehavior.Strict);
                meter.Setup(x => x.Lifetime).Returns(value);
                _meterManager.Setup(x => x.GetMeter(name)).Returns(meter.Object);
                _meterManager.Setup(x => x.IsMeterProvided(name)).Returns(true);
            }

            _propertiesManager.Setup(c => c.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { ValidationType = sasValidationType });

            var response = _target.Handle(new LongPollSingleValueData<TicketValidationType>(ticketValidationType));
            Assert.AreEqual(expectedValidationCount, response.ValidationCount);
            Assert.AreEqual(expectedValidationAmount, response.ValidationTotalAmount);
        }
    }
}