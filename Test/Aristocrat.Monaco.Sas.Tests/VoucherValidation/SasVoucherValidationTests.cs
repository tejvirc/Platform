namespace Aristocrat.Monaco.Sas.Tests.VoucherValidation
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.TransferOut;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.VoucherValidation;

    [TestClass]
    public class SasVoucherValidationTests
    {
        private const int WaitTime = 1000;

        private static IEnumerable<object[]> RedemptionTest => new List<object[]>
        {
            new object[]
            {
                100L.CentsToMillicents(),
                AccountType.NonCash,
                true,
                false,
                new TicketInInfo
                {
                    Amount = 100,
                    Barcode = "123456",
                    TransferCode = TicketTransferCode.ValidRestrictedPromotionalTicket
                }
            },
            new object[]
            {
                100L.CentsToMillicents(),
                AccountType.Cashable,
                true,
                false,
                new TicketInInfo
                {
                    Amount = 100,
                    Barcode = "123456",
                    TransferCode = TicketTransferCode.ValidCashableTicket
                }
            },
            new object[]
            {
                100L.CentsToMillicents(),
                AccountType.Promo,
                true,
                false,
                new TicketInInfo
                {
                    Amount = 100,
                    Barcode = "123456",
                    TransferCode = TicketTransferCode.ValidNonRestrictedPromotionalTicket
                }
            },
            new object[]
            {
                0,
                AccountType.Cashable,
                false,
                false,
                null
            },
            new object[]
            {
                0,
                AccountType.Cashable,
                true,
                true,
                null
            },
            new object[]
            {
                0,
                AccountType.Cashable,
                true,
                false,
                null
            }
        };

        private static IEnumerable<object[]> VoucherIssueTest => new List<object[]>
        {
            new object[]
            {
                new VoucherAmount(1000L, 0, 0),
                new TicketOutInfo
                {
                    Barcode = "12345678",
                    Pool = 12,
                    Amount = 100,
                    Time = DateTime.Now
                },
                AccountType.NonCash,
                TicketType.Restricted
            },
            new object[]
            {
                new VoucherAmount(1000L, 0, 0),
                new TicketOutInfo
                {
                    Barcode = "12345678",
                    Pool = 0,
                    Amount = 100,
                    Time = DateTime.Now
                },
                AccountType.Cashable,
                TicketType.CashOut
            },
            new object[]
            {
                new VoucherAmount(1000L, 0, 0),
                new TicketOutInfo
                {
                    Barcode = "12345678",
                    Pool = 0,
                    Amount = 100,
                    Time = DateTime.Now
                },
                AccountType.Promo,
                TicketType.CashOut
            },
            new object[]
            {
                new VoucherAmount(1000L, 0, 0),
                null,
                AccountType.Cashable,
                TicketType.CashOut
            }
        };

        private SasVoucherValidation _target;
        private Mock<ISasHost> _sasHost;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IIdProvider> _idProvider;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _sasHost = new Mock<ISasHost>(MockBehavior.Default);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _idProvider = new Mock<IIdProvider>(MockBehavior.Default);

            _propertiesManager.Setup(
                    x => x.GetProperty(VoucherValidationConstants.TicketLocationKey, It.IsAny<string>()))
                .Returns(string.Empty);
            _propertiesManager.Setup(
                    x => x.GetProperty(VoucherValidationConstants.TicketAddressLine1Key, It.IsAny<string>()))
                .Returns(string.Empty);
            _propertiesManager.Setup(
                    x => x.GetProperty(VoucherValidationConstants.TicketAddressLine2Key, It.IsAny<string>()))
                .Returns(string.Empty);
            _propertiesManager.Setup(
                    x => x.GetProperty(AccountingConstants.TicketTitleNonCash, It.IsAny<string>()))
                .Returns(string.Empty);

            _target = new SasVoucherValidation(_sasHost.Object, _propertiesManager.Object, _idProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullSasHostTest()
        {
            _target = new SasVoucherValidation(null, _propertiesManager.Object, _idProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPropertiesManagerTest()
        {
            _target = new SasVoucherValidation(_sasHost.Object, null, _idProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullIdProviderTest()
        {
            _target = new SasVoucherValidation(_sasHost.Object, _propertiesManager.Object, null);
        }

        [TestMethod]
        public void ServiceTest()
        {
            var targetServiceTypes = _target.ServiceTypes;
            Assert.AreEqual(2, targetServiceTypes.Count);
            Assert.IsTrue(targetServiceTypes.Contains(typeof(ITicketDataProvider)));
            Assert.IsTrue(targetServiceTypes.Contains(typeof(IVoucherValidator)));
            Assert.AreEqual(typeof(SasVoucherValidation).ToString(), _target.Name);
        }

        [TestMethod]
        public void SetTicketDataTest()
        {
            const string locationText = "Test Loc";
            const string addressLine1Text = "Address Line 1";
            const string addressLine2Text = "Address Line 2";
            const string nonCashTicketTile = "No cash title";

            _propertiesManager.Setup(
                    x => x.SetProperty(VoucherValidationConstants.TicketLocationKey, locationText))
                .Verifiable();
            _propertiesManager.Setup(
                    x => x.SetProperty(VoucherValidationConstants.TicketAddressLine1Key, addressLine1Text))
                .Verifiable();
            _propertiesManager.Setup(
                    x => x.SetProperty(VoucherValidationConstants.TicketAddressLine2Key, addressLine2Text))
                .Verifiable();
            _propertiesManager.Setup(
                    x => x.SetProperty(AccountingConstants.TicketTitleNonCash, nonCashTicketTile))
                .Verifiable();

            _target.SetTicketData(
                new TicketData
                {
                    Location = locationText,
                    Address1 = addressLine1Text,
                    Address2 = addressLine2Text,
                    RestrictedTicketTitle = nonCashTicketTile
                });

            _propertiesManager.Verify();
        }

        [DynamicData(nameof(RedemptionTest))]
        [DataTestMethod]
        public void RedeemVoucherTest(
            long expectedAmount,
            AccountType expectedAccountType,
            bool redemptionEnabled,
            bool inOperatorMenu,
            TicketInInfo ticketInInfo)
        {
            const string barcode = "1234567";
            const long voucherSequence = 1;
            var voucherInTransaction = new VoucherInTransaction(0, DateTime.Now, barcode);
            _sasHost.Setup(x => x.IsRedemptionEnabled()).Returns(redemptionEnabled);
            _target.InOperatorMenu = inOperatorMenu;
            _sasHost.Setup(x => x.ValidateTicketInRequest(It.IsAny<VoucherInTransaction>()))
                .Returns(Task.FromResult(ticketInInfo));
            _idProvider.Setup(m => m.GetNextLogSequence<SasVoucherValidation>()).Returns(voucherSequence);

            Assert.IsTrue(_target.RedeemVoucher(voucherInTransaction).Wait(WaitTime));
            Assert.AreEqual(barcode, voucherInTransaction.Barcode);
            Assert.AreEqual(expectedAmount, voucherInTransaction.Amount);
            if (ticketInInfo != null)
            {
                Assert.AreEqual(voucherSequence, voucherInTransaction.VoucherSequence);
                Assert.AreEqual(expectedAccountType, voucherInTransaction.TypeOfAccount);
            }
        }

        [TestMethod]
        public void CanValidateVoucherTest()
        {
            const int amount = 100;

            _sasHost.Setup(x => x.CanValidateTicketOutRequest(amount, TicketType.CashOut))
                .Returns(false);
            Assert.IsFalse(_target.CanValidateVoucherOut(amount, AccountType.Cashable));

            _sasHost.Setup(x => x.CanValidateTicketOutRequest(amount, TicketType.CashOut))
                .Returns(true);
            Assert.IsTrue(_target.CanValidateVoucherOut(amount, AccountType.Cashable));
        }

        [TestMethod]
        public void VoucherRedeemedTest()
        {
            _sasHost.Setup(x => x.TicketTransferComplete(It.IsAny<AccountType>())).Verifiable();
            _target.CommitVoucher(new VoucherInTransaction { State = VoucherState.Redeemed });
            _sasHost.Verify();
        }

        [TestMethod]
        public void VoucherRejectedTest()
        {
            _sasHost.Setup(x => x.TicketTransferFailed(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>())).Verifiable();
            _target.CommitVoucher(new VoucherInTransaction { State = VoucherState.Rejected });
            _sasHost.Verify();
        }

        [DynamicData(nameof(VoucherIssueTest))]
        [DataTestMethod]
        public void IssueVoucherTest(VoucherAmount amount, TicketOutInfo ticketOutInfo, AccountType accountType, TicketType ticketType)
        {
            _sasHost.Setup(x => x.ValidateTicketOutRequest((ulong)amount.Amount, ticketType))
                .Returns(Task.FromResult(ticketOutInfo));

            var issueVoucher = _target.IssueVoucher(amount, accountType, Guid.NewGuid(), TransferOutReason.CashOut);

            Assert.IsTrue(issueVoucher.Wait(WaitTime));
            var voucherOutTransaction = issueVoucher.Result;
            if (ticketOutInfo == null)
            {
                Assert.IsNull(voucherOutTransaction);
            }
            else
            {
                Assert.AreEqual(ticketOutInfo.Barcode, voucherOutTransaction.Barcode);
                Assert.AreEqual(accountType, voucherOutTransaction.TypeOfAccount);
                Assert.AreEqual(ticketOutInfo.Amount, (ulong)voucherOutTransaction.Amount);
                Assert.AreEqual(ticketOutInfo.Pool, voucherOutTransaction.ReferenceId);
                Assert.AreEqual(ticketOutInfo.Time, voucherOutTransaction.TransactionDateTime);
                Assert.AreEqual((int)ticketOutInfo.TicketExpiration, voucherOutTransaction.Expiration);
            }
        }
    }
}