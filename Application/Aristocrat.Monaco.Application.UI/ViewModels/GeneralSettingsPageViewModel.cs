namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using Contracts;
    using Contracts.OperatorMenu;
    using Contracts.Tickets;
    using Hardware.Contracts;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.Contracts;
    using Monaco.Common;
    using OperatorMenu;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Input;
    using CommunityToolkit.Mvvm.Input;
    using Contracts.Localization;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     Contains logic for GeneralSettingsPageViewModel.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class GeneralSettingsPageViewModel : OperatorMenuPageViewModelBase
    {
        private const string HardBootTimeKey = "System.HardBoot.Time";
        private const string SoftBootTimeKey = "System.SoftBoot.Time";

        private string _hardBootTime;
        private string _ipAddress;
        private string _macAddress;
        private string _softBootTime;

        private string _timeZone = string.Empty;
        private string _timeZoneOffset = string.Empty;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GeneralSettingsPageViewModel" /> class.
        /// </summary>
        public GeneralSettingsPageViewModel() : base(true)
        {
            VisibilityChangedCommand = new RelayCommand<object>(OnVisibilityChanged);

            Version = PropertiesManager.GetValue(KernelConstants.SystemVersion, string.Empty);

            Jurisdiction = PropertiesManager.GetValue(ApplicationConstants.JurisdictionKey, string.Empty);

            var now = ServiceManager.GetInstance().GetService<ITime>().GetFormattedLocationTime();

            _hardBootTime = now;
            _softBootTime = now;

            GetRetailInfo();
        }

        /// <summary>
        ///     Gets or sets the time Zone string.
        /// </summary>
        public string TimeZone
        {
            get => _timeZone;

            set
            {
                if (_timeZone != value)
                {
                    _timeZone = value;
                    OnPropertyChanged(nameof(TimeZone));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the time Zone string.
        /// </summary>
        public string TimeZoneOffset
        {
            get => _timeZoneOffset;

            set
            {
                if (_timeZoneOffset != value)
                {
                    _timeZoneOffset = value;
                    OnPropertyChanged(nameof(TimeZoneOffset));
                }
            }
        }

        /// <summary>
        ///     Gets the local IP addresses.
        /// </summary>
        public string IPAddress
        {
            get => _ipAddress;

            private set
            {
                if (_ipAddress != value)
                {
                    _ipAddress = value;
                    OnPropertyChanged(nameof(IPAddress));
                }
            }
        }

        /// <summary>
        ///     Gets the MAC address
        /// </summary>
        public string MacAddress
        {
            get => _macAddress;

            private set
            {
                if (_macAddress != value)
                {
                    _macAddress = value;
                    OnPropertyChanged(nameof(MacAddress));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the hard boot time value.
        /// </summary>
        public string HardBootTime
        {
            get => _hardBootTime;

            set
            {
                if (_hardBootTime != value)
                {
                    _hardBootTime = value;
                    OnPropertyChanged(nameof(HardBootTime));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the Soft boot time value.
        /// </summary>
        public string SoftBootTime
        {
            get => _softBootTime;

            set
            {
                if (_softBootTime != value)
                {
                    _softBootTime = value;
                    OnPropertyChanged(nameof(SoftBootTime));
                }
            }
        }

        public string LicenseNumber { get; set; }
        public string PropertyName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string TerminalNumber { get; set; }
        public string SerialNumber { get; set; }

        /// <summary>
        ///     Gets the jurisdiction
        /// </summary>
        public string Jurisdiction { get; }

        /// <summary>
        ///     Gets the Windows Version
        /// </summary>
        public string WindowsVersion => Environment.OSVersion.Version.ToString();

        /// <summary>
        ///     Gets the Windows OS Image Version
        /// </summary>
        public string OsImageVersion => ServiceManager.GetInstance().GetService<IOSService>().OsImageVersion.ToString();

        /// <summary>
        ///     Gets the platform version value string.
        /// </summary>
        public string Version { get; }

        /// <summary>
        ///     Gets the command that fires when page unloaded.
        /// </summary>
        public ICommand VisibilityChangedCommand { get; }

        public Visibility RetailerInfoVisibility { get; set; }

        protected override void OnLoaded()
        {
            UpdateTimeZone();
            UpdateTimeInformation();
            GetNetworkInfo();
        }

        private void OnVisibilityChanged(object obj)
        {
            UpdateTimeZone();
            UpdateTimeInformation();
        }

        private void GetRetailInfo()
        {
            var visibility = GetConfigSetting(OperatorMenuSetting.RetailerInfoVisible, false);
            if (visibility)
            {
                RetailerInfoVisibility = Visibility.Visible;
                PropertyName =
                    (string)PropertiesManager.GetProperty(PropertyKey.TicketTextLine1, string.Empty);
                AddressLine1 =
                    (string)PropertiesManager.GetProperty(PropertyKey.TicketTextLine2, string.Empty);
                AddressLine2 =
                    (string)PropertiesManager.GetProperty(PropertyKey.TicketTextLine3, string.Empty);
                LicenseNumber = (string)PropertiesManager.GetProperty(ApplicationConstants.Zone, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailableText));
                TerminalNumber = PropertiesManager.GetProperty(ApplicationConstants.MachineId, (uint)0).ToString();
                SerialNumber = (string)PropertiesManager.GetProperty(ApplicationConstants.SerialNumber, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailableText));
            }
            else
            {
                RetailerInfoVisibility = Visibility.Collapsed;
            }
        }

        private void GetNetworkInfo()
        {
            IPAddress = NetworkInterfaceInfo.DefaultIpAddress?.ToString();

            MacAddress = NetworkInterfaceInfo.DefaultPhysicalAddress;
        }

        private void UpdateTimeInformation()
        {
            var time = ServiceManager.GetInstance().GetService<ITime>();
            var zone = PropertiesManager.GetValue<TimeZoneInfo>(ApplicationConstants.TimeZoneKey, null);
            TimeZone = zone?.DisplayName ?? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailableText);
            Logger.Debug($"TimeZone: {TimeZone}");

            var softBootTime = PropertiesManager.GetValue(SoftBootTimeKey, DateTime.UtcNow);
            Logger.Debug($"property manager softBootTime: {softBootTime}");
            var softBootTimeLocalTime = time.GetLocationTime(softBootTime, zone).ToString(CultureInfo.CurrentCulture);
            Logger.Debug($"softBootTimeLocalTime: {softBootTimeLocalTime}");
            SoftBootTime = softBootTimeLocalTime;

            var hardBootTime = PropertiesManager.GetValue(HardBootTimeKey, DateTime.UtcNow);
            Logger.Debug($"property manager hardBootTime: {hardBootTime}");
            var hardBootTimeLocalTime = time.GetLocationTime(hardBootTime, zone).ToString(CultureInfo.CurrentCulture);
            Logger.Debug($"hardBootTimeLocalTime: {hardBootTimeLocalTime}");
            HardBootTime = hardBootTimeLocalTime;
        }

        private void UpdateTimeZone()
        {
            TimeZone = PropertiesManager.GetValue(ApplicationConstants.TimeZoneKey, TimeZoneInfo.Local).DisplayName;

            TimeZoneOffset = PropertiesManager.GetValue(ApplicationConstants.TimeZoneOffsetKey, TimeSpan.Zero)
                .GetFormattedOffset();
        }

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            if (dataType != OperatorMenuPrintData.Main)
            {
                return null;
            }

            var ticketCreator = ServiceManager.GetInstance().TryGetService<IMachineInfoTicketCreator>();
            return ticketCreator?.Create();
        }
    }
}
