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
    using System.Globalization;
    using System.IO;
    using Test.Common;

    /// <summary>
    ///     Tests for the AccountingPropertyProvider class
    /// </summary>
    [TestClass]
    public class AccountingPropertyProviderNotInitializedTest
    {
        private Mock<IPersistentStorageAccessor> _block;

        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IPersistentStorageManager> _storageManager;
        private AccountingPropertyProvider _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Strict);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(m => m.GetProperty("Mono.SelectedAddinConfigurationHashCode", null)).Returns(null);
            _propertiesManager.Setup(
                m => m.AddPropertyProvider(It.IsAny<AccountingPropertyProvider>()));
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<long>()))
                .Returns(100L);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.LocalizationPlayerTicketLocale, "en-US")).Returns(new CultureInfo("en-US"))
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.LocalizationPlayerTicketSelectable, new[] { CultureInfo.CurrentCulture.Name })).Returns(It.IsAny<string[]>())
                .Verifiable();

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None)).Returns(ImportMachineSettings.None).Verifiable();

            _block = MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Strict);

            _block.SetupGet(m => m[PropertyKey.NoteIn]).Returns(true);
            _block.SetupSet(m => m[PropertyKey.NoteIn] = It.IsAny<bool>());
            _block.SetupGet(m => m[PropertyKey.VoucherIn]).Returns(false);
            _block.SetupSet(m => m[PropertyKey.VoucherIn] = It.IsAny<bool>());

            _block.SetupGet(m => m[AccountingConstants.VoucherInLimit]).Returns(AccountingConstants.DefaultVoucherInLimit);
            _block.SetupSet(m => m[AccountingConstants.VoucherInLimit] = It.IsAny<long>());

            _block.SetupGet(m => m[AccountingConstants.VoucherOut]).Returns(true);
            _block.SetupSet(m => m[AccountingConstants.VoucherOut] = It.IsAny<bool>());

            _block.SetupGet(m => m[AccountingConstants.VoucherInLimitEnabled]).Returns(true);
            _block.SetupSet(m => m[AccountingConstants.VoucherInLimitEnabled] = It.IsAny<bool>());

            _block.SetupGet(m => m[AccountingConstants.VoucherOutLimitEnabled]).Returns(true);
            _block.SetupSet(m => m[AccountingConstants.VoucherOutLimitEnabled] = It.IsAny<bool>());

            _block.SetupGet(m => m[AccountingConstants.VoucherOutNonCash]).Returns(true);
            _block.SetupSet(m => m[AccountingConstants.VoucherOutNonCash] = It.IsAny<bool>());

            _block.SetupGet(m => m[AccountingConstants.VoucherOutLimit]).Returns(AccountingConstants.DefaultVoucherOutLimit);
            _block.SetupSet(m => m[AccountingConstants.VoucherOutLimit] = It.IsAny<long>());
            _block.SetupGet(m => m[AccountingConstants.VoucherOutExpirationDays]).Returns(0);
            _block.SetupSet(m => m[AccountingConstants.VoucherOutExpirationDays] = It.IsAny<int>());
            _block.SetupGet(m => m[AccountingConstants.VoucherOutNonCashExpirationDays]).Returns(0);
            _block.SetupSet(m => m[AccountingConstants.VoucherOutNonCashExpirationDays] = It.IsAny<int>());

            _block.SetupGet(m => m[PropertyKey.MaxCreditsIn]).Returns(1000000000L);
            _block.SetupSet(m => m[PropertyKey.MaxCreditsIn] = It.IsAny<long>());

            _block.SetupGet(m => m[AccountingConstants.LargeWinHandpayResetMethod]).Returns(AccountingConstants.LargeWinHandpayResetMethod);
            _block.SetupSet(m => m[AccountingConstants.LargeWinHandpayResetMethod] = It.IsAny<int>());

            _block.SetupGet(m => m[AccountingConstants.HandpayLimit])
                .Returns(AccountingConstants.HandpayLimit);
            _block.SetupSet(m => m[AccountingConstants.HandpayLimit] = It.IsAny<long>());

            _block.SetupGet(m => m[AccountingConstants.LargeWinLimit])
                .Returns(AccountingConstants.DefaultLargeWinLimit);
            _block.SetupSet(m => m[AccountingConstants.LargeWinLimit] = It.IsAny<long>());

            _block.SetupGet(m => m[AccountingConstants.OverwriteLargeWinLimit]).Returns(true);
            _block.SetupSet(m => m[AccountingConstants.OverwriteLargeWinLimit] = It.IsAny<bool>());

            _block.SetupGet(m => m[AccountingConstants.LargeWinRatio])
                .Returns(AccountingConstants.DefaultLargeWinRatio);
            _block.SetupSet(m => m[AccountingConstants.LargeWinRatio] = It.IsAny<long>());

            _block.SetupGet(m => m[AccountingConstants.LargeWinRatioThreshold])
                .Returns(AccountingConstants.DefaultLargeWinRatioThreshold);
            _block.SetupSet(m => m[AccountingConstants.LargeWinRatioThreshold] = It.IsAny<long>());

            _block.SetupGet(m => m[AccountingConstants.OverwriteLargeWinRatio]).Returns(true);
            _block.SetupSet(m => m[AccountingConstants.OverwriteLargeWinRatio] = It.IsAny<bool>());

            _block.SetupGet(m => m[AccountingConstants.OverwriteLargeWinRatioThreshold]).Returns(true);
            _block.SetupSet(m => m[AccountingConstants.OverwriteLargeWinRatioThreshold] = It.IsAny<bool>());

            _block.SetupGet(m => m[AccountingConstants.CelebrationLockupLimit])
                .Returns(AccountingConstants.CelebrationLockupLimit);
            _block.SetupSet(m => m[AccountingConstants.CelebrationLockupLimit] = It.IsAny<long>());

            _block.SetupGet(m => m[AccountingConstants.CheckCreditsIn]).Returns(CheckCreditsStrategy.None);
            _block.SetupSet(m => m[AccountingConstants.CheckCreditsIn] = It.IsAny<CheckCreditsStrategy>());

            _block.SetupGet(m => m[AccountingConstants.MaxTenderInLimit]).Returns(0);
            _block.SetupSet(m => m[AccountingConstants.MaxTenderInLimit] = It.IsAny<long>());

            _block.SetupGet(m => m[AccountingConstants.CashInLaundry]).Returns(0);
            _block.SetupSet(m => m[AccountingConstants.CashInLaundry] = It.IsAny<long>());

            _block.SetupGet(m => m[AccountingConstants.VoucherInLaundry]).Returns(0);
            _block.SetupSet(m => m[AccountingConstants.VoucherInLaundry] = It.IsAny<long>());

            _block.SetupGet(m => m[AccountingConstants.SelfAuditErrorOccurred]).Returns(false);
            _block.SetupSet(m => m[AccountingConstants.SelfAuditErrorOccurred] = It.IsAny<bool>());

            _block.SetupGet(m => m[AccountingConstants.MaxCreditMeter]).Returns(0);
            _block.SetupSet(m => m[AccountingConstants.MaxCreditMeter] = It.IsAny<long>());

            _block.SetupGet(m => m[AccountingConstants.ShowMessageWhenCreditLimitReached]).Returns(false);
            _block.SetupSet(m => m[AccountingConstants.ShowMessageWhenCreditLimitReached] = It.IsAny<bool>());

            _block.SetupGet(m => m[AccountingConstants.DisableBankNoteAcceptorWhenCreditLimitReached]).Returns(false);
            _block.SetupSet(m => m[AccountingConstants.DisableBankNoteAcceptorWhenCreditLimitReached] = It.IsAny<bool>());

            _block.SetupGet(m => m[AccountingConstants.MaxCreditMeterMaxAllowed]).Returns(0);
            _block.SetupSet(m => m[AccountingConstants.MaxCreditMeterMaxAllowed] = It.IsAny<long>());

            _block.SetupGet(m => m[AccountingConstants.MaxBetLimit]).Returns(0);
            _block.SetupSet(m => m[AccountingConstants.MaxBetLimit] = It.IsAny<long>());

            _block.SetupGet(m => m[AccountingConstants.OverwriteMaxBetLimit]).Returns(true);
            _block.SetupSet(m => m[AccountingConstants.OverwriteMaxBetLimit] = It.IsAny<bool>());

            _block.SetupGet(m => m[PropertyKey.TicketTextLine1]).Returns(string.Empty);
            _block.SetupSet(m => m[PropertyKey.TicketTextLine1] = It.IsAny<string>());

            _block.SetupGet(m => m[PropertyKey.TicketTextLine2]).Returns(string.Empty);
            _block.SetupSet(m => m[PropertyKey.TicketTextLine2] = It.IsAny<string>());

            _block.SetupGet(m => m[PropertyKey.TicketTextLine3]).Returns(string.Empty);
            _block.SetupSet(m => m[PropertyKey.TicketTextLine3] = It.IsAny<string>());

            _block.SetupGet(m => m[AccountingConstants.TicketTitleCash]).Returns("Cash Out");
            _block.SetupSet(m => m[AccountingConstants.TicketTitleCash] = It.IsAny<string>());

            _block.SetupGet(m => m[AccountingConstants.TicketTitlePromo])
                .Returns(AccountingConstants.DefaultNonCashTicketTitle);
            _block.SetupSet(m => m[AccountingConstants.TicketTitlePromo] = It.IsAny<string>());

            _block.SetupGet(m => m[AccountingConstants.TicketTitleNonCash])
                .Returns(AccountingConstants.DefaultNonCashTicketTitle);
            _block.SetupSet(m => m[AccountingConstants.TicketTitleNonCash] = It.IsAny<string>());

            _block.SetupGet(m => m[AccountingConstants.TicketTitleLargeWin]).Returns(string.Empty);
            _block.SetupSet(m => m[AccountingConstants.TicketTitleLargeWin] = It.IsAny<string>());

            _block.SetupGet(m => m[AccountingConstants.TicketTitleBonusCash]).Returns(string.Empty);
            _block.SetupSet(m => m[AccountingConstants.TicketTitleBonusCash] = It.IsAny<string>());

            _block.SetupGet(m => m[AccountingConstants.TicketTitleBonusNonCash]).Returns(string.Empty);
            _block.SetupSet(m => m[AccountingConstants.TicketTitleBonusNonCash] = It.IsAny<string>());

            _block.SetupGet(m => m[AccountingConstants.TicketTitleBonusPromo]).Returns(string.Empty);
            _block.SetupSet(m => m[AccountingConstants.TicketTitleBonusPromo] = It.IsAny<string>());

            _block.SetupGet(m => m[AccountingConstants.TicketTitleWatCash]).Returns(string.Empty);
            _block.SetupSet(m => m[AccountingConstants.TicketTitleWatCash] = It.IsAny<string>());

            _block.SetupGet(m => m[AccountingConstants.TicketTitleWatPromo]).Returns(string.Empty);
            _block.SetupSet(m => m[AccountingConstants.TicketTitleWatPromo] = It.IsAny<string>());

            _block.SetupGet(m => m[AccountingConstants.TicketTitleWatNonCash]).Returns(string.Empty);
            _block.SetupSet(m => m[AccountingConstants.TicketTitleWatNonCash] = It.IsAny<string>());

            _block.SetupGet(m => m[AccountingConstants.TitleCancelReceipt]).Returns(string.Empty);
            _block.SetupSet(m => m[AccountingConstants.TitleCancelReceipt] = It.IsAny<string>());

            _block.SetupGet(m => m[AccountingConstants.TitleJackpotReceipt]).Returns(string.Empty);
            _block.SetupSet(m => m[AccountingConstants.TitleJackpotReceipt] = It.IsAny<string>());

            _block.SetupGet(m => m[AccountingConstants.RedeemText]).Returns(string.Empty);
            _block.SetupSet(m => m[AccountingConstants.RedeemText] = It.IsAny<string>());

            _block.SetupGet(m => m[AccountingConstants.IgnoreVoucherStackedDuringReboot]).Returns(false);
            _block.SetupSet(m => m[AccountingConstants.IgnoreVoucherStackedDuringReboot] = It.IsAny<bool>());

            _block.SetupGet(m => m[AccountingConstants.MoneyInEnabled]).Returns(true);
            _block.SetupSet(m => m[AccountingConstants.MoneyInEnabled] = It.IsAny<bool>());

            _block.SetupGet(m => m[AccountingConstants.ReprintLoggedVoucherBehavior]).Returns("None");
            _block.SetupSet(m => m[AccountingConstants.ReprintLoggedVoucherBehavior] = It.IsAny<string>());

            _block.SetupGet(m => m[PropertyKey.VoucherIn]).Returns(true);
            _block.SetupSet(m => m[PropertyKey.VoucherIn] = It.IsAny<bool>());

            _block.SetupGet(m => m[AccountingConstants.CheckCreditsIn]).Returns(CheckCreditsStrategy.None);
            _block.SetupSet(m => m[AccountingConstants.CheckCreditsIn] = It.IsAny<CheckCreditsStrategy>());

            _block.SetupGet(m => m[AccountingConstants.AllowCreditUnderLimit]).Returns(true);
            _block.SetupSet(m => m[AccountingConstants.AllowCreditUnderLimit] = It.IsAny<bool>());

            _block.SetupGet(m => m[AccountingConstants.ReprintLoggedVoucherBehavior]).Returns(string.Empty);
            _block.SetupSet(m => m[AccountingConstants.ReprintLoggedVoucherBehavior] = It.IsAny<string>());

            _block.SetupGet(m => m[AccountingConstants.ReprintLoggedVoucherTitleOverride]).Returns(true);
            _block.SetupSet(m => m[AccountingConstants.ReprintLoggedVoucherTitleOverride] = It.IsAny<bool>());

            _block.SetupGet(m => m[AccountingConstants.ReprintLoggedVoucherDoorOpenRequirement]).Returns(string.Empty);
            _block.SetupSet(m => m[AccountingConstants.ReprintLoggedVoucherDoorOpenRequirement] = It.IsAny<string>());

            _block.SetupGet(m => m[AccountingConstants.TransferOutContext]).Returns(string.Empty);
            _block.SetupSet(m => m[AccountingConstants.TransferOutContext] = It.IsAny<string>());

            _block.SetupGet(m => m[AccountingConstants.TicketBarcodeLength]).Returns(0);
            _block.SetupSet(m => m[AccountingConstants.TicketBarcodeLength] = It.IsAny<int>());

            _storageManager = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);
            _storageManager.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(false);
            _storageManager.Setup(m => m.CreateBlock(It.IsAny<PersistenceLevel>(), It.IsAny<string>(), It.IsAny<int>())).Returns(_block.Object);

            _block.SetupGet(m => m[AccountingConstants.HandpayLimitEnabled]).Returns(true);
            _block.SetupSet(m => m[AccountingConstants.HandpayLimitEnabled] = It.IsAny<bool>());

            _block.SetupGet(m => m[AccountingConstants.LargeWinLimitEnabled]).Returns(true);
            _block.SetupSet(m => m[AccountingConstants.LargeWinLimitEnabled] = It.IsAny<bool>());

            _block.SetupGet(m => m[AccountingConstants.LargeWinRatioEnabled]).Returns(true);
            _block.SetupSet(m => m[AccountingConstants.LargeWinRatioEnabled] = It.IsAny<bool>());

            _block.SetupGet(m => m[AccountingConstants.LargeWinRatioThresholdEnabled]).Returns(true);
            _block.SetupSet(m => m[AccountingConstants.LargeWinRatioThresholdEnabled] = It.IsAny<bool>());

            _block.SetupGet(m => m[AccountingConstants.CreditLimitEnabled]).Returns(true);
            _block.SetupSet(m => m[AccountingConstants.CreditLimitEnabled] = It.IsAny<bool>());

            _block.SetupGet(m => m[AccountingConstants.MaxBetLimitEnabled]).Returns(true);
            _block.SetupSet(m => m[AccountingConstants.MaxBetLimitEnabled] = It.IsAny<bool>());

            _block.SetupGet(m => m[AccountingConstants.ExcessiveMeterSound]).Returns(string.Empty);
            _block.SetupGet(m => m[AccountingConstants.LaunderingMonitorVisible]).Returns(true);
            _block.SetupGet(m => m[AccountingConstants.ExcessiveMeterValue]).Returns(0L);
            _block.SetupGet(m => m[AccountingConstants.DisabledDueToExcessiveMeter]).Returns(false);
            _block.SetupGet(m => m[AccountingConstants.IncrementThreshold]).Returns(100000L);
            _block.SetupSet(m => m[AccountingConstants.IncrementThreshold] = It.IsAny<long>());
            _block.SetupGet(m => m[AccountingConstants.IncrementThresholdIsChecked]).Returns(true);
            _block.SetupSet(m => m[AccountingConstants.IncrementThresholdIsChecked] = It.IsAny<bool>());
            _block.SetupGet(m => m[AccountingConstants.ExcessiveDocumentRejectLockupEnabled]).Returns(false);
            _block.SetupSet(m => m[AccountingConstants.ExcessiveDocumentRejectLockupEnabled] = It.IsAny<bool>());

            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _target = new AccountingPropertyProvider();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
            AddinManager.Shutdown();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }
    }
}
