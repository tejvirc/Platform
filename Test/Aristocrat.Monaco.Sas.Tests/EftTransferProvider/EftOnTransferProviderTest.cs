namespace Aristocrat.Monaco.Sas.Tests.EftTransferProvider
{
    using System;
    using Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.Sas.EftTransferProvider;
    using Contracts.SASProperties;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class EftOnTransferProviderTest
    {
        private EftOnTransferProvider _target;
        private readonly Mock<IWatTransferOnHandler> _watTransferOnHandler = new Mock<IWatTransferOnHandler>(MockBehavior.Strict);
        private readonly Mock<ITransactionCoordinator> _transactionCoordinator = new Mock<ITransactionCoordinator>(MockBehavior.Strict);
        private readonly Mock<IBank> _bank = new Mock<IBank>(MockBehavior.Strict);

        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);
        private const ulong TransferLimit = 1000UL;

        private readonly SasFeatures settings = new SasFeatures
        {
            FundTransferType = FundTransferType.Eft,
            TransferLimit = (long)TransferLimit,
        };

        [TestInitialize]
        public void MyTestInitialize()
        {
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(settings);
            _bank.Setup(x => x.Limit).Returns(10000);
            _target = CreateProvider();
            _target.Initialize();
        }

        private EftOnTransferProvider CreateProvider()
        {
            return new EftOnTransferProvider(_watTransferOnHandler.Object,
                _transactionCoordinator.Object,
                _bank.Object,
                _propertiesManager.Object);
        }

        [DataTestMethod]
        [DataRow(AccountType.Cashable, false)]
        [DataRow(AccountType.Promo, false)]
        [DataRow(AccountType.NonCash, false)]
        [DataRow(AccountType.Cashable, true)]
        [DataRow(AccountType.Promo, true)]
        [DataRow(AccountType.NonCash, true)]
        public void ProcessEftOnRequestTest(AccountType accountType, bool reduceAmount)
        {
            var amount = 100ul;
            long cashableAmount = (long)(accountType == AccountType.Cashable ? amount : 0) * 1000;
            var promoteAmount = (long)(accountType == AccountType.Promo ? amount : 0) * 1000;
            var nonCashableAmount = (long)(accountType == AccountType.NonCash ? amount : 0) * 1000;

            settings.PartialTransferAllowed = reduceAmount;
            settings.TransferInAllowed = true;
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(settings);
            _bank.Setup(x => x.QueryBalance()).Returns(1000L.CentsToMillicents());
            _bank.Setup(x => x.Limit).Returns(2000L.CentsToMillicents());
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.AllowCreditsInAboveMaxCredit, It.IsAny<bool>())).Returns(false);

            const string RequestID = "TRANSACTIONID";
            _watTransferOnHandler.Setup(x => x.RequestTransfer(It.IsAny<Guid>(), RequestID, cashableAmount, promoteAmount, nonCashableAmount, reduceAmount)).Returns(true);
            var r = _target.EftOnRequest(RequestID, accountType, amount);
            Assert.IsTrue(r);
        }

        [DataTestMethod]
        [DataRow(100L, 120L, 200L, true, 150L, 80L, true, false)]
        [DataRow(50L, 120L, 200L, true, 150L, 50L, false, false)]
        public void GetAcceptedTransferOutAmountTest(
            long requestedAmount,
            long balance,
            long bankLimit,
            bool partialTransferAllowed,
            long transferLimit,
            long acceptedAmount,
            bool limitExceeded,
            bool allowCreditsInAboveMaxCredit)
        {
            settings.TransferInAllowed = true;
            settings.TransferLimit = transferLimit;
            settings.PartialTransferAllowed = partialTransferAllowed;
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(settings);
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.AllowCreditsInAboveMaxCredit, It.IsAny<bool>())).Returns(allowCreditsInAboveMaxCredit);
            _bank.Setup(x => x.QueryBalance()).Returns(balance.CentsToMillicents());
            _bank.Setup(x => x.Limit).Returns(bankLimit.CentsToMillicents());
            var r = _target.GetAcceptedTransferInAmount((ulong)requestedAmount);
            Assert.AreEqual(r, ((ulong)acceptedAmount, limitExceeded));
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void CanTransferTest(bool value)
        {
            settings.TransferInAllowed = value;
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(settings);

            var r = _target.CanTransfer;
            Assert.AreEqual(r, value);
        }
    }
}
