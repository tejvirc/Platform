namespace Aristocrat.Monaco.Accounting.Tests
{
    using Application.Contracts;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using System;
    using System.IO;
    using Test.Common;

    /// <summary>
    ///     Tests for the AccountingPropertyProvider class
    /// </summary>
    [TestClass]
    public class AccountingPropertyProviderTest
    {
// TODO these unit tests are currently defanged when USE_MARKET_CONFIG is defined until a decision is reached on how to
// handle generation and loading of test fixture data
#if !USE_MARKET_CONFIG
        private Mock<IPersistentStorageAccessor> _block;

        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IPersistentStorageManager> _storageManager;
        private AccountingPropertyProvider _target;
#endif

        [TestInitialize]
        public void MyTestInitialize()
        {
#if !USE_MARKET_CONFIG
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(m => m.GetProperty("Mono.SelectedAddinConfigurationHashCode", null)).Returns(null);
            _propertiesManager.Setup(
                m => m.AddPropertyProvider(It.IsAny<AccountingPropertyProvider>()));
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<long>()))
                .Returns(100L);

            _block = MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Strict);
            _block.SetupGet(m => m[PropertyKey.NoteIn]).Returns(true);
            _block.SetupGet(m => m[PropertyKey.VoucherIn]).Returns(false);
            _block.SetupGet(m => m[AccountingConstants.VoucherInLimit]).Returns(AccountingConstants.DefaultVoucherInLimit);
            _block.SetupGet(m => m[AccountingConstants.VoucherOut]).Returns(true);
            _block.SetupGet(m => m[AccountingConstants.VoucherInLimitEnabled]).Returns(true);
            _block.SetupGet(m => m[AccountingConstants.VoucherOutLimitEnabled]).Returns(true);
            _block.SetupGet(m => m[AccountingConstants.VoucherOutLimit]).Returns(AccountingConstants.DefaultVoucherOutLimit);
            _block.SetupGet(m => m[AccountingConstants.VoucherOutExpirationDays]).Returns(0);
            _block.SetupGet(m => m[AccountingConstants.VoucherOutNonCashExpirationDays]).Returns(0);
            _block.SetupGet(m => m[PropertyKey.MaxCreditsIn]).Returns(It.IsAny<long>());
            _block.SetupGet(m => m[AccountingConstants.LargeWinLimit])
                .Returns(AccountingConstants.DefaultLargeWinLimit);
            _block.SetupGet(m => m[AccountingConstants.LargeWinRatio])
                .Returns(AccountingConstants.DefaultLargeWinRatio);
            _block.SetupGet(m => m[AccountingConstants.LargeWinRatioThreshold])
                .Returns(AccountingConstants.DefaultLargeWinRatioThreshold);
            _block.SetupGet(m => m[AccountingConstants.OverwriteLargeWinLimit]).Returns(It.IsAny<bool>());
            _block.SetupGet(m => m[AccountingConstants.OverwriteLargeWinRatio]).Returns(It.IsAny<bool>());
            _block.SetupGet(m => m[AccountingConstants.OverwriteLargeWinRatioThreshold]).Returns(It.IsAny<bool>());
            _block.SetupGet(m => m[AccountingConstants.CelebrationLockupLimit]).Returns(0);
            _block.SetupGet(m => m[AccountingConstants.LargeWinHandpayResetMethod]).Returns(It.IsAny<int>());
            _block.SetupGet(m => m[AccountingConstants.HandpayLimit]).Returns(AccountingConstants.DefaultHandpayLimit);
            _block.SetupGet(m => m[AccountingConstants.MaxTenderInLimit]).Returns(0);
            _block.SetupGet(m => m[AccountingConstants.CashInLaundry]).Returns(0);
            _block.SetupGet(m => m[AccountingConstants.VoucherInLaundry]).Returns(0);
            _block.SetupGet(m => m[AccountingConstants.MaxCreditMeter])
                .Returns(AccountingConstants.DefaultLargeWinLimit);
            _block.SetupGet(m => m[AccountingConstants.MaxCreditMeterMaxAllowed])
                .Returns(AccountingConstants.DefaultLargeWinLimit);
            _block.SetupGet(m => m[AccountingConstants.MaxBetLimit]).Returns(It.IsAny<long>());
            _block.SetupGet(m => m[AccountingConstants.OverwriteMaxBetLimit]).Returns(It.IsAny<bool>());
            _block.SetupGet(m => m[PropertyKey.TicketTextLine1]).Returns(string.Empty);
            _block.SetupGet(m => m[PropertyKey.TicketTextLine2]).Returns(string.Empty);
            _block.SetupGet(m => m[PropertyKey.TicketTextLine3]).Returns(string.Empty);
            _block.SetupGet(m => m[AccountingConstants.TicketTitleCash]).Returns("Cash Out");
            _block.SetupGet(m => m[AccountingConstants.TicketTitlePromo])
                .Returns(AccountingConstants.DefaultNonCashTicketTitle);
            _block.SetupGet(m => m[AccountingConstants.TicketTitleNonCash])
                .Returns(AccountingConstants.DefaultNonCashTicketTitle);
            _block.SetupGet(m => m[AccountingConstants.TicketTitleLargeWin]).Returns(AccountingConstants.DefaultLargeWinLimit);
            _block.SetupGet(m => m[AccountingConstants.TicketTitleBonusNonCash]).Returns(string.Empty);
            _block.SetupGet(m => m[AccountingConstants.TicketTitleBonusPromo]).Returns(string.Empty);
            _block.SetupGet(m => m[AccountingConstants.TicketTitleWatCash]).Returns(string.Empty);
            _block.SetupGet(m => m[AccountingConstants.TicketTitleWatPromo]).Returns(string.Empty);
            _block.SetupGet(m => m[AccountingConstants.TicketTitleWatNonCash]).Returns(string.Empty);
            _block.SetupGet(m => m[AccountingConstants.TitleCancelReceipt]).Returns(string.Empty);
            _block.SetupGet(m => m[AccountingConstants.TitleJackpotReceipt]).Returns(string.Empty);
            _block.SetupGet(m => m[AccountingConstants.RedeemText]).Returns(string.Empty);
            _block.SetupGet(m => m[AccountingConstants.IgnoreVoucherStackedDuringReboot]).Returns(false);
            _block.SetupSet(m => m[AccountingConstants.IgnoreVoucherStackedDuringReboot] = It.IsAny<bool>());
            _block.SetupGet(m => m[AccountingConstants.MoneyInEnabled]).Returns(true);
            _block.SetupGet(m => m[AccountingConstants.ReprintLoggedVoucherBehavior]).Returns("None");
            _block.SetupGet(m => m[PropertyKey.VoucherIn]).Returns(true);
            _block.SetupGet(m => m[AccountingConstants.VoucherOutNonCash]).Returns(true);
            _block.SetupGet(m => m[AccountingConstants.CheckCreditsIn]).Returns(CheckCreditsStrategy.None);
            _block.SetupGet(m => m[AccountingConstants.AllowCreditUnderLimit]).Returns(true);
            _block.SetupGet(m => m[AccountingConstants.ReprintLoggedVoucherTitleOverride]).Returns(true);
            _block.SetupGet(m => m[AccountingConstants.ReprintLoggedVoucherDoorOpenRequirement]).Returns(string.Empty);
            _block.SetupGet(m => m[AccountingConstants.TransferOutContext]).Returns(string.Empty);
            _block.SetupSet(m => m[AccountingConstants.TransferOutContext] = It.IsAny<string>());
            _block.SetupGet(m => m[AccountingConstants.TicketBarcodeLength]).Returns(0);
            _block.SetupGet(m => m[AccountingConstants.HandpayLimitEnabled]).Returns(true);
            _block.SetupGet(m => m[AccountingConstants.LargeWinLimitEnabled]).Returns(true);
            _block.SetupGet(m => m[AccountingConstants.LargeWinRatioEnabled]).Returns(true);
            _block.SetupGet(m => m[AccountingConstants.LargeWinRatioThresholdEnabled]).Returns(true);
            _block.SetupGet(m => m[AccountingConstants.CreditLimitEnabled]).Returns(true);
            _block.SetupGet(m => m[AccountingConstants.MaxBetLimitEnabled]).Returns(true);
            _block.SetupGet(m => m[AccountingConstants.ExcessiveMeterSound]).Returns(string.Empty);
            _block.SetupGet(m => m[AccountingConstants.LaunderingMonitorVisible]).Returns(true);
            _block.SetupGet(m => m[AccountingConstants.ExcessiveMeterValue]).Returns(0L);
            _block.SetupGet(m => m[AccountingConstants.DisabledDueToExcessiveMeter]).Returns(false);
            _block.SetupGet(m => m[AccountingConstants.IncrementThreshold]).Returns(100000L);
            _block.SetupGet(m => m[AccountingConstants.IncrementThresholdIsChecked]).Returns(true);
            _block.SetupGet(m => m[AccountingConstants.ExcessiveDocumentRejectLockupEnabled]).Returns(false);
            _block.SetupGet(m => m[AccountingConstants.HandCountPayoutLimit]).Returns(1_199_00_000L);

            _storageManager = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);
            _storageManager.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(true);
            _storageManager.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_block.Object);

            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _target = new AccountingPropertyProvider();
#endif
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
            try
            {
                AddinManager.Shutdown();
            }
            catch (InvalidOperationException)
            {
                // temporarily swallow exception
            }
        }

#if !USE_MARKET_CONFIG
        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void GetCollectionTest()
        {
            var collection = _target.GetCollection;
            Assert.IsNotNull(collection);
            Assert.IsTrue(collection.Count > 1);
        }

        [TestMethod]
        public void GetPropertyTest()
        {
            var property = (bool)_target.GetProperty(PropertyKey.VoucherIn);
            Assert.IsTrue(property);
        }

        [TestMethod]
        [ExpectedException(typeof(UnknownPropertyException))]
        public void GetPropertyUnknownPropertyTest()
        {
            _target.GetProperty("Unknown Property");
        }

        [TestMethod]
        public void SetPropertyTest()
        {
            _block.SetupSet(m => m[PropertyKey.TicketTextLine1] = "TEST");

            string testString = "TEST";
            _target.SetProperty(PropertyKey.TicketTextLine1, testString);
            var property = (string)_target.GetProperty(PropertyKey.TicketTextLine1);
            Assert.AreEqual(testString, property);
        }

        [TestMethod]
        [ExpectedException(typeof(UnknownPropertyException))]
        public void SetPropertyUnknownPropertyTest()
        {
            _target.SetProperty("Unknown Property", null);
        }
#endif
    }
}
