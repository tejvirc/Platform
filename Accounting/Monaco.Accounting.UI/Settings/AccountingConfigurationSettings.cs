namespace Aristocrat.Monaco.Accounting.UI.Settings
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using Application.Contracts.Settings;
    using Contracts;
    using Contracts.Handpay;
    using Kernel;
    using MVVM;

    /// <summary>
    ///     Implements <see cref="IConfigurationSettings"/> for Accounting settings.
    /// </summary>
    public class AccountingConfigurationSettings : IConfigurationSettings
    {
        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AccountingConfigurationSettings"/> class.
        /// </summary>
        public AccountingConfigurationSettings()
            : this(ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AccountingConfigurationSettings"/> class.
        /// </summary>
        /// <param name="properties">A <see cref="IPropertiesManager"/> instance.</param>
        public AccountingConfigurationSettings(IPropertiesManager properties)
        {
            _properties = properties;
        }

        /// <inheritdoc />
        public string Name => "Accounting";

        /// <inheritdoc />
        public ConfigurationGroup Groups => ConfigurationGroup.Machine;

        /// <inheritdoc />
        public async Task<object> Get(ConfigurationGroup configGroup)
        {
            if (!Groups.HasFlag(configGroup))
            {
                throw new ArgumentOutOfRangeException(nameof(configGroup));
            }

            return await GetSettings();
        }

        /// <inheritdoc />
        public async Task Initialize()
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    var resourceDictionary = new ResourceDictionary
                    {
                        Source = new Uri("/Monaco.Accounting.UI;component/Settings/MachineSettings.xaml", UriKind.RelativeOrAbsolute)
                    };

                    Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
                });

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task Apply(ConfigurationGroup configGroup, object settings)
        {
            if (!Groups.HasFlag(configGroup))
            {
                throw new ArgumentOutOfRangeException(nameof(configGroup));
            }

            if (!(settings is MachineSettings machineSettings))
            {
                throw new ArgumentException($@"Invalid settings type, {settings?.GetType()}", nameof(settings));
            }

            await ApplySettings(machineSettings);
        }

        private async Task<MachineSettings> GetSettings()
        {
            return await Task.FromResult(

                new MachineSettings
                {
                    AllowCashWinTicket =
                        _properties.GetValue(AccountingConstants.AllowCashWinTicket, false),
                    AllowCreditUnderLimit =
                        _properties.GetValue(AccountingConstants.AllowCreditUnderLimit, false),
                    CelebrationLockupLimit =
                        _properties.GetValue(AccountingConstants.CelebrationLockupLimit, long.MaxValue),
                    CheckCreditsIn =
                        _properties.GetValue(AccountingConstants.CheckCreditsIn, CheckCreditsStrategy.None),
                    CombineCashableOut =
                        _properties.GetValue(AccountingConstants.CombineCashableOut, false),
                    DisabledLocalCredit =
                        _properties.GetValue(AccountingConstants.DisabledLocalCredit, false),
                    DisabledLocalHandpay =
                        _properties.GetValue(AccountingConstants.DisabledLocalHandpay, false),
                    DisabledLocalVoucher =
                        _properties.GetValue(AccountingConstants.DisabledLocalVoucher, false),
                    DisabledLocalWat =
                        _properties.GetValue(AccountingConstants.DisabledLocalWat, false),
                    DisabledRemoteCredit =
                        _properties.GetValue(AccountingConstants.DisabledRemoteCredit, false),
                    DisabledRemoteHandpay =
                        _properties.GetValue(AccountingConstants.DisabledRemoteHandpay, false),
                    DisabledRemoteVoucher =
                        _properties.GetValue(AccountingConstants.DisabledRemoteVoucher, false),
                    DisabledRemoteWat =
                        _properties.GetValue(AccountingConstants.DisabledRemoteWat, false),
                    EnabledLocalCredit =
                        _properties.GetValue(AccountingConstants.EnabledLocalCredit, false),
                    EnabledLocalHandpay =
                        _properties.GetValue(AccountingConstants.EnabledLocalHandpay, false),
                    EnabledLocalVoucher =
                        _properties.GetValue(AccountingConstants.EnabledLocalVoucher, false),
                    EnabledLocalWat =
                        _properties.GetValue(AccountingConstants.EnabledLocalWat, false),
                    EnabledRemoteCredit =
                        _properties.GetValue(AccountingConstants.EnabledRemoteCredit, false),
                    EnabledRemoteHandpay =
                        _properties.GetValue(AccountingConstants.EnabledRemoteHandpay, false),
                    EnabledRemoteVoucher =
                        _properties.GetValue(AccountingConstants.EnabledRemoteVoucher, false),
                    EnabledRemoteWat =
                        _properties.GetValue(AccountingConstants.EnabledRemoteWat, false),
                    EnableReceipts =
                        _properties.GetValue(AccountingConstants.EnableReceipts, false),
                    IdReaderId =
                        _properties.GetValue(AccountingConstants.IdReaderId, 0),
                    IgnoreVoucherStackedDuringReboot =
                        _properties.GetValue(AccountingConstants.IgnoreVoucherStackedDuringReboot, false),
                    LocalKeyOff =
                        _properties.GetValue(AccountingConstants.LocalKeyOff, LocalKeyOff.AnyKeyOff),
                    LargeWinLimit =
                        _properties.GetValue(AccountingConstants.LargeWinLimit, AccountingConstants.DefaultLargeWinLimit),
                    OverwriteLargeWinLimit =
                        _properties.GetValue(AccountingConstants.OverwriteLargeWinLimit, false),
                    LargeWinRatio =
                        _properties.GetValue(AccountingConstants.LargeWinRatio, AccountingConstants.DefaultLargeWinRatio),
                    OverwriteLargeWinRatio =
                        _properties.GetValue(AccountingConstants.OverwriteLargeWinRatio, false),
                    LargeWinRatioIsChecked =
                        _properties.GetValue(AccountingConstants.LargeWinRatioEnabled, false),
                    LargeWinRatioThreshold =
                        _properties.GetValue(AccountingConstants.LargeWinRatioThreshold, AccountingConstants.DefaultLargeWinRatioThreshold),
                    OverwriteLargeWinRatioThreshold =
                        _properties.GetValue(AccountingConstants.OverwriteLargeWinRatioThreshold, false),
                    LargeWinRatioThresholdIsChecked =
                        _properties.GetValue(AccountingConstants.LargeWinRatioThresholdEnabled, false),
                    MaxBetLimit =
                        _properties.GetValue(AccountingConstants.MaxBetLimit, AccountingConstants.DefaultMaxBetLimit),
                    OverwriteMaxBetLimit =
                        _properties.GetValue(AccountingConstants.OverwriteMaxBetLimit, false),
                    MaxCreditMeter =
                        _properties.GetValue(AccountingConstants.MaxCreditMeter, long.MaxValue),
                    MaxCreditMeterMaxAllowed =
                        _properties.GetValue(AccountingConstants.MaxCreditMeterMaxAllowed, long.MaxValue),
                    MaxTenderInLimit =
                        _properties.GetValue(AccountingConstants.MaxTenderInLimit, AccountingConstants.DefaultMaxTenderInLimit),
                    MixCreditTypes =
                        _properties.GetValue(AccountingConstants.MixCreditTypes, true),
                    MoneyInEnabled =
                        _properties.GetValue(AccountingConstants.MoneyInEnabled, false),
                    PartialHandpays =
                        _properties.GetValue(AccountingConstants.PartialHandpays, false),
                    RedeemText =
                        _properties.GetValue(AccountingConstants.RedeemText, string.Empty),
                    ReprintLoggedVoucherBehavior =
                        _properties.GetValue(AccountingConstants.ReprintLoggedVoucherBehavior, "None"),
                    ReprintLoggedVoucherDoorOpenRequirement =
                        _properties.GetValue(AccountingConstants.ReprintLoggedVoucherDoorOpenRequirement, "None"),
                    ReprintLoggedVoucherTitleOverride =
                        _properties.GetValue(AccountingConstants.ReprintLoggedVoucherTitleOverride, false),
                    RequestNonCash =
                        _properties.GetValue(AccountingConstants.RequestNonCash, false),
                    TicketBarcodeLength =
                        _properties.GetValue(AccountingConstants.TicketBarcodeLength, AccountingConstants.DefaultTicketBarcodeLength),
                    TicketTitleBonusCash =
                        _properties.GetValue(AccountingConstants.TicketTitleBonusCash, string.Empty),
                    TicketTitleBonusNonCash =
                        _properties.GetValue(AccountingConstants.TicketTitleBonusNonCash, string.Empty),
                    TicketTitleBonusPromo =
                        _properties.GetValue(AccountingConstants.TicketTitleBonusPromo, string.Empty),
                    TicketTitleCash =
                        _properties.GetValue(AccountingConstants.TicketTitleCash, AccountingConstants.DefaultCashoutTicketTitle),
                    TicketTitleLargeWin =
                        _properties.GetValue(AccountingConstants.TicketTitleLargeWin, AccountingConstants.DefaultLargeWinTicketTitle),
                    TicketTitleNonCash =
                        _properties.GetValue(AccountingConstants.TicketTitleNonCash, string.Empty),
                    TicketTitlePromo =
                        _properties.GetValue(AccountingConstants.TicketTitlePromo, string.Empty),
                    TicketTitleWatNonCash =
                        _properties.GetValue(AccountingConstants.TicketTitleWatNonCash, string.Empty),
                    TicketTitleWatPromo =
                        _properties.GetValue(AccountingConstants.TicketTitleWatPromo, string.Empty),
                    TitleCancelReceipt =
                        _properties.GetValue(AccountingConstants.TitleCancelReceipt, string.Empty),
                    TitleJackpotReceipt =
                        _properties.GetValue(AccountingConstants.TitleJackpotReceipt, string.Empty),
                    UsePlayerIdReader =
                        _properties.GetValue(AccountingConstants.UsePlayerIdReader, false),
                    ValidateHandpays =
                        _properties.GetValue(AccountingConstants.ValidateHandpays, false),
                    VoucherInLimit =
                        _properties.GetValue(AccountingConstants.VoucherInLimit, AccountingConstants.DefaultVoucherInLimit),
                    VoucherOut =
                        _properties.GetValue(AccountingConstants.VoucherOut, true),
                    VoucherOutExpirationDays =
                        _properties.GetValue(AccountingConstants.VoucherOutExpirationDays, AccountingConstants.DefaultVoucherExpirationDays),
                    VoucherOutLimit =
                        _properties.GetValue(AccountingConstants.VoucherOutLimit, AccountingConstants.DefaultVoucherOutLimit),
                    VoucherOutNonCash =
                        _properties.GetValue(AccountingConstants.VoucherOutNonCash, true),
                    VoucherOutNonCashExpirationDays =
                        _properties.GetValue(AccountingConstants.VoucherOutNonCashExpirationDays, AccountingConstants.DefaultVoucherExpirationDays),
                    VoucherOutCheckBoxChecked = 
                        _properties.GetValue(AccountingConstants.VoucherOutLimitEnabled, true),
                    VoucherInCheckBoxChecked =
                        _properties.GetValue(AccountingConstants.VoucherInLimitEnabled, true),
                    HandpayLimit =
                        _properties.GetValue(AccountingConstants.HandpayLimit, long.MaxValue),
                    AllowRemoteHandpayReset =
                        _properties.GetValue(AccountingConstants.RemoteHandpayResetAllowed, true),
                    LargeWinHandpayResetMethod = 
                        _properties.GetValue(AccountingConstants.LargeWinHandpayResetMethod, LargeWinHandpayResetMethod.PayByHand),
                    HandpayLimitIsChecked =
                        _properties.GetValue(AccountingConstants.HandpayLimitEnabled, true),
                    LargeWinLimitIsChecked =
                        _properties.GetValue(AccountingConstants.LargeWinLimitEnabled, true),
                    CreditLimitIsChecked =
                    _properties.GetValue(AccountingConstants.CreditLimitEnabled, true),
                    MaxBetLimitIsChecked =
                        _properties.GetValue(AccountingConstants.MaxBetLimitEnabled, true)
                });
        }

        private async Task ApplySettings(MachineSettings settings)
        {
            _properties.SetProperty(AccountingConstants.AllowCashWinTicket, settings.AllowCashWinTicket);
            _properties.SetProperty(AccountingConstants.AllowCreditUnderLimit, settings.AllowCreditUnderLimit);
            _properties.SetProperty(AccountingConstants.CelebrationLockupLimit, settings.CelebrationLockupLimit);
            _properties.SetProperty(AccountingConstants.CheckCreditsIn, settings.CheckCreditsIn);
            _properties.SetProperty(AccountingConstants.CombineCashableOut, settings.CombineCashableOut);
            _properties.SetProperty(AccountingConstants.DisabledLocalCredit, settings.DisabledLocalCredit);
            _properties.SetProperty(AccountingConstants.DisabledLocalHandpay, settings.DisabledLocalHandpay);
            _properties.SetProperty(AccountingConstants.DisabledLocalVoucher, settings.DisabledLocalVoucher);
            _properties.SetProperty(AccountingConstants.DisabledLocalWat, settings.DisabledLocalWat);
            _properties.SetProperty(AccountingConstants.DisabledRemoteCredit, settings.DisabledRemoteCredit);
            _properties.SetProperty(AccountingConstants.DisabledRemoteHandpay, settings.DisabledRemoteHandpay);
            _properties.SetProperty(AccountingConstants.DisabledRemoteVoucher, settings.DisabledRemoteVoucher);
            _properties.SetProperty(AccountingConstants.DisabledRemoteWat, settings.DisabledRemoteWat);
            _properties.SetProperty(AccountingConstants.EnabledLocalCredit, settings.EnabledLocalCredit);
            _properties.SetProperty(AccountingConstants.EnabledLocalHandpay, settings.EnabledLocalHandpay);
            _properties.SetProperty(AccountingConstants.EnabledLocalVoucher, settings.EnabledLocalVoucher);
            _properties.SetProperty(AccountingConstants.EnabledLocalWat, settings.EnabledLocalWat);
            _properties.SetProperty(AccountingConstants.EnabledRemoteCredit, settings.EnabledRemoteCredit);
            _properties.SetProperty(AccountingConstants.EnabledRemoteHandpay, settings.EnabledRemoteHandpay);
            _properties.SetProperty(AccountingConstants.EnabledRemoteVoucher, settings.EnabledRemoteVoucher);
            _properties.SetProperty(AccountingConstants.EnabledRemoteWat, settings.EnabledRemoteWat);
            _properties.SetProperty(AccountingConstants.EnableReceipts, settings.EnableReceipts);
            _properties.SetProperty(AccountingConstants.PartialHandpays, settings.PartialHandpays);
            _properties.SetProperty(AccountingConstants.LocalKeyOff, settings.LocalKeyOff);
            _properties.SetProperty(AccountingConstants.IdReaderId, settings.IdReaderId);
            _properties.SetProperty(AccountingConstants.IgnoreVoucherStackedDuringReboot, settings.IgnoreVoucherStackedDuringReboot);
            _properties.SetProperty(AccountingConstants.LargeWinLimit, settings.LargeWinLimit);
            _properties.SetProperty(AccountingConstants.OverwriteLargeWinLimit, settings.OverwriteLargeWinLimit);
            _properties.SetProperty(AccountingConstants.LargeWinLimitEnabled, settings.LargeWinLimitIsChecked);
            _properties.SetProperty(AccountingConstants.LargeWinRatio, settings.LargeWinRatio);
            _properties.SetProperty(AccountingConstants.OverwriteLargeWinRatio, settings.OverwriteLargeWinRatio);
            _properties.SetProperty(AccountingConstants.LargeWinRatioEnabled, settings.LargeWinRatioIsChecked);
            _properties.SetProperty(AccountingConstants.LargeWinRatioThreshold, settings.LargeWinRatioThreshold);
            _properties.SetProperty(AccountingConstants.OverwriteLargeWinRatioThreshold, settings.OverwriteLargeWinRatioThreshold);
            _properties.SetProperty(AccountingConstants.LargeWinRatioThresholdEnabled, settings.LargeWinRatioThresholdIsChecked);
            _properties.SetProperty(AccountingConstants.MaxBetLimit, settings.MaxBetLimit);
            _properties.SetProperty(AccountingConstants.OverwriteMaxBetLimit, settings.OverwriteMaxBetLimit);
            _properties.SetProperty(AccountingConstants.MaxCreditMeter, settings.MaxCreditMeter);
            _properties.SetProperty(AccountingConstants.MaxCreditMeterMaxAllowed, settings.MaxCreditMeterMaxAllowed);
            _properties.SetProperty(AccountingConstants.MaxTenderInLimit, settings.MaxTenderInLimit);
            _properties.SetProperty(AccountingConstants.MixCreditTypes, settings.MixCreditTypes);
            _properties.SetProperty(AccountingConstants.MoneyInEnabled, settings.MoneyInEnabled);
            _properties.SetProperty(AccountingConstants.RedeemText, settings.RedeemText);
            _properties.SetProperty(AccountingConstants.ReprintLoggedVoucherBehavior, settings.ReprintLoggedVoucherBehavior);
            _properties.SetProperty(AccountingConstants.ReprintLoggedVoucherDoorOpenRequirement, settings.ReprintLoggedVoucherDoorOpenRequirement);
            _properties.SetProperty(AccountingConstants.ReprintLoggedVoucherTitleOverride, settings.ReprintLoggedVoucherTitleOverride);
            _properties.SetProperty(AccountingConstants.RequestNonCash, settings.RequestNonCash);
            _properties.SetProperty(AccountingConstants.TicketBarcodeLength, settings.TicketBarcodeLength);
            _properties.SetProperty(AccountingConstants.TicketTitleBonusCash, settings.TicketTitleBonusCash);
            _properties.SetProperty(AccountingConstants.TicketTitleBonusNonCash, settings.TicketTitleBonusNonCash);
            _properties.SetProperty(AccountingConstants.TicketTitleBonusPromo, settings.TicketTitleBonusPromo);
            _properties.SetProperty(AccountingConstants.TicketTitleCash, settings.TicketTitleCash);
            _properties.SetProperty(AccountingConstants.TicketTitleLargeWin, settings.TicketTitleLargeWin);
            _properties.SetProperty(AccountingConstants.TicketTitleNonCash, settings.TicketTitleNonCash);
            _properties.SetProperty(AccountingConstants.TicketTitlePromo, settings.TicketTitlePromo);
            _properties.SetProperty(AccountingConstants.TicketTitleWatNonCash, settings.TicketTitleWatNonCash);
            _properties.SetProperty(AccountingConstants.TicketTitleWatPromo, settings.TicketTitleWatPromo);
            _properties.SetProperty(AccountingConstants.TitleCancelReceipt, settings.TitleCancelReceipt);
            _properties.SetProperty(AccountingConstants.TitleJackpotReceipt, settings.TitleJackpotReceipt);
            _properties.SetProperty(AccountingConstants.UsePlayerIdReader, settings.UsePlayerIdReader);
            _properties.SetProperty(AccountingConstants.ValidateHandpays, settings.ValidateHandpays);
            _properties.SetProperty(AccountingConstants.VoucherInLimit, settings.VoucherInLimit);
            _properties.SetProperty(AccountingConstants.VoucherOut, settings.VoucherOut);
            _properties.SetProperty(AccountingConstants.VoucherOutExpirationDays, settings.VoucherOutExpirationDays);
            _properties.SetProperty(AccountingConstants.VoucherOutLimit, settings.VoucherOutLimit);
            _properties.SetProperty(AccountingConstants.VoucherOutNonCash, settings.VoucherOutNonCash);
            _properties.SetProperty(AccountingConstants.VoucherOutNonCashExpirationDays, settings.VoucherOutNonCashExpirationDays);
            _properties.SetProperty(AccountingConstants.VoucherOutLimitEnabled, settings.VoucherOutCheckBoxChecked);
            _properties.SetProperty(AccountingConstants.VoucherInLimitEnabled, settings.VoucherInCheckBoxChecked);
            _properties.SetProperty(AccountingConstants.HandpayLimit, settings.HandpayLimit);
            _properties.SetProperty(AccountingConstants.RemoteHandpayResetAllowed, settings.AllowRemoteHandpayReset);
            _properties.SetProperty(AccountingConstants.LargeWinHandpayResetMethod, settings.LargeWinHandpayResetMethod);
            _properties.SetProperty(AccountingConstants.HandpayLimitEnabled, settings.HandpayLimitIsChecked);
            _properties.SetProperty(AccountingConstants.CreditLimitEnabled, settings.CreditLimitIsChecked);
            _properties.SetProperty(AccountingConstants.MaxBetLimitEnabled, settings.MaxBetLimitIsChecked);

            await Task.CompletedTask;
        }
    }
}
