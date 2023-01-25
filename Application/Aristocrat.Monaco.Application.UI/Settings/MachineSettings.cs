namespace Aristocrat.Monaco.Application.UI.Settings
{
    using System;
    using System.Collections.ObjectModel;
    using Contracts.Extensions;
    using Contracts.Localization;
    using Contracts.Protocol;
    using Hardware.Contracts.Audio;
    using Monaco.Common;
    using Monaco.Localization.Properties;
    using MVVM.Model;
    using Newtonsoft.Json;
    using Contracts;

    /// <summary>
    ///     Application machine settings.
    /// </summary>
    internal class MachineSettings : BaseNotify
    {
        private bool _noteAcceptorEnabled;
        private string _noteAcceptorManufacturer;
        private bool _printerEnabled;
        private string _printerManufacturer;
        private string _currencyId;
        private string _currencyDescription;
        private string _jurisdiction;
        private bool _showMode;
        private bool _gameRules;
        private bool _demonstrationMode;
        private bool _hardMetersEnabled;
        private string _hardMeterMapSelectionValue;
        private long _hardMeterTickValue;
        private bool _deletePackageAfterInstall;
        private int _screenBrightness;
        private int _reserveServiceTimeoutInSeconds;
        private bool _mediaDisplayEnabled;
        private string _rebootWhilePrintingBehavior;
        private string _ticketTextLine1;
        private string _ticketTextLine2;
        private string _ticketTextLine3;
        private long _maxCreditsIn;
        private byte _defaultVolumeLevel;
        private bool _voucherIn;
        private bool _idReaderEnabled;
        private string _idReaderManufacturer;
        private bool _doorOpticSensorEnabled;
        private bool _requireZeroCreditsForOutOfService;
        private bool _reserveServiceEnabled;
        private int _excessiveDocumentRejectCount;
        private bool _reelControllerEnabled;
        private string _reelControllerManufacturer;
        private BarcodeTypeOptions _barcodeType;
        private ValidationLengthOptions _validationLength;
        private LayoutTypeOptions _layoutType;
        private VolumeControlLocation _volumeControlLocation;
        private bool _bellEnabled;

        /// <summary>
        ///     Gets or sets a value that indicates whether the note acceptor is enabled.
        /// </summary>
        public bool NoteAcceptorEnabled
        {
            get => _noteAcceptorEnabled;

            set => SetProperty(ref _noteAcceptorEnabled, value);
        }

        /// <summary>
        ///     Gets or sets the note acceptor manufacturer.
        /// </summary>
        public string NoteAcceptorManufacturer
        {
            get => _noteAcceptorManufacturer;

            set => SetProperty(ref _noteAcceptorManufacturer, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether the printer is enabled.
        /// </summary>
        public bool PrinterEnabled
        {
            get => _printerEnabled;

            set => SetProperty(ref _printerEnabled, value);
        }

        /// <summary>
        ///     Gets or sets the printer manufacturer.
        /// </summary>
        public string PrinterManufacturer
        {
            get => _printerManufacturer;

            set => SetProperty(ref _printerManufacturer, value);
        }

        /// <summary>
        ///     Gets or sets the currency ID.
        /// </summary>
        public string CurrencyId
        {
            get => _currencyId;

            set => SetProperty(ref _currencyId, value);
        }

        /// <summary>
        ///     Gets or sets the currency description.
        /// </summary>
        public string CurrencyDescription
        {
            get => _currencyDescription;

            set => SetProperty(ref _currencyDescription, value);
        }

        /// <summary>
        ///     Gets or sets a list of operating hours.
        /// </summary>
        public ObservableCollection<OperatingHoursSetting> OperatingHours { get; set; }

        /// <summary>
        ///     Gets or sets the jurisdiction key.
        /// </summary>
        public string Jurisdiction
        {
            get => _jurisdiction;

            set => SetProperty(ref _jurisdiction, value);
        }

        /// <summary>
        ///     Gets or sets a list of ProtocolConfiguration.
        /// </summary>
        public ObservableCollection<ProtocolConfiguration> MultiProtocolConfiguration { get; set; }

        /// <summary>
        ///     Gets or sets a value that indicates whether show mode is enabled.
        /// </summary>
        public bool ShowMode
        {
            get => _showMode;

            set => SetProperty(ref _showMode, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether game rules are enabled.
        /// </summary>
        public bool GameRules
        {
            get => _gameRules;

            set => SetProperty(ref _gameRules, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether demonstration mode is enabled.
        /// </summary>
        public bool DemonstrationMode
        {
            get => _demonstrationMode;

            set => SetProperty(ref _demonstrationMode, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether hard meters is enabled.
        /// </summary>
        public bool HardMetersEnabled
        {
            get => _hardMetersEnabled;

            set => SetProperty(ref _hardMetersEnabled, value);
        }

        /// <summary>
        ///     Gets or sets the hard meter map selection value.
        /// </summary>
        public string HardMeterMapSelectionValue
        {
            get => _hardMeterMapSelectionValue;

            set => SetProperty(ref _hardMeterMapSelectionValue, value);
        }

        /// <summary>
        ///     Gets or sets the hard meter tick value.
        /// </summary>
        public long HardMeterTickValue
        {
            get => _hardMeterTickValue;

            set
            {
                SetProperty(ref _hardMeterTickValue, value);
                RaisePropertyChanged(nameof(HardMeterTickValueDisplay));
            }
        }

        /// <summary>
        ///     Gets the hard meter tick value to display.
        /// </summary>
        [JsonIgnore]
        public string HardMeterTickValueDisplay => _hardMeterTickValue.CentsToDollars().FormattedCurrencyString();

        /// <summary>
        ///     Gets or sets a value that indicates whether hard meter values are visible.
        /// </summary>
        [JsonIgnore]
        public bool HardMeterVisible { get; set; }

        /// <summary>
        ///     Gets or sets a value that indicates whether to delete downloaded packages after install.
        /// </summary>
        public bool DeletePackageAfterInstall
        {
            get => _deletePackageAfterInstall;

            set => SetProperty(ref _deletePackageAfterInstall, value);
        }

        /// <summary>
        ///     Gets or sets the screen brightness value.
        /// </summary>
        public int ScreenBrightness
        {
            get => _screenBrightness;

            set => SetProperty(ref _screenBrightness, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether the media display is enabled.
        /// </summary>
        public bool MediaDisplayEnabled
        {
            get => _mediaDisplayEnabled;

            set => SetProperty(ref _mediaDisplayEnabled, value);
        }

        /// <summary>
        ///     Gets or sets reboot while printing behavior.
        /// </summary>
        public string RebootWhilePrintingBehavior
        {
            get => _rebootWhilePrintingBehavior;

            set => SetProperty(ref _rebootWhilePrintingBehavior, value);
        }

        /// <summary>
        ///     Gets or sets the ticket text line 1.
        /// </summary>
        public string TicketTextLine1
        {
            get => _ticketTextLine1;

            set => SetProperty(ref _ticketTextLine1, value);
        }

        /// <summary>
        ///     Gets or sets the ticket text line 2.
        /// </summary>
        public string TicketTextLine2
        {
            get => _ticketTextLine2;

            set => SetProperty(ref _ticketTextLine2, value);
        }

        /// <summary>
        ///     Gets or sets the ticket text line 3.
        /// </summary>
        public string TicketTextLine3
        {
            get => _ticketTextLine3;

            set => SetProperty(ref _ticketTextLine3, value);
        }

        /// <summary>
        ///     Gets or sets the max credits in.
        /// </summary>
        public long MaxCreditsIn
        {
            get => _maxCreditsIn;

            set
            {
                SetProperty(ref _maxCreditsIn, value);
                RaisePropertyChanged(nameof(MaxCreditsInDisplay));
            }
        }

        /// <summary>
        ///     Gets the max credits in to display.
        /// </summary>
        [JsonIgnore]
        public string MaxCreditsInDisplay => _maxCreditsIn == 0
                    ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoLimit)
                    : _maxCreditsIn.MillicentsToDollars().FormattedCurrencyString();

        /// <summary>
        ///     Gets or sets the platform default volume level.
        /// </summary>
        public byte DefaultVolumeLevel
        {
            get => _defaultVolumeLevel;

            set
            {
                SetProperty(ref _defaultVolumeLevel, value);
                RaisePropertyChanged(nameof(DefaultVolumeLevelDisplay));
            }
        }

        /// <summary>
        ///     Gets the platform default volume level to display.
        /// </summary>
        [JsonIgnore]
        public string DefaultVolumeLevelDisplay => ((VolumeLevel)_defaultVolumeLevel).GetDescription(((VolumeLevel)_defaultVolumeLevel).GetType());

        /// <summary>
        ///     Gets or sets the volume control location.
        /// </summary>
        public VolumeControlLocation VolumeControlLocation
        {
            get => _volumeControlLocation;

            set
            {
                SetProperty(ref _volumeControlLocation, value);
                RaisePropertyChanged(nameof(VolumeControlLocation));
            }
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether voucher is inserted.
        /// </summary>
        public bool VoucherIn
        {
            get => _voucherIn;

            set => SetProperty(ref _voucherIn, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether the id reader is enabled.
        /// </summary>
        public bool IdReaderEnabled
        {
            get => _idReaderEnabled;

            set => SetProperty(ref _idReaderEnabled, value);
        }

        /// <summary>
        ///     Gets or sets the id reader manufacturer.
        /// </summary>
        public string IdReaderManufacturer
        {
            get => _idReaderManufacturer;

            set => SetProperty(ref _idReaderManufacturer, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether the reel controller is enabled.
        /// </summary>
        public bool ReelControllerEnabled
        {
            get => _reelControllerEnabled;

            set => SetProperty(ref _reelControllerEnabled, value);
        }

        /// <summary>
        ///     Gets or sets the reel controller manufacturer.
        /// </summary>
        public string ReelControllerManufacturer
        {
            get => _reelControllerManufacturer;

            set => SetProperty(ref _reelControllerManufacturer, value);
        }

        /// <summary>
        ///     Gets or sets the bell enabled or not
        /// </summary>
        public bool BellEnabled
        {
            get => _bellEnabled;
            set => SetProperty(ref _bellEnabled, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether the door optic sensor is enabled.
        /// </summary>
        public bool DoorOpticSensorEnabled
        {
            get => _doorOpticSensorEnabled;

            set => SetProperty(ref _doorOpticSensorEnabled, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether to require zero credits for out-of-service.
        /// </summary>
        public bool RequireZeroCreditsForOutOfService
        {
            get => _requireZeroCreditsForOutOfService;

            set => SetProperty(ref _requireZeroCreditsForOutOfService, value);
        }

        /// <summary>
        ///     Gets or sets the excessive document reject count.
        /// </summary>
        public int ExcessiveDocumentRejectCount
        {
            get => _excessiveDocumentRejectCount;

            set => SetProperty(ref _excessiveDocumentRejectCount, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether excessive reject disable.
        /// </summary>
        public bool ExcessiveRejectDisable => _excessiveDocumentRejectCount > -1;

        /// <summary>
        ///     Gets or sets the barcode type.
        /// </summary>
        public BarcodeTypeOptions BarcodeType
        {
            get => _barcodeType;

            set => SetProperty(ref _barcodeType, value);
        }

        /// <summary>
        ///     Gets or sets the validation length.
        /// </summary>
        public ValidationLengthOptions ValidationLength
        {
            get => _validationLength;

            set => SetProperty(ref _validationLength, value);
        }

        /// <summary>
        ///     Gets or sets the layout type.
        /// </summary>
        public LayoutTypeOptions LayoutType
        {
            get => _layoutType;

            set => SetProperty(ref _layoutType, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether to Reserve Machine is allowed for the player.
        /// </summary>
        public bool ReserveServiceEnabled
        {
            get => _reserveServiceEnabled;

            set => SetProperty(ref _reserveServiceEnabled, value);
        }

        /// <summary>
        ///     Gets or sets the time for the player to reserve the machine.
        /// </summary>
        public int ReserveServiceTimeoutInSeconds
        {
            get => _reserveServiceTimeoutInSeconds;

            set => SetProperty(ref _reserveServiceTimeoutInSeconds, value);
        }

        [JsonIgnore]
        public int ReserveServiceTimeoutInMinutes => TimeSpan.FromSeconds(_reserveServiceTimeoutInSeconds).Minutes;

        /// <summary>
        ///     Gets or sets a list of disabled notes.
        /// </summary>
        public ObservableCollection<DisabledNotesSetting> DisabledNotes { get; set; }


        /// <summary>
        ///     Gets or sets a value that indicates whether disabled notes are visible.
        /// </summary>
        [JsonIgnore]
        public bool DisabledNotesVisible { get; set;  }

        /// <summary>
        ///     Gets a comma separated value of currently configured protocols.
        /// </summary>
        public string Protocols { get; set; }
    }
}
