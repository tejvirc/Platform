namespace Aristocrat.Monaco.Application.UI.Settings
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using Monaco.Localization.Properties;
    using Contracts;
    using Contracts.Operations;
    using Contracts.Protocol;
    using Contracts.Settings;
    using Hardware.Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Kernel.Contracts;
    using MVVM;
    using Application.Contracts.Localization;
    using Aristocrat.Monaco.Application.Helpers;
    using Aristocrat.Monaco.Application.UI.ViewModels;
    using Aristocrat.Monaco.Application.Localization;

    /// <summary>
    ///     Implements the <see cref="IConfigurationSettings"/> interface.
    /// </summary>
    public class ApplicationConfigurationSettings : IConfigurationSettings
    {
        private readonly IPropertiesManager _propertiesManager;
        private readonly IDisabledNotesService _disabledNotesService;
        private readonly IMultiProtocolConfigurationProvider _multiProtocolConfigurationProvider;
 
        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationConfigurationSettings"/> class.
        /// </summary>
        public ApplicationConfigurationSettings()
            : this(ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IDisabledNotesService>(),
                ServiceManager.GetInstance().GetService<IMultiProtocolConfigurationProvider>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationConfigurationSettings"/> class.
        /// </summary>
        /// <param name="propertiesManager">A <see cref="IPropertiesManager"/> instance.</param>
        /// <param name="disabledNotesService">A <see cref="IDisabledNotesService"/> instance.</param>
        /// <param name="multiProtocolConfigurationProvider">A <see cref="IMultiProtocolConfigurationProvider"/> instance.</param>
        public ApplicationConfigurationSettings(
            IPropertiesManager propertiesManager,
            IDisabledNotesService disabledNotesService,
            IMultiProtocolConfigurationProvider multiProtocolConfigurationProvider)
        {
            _propertiesManager = propertiesManager;
            _disabledNotesService = disabledNotesService;
            _multiProtocolConfigurationProvider = multiProtocolConfigurationProvider;
        }

        /// <inheritdoc />
        public string Name => "Application";

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
                        Source = new Uri("/Aristocrat.Monaco.Application.UI;component/Settings/MachineSettings.xaml", UriKind.RelativeOrAbsolute)
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
            DisabledNotes[] disabledNotes;
            var noteInfo = _disabledNotesService.NoteInfo;
            if (noteInfo != null)
            {
                disabledNotes = new DisabledNotes[noteInfo.Notes.Length];
                for (var i = 0; i < noteInfo.Notes.Length; i++)
                {
                    disabledNotes[i] = new DisabledNotes
                    {
                        Denom = noteInfo.Notes[i].Denom, IsoCode = noteInfo.Notes[i].IsoCode
                    };
                }
            }
            else
            {
                disabledNotes = new DisabledNotes[0];
            }


            var noteAcceptorEnabled = _propertiesManager.GetValue(ApplicationConstants.NoteAcceptorEnabled, false);
            var printerEnabled = _propertiesManager.GetValue(ApplicationConstants.PrinterEnabled, false);
            var idReaderEnabled = _propertiesManager.GetValue(ApplicationConstants.IdReaderEnabled, false);
            var reelControllerEnabled = _propertiesManager.GetValue(ApplicationConstants.ReelControllerEnabled, false);
            var notAvailable = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable);
            return await Task.FromResult(
                new MachineSettings
                {
                    NoteAcceptorEnabled =noteAcceptorEnabled,
                    NoteAcceptorManufacturer =
                        noteAcceptorEnabled ? _propertiesManager.GetValue(ApplicationConstants.NoteAcceptorManufacturer, string.Empty) : notAvailable,
                    PrinterEnabled = printerEnabled,
                    PrinterManufacturer =
                        printerEnabled?_propertiesManager.GetValue(ApplicationConstants.PrinterManufacturer, string.Empty): notAvailable,
                    CurrencyId =
                        _propertiesManager.GetValue(ApplicationConstants.CurrencyId, ApplicationConstants.DefaultCurrencyId),
                    CurrencyDescription =
                        _propertiesManager.GetValue(ApplicationConstants.CurrencyDescription, string.Empty), // Used for display only
                    OperatingHours = new ObservableCollection<OperatingHoursSetting>(
                        _propertiesManager.GetValues<OperatingHours>(ApplicationConstants.OperatingHours)
                            .Select(x => (OperatingHoursSetting)x)),
                    Jurisdiction =
                        _propertiesManager.GetValue(ApplicationConstants.JurisdictionKey, string.Empty),
                    ShowMode =
                        _propertiesManager.GetValue(ApplicationConstants.ShowMode, false),
                    GameRules =
                        _propertiesManager.GetValue(ApplicationConstants.GameRules, true),
                    DemonstrationMode =
                        _propertiesManager.GetValue(ApplicationConstants.DemonstrationMode, false),
                    DeletePackageAfterInstall =
                        _propertiesManager.GetValue(ApplicationConstants.DeletePackageAfterInstall, false),
                    ScreenBrightness =
                        _propertiesManager.GetValue(ApplicationConstants.ScreenBrightness, 0),
                    MediaDisplayEnabled =
                        _propertiesManager.GetValue(ApplicationConstants.MediaDisplayEnabled, false),
                    HardMetersEnabled =
                        _propertiesManager.GetValue(HardwareConstants.HardMetersEnabledKey, false),
                    HardMeterMapSelectionValue =
                        _propertiesManager.GetValue(ApplicationConstants.HardMeterMapSelectionValue, ApplicationConstants.HardMeterDefaultMeterMappingName),
                    HardMeterTickValue =
                        _propertiesManager.GetValue(ApplicationConstants.HardMeterTickValue, 100L),
                    HardMeterVisible =
                        _propertiesManager.GetValue(ApplicationConstants.ConfigWizardHardMetersConfigVisible, true),
                    //RebootWhilePrintingBehavior =
                    //    _propertiesManager.GetValue(PropertyKey.RebootWhilePrintingBehavior, string.Empty),
                    TicketTextLine1 =
                        _propertiesManager.GetValue(PropertyKey.TicketTextLine1, string.Empty),
                    TicketTextLine2 =
                        _propertiesManager.GetValue(PropertyKey.TicketTextLine2, string.Empty),
                    TicketTextLine3 =
                        _propertiesManager.GetValue(PropertyKey.TicketTextLine3, string.Empty),
                    MaxCreditsIn =
                        _propertiesManager.GetValue(PropertyKey.MaxCreditsIn, ApplicationConstants.DefaultMaxCreditsIn),
                    DefaultVolumeLevel =
                        _propertiesManager.GetValue(PropertyKey.DefaultVolumeLevel, (byte)2),
                    VolumeControlLocation =
                        _propertiesManager.GetValue(ApplicationConstants.VolumeControlLocationKey, (VolumeControlLocation)ApplicationConstants.VolumeControlLocationDefault),
                    VoucherIn =
                        _propertiesManager.GetValue(PropertyKey.VoucherIn, false),
                    IdReaderEnabled = idReaderEnabled,
                    IdReaderManufacturer =
                        idReaderEnabled?_propertiesManager.GetValue(ApplicationConstants.IdReaderManufacturer, string.Empty): notAvailable,
                    ReelControllerEnabled =
                        reelControllerEnabled,
                    ReelControllerManufacturer =
                        reelControllerEnabled?_propertiesManager.GetValue(ApplicationConstants.ReelControllerManufacturer, string.Empty): notAvailable,
                    DoorOpticSensorEnabled =
                        _propertiesManager.GetValue(ApplicationConstants.ConfigWizardDoorOpticsEnabled, false),
                    RequireZeroCreditsForOutOfService =
                        !_propertiesManager.GetValue(ApplicationConstants.MachineSetupConfigEnterOutOfServiceWithCreditsEnabled, true),
                    ExcessiveDocumentRejectCount =
                        _propertiesManager.GetValue(ApplicationConstants.ExcessiveDocumentRejectCount, -1),
                    BarcodeType =
                        _propertiesManager.GetValue(ApplicationConstants.BarCodeType, BarcodeTypeOptions.Interleave2of5),
                    ValidationLength =
                        _propertiesManager.GetValue(ApplicationConstants.ValidationLength, ValidationLengthOptions.System),
                    LayoutType =
                        _propertiesManager.GetValue(ApplicationConstants.LayoutType, LayoutTypeOptions.ExtendedLayout),
                    DisabledNotes =
                        new ObservableCollection<DisabledNotesSetting>(disabledNotes.OrderBy(x => x.Denom).Select(x => (DisabledNotesSetting)x)),
                    DisabledNotesVisible = disabledNotes.Any(x => x.Denom > 0),
                    ReserveServiceEnabled = _propertiesManager.GetValue(ApplicationConstants.ReserveServiceEnabled, true),
                    ReserveServiceTimeoutInSeconds =
                        _propertiesManager.GetValue(ApplicationConstants.ReserveServiceTimeoutInSeconds, (int)TimeSpan.FromMinutes(5).TotalSeconds),
                    MultiProtocolConfiguration =
                        new ObservableCollection<ProtocolConfiguration>(
                            _multiProtocolConfigurationProvider.MultiProtocolConfiguration),
                    Protocols = string.Join(
                        ", ",
                        (_multiProtocolConfigurationProvider
                            .MultiProtocolConfiguration).Select(x => x.Protocol))
                });
        }

        private async Task ApplySettings(MachineSettings settings)
        {
            _propertiesManager.SetProperty(ApplicationConstants.NoteAcceptorEnabled, settings.NoteAcceptorEnabled);
            _propertiesManager.SetProperty(ApplicationConstants.NoteAcceptorManufacturer, settings.NoteAcceptorManufacturer);
            _propertiesManager.SetProperty(ApplicationConstants.PrinterEnabled, settings.PrinterEnabled);
            _propertiesManager.SetProperty(ApplicationConstants.PrinterManufacturer, settings.PrinterManufacturer);
            _propertiesManager.SetProperty(ApplicationConstants.CurrencyId, settings.CurrencyId);
            _propertiesManager.SetProperty(ApplicationConstants.CurrencyDescription, settings.CurrencyDescription);
            _propertiesManager.SetProperty(
                ApplicationConstants.OperatingHours,
                settings.OperatingHours.Select(x => (OperatingHours)x).ToArray());
            _propertiesManager.SetProperty(ApplicationConstants.JurisdictionKey, settings.Jurisdiction);
            _propertiesManager.SetProperty(ApplicationConstants.ShowMode, settings.ShowMode);
            _propertiesManager.SetProperty(ApplicationConstants.GameRules, settings.GameRules);
            _propertiesManager.SetProperty(ApplicationConstants.DemonstrationMode, settings.DemonstrationMode);
            _propertiesManager.SetProperty(HardwareConstants.HardMetersEnabledKey, settings.HardMetersEnabled);
            _propertiesManager.SetProperty(ApplicationConstants.HardMeterMapSelectionValue, settings.HardMeterMapSelectionValue);
            _propertiesManager.SetProperty(ApplicationConstants.HardMeterTickValue, settings.HardMeterTickValue);
            _propertiesManager.SetProperty(ApplicationConstants.ConfigWizardHardMetersConfigVisible, settings.HardMeterVisible);
            _propertiesManager.SetProperty(ApplicationConstants.DeletePackageAfterInstall, settings.DeletePackageAfterInstall);
            _propertiesManager.SetProperty(ApplicationConstants.ScreenBrightness, settings.ScreenBrightness);
            _propertiesManager.SetProperty(ApplicationConstants.MediaDisplayEnabled, settings.MediaDisplayEnabled);
            // _propertiesManager.SetProperty(PropertyKey.RebootWhilePrintingBehavior, settings.RebootWhilePrintingBehavior);
            _propertiesManager.SetProperty(PropertyKey.TicketTextLine1, settings.TicketTextLine1);
            _propertiesManager.SetProperty(PropertyKey.TicketTextLine2, settings.TicketTextLine2);
            _propertiesManager.SetProperty(PropertyKey.TicketTextLine3, settings.TicketTextLine3);
            _propertiesManager.SetProperty(PropertyKey.MaxCreditsIn, settings.MaxCreditsIn);
            _propertiesManager.SetProperty(PropertyKey.DefaultVolumeLevel, settings.DefaultVolumeLevel);
            _propertiesManager.SetProperty(ApplicationConstants.VolumeControlLocationKey, settings.VolumeControlLocation);
            _propertiesManager.SetProperty(PropertyKey.VoucherIn, settings.VoucherIn);
            _propertiesManager.SetProperty(ApplicationConstants.IdReaderEnabled, settings.IdReaderEnabled);
            _propertiesManager.SetProperty(ApplicationConstants.IdReaderManufacturer, settings.IdReaderManufacturer);
            _propertiesManager.SetProperty(ApplicationConstants.ReelControllerEnabled, settings.ReelControllerEnabled);
            _propertiesManager.SetProperty(ApplicationConstants.ReelControllerManufacturer, settings.ReelControllerManufacturer);
            _propertiesManager.SetProperty(ApplicationConstants.ConfigWizardDoorOpticsEnabled, settings.DoorOpticSensorEnabled);
            _propertiesManager.SetProperty(ApplicationConstants.MachineSetupConfigEnterOutOfServiceWithCreditsEnabled, !settings.RequireZeroCreditsForOutOfService);
            _propertiesManager.SetProperty(ApplicationConstants.ExcessiveDocumentRejectCount, settings.ExcessiveDocumentRejectCount);
            _propertiesManager.SetProperty(ApplicationConstants.BarCodeType, settings.BarcodeType);
            _propertiesManager.SetProperty(ApplicationConstants.ValidationLength, settings.ValidationLength);
            _propertiesManager.SetProperty(ApplicationConstants.LayoutType, settings.LayoutType);
            _propertiesManager.SetProperty(ApplicationConstants.ReserveServiceEnabled, settings.ReserveServiceEnabled);
            _propertiesManager.SetProperty(ApplicationConstants.ReserveServiceTimeoutInSeconds, settings.ReserveServiceTimeoutInSeconds);
            _multiProtocolConfigurationProvider.MultiProtocolConfiguration = settings.MultiProtocolConfiguration;

            if (settings.DisabledNotes.Count > 0)
            {
                var noteInfo = new NoteInfo { Notes = new (int, string)[settings.DisabledNotes.Count] };
                for (var i = 0; i < settings.DisabledNotes.Count; i++)
                {
                    noteInfo.Notes[i].Denom = settings.DisabledNotes[i].Denom;
                    noteInfo.Notes[i].IsoCode = settings.DisabledNotes[i].IsoCode;
                }

                _disabledNotesService.NoteInfo = noteInfo;
            }

            await Task.CompletedTask;
        }
    }
}
