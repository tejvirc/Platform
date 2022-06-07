namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Sas.VoucherValidation;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Storage.Models;
    using Storage.Repository;

    [TestClass]
    public class EnhancedValidationProviderTests
    {
        private const long ExpectedCashableAmountMilliCents = 1000000;
        private const long ExpectedPromoAmountMilliCents = 500000;
        private const long ExpectedNonCashAmountMilliCents = 20000;
        private const ulong ExpectedValidationNumber = 6429188185446104;
        private const ulong ExpectedValidationSystemId = 3;
        private const string Barcode1 = "036429188185446104";

        private static readonly VoucherOutTransaction ValidReadResult = new VoucherOutTransaction(
            0,
            new DateTime(2019, 12, 21, 15, 31, 44),
            ExpectedCashableAmountMilliCents,
            AccountType.NonCash,
            Barcode1,
            SasConstants.MaxTicketExpirationDays,
            4,
            "")
        {
            VoucherSequence = 3,
            HostAcknowledged = false,
            HostSequence = 62,
            TransactionId = 1000
        };

        private static readonly SendEnhancedValidationInformationResponse ValidResponse =
            new SendEnhancedValidationInformationResponse
            {
                TicketNumber = 3,
                Amount = ExpectedCashableAmountMilliCents.MillicentsToCents(),
                ExpirationDate = SasConstants.MaxTicketExpirationDays,
                ValidationType = (int)TicketValidationType.RestrictedPromotionalTicketFromCashOut,
                PoolId = 4,
                Index = 31,
                Successful = true,
                ValidationDate = new DateTime(2019, 12, 21, 15, 31, 44).ToLocalTime(),
                ValidationNumber = ExpectedValidationNumber,
                ValidationSystemId = ExpectedValidationSystemId
            };

        private static readonly SendEnhancedValidationInformationResponse ValidNoBarcodeResponse =
            new SendEnhancedValidationInformationResponse
            {
                TicketNumber = 3,
                Amount = ExpectedCashableAmountMilliCents.MillicentsToCents(),
                ExpirationDate = SasConstants.MaxTicketExpirationDays,
                ValidationType = (int)TicketValidationType.RestrictedPromotionalTicketFromCashOut,
                PoolId = 4,
                Index = 3,
                Successful = true,
                ValidationDate = new DateTime(2019, 12, 21, 15, 31, 44).ToLocalTime(),
                ValidationNumber = 0,
                ValidationSystemId = 0
            };

        private static IEnumerable<object[]> ParseValidFunctionCodesData => new List<object[]>
        {
            new object[]
            {
                "Current RestrictedPromotionalTicketFromCashOut",
                SasConstants.CurrentValidation,
                new List<ITransaction> { ValidReadResult },
                ValidResponse
            },
            new object[]
            {
                "Max RestrictedPromotionalTicketFromCashOut",
                SasConstants.MaxValidationIndex,
                new List<ITransaction> { ValidReadResult },
                ValidResponse
            },
            new object[]
            {
                "Ahead RestrictedPromotionalTicketFromCashOut",
                SasConstants.LookAhead,
                new List<ITransaction> { ValidReadResult },
                ValidResponse
            },
            new object[]
            {
                "HandPayFromCashOutNoReceipt",
                SasConstants.CurrentValidation,
                new List<ITransaction>
                {
                    new HandpayTransaction(
                        0,
                        new DateTime(2019, 12, 21, 15, 31, 44),
                        ExpectedCashableAmountMilliCents,
                        ExpectedPromoAmountMilliCents,
                        ExpectedNonCashAmountMilliCents,
                        123,
                        HandpayType.CancelCredit,
                        false,
                        Guid.NewGuid())
                    {
                        ReceiptSequence = 4,
                        State = HandpayState.Committed,
                        HostSequence = 58,
                        Barcode = Barcode1
                    }
                },
                new SendEnhancedValidationInformationResponse
                {
                    TicketNumber = ushort.MaxValue,
                    Amount = ExpectedCashableAmountMilliCents.MillicentsToCents() +
                             ExpectedPromoAmountMilliCents.MillicentsToCents() +
                             ExpectedNonCashAmountMilliCents.MillicentsToCents(),
                    ExpirationDate = 0,
                    ValidationType = (int)TicketValidationType.HandPayFromCashOutNoReceipt,
                    PoolId = 0,
                    Index = 27,
                    Successful = true,
                    ValidationDate = new DateTime(2019, 12, 21, 15, 31, 44).ToLocalTime(),
                    ValidationNumber = ExpectedValidationNumber,
                    ValidationSystemId = ExpectedValidationSystemId
                }
            },
            new object[]
            {
                "HandPayFromCashOutReceiptPrinted",
                SasConstants.CurrentValidation,
                new List<ITransaction>
                {
                    new HandpayTransaction(
                        0,
                        new DateTime(2019, 12, 21, 15, 31, 44),
                        ExpectedCashableAmountMilliCents,
                        ExpectedPromoAmountMilliCents,
                        ExpectedNonCashAmountMilliCents,
                        123,
                        HandpayType.CancelCredit,
                        true,
                        Guid.NewGuid())
                    {
                        ReceiptSequence = 4,
                        Expiration = 100,
                        State = HandpayState.Committed,
                        HostSequence = 58,
                        Printed = true,
                        Barcode = Barcode1
                    }
                },
                new SendEnhancedValidationInformationResponse
                {
                    TicketNumber = 4,
                    Amount = ExpectedCashableAmountMilliCents.MillicentsToCents() +
                             ExpectedPromoAmountMilliCents.MillicentsToCents() +
                             ExpectedNonCashAmountMilliCents.MillicentsToCents(),
                    ExpirationDate = 100,
                    ValidationType = (int)TicketValidationType.HandPayFromCashOutReceiptPrinted,
                    PoolId = 0,
                    Index = 27,
                    Successful = true,
                    ValidationDate = new DateTime(2019, 12, 21, 15, 31, 44).ToLocalTime(),
                    ValidationNumber = ExpectedValidationNumber,
                    ValidationSystemId = ExpectedValidationSystemId
                }
            },
            new object[]
            {
                "HandPayFromWinNoReceipt",
                SasConstants.CurrentValidation,
                new List<ITransaction>
                {
                    new HandpayTransaction(
                        0,
                        new DateTime(2019, 12, 21, 15, 31, 44),
                        ExpectedCashableAmountMilliCents,
                        ExpectedPromoAmountMilliCents,
                        ExpectedNonCashAmountMilliCents,
                        123,
                        HandpayType.GameWin,
                        false,
                        Guid.NewGuid())
                    {
                        ReceiptSequence = 4,
                        State = HandpayState.Committed,
                        HostSequence = 58,
                        Printed = false,
                        Barcode = Barcode1
                    }
                },
                new SendEnhancedValidationInformationResponse
                {
                    TicketNumber = ushort.MaxValue,
                    Amount = ExpectedCashableAmountMilliCents.MillicentsToCents() +
                             ExpectedPromoAmountMilliCents.MillicentsToCents() +
                             ExpectedNonCashAmountMilliCents.MillicentsToCents(),
                    ExpirationDate = 0,
                    ValidationType = (int)TicketValidationType.HandPayFromWinNoReceipt,
                    PoolId = 0,
                    Index = 27,
                    Successful = true,
                    ValidationDate = new DateTime(2019, 12, 21, 15, 31, 44).ToLocalTime(),
                    ValidationNumber = ExpectedValidationNumber,
                    ValidationSystemId = ExpectedValidationSystemId
                }
            },
            new object[]
            {
                "HandPayFromWinReceiptPrinted",
                SasConstants.CurrentValidation,
                new List<ITransaction>
                {
                    new HandpayTransaction(
                        0,
                        new DateTime(2019, 12, 21, 15, 31, 44),
                        ExpectedCashableAmountMilliCents,
                        ExpectedPromoAmountMilliCents,
                        ExpectedNonCashAmountMilliCents,
                        123,
                        HandpayType.BonusPay,
                        true,
                        Guid.NewGuid())
                    {
                        ReceiptSequence = 4,
                        State = HandpayState.Committed,
                        HostSequence = 58,
                        Printed = true,
                        Expiration = 0,
                        Barcode = Barcode1
                    }
                },
                new SendEnhancedValidationInformationResponse
                {
                    TicketNumber = 4,
                    Amount = ExpectedCashableAmountMilliCents.MillicentsToCents() +
                             ExpectedPromoAmountMilliCents.MillicentsToCents() +
                             ExpectedNonCashAmountMilliCents.MillicentsToCents(),
                    ExpirationDate = SasConstants.MaxTicketExpirationDays,
                    ValidationType = (int)TicketValidationType.HandPayFromWinReceiptPrinted,
                    PoolId = 0,
                    Index = 27,
                    Successful = true,
                    ValidationDate = new DateTime(2019, 12, 21, 15, 31, 44).ToLocalTime(),
                    ValidationNumber = ExpectedValidationNumber,
                    ValidationSystemId = ExpectedValidationSystemId
                }
            },
            new object[]
            {
                "Ahead RestrictedPromotionalTicketFromCashOut Too Short Barcode",
                SasConstants.LookAhead,
                new List<ITransaction>
                {
                    new VoucherOutTransaction(
                        0,
                        new DateTime(2019, 12, 21, 15, 31, 44),
                        ExpectedCashableAmountMilliCents,
                        AccountType.NonCash,
                        "12",
                        SasConstants.MaxTicketExpirationDays,
                        4,
                        "")
                    {
                        VoucherSequence = 3,
                        HostAcknowledged = false,
                        HostSequence = 34
                    }
                },
                ValidNoBarcodeResponse
            },
            new object[]
            {
                "Ahead RestrictedPromotionalTicketFromCashOut Null Barcode",
                SasConstants.LookAhead,
                new List<ITransaction>
                {
                    new VoucherOutTransaction(
                        0,
                        new DateTime(2019, 12, 21, 15, 31, 44),
                        ExpectedCashableAmountMilliCents,
                        AccountType.NonCash,
                        null,
                        SasConstants.MaxTicketExpirationDays,
                        4,
                        "")
                    {
                        VoucherSequence = 3,
                        HostAcknowledged = false,
                        HostSequence = 34
                    }
                },
                ValidNoBarcodeResponse
            },
            new object[]
            {
                "Ahead RestrictedPromotionalTicketFromCashOut Non-digit Barcode",
                SasConstants.LookAhead,
                new List<ITransaction>
                {
                    new VoucherOutTransaction(
                        0,
                        new DateTime(2019, 12, 21, 15, 31, 44),
                        ExpectedCashableAmountMilliCents,
                        AccountType.NonCash,
                        "ABCDEFGHIJKLMNOP",
                        SasConstants.MaxTicketExpirationDays,
                        4,
                        "")
                    {
                        VoucherSequence = 3,
                        HostAcknowledged = false,
                        HostSequence = 34
                    }
                },
                ValidNoBarcodeResponse
            },
            new object[]
            {
                "Current Voucher and Handpay",
                SasConstants.CurrentValidation,
                new List<ITransaction>
                {
                    ValidReadResult,
                    new HandpayTransaction(
                        0,
                        ValidReadResult.TransactionDateTime.AddSeconds(1.0),
                        ExpectedCashableAmountMilliCents,
                        ExpectedPromoAmountMilliCents,
                        ExpectedNonCashAmountMilliCents,
                        123,
                        HandpayType.GameWin,
                        false,
                        Guid.NewGuid())
                    {
                        ReceiptSequence = 4,
                        State = HandpayState.Committed,
                        HostSequence = ValidReadResult.HostSequence + 1,
                        Printed = false,
                        TransactionId = ValidReadResult.TransactionId + 1,
                        Barcode = Barcode1
                    }
                },
                ValidResponse
            }
        };

        private EnhancedValidationProvider _target;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ISasTicketPrintedHandler> _sasTicketPrintedHandler;
        private Mock<IStorageDataProvider<EnhancedValidationItem>> _storageDataProvider;
        private Mock<ITransactionHistory> _history;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);
            _sasTicketPrintedHandler = new Mock<ISasTicketPrintedHandler>(MockBehavior.Default);
            _storageDataProvider = new Mock<IStorageDataProvider<EnhancedValidationItem>>(MockBehavior.Default);

            _storageDataProvider.Setup(x => x.GetData()).Returns(new EnhancedValidationItem
            {
                EnhancedValidationDataLog = string.Empty
            });

            _history = new Mock<ITransactionHistory>();
            _history.Setup(x => x.RecallTransactions(It.IsAny<bool>()))
                .Returns(Enumerable.Empty<ITransaction>().OrderBy(x => x.TransactionId));
            _target = CreateTarget();
        }

        [DataRow(true, false, false, false)]
        [DataRow(false, true, false, false)]
        [DataRow(false, false, true, false)]
        [DataRow(false, false, false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullArgumentsTest(
            bool nullProperties,
            bool nullTicketHandler,
            bool nullStorage,
            bool nullHistory)
        {
            _target = CreateTarget(nullProperties, nullTicketHandler, nullStorage, nullHistory);
        }

        [DynamicData(nameof(ParseValidFunctionCodesData))]
        [DataTestMethod]
        public void ValidFunctionCodeHandler(
            string displayName,
            int functionCode,
            List<ITransaction> readResults,
            SendEnhancedValidationInformationResponse expectedResults)
        {
            _propertiesManager.Setup(c => c.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { ValidationType = SasValidationType.SecureEnhanced });

            AddResultsToTarget(readResults);

            var response = _target.GetResponseFromInfo(new SendEnhancedValidationInformation { FunctionCode = functionCode });

            Assert.IsTrue(response.Successful);
            Assert.AreEqual(expectedResults.ValidationType, response.ValidationType);
            Assert.AreEqual(expectedResults.Amount, response.Amount);
            Assert.AreEqual(expectedResults.ExpirationDate, response.ExpirationDate);
            Assert.AreEqual(expectedResults.Index, response.Index);
            Assert.AreEqual(expectedResults.PoolId, response.PoolId);
            Assert.AreEqual(expectedResults.TicketNumber, response.TicketNumber);
            Assert.AreEqual(expectedResults.ValidationDate, response.ValidationDate);
            Assert.AreEqual(expectedResults.ValidationNumber, response.ValidationNumber);
            Assert.AreEqual(expectedResults.ValidationSystemId, response.ValidationSystemId);
        }

        [TestMethod]
        public void NoReadResultsFailsHandler()
        {
            _propertiesManager.Setup(c => c.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { ValidationType = SasValidationType.SecureEnhanced });

            var response = _target.GetResponseFromInfo(new SendEnhancedValidationInformation
            {
                FunctionCode = SasConstants.CurrentValidation
            });

            Assert.IsFalse(response.Successful);
        }

        [TestMethod]
        public void AckNackHandlerTest()
        {
            var readResults = new List<ITransaction> { ValidReadResult };
            _propertiesManager.Setup(c => c.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { ValidationType = SasValidationType.SecureEnhanced });

            AddResultsToTarget(readResults);

            var response = _target.GetResponseFromInfo(new SendEnhancedValidationInformation { FunctionCode = SasConstants.CurrentValidation });

            // setups for Ack handler
            _sasTicketPrintedHandler.Setup(m => m.TicketPrintedAcknowledged()).Verifiable();

            // Invoke Ack handler and verify setups matched
            response.Handlers.ImpliedAckHandler.Invoke();

            // setups for Nack handler
            _sasTicketPrintedHandler.Setup(m => m.ClearPendingTicketPrinted()).Verifiable();

            // Invoke Nack handler and verify setups matched
            response.Handlers.ImpliedNackHandler.Invoke();

            _sasTicketPrintedHandler.Verify();
        }

        private EnhancedValidationProvider CreateTarget(
            bool nullProperties = false,
            bool nullTicketHandler = false,
            bool nullStorage = false,
            bool nullHistory = false)
        {
            return new EnhancedValidationProvider(
                nullProperties ? null : _propertiesManager.Object,
                nullTicketHandler ? null : _sasTicketPrintedHandler.Object,
                nullStorage ? null : _storageDataProvider.Object,
                nullHistory ? null : _history.Object);
        }

        private void AddResultsToTarget(IReadOnlyList<ITransaction> results)
        {
            _history.Setup(x => x.RecallTransactions(It.IsAny<bool>())).Returns(results.OrderBy(x => x.TransactionId));
            foreach (var result in results)
            {
                switch (result)
                {
                    case VoucherOutTransaction transaction:
                        _target.HandleTicketOutCompleted(transaction);
                        break;
                    case HandpayTransaction handpayTransaction:
                        _target.HandPayReset(handpayTransaction);
                        break;
                }
            }
        }
    }
}