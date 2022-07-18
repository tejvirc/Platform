namespace Aristocrat.Monaco.Sas.Tests.EftTransferProvider
{
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts.Wat;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.Sas.Contracts.Eft;
    using Aristocrat.Monaco.Sas.EftTransferProvider;
    using Contracts.SASProperties;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class EftOffTransferProviderTest
    {
        private EftOffTransferProvider _target;
        private readonly Mock<IWatOffProvider> _watOffProvider = new Mock<IWatOffProvider>(MockBehavior.Strict);
        private readonly Mock<IBank> _bank = new Mock<IBank>(MockBehavior.Strict);
        private readonly Mock<ITransactionCoordinator> _transactionCoordinator = new Mock<ITransactionCoordinator>(MockBehavior.Strict);
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);
        private readonly Mock<IEftHostCashOutProvider> _eftHostCashOutProvider = new Mock<IEftHostCashOutProvider>();
        private const ulong TransferLimit = 1000UL;

        private readonly SasFeatures settings = new SasFeatures
        {
            FundTransferType = FundTransferType.Eft,
            TransferLimit = (long)TransferLimit,
        };

        [TestInitialize]
        public void MyTestInitialize()
        {
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(settings); ;
            _target = CreateProvider();
        }

        private EftOffTransferProvider CreateProvider()
        {
            return new EftOffTransferProvider(_watOffProvider.Object,
                _transactionCoordinator.Object,
                _bank.Object,
                _propertiesManager.Object,
                _eftHostCashOutProvider.Object);
        }

        [DataTestMethod]
        [DataRow(AccountType.Cashable, false)]
        [DataRow(AccountType.Promo, false)]
        [DataRow(AccountType.NonCash, false)]
        [DataRow(AccountType.Cashable, true)]
        [DataRow(AccountType.Promo, true)]
        [DataRow(AccountType.NonCash, true)]
        public void ProcessEftOffRequestTest(AccountType accountType, bool reduceAmount)
        {
            var amount = 100ul;
            long cashableAmount = (long)(accountType == AccountType.Cashable ? amount : 0);
            long promoteAmount = (long)(accountType == AccountType.Promo ? amount : 0);
            long nonCashableAmount = (long)(accountType == AccountType.NonCash ? amount : 0);
            settings.TransferOutAllowed = true;
            settings.PartialTransferAllowed = reduceAmount;
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(settings);
            _bank.Setup(x => x.QueryBalance(accountType)).Returns(1000L.CentsToMillicents());

            const string RequestID = "TRANSACTIONID";
            _watOffProvider.Setup(x => x.RequestTransfer(RequestID, cashableAmount * 1000, promoteAmount * 1000, nonCashableAmount * 1000, reduceAmount)).Returns(true);
            var r = _target.EftOffRequest(RequestID, new[] { accountType }, amount);
            Assert.IsTrue(r);
        }

        [DataTestMethod]
        [DataRow(new[] { AccountType.Cashable, AccountType.NonCash }, 15, true, 10L, 5L, 0L)]
        [DataRow(new[] { AccountType.Cashable, AccountType.NonCash }, 8, true, 8L, 0L, 0L)]
        [DataRow(new[] { AccountType.Cashable, AccountType.NonCash }, 30, true, 10L, 12L, 0L)]
        public void ProcessMultipleAccountEftOffRequestTest(AccountType[] accountTypes, long transferLimit, bool partialTransferAllowed, long cashableAmount, long nonCashableAmount, long promoteAmount)
        {
            var amount = 100ul;
            settings.TransferOutAllowed = true;
            settings.TransferLimit = transferLimit;
            settings.PartialTransferAllowed = partialTransferAllowed;
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(settings);
            _bank.Setup(x => x.QueryBalance(accountTypes[0])).Returns(10L.CentsToMillicents());
            _bank.Setup(x => x.QueryBalance(accountTypes[1])).Returns(12L.CentsToMillicents());
            _eftHostCashOutProvider.Setup(x => x.CashOutAccepted()).Returns(false);

            const string RequestID = "TRANSACTIONID";
            _watOffProvider.Setup(x => x.RequestTransfer(RequestID, cashableAmount * 1000, promoteAmount * 1000, nonCashableAmount * 1000, partialTransferAllowed)).Returns(true);
            var r = _target.EftOffRequest(RequestID, accountTypes, amount);
            Assert.IsTrue(r);
        }

        [DataTestMethod]
        [DataRow(new[] { AccountType.Cashable, AccountType.NonCash }, 15, true, 10L, 5L, 0L)]
        [DataRow(new[] { AccountType.Cashable, AccountType.NonCash }, 8, true, 8L, 0L, 0L)]
        [DataRow(new[] { AccountType.Cashable, AccountType.NonCash }, 30, true, 10L, 12L, 0L)]
        public void ProcessEgmInitiatedCashOutRequestTest(AccountType[] accountTypes, long transferLimit, bool partialTransferAllowed, long cashableAmount, long nonCashableAmount, long promoteAmount)
        {
            var amount = 100ul;
            settings.TransferOutAllowed = true;
            settings.TransferLimit = transferLimit;
            settings.PartialTransferAllowed = partialTransferAllowed;
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(settings);
            _bank.Setup(x => x.QueryBalance(accountTypes[0])).Returns(10L.CentsToMillicents());
            _bank.Setup(x => x.QueryBalance(accountTypes[1])).Returns(12L.CentsToMillicents());
            _eftHostCashOutProvider.Setup(x => x.CashOutAccepted()).Returns(true);
            const string RequestID = "TRANSACTIONID";
            _watOffProvider.Setup(x => x.RequestTransfer(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<bool>())).Returns(true);
            var r = _target.EftOffRequest(RequestID, accountTypes, amount);
            _watOffProvider.Verify(x => x.RequestTransfer(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<bool>()), Times.Never);
            Assert.IsTrue(r);
        }

        [DataTestMethod]
        public async Task InitialTransferWithTransferOutDisabledTest()
        {
            var watTransaction = new Mock<WatTransaction>();
            settings.TransferOutAllowed = false;
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(settings);
            var r = await _target.InitiateTransfer(watTransaction.Object);
            Assert.IsFalse(r);
        }

        [DataTestMethod]
        [DataRow(true)]
        public async Task InitialTransferTest(bool TransferOutAllowed)
        {
            var watTransaction = new Mock<WatTransaction>();
            settings.TransferOutAllowed = TransferOutAllowed;
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(settings);
            _eftHostCashOutProvider.Setup(x => x.HandleHostCashOut());
            var r = await _target.InitiateTransfer(watTransaction.Object);
            _eftHostCashOutProvider.Verify(x => x.HandleHostCashOut(), Times.Once);
            Assert.IsTrue(r);
        }

        [DataTestMethod]
        [DataRow(AccountType.Cashable, 20L, true, 50L, 20L, false, true)]
        [DataRow(AccountType.Promo, 20L, true, 50L, 20L, false, true)]
        [DataRow(AccountType.NonCash, 20L, true, 50L, 20L, false, true)]
        [DataRow(AccountType.Cashable, 20L, false, 50L, 20L, false, true)]
        [DataRow(AccountType.Promo, 20L, false, 50L, 20L, false, true)]
        [DataRow(AccountType.NonCash, 20L, false, 50L, 20L, false, true)]
        [DataRow(AccountType.Cashable, 80L, true, 50L, 50L, true, true)]
        [DataRow(AccountType.Promo, 80L, true, 50L, 50L, true, true)]
        [DataRow(AccountType.NonCash, 80L, true, 50L, 50L, true, true)]
        [DataRow(AccountType.Cashable, 80L, false, 50L, 0L, true, true)]
        [DataRow(AccountType.Promo, 80L, false, 50L, 0L, true, true)]
        [DataRow(AccountType.NonCash, 80L, false, 50L, 0L, true, true)]
        [DataRow(AccountType.Cashable, 20L, true, 50L, 0L, true, false)]
        [DataRow(AccountType.Promo, 20L, true, 50L, 0L, true, false)]
        [DataRow(AccountType.NonCash, 20L, true, 50L, 0L, true, false)]
        public void GetAcceptedTransferOutAmountTest(
            AccountType accountType,
            long balance,
            bool partialTransferAllowed,
            long transferLimit,
            long acceptedAmount,
            bool limitExceeded,
            bool transferOutAllowed)
        {
            settings.TransferOutAllowed = transferOutAllowed;
            settings.TransferLimit = transferLimit;
            settings.PartialTransferAllowed = partialTransferAllowed;
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(settings);
            _bank.Setup(x => x.QueryBalance(accountType)).Returns(balance.CentsToMillicents());
            var r = _target.GetAcceptedTransferOutAmount(new[] { accountType });
            Assert.AreEqual(r, ((ulong)acceptedAmount, limitExceeded));
        }

        [DataRow(new[] { AccountType.Cashable, AccountType.NonCash }, 20L, 100L, true, 50L, 0L, true, false)]
        [DataRow(new[] { AccountType.Cashable, AccountType.NonCash }, 20L, 100L, true, 50L, 50L, true, true)]
        [DataRow(new[] { AccountType.Cashable, AccountType.NonCash }, 20L, 100L, true, 150L, 120L, false, true)]
        [DataTestMethod]
        public void GetAcceptedTransferOutAmount4MultipleAccountsTest(
           AccountType[] accountTypes,
           long balance0,
           long balance1,
           bool partialTransferAllowed,
           long transferLimit,
           long acceptedAmount,
           bool limitExceeded,
           bool transferOutAllowed)
        {
            settings.TransferOutAllowed = transferOutAllowed;
            settings.TransferLimit = transferLimit;
            settings.PartialTransferAllowed = partialTransferAllowed;
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(settings);
            _bank.Setup(x => x.QueryBalance(accountTypes[0])).Returns(balance0.CentsToMillicents());
            _bank.Setup(x => x.QueryBalance(accountTypes[1])).Returns(balance1.CentsToMillicents());
            var r = _target.GetAcceptedTransferOutAmount(accountTypes);
            Assert.AreEqual(r, ((ulong)acceptedAmount, limitExceeded));
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void CanTransferTest(bool value)
        {
            settings.TransferOutAllowed = value;
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(settings);
            var r = _target.CanTransfer;
            Assert.AreEqual(r, value);
        }

        [TestMethod]
        public void RestartCashoutTimerTest()
        {
            _eftHostCashOutProvider.Setup(x => x.RestartTimerIfPendingCallbackFromHost());
            _target.RestartCashoutTimer();
            _eftHostCashOutProvider.Verify(x => x.RestartTimerIfPendingCallbackFromHost(), Times.Once);
        }
    }
}
