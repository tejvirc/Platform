namespace Aristocrat.Monaco.Sas.UI.Settings
{
    using System.Collections.ObjectModel;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using CommunityToolkit.Mvvm.ComponentModel;

    /// <summary>
    ///     SAS machine settings.
    /// </summary>
    public class MachineSettings : ObservableObject
    {
        private PortAssignmentSetting _portAssignmentSetting;
        private SasFeaturesSettings _sasFeaturesSettings;
        private CashableLockupStrategy _hostDisableCashoutAction;
        private bool _extendedMetersSupported;
        private bool _ticketsToDropMeters;
        private SasMeterModel _meterModel;
        private bool _jackpotKeyoffExceptionSupported;
        private bool _multiDenomExtensionsSupported;
        private bool _maxPollingRateSupported;
        private bool _multipleSasProgressiveWinReportingSupported;
        private bool _meterChangeNotificationSupported;
        private bool _sessionPlaySupported;
        private bool _foreignCurrencyRedemptionSupported;
        private bool _enhancedProgressiveDataReporting;
        private bool _maxProgressivePaybackSupported;
        private bool _changeAssetNumberSupported;
        private bool _changeFloorLocationSupported;

        /// <summary>
        ///     Gets or sets the SAS Host settings.
        /// </summary>
        public ObservableCollection<SasHostSetting> SasHostSettings { get; set; }

        /// <summary>
        ///     Gets or sets the port assignment settings
        /// </summary>
        public PortAssignmentSetting PortAssignmentSetting
        {
            get => _portAssignmentSetting;
            set => SetProperty(ref _portAssignmentSetting, value);
        }

        /// <summary>
        ///     Gets or sets the sas features settings
        /// </summary>
        public SasFeaturesSettings SasFeaturesSettings
        {
            get => _sasFeaturesSettings;
            set => SetProperty(ref _sasFeaturesSettings, value);
        }

        /// <summary>
        ///     Gets or sets the host disable cashout action.
        /// </summary>
        public CashableLockupStrategy HostDisableCashoutAction
        {
            get => _hostDisableCashoutAction;

            set => SetProperty(ref _hostDisableCashoutAction, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether extended meters is supported.
        /// </summary>
        public bool ExtendedMetersSupported
        {
            get => _extendedMetersSupported;

            set => SetProperty(ref _extendedMetersSupported, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether ticket to drop meters functionality is supported.
        /// </summary>
        public bool TicketsToDropMeters
        {
            get => _ticketsToDropMeters;

            set => SetProperty(ref _ticketsToDropMeters, value);
        }

        /// <summary>
        ///     Gets or sets the meter mode.
        /// </summary>
        public SasMeterModel MeterModel
        {
            get => _meterModel;

            set => SetProperty(ref _meterModel, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether jackpot key-off exception is supported.
        /// </summary>
        public bool JackpotKeyoffExceptionSupported
        {
            get => _jackpotKeyoffExceptionSupported;

            set => SetProperty(ref _jackpotKeyoffExceptionSupported, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether multi-denominations extensions supported.
        /// </summary>
        public bool MultiDenomExtensionsSupported
        {
            get => _multiDenomExtensionsSupported;

            set => SetProperty(ref _multiDenomExtensionsSupported, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether max polling rate supported.
        /// </summary>
        public bool MaxPollingRateSupported
        {
            get => _maxPollingRateSupported;

            set => SetProperty(ref _maxPollingRateSupported, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether multiple SAS progressive win reporting is supported.
        /// </summary>
        public bool MultipleSasProgressiveWinReportingSupported
        {
            get => _multipleSasProgressiveWinReportingSupported;

            set => SetProperty(ref _multipleSasProgressiveWinReportingSupported, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether meter change notification supported.
        /// </summary>
        public bool MeterChangeNotificationSupported
        {
            get => _meterChangeNotificationSupported;

            set => SetProperty(ref _meterChangeNotificationSupported, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether session player is supported.
        /// </summary>
        public bool SessionPlaySupported
        {
            get => _sessionPlaySupported;

            set => SetProperty(ref _sessionPlaySupported, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether foreign currency redemption is supported.
        /// </summary>
        public bool ForeignCurrencyRedemptionSupported
        {
            get => _foreignCurrencyRedemptionSupported;

            set => SetProperty(ref _foreignCurrencyRedemptionSupported, value);
        }

        /// <summary>
        ///     Gets or sets
        /// </summary>
        public bool EnhancedProgressiveDataReporting
        {
            get => _enhancedProgressiveDataReporting;

            set => SetProperty(ref _enhancedProgressiveDataReporting, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether max progressive payback supported.
        /// </summary>
        public bool MaxProgressivePaybackSupported
        {
            get => _maxProgressivePaybackSupported;

            set => SetProperty(ref _maxProgressivePaybackSupported, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether changed asset number is supported.
        /// </summary>
        public bool ChangeAssetNumberSupported
        {
            get => _changeAssetNumberSupported;

            set => SetProperty(ref _changeAssetNumberSupported, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether change floor location is supported.
        /// </summary>
        public bool ChangeFloorLocationSupported
        {
            get => _changeFloorLocationSupported;

            set => SetProperty(ref _changeFloorLocationSupported, value);
        }
    }
}
