namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.Monaco.Accounting.Contracts.Handpay;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts;
    using Kernel.MarketConfig;
    using Kernel.MarketConfig.Accounting;
    using Localization.Properties;
    using log4net;
    using Newtonsoft.Json;

    /// <summary>
    ///     A <see cref="IPropertyProvider" /> implementation for the accounting layer
    /// </summary>
    public class AccountingPropertyProvider : IPropertyProvider
    {
        private const string ConfigurationExtensionPath = "/Accounting/Configuration";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPersistentStorageAccessor _persistentStorageAccessor;

        /// <summary>
        ///     Dictionary of (property key string, (Tuple of property value, bool indicating persistence))
        /// </summary>
        private readonly Dictionary<string, Tuple<object, bool>> _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AccountingPropertyProvider" /> class.
        /// </summary>
        public AccountingPropertyProvider()
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

#if USE_MARKET_CONFIG // Use a compile time flag to allow switching the configuration source from the Jurisdiction addins to the config tool
            Logger.Debug("Prefer JSON config is enabled, using config export");

            var marketConfigManager = ServiceManager.GetInstance().GetService<IMarketConfigManager>();

            // Get the current jurisdiction installation id that was selected
            var jurisdictionInstallationId = propertiesManager.GetValue(
                ApplicationConstants.JurisdictionKey, string.Empty);

            // Use the MarketConfigManager to get the accounting configuration
            // TODO should this use the same defaultOnError behavior as ConfigManager?
            var configuration = marketConfigManager.GetMarketConfiguration<AccountingConfigSegment>(
                jurisdictionInstallationId);
#else
            Logger.Debug("Prefer JSON config is disabled, using Addins config");

            var configuration = ConfigurationUtilities.GetConfiguration(
            ConfigurationExtensionPath,
            () => new AccountingConfiguration
            {
                MoneyLaunderingMonitor = new AccountingConfigurationMoneyLaunderingMonitor()
                {
                    SoundFilePath = string.Empty,
                    Enabled = false,
                    Visible = false
                },
                TenderIn = new AccountingConfigurationTenderIn
                {
                    CheckLaundryLimit = false,
                    MaxTenderInLimit = 0,
                    CheckCreditsIn = CheckCreditsStrategy.None,
                    AllowCreditUnderLimit = false
                },
                CreditLimits = new AccountingConfigurationCreditLimits(),
                RebootWhilePrinting = new AccountingConfigurationRebootWhilePrinting
                {
                    Behavior = "Prompt"
                },
                ReprintLoggedVoucher = new AccountingConfigurationReprintLoggedVoucher
                {
                    Behavior = "None",
                    TitleOverride = false,
                    DoorOpenRequirement = "None"
                },
                NoteIn = new AccountingConfigurationNoteIn
                {
                    State = "Disabled"
                },
                VoucherIn = new AccountingConfigurationVoucherIn
                {
                    State = "Disabled"
                },
                VoucherOut = new AccountingConfigurationVoucherOut
                {
                    State = "Enabled",
                    LimitDefault = long.MaxValue,
                    AllowCashWinTicket = false,
                    SeparateMeteringCashableAndPromoAmounts = false
                },
                WinLimits = new AccountingConfigurationWinLimits(),
                Handpay = new AccountingConfigurationHandpay
                {
                    LargeWinForcedKeyOff = false,
                    LargeWinKeyOffStrategy = KeyOffType.LocalHandpay
                },
                MysteryProgressive = new AccountingConfigurationMysteryProgressive
                {
                    WinAsExternalBonus = false
                },
                CashoutOnCarrierBoardRemoval = new AccountingConfigurationCashoutOnCarrierBoardRemoval
                {
                    Enabled = false
                },
                BillClearance = new AccountingConfigurationBillClearance()
                {
                    Enabled = false
                },
            });
#endif

            var storageManager = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();

            var storageName = GetType().ToString();

            var blockExists = storageManager.BlockExists(storageName);

            _persistentStorageAccessor = blockExists
                ? storageManager.GetBlock(storageName)
                : storageManager.CreateBlock(PersistenceLevel.Transient, storageName, 1);

            // VoucherOutLimit should never be zero. Overwrite it with the default value if the value taken from the persistent storage is zero.
            var voucherOutLimit = (long)InitFromStorage(AccountingConstants.VoucherOutLimit);
            if (voucherOutLimit == 0)
            {
#if USE_MARKET_CONFIG
                voucherOutLimit = configuration.VoucherOut.LimitDefault;
#else
                voucherOutLimit = configuration.VoucherOut.LimitDefault > 0
                    ? configuration.VoucherOut.LimitDefault
                    : AccountingConstants.DefaultVoucherOutLimit;
#endif
            }

            var voucherInLimit = (long)InitFromStorage(AccountingConstants.VoucherInLimit);
            if (voucherInLimit == 0)
            {
#if USE_MARKET_CONFIG
                voucherInLimit = configuration.VoucherIn.LimitDefault;
#else
                voucherInLimit = configuration.VoucherIn.LimitDefault > 0
                    ? configuration.VoucherIn.LimitDefault
                    : AccountingConstants.DefaultVoucherInLimit;
#endif
            }

            _properties = new Dictionary<string, Tuple<object, bool>>
            {
                {
                    AccountingConstants.ExcessiveMeterSound,
                    Tuple.Create((object)configuration.MoneyLaunderingMonitor?.SoundFilePath ?? string.Empty, false)
                },
                {
                    AccountingConstants.LaunderingMonitorVisible,
                    Tuple.Create((object)configuration.MoneyLaunderingMonitor?.Visible ?? false, false)
                },
                {
                    AccountingConstants.ExcessiveMeterValue,
                    Tuple.Create(InitFromStorage(AccountingConstants.ExcessiveMeterValue), true)
                },
                {
                    AccountingConstants.DisabledDueToExcessiveMeter,
                    Tuple.Create(InitFromStorage(AccountingConstants.DisabledDueToExcessiveMeter), true)
                },
                {
                    AccountingConstants.IncrementThreshold,
                    Tuple.Create(InitFromStorage(AccountingConstants.IncrementThreshold), true)
                },
                {
                    AccountingConstants.IncrementThresholdIsChecked,
                    Tuple.Create(InitFromStorage(AccountingConstants.IncrementThresholdIsChecked), true)
                },
                {
                    PropertyKey.NoteIn,
                    Tuple.Create(InitFromStorage(PropertyKey.NoteIn), true)
                },
                {
                    PropertyKey.VoucherIn,
                    Tuple.Create(InitFromStorage(PropertyKey.VoucherIn), true)
                },
                {
                    AccountingConstants.VoucherInLimit,
                    Tuple.Create((object)voucherInLimit, true)
                },
                {
                    AccountingConstants.VoucherInLimitEnabled,
                    Tuple.Create(InitFromStorage(AccountingConstants.VoucherInLimitEnabled), true)
                },
                {
                    AccountingConstants.VoucherInLimitEditable,
                    Tuple.Create((object)configuration.VoucherIn?.AllowLimitEdit ?? true, false)
                },
                {
                    AccountingConstants.VoucherOut,
                    Tuple.Create(InitFromStorage(AccountingConstants.VoucherOut), true)
                },
                {
                    AccountingConstants.VoucherOutNonCash,
                    Tuple.Create(InitFromStorage(AccountingConstants.VoucherOutNonCash), true)
                },
                {
                    AccountingConstants.VoucherOutMaxSequence,
                    Tuple.Create((object)configuration.VoucherOut?.MaxSequence ?? AccountingConstants.DefaultVoucherOutMaxSequence, false)
                },
                {
                    AccountingConstants.VoucherOutLimit,
                    Tuple.Create((object)voucherOutLimit, true)
                },
                {
                    AccountingConstants.VoucherOutLimitEnabled,
                    Tuple.Create(InitFromStorage(AccountingConstants.VoucherOutLimitEnabled), true)
                },
                {
                    AccountingConstants.VoucherOutLimitEditable,
                    Tuple.Create((object)configuration.VoucherOut?.AllowLimitEdit ?? true, false)
                },
                {
                    AccountingConstants.VoucherOutExpirationDays,
                    Tuple.Create(InitFromStorage(AccountingConstants.VoucherOutExpirationDays), true)
                },
                {
                    AccountingConstants.EditableExpiration,
#if USE_MARKET_CONFIG
                    Tuple.Create((object)configuration.VoucherOut.ExpirationEditable, false)
#else
                    Tuple.Create((object)configuration.VoucherOut?.Expiration?.Editable ?? true, false)
#endif
                },
                {
                    AccountingConstants.AllowCashWinTicket,
                    Tuple.Create((object)configuration.VoucherOut?.AllowCashWinTicket ?? false, false)
                },
                {
                    AccountingConstants.VoucherOutNonCashExpirationDays,
                    Tuple.Create(InitFromStorage(AccountingConstants.VoucherOutNonCashExpirationDays), true)
                },
                {
                    AccountingConstants.VoucherOutMaxAllowed,
                    Tuple.Create((object)configuration.VoucherOut?.LimitMax ?? long.MaxValue, false)
                },
                {
                    AccountingConstants.VoucherInMaxAllowed,
                    Tuple.Create((object)configuration.VoucherIn?.LimitMax ?? long.MaxValue, false)
                },
                {
                    AccountingConstants.CheckLaundryLimit,
                    Tuple.Create((object)configuration.TenderIn.CheckLaundryLimit, false)
                },
                {
                    AccountingConstants.CashInLaundry,
                    Tuple.Create(InitFromStorage(AccountingConstants.CashInLaundry), true)
                },
                {
                    AccountingConstants.VoucherInLaundry,
                    Tuple.Create(InitFromStorage(AccountingConstants.VoucherInLaundry), true)
                },
                {
                    AccountingConstants.ExcessiveDocumentRejectLockupEnabled,
                    Tuple.Create(InitFromStorage(AccountingConstants.ExcessiveDocumentRejectLockupEnabled), true)
                },
                {
                    PropertyKey.MaxCreditsIn,
                    Tuple.Create(InitFromStorage(PropertyKey.MaxCreditsIn), true)
                },
                {
                    AccountingConstants.LargeWinHandpayResetMethod,
                    Tuple.Create(InitFromStorage(AccountingConstants.LargeWinHandpayResetMethod), true)
                },
                {
                    AccountingConstants.HandpayLimit,
                    Tuple.Create(InitFromStorage(AccountingConstants.HandpayLimit), true)
                },
                {
                    AccountingConstants.HandpayLimitEnabled,
                    Tuple.Create(InitFromStorage(AccountingConstants.HandpayLimitEnabled), true)
                },
                {
                    AccountingConstants.LargeWinLimit,
                    Tuple.Create(InitFromStorage(AccountingConstants.LargeWinLimit), true)
                },
                {
                    AccountingConstants.LargeWinLimitMaxValue,
#if USE_MARKET_CONFIG
                    Tuple.Create((object)configuration.LargeWinLimit.MaxAllowed, false)
#else
                    Tuple.Create((object)configuration.WinLimits?.LargeWinLimit?.MaxAllowed ?? AccountingConstants.DefaultHandpayLimit, false)
#endif
                },
                {
                    AccountingConstants.LargeWinLimitEnabled,
                    Tuple.Create(InitFromStorage(AccountingConstants.LargeWinLimitEnabled), true)
                },
                {
                    AccountingConstants.OverwriteLargeWinLimit,
#if USE_MARKET_CONFIG
                    Tuple.Create((object)configuration.LargeWinLimit.Editable, false)
#else
                    Tuple.Create((object)configuration.WinLimits?.LargeWinLimit?.Editable ?? true, false)
#endif
                },
                {
                    AccountingConstants.LargeWinRatio,
                    Tuple.Create(InitFromStorage(AccountingConstants.LargeWinRatio), true)
                },
                {
                    AccountingConstants.LargeWinRatioEnabled,
                    Tuple.Create(InitFromStorage(AccountingConstants.LargeWinRatioEnabled), true)
                },
                {
                    AccountingConstants.OverwriteLargeWinRatio,
#if USE_MARKET_CONFIG
                    Tuple.Create((object)configuration.LargeWinRatio.Editable, false)
#else
                    Tuple.Create((object)configuration.WinLimits?.LargeWinRatio?.Editable ?? true, false)
#endif
                },
                {
                    AccountingConstants.DisplayLargeWinRatio,
#if USE_MARKET_CONFIG
                    Tuple.Create((object)configuration.LargeWinRatio.Visible, false)
#else
                    Tuple.Create((object)configuration.WinLimits?.LargeWinRatio?.Visible ?? false, false)
#endif
                },
                {
                    AccountingConstants.LargeWinRatioThreshold,
                    Tuple.Create(InitFromStorage(AccountingConstants.LargeWinRatioThreshold), true)
                },
                {
                    AccountingConstants.LargeWinRatioThresholdEnabled,
                    Tuple.Create(InitFromStorage(AccountingConstants.LargeWinRatioThresholdEnabled), true)
                },
                {
                    AccountingConstants.OverwriteLargeWinRatioThreshold,
#if USE_MARKET_CONFIG
                    Tuple.Create((object)configuration.LargeWinRatioThreshold.Editable, false)
#else
                    Tuple.Create((object)configuration.WinLimits?.LargeWinRatioThreshold?.Editable ?? true, false)
#endif
                },
                {
                    AccountingConstants.DisplayLargeWinRatioThreshold,
#if USE_MARKET_CONFIG
                    Tuple.Create((object)configuration.LargeWinRatioThreshold.Visible, false)
#else
                    Tuple.Create((object)configuration.WinLimits?.LargeWinRatioThreshold?.Visible ?? false, false)
#endif
                },
                {
                    AccountingConstants.DisplayHandpayResetOptions,
#if USE_MARKET_CONFIG
                    Tuple.Create((object)configuration.HandpayResetVisible, false)
#else
                    Tuple.Create((object)configuration.WinLimits?.HandpayResetOptions?.Visible ?? true, false)
#endif
                },
                {
                    AccountingConstants.CelebrationLockupLimit,
                    Tuple.Create(InitFromStorage(AccountingConstants.CelebrationLockupLimit), true)
                },
                {
                    AccountingConstants.MaxTenderInLimit,
                    Tuple.Create(InitFromStorage(AccountingConstants.MaxTenderInLimit), true)
                },
                {
                    AccountingConstants.CheckCreditsIn,
                    Tuple.Create(InitFromStorage(AccountingConstants.CheckCreditsIn), true)
                },
                {
                    AccountingConstants.AllowCreditUnderLimit,
                    Tuple.Create(InitFromStorage(AccountingConstants.AllowCreditUnderLimit), true)
                },
                {
                    AccountingConstants.EditableMaxCreditsIn,
#if USE_MARKET_CONFIG
                    Tuple.Create((object)configuration.TenderIn.MaxCreditsInEditable, false)
#else
                    Tuple.Create((object)configuration.TenderIn.MaxCreditsIn?.Editable ?? true, false)
#endif
                },
                {
                    AccountingConstants.MaxCreditMeter,
                    Tuple.Create(InitFromStorage(AccountingConstants.MaxCreditMeter), true)
                },
                {
                    AccountingConstants.MaxCreditMeterMaxAllowed,
                    Tuple.Create(InitFromStorage(AccountingConstants.MaxCreditMeterMaxAllowed), true)
                },
                {
                    AccountingConstants.CreditLimitEnabled,
                    Tuple.Create(InitFromStorage(AccountingConstants.CreditLimitEnabled), true)
                },
                {
                    AccountingConstants.ShowMessageWhenCreditLimitReached,
#if USE_MARKET_CONFIG
                    Tuple.Create((object)configuration.MaxCreditMeter.ShowMessageWhenCreditLimitReached, false)
#else
                    Tuple.Create((object)configuration.CreditLimits?.MaxCreditMeter?.ShowMessageWhenCreditLimitReached ?? false, false)
#endif
                },
                {
                    AccountingConstants.DisableBankNoteAcceptorWhenCreditLimitReached,
#if USE_MARKET_CONFIG
                    Tuple.Create( (object)configuration.MaxCreditMeter.DisableBankNoteAcceptorWhenCreditLimitReached, false)
#else
                    Tuple.Create( (object)configuration.CreditLimits?.MaxCreditMeter?.DisableBankNoteAcceptorWhenCreditLimitReached ?? false, false)
#endif
                },
                {
                    AccountingConstants.MaxBetLimit,
                    Tuple.Create(InitFromStorage(AccountingConstants.MaxBetLimit), true)
                },
                {
                    AccountingConstants.HighestMaxBetLimitAllowed,
#if USE_MARKET_CONFIG
                    Tuple.Create((object)configuration.MaxBetLimit.LimitMax, false)
#else
                    Tuple.Create((object)configuration.CreditLimits?.MaxBetLimit?.LimitMax ?? long.MaxValue, false)
#endif
                },
                {
                    AccountingConstants.OverwriteMaxBetLimit,
#if USE_MARKET_CONFIG
                    Tuple.Create((object)configuration.MaxBetLimit.Editable, false)
#else
                    Tuple.Create((object)configuration.CreditLimits?.MaxBetLimit?.Editable ?? false, false)
#endif
                },
                {
                    AccountingConstants.MaxBetLimitEnabled,
                    Tuple.Create(InitFromStorage(AccountingConstants.MaxBetLimitEnabled), true)
                },
                {
                    AccountingConstants.AllowCreditsInAboveMaxCredit,
#if USE_MARKET_CONFIG
                    Tuple.Create((object)configuration.AllowCreditsInAboveMaxCredit, false)
#else
                    Tuple.Create((object)configuration.CreditLimits?.AllowCreditsInAboveMaxCredit, false)
#endif
                },
                { PropertyKey.TicketTextLine1, Tuple.Create(InitFromStorage(PropertyKey.TicketTextLine1), true) },
                { PropertyKey.TicketTextLine2, Tuple.Create(InitFromStorage(PropertyKey.TicketTextLine2), true) },
                { PropertyKey.TicketTextLine3, Tuple.Create(InitFromStorage(PropertyKey.TicketTextLine3), true) },
                { AccountingConstants.TicketTitleCash, Tuple.Create(InitFromStorage(AccountingConstants.TicketTitleCash), true) },
                { AccountingConstants.TicketTitlePromo, Tuple.Create(InitFromStorage(AccountingConstants.TicketTitlePromo), true) },
                { AccountingConstants.TicketTitleNonCash, Tuple.Create(InitFromStorage(AccountingConstants.TicketTitleNonCash), true) },
                {
                    AccountingConstants.TicketTitleLargeWin,
                    Tuple.Create(InitFromStorage(AccountingConstants.TicketTitleLargeWin), true)
                },
                {
                    AccountingConstants.TicketTitleBonusCash,
                    Tuple.Create(InitFromStorage(AccountingConstants.TicketTitleBonusNonCash), true)
                },
                {
                    AccountingConstants.TicketTitleBonusPromo,
                    Tuple.Create(InitFromStorage(AccountingConstants.TicketTitleBonusPromo), true)
                },
                {
                    AccountingConstants.TicketTitleBonusNonCash,
                    Tuple.Create(InitFromStorage(AccountingConstants.TicketTitleBonusNonCash), true)
                },
                { AccountingConstants.TicketTitleWatCash, Tuple.Create(InitFromStorage(AccountingConstants.TicketTitleWatCash), true) },
                {
                    AccountingConstants.TicketTitleWatPromo,
                    Tuple.Create(InitFromStorage(AccountingConstants.TicketTitleWatPromo), true)
                },
                {
                    AccountingConstants.TicketTitleWatNonCash,
                    Tuple.Create(InitFromStorage(AccountingConstants.TicketTitleWatNonCash), true)
                },
                { AccountingConstants.RedeemText, Tuple.Create(InitFromStorage(AccountingConstants.RedeemText), true) },
                { AccountingConstants.IgnoreVoucherStackedDuringReboot, Tuple.Create(InitFromStorage(AccountingConstants.IgnoreVoucherStackedDuringReboot), true) },
                {
                    AccountingConstants.MoneyInEnabled,
                    Tuple.Create(
                        InitFromStorage(
                            AccountingConstants.MoneyInEnabled,
                            value => PublishMoneyInEvent((bool)(value ?? true))),
                        true)
                },
                {
                    AccountingConstants.ReprintLoggedVoucherBehavior,
                    Tuple.Create(InitFromStorage(AccountingConstants.ReprintLoggedVoucherBehavior), true)
                },
                {
                    AccountingConstants.ReprintLoggedVoucherTitleOverride,
                    Tuple.Create(InitFromStorage(AccountingConstants.ReprintLoggedVoucherTitleOverride), true)
                },
                {
                    AccountingConstants.ReprintLoggedVoucherDoorOpenRequirement,
                    Tuple.Create(InitFromStorage(AccountingConstants.ReprintLoggedVoucherDoorOpenRequirement), true)
                },
                {
                    AccountingConstants.TransferOutContext,
                    Tuple.Create(InitTransferOutContext(), true)
                },
                {
                    AccountingConstants.HandpayLargeWinForcedKeyOff,
                    Tuple.Create((object)configuration.Handpay?.LargeWinForcedKeyOff ?? false, false)
                },
                {
                    AccountingConstants.HandpayNoteAcceptorConnectedRequired,
                    Tuple.Create((object)configuration.Handpay?.NoteAcceptorConnectedRequired ?? false, false)
                },
                {
                    AccountingConstants.HandpayLargeWinKeyOffStrategy,
                    Tuple.Create((object)configuration.Handpay?.LargeWinKeyOffStrategy ?? KeyOffType.LocalHandpay, false)
                },
                {
                    AccountingConstants.TicketBarcodeLength,
                    Tuple.Create(InitFromStorage(AccountingConstants.TicketBarcodeLength), true)
                },
                {
                    AccountingConstants.MysteryWinAsExternalBonus,
                    Tuple.Create((object)configuration.MysteryProgressive?.WinAsExternalBonus ?? false, false)
                },
                {
                    AccountingConstants.VoucherOfflineNotify,
                    Tuple.Create((object)configuration.VoucherOut?.Offline ?? true, false)
                },
                {
                    AccountingConstants.CashoutOnCarrierBoardRemovalEnabled,
#if USE_MARKET_CONFIG
                    Tuple.Create((object)configuration.CashoutOnCarrierBoardRemovalEnabled, false)
#else
                    Tuple.Create((object)configuration.CashoutOnCarrierBoardRemoval?.Enabled ?? false, false)
#endif
                },
                {
                    AccountingConstants.NoteAcceptorTimeLimitEnabled,
#if USE_MARKET_CONFIG
                    Tuple.Create((object)configuration.NoteAcceptorTimeLimit.Enabled, false)
#else
                    Tuple.Create((object)configuration.NoteAcceptorTimeLimit?.Enable ?? false, false)
#endif
                },
                {
                    AccountingConstants.NoteAcceptorTimeLimitValue,
                    Tuple.Create((object)configuration.NoteAcceptorTimeLimit?.Value ?? 5000, false)
                },
                {
                    AccountingConstants.MenuSelectionHandpayInProgress,
                    Tuple.Create((object)false, false)
                },
                {
                    AccountingConstants.SeparateMeteringCashableAndPromoOutAmounts,
                    Tuple.Create((object)configuration.VoucherOut?.SeparateMeteringCashableAndPromoAmounts ?? false, false)
                },
                {
                    AccountingConstants.BillClearanceEnabled,
#if USE_MARKET_CONFIG
                    Tuple.Create((object)configuration.BillClearanceEnabled, false)
#else
                    Tuple.Create((object)configuration.BillClearance?.Enabled ?? false, false)
#endif
                },
                {
                    AccountingConstants.TestTicketType,
#if USE_MARKET_CONFIG
                    Tuple.Create((object)configuration.TestTicketType, false)
#else
                    Tuple.Create((object)configuration.TestTicket?.Type ?? string.Empty, false)
#endif
                }
            };

            if (!blockExists)
            {
#if USE_MARKET_CONFIG
                SetProperty(PropertyKey.NoteIn, configuration.NoteInEnabled);
                SetProperty(AccountingConstants.AllowCreditsInAboveMaxCredit, configuration.AllowCreditsInAboveMaxCredit);
#else
                SetProperty(PropertyKey.NoteIn, configuration.NoteIn?.State.Equals("Enabled") ?? true);
                SetProperty(AccountingConstants.AllowCreditsInAboveMaxCredit, configuration.CreditLimits?.AllowCreditsInAboveMaxCredit ?? false);
#endif
                SetProperty(AccountingConstants.VoucherOutLimitEnabled, configuration.VoucherOut?.EnableLimit ?? true);
                SetProperty(AccountingConstants.VoucherInLimitEnabled, configuration.VoucherIn?.EnableLimit ?? true);
                SetProperty(AccountingConstants.TicketTitleWatCash, string.Empty);
                SetProperty(AccountingConstants.CashInLaundry, 0L);
                SetProperty(AccountingConstants.VoucherInLaundry, 0L);
                SetProperty(AccountingConstants.ExcessiveDocumentRejectLockupEnabled, false);

                var machineSettingsImported = propertiesManager.GetValue(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None);
                if (machineSettingsImported == ImportMachineSettings.None)
                {
                // This is just weird, but because the storage block accessor is typed it will return the default value vs. a null
                // It renders the default passed in to GetProperty useless, since it returns the default type.
#if USE_MARKET_CONFIG
                    SetProperty(PropertyKey.VoucherIn, configuration.VoucherIn.Enabled);
                    SetProperty(AccountingConstants.VoucherOut, configuration.VoucherOut.Enabled);
                    SetProperty(AccountingConstants.VoucherOutLimit, Math.Min(configuration.MaxCreditMeter.Default, voucherOutLimit));
                    SetProperty(AccountingConstants.VoucherOutExpirationDays, configuration.VoucherOut.ExpirationDays);
                    SetProperty(AccountingConstants.VoucherOutNonCashExpirationDays, configuration.VoucherOut.ExpirationDays);
                    SetProperty(PropertyKey.MaxCreditsIn, configuration.TenderIn.MaxCreditsInDefault);
                    SetProperty(AccountingConstants.HandpayLimit, configuration.HandpayLimit);
                    SetProperty(AccountingConstants.HandpayLimitEnabled, configuration.HandpayLimit < AccountingConstants.DefaultHandpayLimit);
                    SetProperty(AccountingConstants.LargeWinLimit, configuration.LargeWinLimit.Default);
                    SetProperty(AccountingConstants.LargeWinLimitEnabled, configuration.LargeWinLimit.Default < AccountingConstants.DefaultLargeWinLimit);
                    SetProperty(AccountingConstants.OverwriteLargeWinLimit, configuration.LargeWinLimit.Editable);
                    SetProperty(AccountingConstants.LargeWinRatio, configuration.LargeWinRatio.Default);
                    SetProperty(AccountingConstants.LargeWinRatioEnabled, configuration.LargeWinRatio.Default == AccountingConstants.DefaultLargeWinRatio);
                    SetProperty(AccountingConstants.OverwriteLargeWinRatio, configuration.LargeWinRatio.Editable);
                    SetProperty(AccountingConstants.DisplayLargeWinRatio, configuration.LargeWinRatio.Visible);
                    SetProperty(AccountingConstants.LargeWinRatioThreshold, configuration.LargeWinRatioThreshold.Default);
                    SetProperty(AccountingConstants.LargeWinRatioThresholdEnabled, configuration.LargeWinRatioThreshold.Default == AccountingConstants.DefaultLargeWinRatioThreshold);
                    SetProperty(AccountingConstants.OverwriteLargeWinRatioThreshold, configuration.LargeWinRatioThreshold.Editable);
                    SetProperty(AccountingConstants.DisplayLargeWinRatioThreshold, configuration.LargeWinRatioThreshold.Visible);
                    SetProperty(AccountingConstants.DisplayHandpayResetOptions, configuration.HandpayResetVisible);
                    SetProperty(AccountingConstants.CelebrationLockupLimit, configuration.CelebrationLockupLimit);
                    SetProperty(AccountingConstants.MaxCreditMeter, configuration.MaxCreditMeter.Default);
                    SetProperty(AccountingConstants.MaxCreditMeterMaxAllowed, configuration.MaxCreditMeter.MaxAllowed);
                    SetProperty(AccountingConstants.CreditLimitEnabled, configuration.MaxCreditMeter.Default != configuration.MaxCreditMeter.MaxAllowed);
                    SetProperty(AccountingConstants.ShowMessageWhenCreditLimitReached, configuration.MaxCreditMeter.ShowMessageWhenCreditLimitReached);
                    SetProperty(AccountingConstants.MaxBetLimit, configuration.MaxBetLimit.Default);
                    SetProperty(AccountingConstants.OverwriteMaxBetLimit, configuration.MaxBetLimit.Editable);
                    SetProperty(AccountingConstants.MaxBetLimitEnabled, configuration.MaxBetLimit.Default != long.MaxValue);
#else
                    SetProperty(PropertyKey.VoucherIn, configuration.VoucherIn.State.Equals("Enabled"));
                    SetProperty(AccountingConstants.VoucherOut, configuration.VoucherOut?.State.Equals("Enabled"));
                    SetProperty(AccountingConstants.VoucherOutLimit, Math.Min(configuration.CreditLimits?.MaxCreditMeter?.Default ?? long.MaxValue, voucherOutLimit));
                    SetProperty(AccountingConstants.VoucherOutExpirationDays, configuration.VoucherOut?.Expiration?.Days ?? AccountingConstants.DefaultVoucherExpirationDays);
                    SetProperty(AccountingConstants.VoucherOutNonCashExpirationDays, configuration.VoucherOut?.Expiration?.Days ?? AccountingConstants.DefaultVoucherExpirationDays);
                    SetProperty(PropertyKey.MaxCreditsIn, configuration.TenderIn.MaxCreditsIn?.Default ?? ApplicationConstants.DefaultMaxCreditsIn);
                    SetProperty(AccountingConstants.HandpayLimit, configuration.WinLimits?.HandpayLimit ?? AccountingConstants.DefaultHandpayLimit);
                    SetProperty(AccountingConstants.HandpayLimitEnabled, configuration.WinLimits?.HandpayLimit < AccountingConstants.DefaultHandpayLimit);
                    SetProperty(AccountingConstants.LargeWinLimit, configuration.WinLimits?.LargeWinLimit?.Default ?? AccountingConstants.DefaultLargeWinLimit);
                    // TODO This has a weird side effect of comparing null to a long when the value isn't set in the object
                    SetProperty(AccountingConstants.LargeWinLimitEnabled, configuration.WinLimits?.LargeWinLimit?.Default < AccountingConstants.DefaultLargeWinLimit);
                    SetProperty(AccountingConstants.OverwriteLargeWinLimit, configuration.WinLimits?.LargeWinLimit?.Editable ?? true);
                    SetProperty(AccountingConstants.LargeWinRatio, configuration.WinLimits?.LargeWinRatio?.Default ?? AccountingConstants.DefaultLargeWinRatio);
                    SetProperty(AccountingConstants.LargeWinRatioEnabled, configuration.WinLimits?.LargeWinRatio?.Default == AccountingConstants.DefaultLargeWinRatio);
                    SetProperty(AccountingConstants.OverwriteLargeWinRatio, configuration.WinLimits?.LargeWinRatio?.Editable ?? true);
                    SetProperty(AccountingConstants.DisplayLargeWinRatio, configuration.WinLimits?.LargeWinRatio?.Visible ?? false);
                    SetProperty(AccountingConstants.LargeWinRatioThreshold, configuration.WinLimits?.LargeWinRatioThreshold?.Default ?? AccountingConstants.DefaultLargeWinRatioThreshold);
                    SetProperty(AccountingConstants.LargeWinRatioThresholdEnabled, configuration.WinLimits?.LargeWinRatioThreshold?.Default == AccountingConstants.DefaultLargeWinRatioThreshold);
                    SetProperty(AccountingConstants.OverwriteLargeWinRatioThreshold, configuration.WinLimits?.LargeWinRatioThreshold?.Editable ?? true);
                    SetProperty(AccountingConstants.DisplayLargeWinRatioThreshold, configuration.WinLimits?.LargeWinRatioThreshold?.Visible ?? false);
                    SetProperty(AccountingConstants.DisplayHandpayResetOptions, configuration.WinLimits?.HandpayResetOptions?.Visible ?? true);
                    SetProperty(AccountingConstants.CelebrationLockupLimit, configuration.WinLimits?.CelebrationLockupLimit ?? 0L);
                    SetProperty(AccountingConstants.MaxCreditMeter, configuration.CreditLimits?.MaxCreditMeter?.Default ?? long.MaxValue);
                    SetProperty(AccountingConstants.MaxCreditMeterMaxAllowed, configuration.CreditLimits?.MaxCreditMeter?.MaxAllowed ?? long.MaxValue);
                    SetProperty(AccountingConstants.CreditLimitEnabled, configuration.CreditLimits?.MaxCreditMeter?.Default != configuration.CreditLimits?.MaxCreditMeter?.MaxAllowed);
                    SetProperty(AccountingConstants.ShowMessageWhenCreditLimitReached, configuration.CreditLimits?.MaxCreditMeter?.ShowMessageWhenCreditLimitReached ?? false);
                    SetProperty(AccountingConstants.MaxBetLimit, configuration.CreditLimits?.MaxBetLimit?.Default ?? AccountingConstants.DefaultMaxBetLimit);
                    SetProperty(AccountingConstants.OverwriteMaxBetLimit, configuration.CreditLimits?.MaxBetLimit?.Editable ?? false);
                    SetProperty(AccountingConstants.MaxBetLimitEnabled, configuration.CreditLimits?.MaxBetLimit?.Default != long.MaxValue);
    #endif
                    SetProperty(AccountingConstants.VoucherOutNonCash, configuration.VoucherOut?.AllowNonCashableTicket.Equals("Enabled"));
                    SetProperty(AccountingConstants.LargeWinHandpayResetMethod, (int)LargeWinHandpayResetMethod.PayByHand);
                    SetProperty(AccountingConstants.MaxTenderInLimit, configuration.TenderIn.MaxTenderInLimit);
                    SetProperty(PropertyKey.TicketTextLine1, string.Empty);
                    SetProperty(PropertyKey.TicketTextLine2, string.Empty);
                    SetProperty(PropertyKey.TicketTextLine3, string.Empty);
                    SetProperty(AccountingConstants.TicketTitleCash, Localizer.For(CultureFor.Player).GetString(ResourceKeys.CashoutTicket));
                    SetProperty(AccountingConstants.TicketTitlePromo, AccountingConstants.DefaultNonCashTicketTitle);
                    SetProperty(AccountingConstants.TicketTitleNonCash, string.Empty);
                    SetProperty(AccountingConstants.TicketTitleLargeWin, AccountingConstants.DefaultLargeWinTicketTitle);
                    SetProperty(AccountingConstants.TicketTitleBonusCash, string.Empty);
                    SetProperty(AccountingConstants.TicketTitleBonusPromo, string.Empty);
                    SetProperty(AccountingConstants.TicketTitleBonusNonCash, string.Empty);
                    SetProperty(AccountingConstants.TicketTitleWatPromo, string.Empty);
                    SetProperty(AccountingConstants.TicketTitleWatNonCash, string.Empty);
                    SetProperty(AccountingConstants.RedeemText, string.Empty);
                    SetProperty(AccountingConstants.MoneyInEnabled, true);
                    SetProperty(AccountingConstants.IgnoreVoucherStackedDuringReboot, false);
                    // TODO This is a workaround to get the value for the check credits in strategy until the config manager supports enums
                    if (!Enum.TryParse(configuration.TenderIn.CheckCreditsIn, out CheckCreditsStrategy checkCreditsIn))
                    {
                        checkCreditsIn = CheckCreditsStrategy.None;
                    }
                    SetProperty(AccountingConstants.CheckCreditsIn, checkCreditsIn);
                    SetProperty(AccountingConstants.AllowCreditUnderLimit, configuration.TenderIn.AllowCreditUnderLimit);
                    SetProperty(AccountingConstants.ReprintLoggedVoucherBehavior, configuration.ReprintLoggedVoucher.Behavior);
                    SetProperty(AccountingConstants.ReprintLoggedVoucherTitleOverride, configuration.ReprintLoggedVoucher.TitleOverride);
                    SetProperty(AccountingConstants.ReprintLoggedVoucherDoorOpenRequirement, configuration.ReprintLoggedVoucher.DoorOpenRequirement);
                    SetProperty(AccountingConstants.TicketBarcodeLength, AccountingConstants.DefaultTicketBarcodeLength);
                    SetProperty(AccountingConstants.IncrementThreshold, AccountingConstants.DefaultIncrementThreshold);
                    SetProperty(AccountingConstants.IncrementThresholdIsChecked, configuration.MoneyLaunderingMonitor?.Enabled ?? false);
                }
                else
                {
                    // The following settings were imported and set to the default property provider since the accounting property provider was not yet loaded
                    // at the time of import, so we set each to their imported values here.
                    SetProperty(PropertyKey.TicketTextLine1, propertiesManager.GetValue(PropertyKey.TicketTextLine1, string.Empty));
                    SetProperty(PropertyKey.TicketTextLine2, propertiesManager.GetValue(PropertyKey.TicketTextLine2, string.Empty));
                    SetProperty(PropertyKey.TicketTextLine3, propertiesManager.GetValue(PropertyKey.TicketTextLine3, string.Empty));
                    SetProperty(PropertyKey.MaxCreditsIn, propertiesManager.GetValue(PropertyKey.MaxCreditsIn, ApplicationConstants.DefaultMaxCreditsIn));
                    SetProperty(PropertyKey.VoucherIn, propertiesManager.GetValue(PropertyKey.VoucherIn, false));
                    SetProperty(AccountingConstants.AllowCashWinTicket, propertiesManager.GetValue(AccountingConstants.AllowCashWinTicket, false));
                    SetProperty(AccountingConstants.AllowCreditUnderLimit, propertiesManager.GetValue(AccountingConstants.AllowCreditUnderLimit, false));
                    SetProperty(AccountingConstants.CelebrationLockupLimit, propertiesManager.GetValue(AccountingConstants.CelebrationLockupLimit, long.MaxValue));
                    SetProperty(AccountingConstants.CheckCreditsIn, propertiesManager.GetValue(AccountingConstants.CheckCreditsIn, CheckCreditsStrategy.None));
                    SetProperty(AccountingConstants.IgnoreVoucherStackedDuringReboot, propertiesManager.GetValue(AccountingConstants.IgnoreVoucherStackedDuringReboot, false));
                    SetProperty(AccountingConstants.LargeWinLimit, propertiesManager.GetValue(AccountingConstants.LargeWinLimit, AccountingConstants.DefaultLargeWinLimit));
                    SetProperty(AccountingConstants.LargeWinLimitEnabled, propertiesManager.GetValue(AccountingConstants.LargeWinLimitEnabled, true));
                    SetProperty(AccountingConstants.OverwriteLargeWinLimit, propertiesManager.GetValue(AccountingConstants.OverwriteLargeWinLimit, false));
                    SetProperty(AccountingConstants.LargeWinRatio, propertiesManager.GetValue(AccountingConstants.LargeWinRatio, AccountingConstants.DefaultLargeWinRatio));
                    SetProperty(AccountingConstants.LargeWinRatioEnabled, propertiesManager.GetValue(AccountingConstants.LargeWinRatioEnabled, false));
                    SetProperty(AccountingConstants.OverwriteLargeWinRatio, propertiesManager.GetValue(AccountingConstants.OverwriteLargeWinRatio, false));
                    SetProperty(AccountingConstants.LargeWinRatioThreshold, propertiesManager.GetValue(AccountingConstants.LargeWinRatioThreshold, AccountingConstants.DefaultLargeWinRatioThreshold));
                    SetProperty(AccountingConstants.LargeWinRatioThresholdEnabled, propertiesManager.GetValue(AccountingConstants.LargeWinRatioThresholdEnabled, false));
                    SetProperty(AccountingConstants.OverwriteLargeWinRatioThreshold, propertiesManager.GetValue(AccountingConstants.OverwriteLargeWinRatioThreshold, false));
                    SetProperty(AccountingConstants.MaxBetLimit, propertiesManager.GetValue(AccountingConstants.MaxBetLimit, AccountingConstants.DefaultMaxBetLimit));
                    SetProperty(AccountingConstants.OverwriteMaxBetLimit, propertiesManager.GetValue(AccountingConstants.OverwriteMaxBetLimit, false));
                    SetProperty(AccountingConstants.MaxCreditMeter, propertiesManager.GetValue(AccountingConstants.MaxCreditMeter, long.MaxValue));
                    SetProperty(AccountingConstants.MaxCreditMeterMaxAllowed, propertiesManager.GetValue(AccountingConstants.MaxCreditMeterMaxAllowed, long.MaxValue));
                    SetProperty(AccountingConstants.MaxTenderInLimit, propertiesManager.GetValue(AccountingConstants.MaxTenderInLimit, AccountingConstants.DefaultMaxTenderInLimit));
                    SetProperty(AccountingConstants.MoneyInEnabled, propertiesManager.GetValue(AccountingConstants.MoneyInEnabled, false));
                    SetProperty(AccountingConstants.RedeemText, propertiesManager.GetValue(AccountingConstants.RedeemText, string.Empty));
                    SetProperty(AccountingConstants.ReprintLoggedVoucherBehavior, propertiesManager.GetValue(AccountingConstants.ReprintLoggedVoucherBehavior, "None"));
                    SetProperty(AccountingConstants.ReprintLoggedVoucherDoorOpenRequirement, propertiesManager.GetValue(AccountingConstants.ReprintLoggedVoucherDoorOpenRequirement, "None"));
                    SetProperty(AccountingConstants.ReprintLoggedVoucherTitleOverride, propertiesManager.GetValue(AccountingConstants.ReprintLoggedVoucherTitleOverride, false));
                    SetProperty(AccountingConstants.TicketBarcodeLength, propertiesManager.GetValue(AccountingConstants.TicketBarcodeLength, AccountingConstants.DefaultTicketBarcodeLength));
                    SetProperty(AccountingConstants.TicketTitleBonusCash, propertiesManager.GetValue(AccountingConstants.TicketTitleBonusCash, string.Empty));
                    SetProperty(AccountingConstants.TicketTitleBonusNonCash, propertiesManager.GetValue(AccountingConstants.TicketTitleBonusNonCash, string.Empty));
                    SetProperty(AccountingConstants.TicketTitleBonusPromo, propertiesManager.GetValue(AccountingConstants.TicketTitleBonusPromo, string.Empty));
                    SetProperty(AccountingConstants.TicketTitleCash, propertiesManager.GetValue(AccountingConstants.TicketTitleCash, AccountingConstants.DefaultCashoutTicketTitle));
                    SetProperty(AccountingConstants.TicketTitleLargeWin, propertiesManager.GetValue(AccountingConstants.TicketTitleLargeWin, AccountingConstants.DefaultLargeWinTicketTitle));
                    SetProperty(AccountingConstants.TicketTitleNonCash, propertiesManager.GetValue(AccountingConstants.TicketTitleNonCash, string.Empty));
                    SetProperty(AccountingConstants.TicketTitlePromo, propertiesManager.GetValue(AccountingConstants.TicketTitlePromo, string.Empty));
                    SetProperty(AccountingConstants.TicketTitleWatNonCash, propertiesManager.GetValue(AccountingConstants.TicketTitleWatNonCash, string.Empty));
                    SetProperty(AccountingConstants.TicketTitleWatPromo, propertiesManager.GetValue(AccountingConstants.TicketTitleWatPromo, string.Empty));
                    SetProperty(AccountingConstants.VoucherInLimit, propertiesManager.GetValue(AccountingConstants.VoucherInLimit, AccountingConstants.DefaultVoucherInLimit));
                    SetProperty(AccountingConstants.VoucherOut, propertiesManager.GetValue(AccountingConstants.VoucherOut, true));
                    SetProperty(AccountingConstants.VoucherOutExpirationDays, propertiesManager.GetValue(AccountingConstants.VoucherOutExpirationDays, AccountingConstants.DefaultVoucherExpirationDays));
                    SetProperty(AccountingConstants.VoucherOutLimit, propertiesManager.GetValue(AccountingConstants.VoucherOutLimit, AccountingConstants.DefaultVoucherOutLimit));
                    SetProperty(AccountingConstants.VoucherOutNonCash, propertiesManager.GetValue(AccountingConstants.VoucherOutNonCash, true));
                    SetProperty(AccountingConstants.VoucherOutNonCashExpirationDays, propertiesManager.GetValue(AccountingConstants.VoucherOutNonCashExpirationDays, AccountingConstants.DefaultVoucherExpirationDays));
                    SetProperty(AccountingConstants.HandpayLimit, propertiesManager.GetValue(AccountingConstants.HandpayLimit, long.MaxValue));
                    SetProperty(AccountingConstants.LargeWinHandpayResetMethod, propertiesManager.GetValue(AccountingConstants.LargeWinHandpayResetMethod, LargeWinHandpayResetMethod.PayByHand));
                    SetProperty(AccountingConstants.HandpayLimitEnabled, propertiesManager.GetValue(AccountingConstants.HandpayLimitEnabled, true));
                    SetProperty(AccountingConstants.CreditLimitEnabled, propertiesManager.GetValue(AccountingConstants.CreditLimitEnabled, true));
                    SetProperty(AccountingConstants.ShowMessageWhenCreditLimitReached, propertiesManager.GetValue(AccountingConstants.ShowMessageWhenCreditLimitReached, false));
                    SetProperty(AccountingConstants.MaxBetLimitEnabled, propertiesManager.GetValue(AccountingConstants.MaxBetLimitEnabled, true));
                    SetProperty(AccountingConstants.IncrementThreshold, propertiesManager.GetValue(AccountingConstants.IncrementThreshold, AccountingConstants.DefaultIncrementThreshold));
                    SetProperty(AccountingConstants.IncrementThresholdIsChecked, propertiesManager.GetValue(AccountingConstants.IncrementThresholdIsChecked, true));

                    machineSettingsImported |= ImportMachineSettings.AccountingPropertiesLoaded;
                    propertiesManager.SetProperty(ApplicationConstants.MachineSettingsImported, machineSettingsImported);
                }
            }
        }

        /// <inheritdoc />
        public ICollection<KeyValuePair<string, object>> GetCollection
            =>
                new List<KeyValuePair<string, object>>(
                    _properties.Select(p => new KeyValuePair<string, object>(p.Key, p.Value.Item1)));

        /// <inheritdoc />
        public object GetProperty(string propertyName)
        {
            if (_properties.TryGetValue(propertyName, out var value))
            {
                return value.Item1;
            }

            var errorMessage = "Unknown accounting property: " + propertyName;
            Logger.Error(errorMessage);
            throw new UnknownPropertyException(errorMessage);
        }

        /// <inheritdoc />
        public void SetProperty(string propertyName, object propertyValue)
        {
            if (!_properties.TryGetValue(propertyName, out var value))
            {
                var errorMessage = $"Cannot set unknown property: {propertyName}";
                Logger.Error(errorMessage);
                throw new UnknownPropertyException(errorMessage);
            }

            // NOTE:  Not all properties are persisted
            if (value.Item2)
            {
                Logger.Debug(
                    $"setting property {propertyName} to {propertyValue}. Type is {propertyValue?.GetType()}");

                switch (propertyName)
                {
                    case AccountingConstants.TransferOutContext:
                        if (propertyValue == null)
                        {
                            _persistentStorageAccessor[AccountingConstants.TransferOutContext] = null;
                        }
                        else if (propertyValue is TransferOutContext)
                        {
                            _persistentStorageAccessor[AccountingConstants.TransferOutContext] =
                                JsonConvert.SerializeObject(propertyValue, Formatting.None);
                        }
                        break;

                    default:
                        _persistentStorageAccessor[propertyName] = propertyValue;
                        break;
                }
            }

            _properties[propertyName] = Tuple.Create(propertyValue, value.Item2);

            switch (propertyName)
            {
                case AccountingConstants.MoneyInEnabled:
                    if (propertyValue is bool enabled)
                    {
                        PublishMoneyInEvent(enabled);
                    }
                    break;
            }
        }

        private object InitFromStorage(string propertyName, Action<object> onInitialized = null)
        {
            var value = _persistentStorageAccessor[propertyName];

            onInitialized?.Invoke(value);

            return value;
        }

        private object InitTransferOutContext()
        {
            var context = (string)_persistentStorageAccessor[AccountingConstants.TransferOutContext];

            return string.IsNullOrEmpty(context) ? null : JsonConvert.DeserializeObject<TransferOutContext>(context);
        }

        private void PublishMoneyInEvent(bool enabled)
        {
            if (enabled)
            {
                ServiceManager.GetInstance().GetService<IEventBus>().Publish(new MoneyInEnabledEvent());
            }
            else
            {
                ServiceManager.GetInstance().GetService<IEventBus>().Publish(new MoneyInDisabledEvent());
            }
        }
    }
}
