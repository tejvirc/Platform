namespace Aristocrat.Monaco.Application.Contracts
{
    using System;

    /// <summary>
    ///     Application Constants
    /// </summary>
    public static class ApplicationConstants
    {
        /// <summary> Property manager key denoting the property this property provider holds. </summary>
        public const string SelectedConfigurationKey = "Mono.SelectedAddinConfigurationHashCode";

        /// <summary> Property manager key for whether Legal Copyright has been accepted. </summary>
        public const string LegalCopyrightAcceptedKey = "LegalCopyright.Accepted";

        /// <summary> Property manager key for Total Banknotes in amount at the end of the previous play. </summary>
        public const string PreviousGameEndTotalBanknotesInKey = "Application.PreviousGameEndTotalBanknotesIn";

        /// <summary> Property manager key for Total Coin in amount at the end of the previous play. </summary>
        public const string PreviousGameEndTotalCoinInKey = "Application.PreviousGameEndTotalCoinIn";

        /// <summary> Property manager key for if excessive meter increment lockup exist. </summary>
        public const string ExcessiveMeterIncrementLockedKey = "Application.ExcessiveMeterIncrementLockedKey";

        /// <summary> Property Manager key for SerialNumber (Alphanumeric). </summary>
        public const string SerialNumber = "Cabinet.SerialNumber";

        /// <summary> Property Manager key for MachineId (Numeric - unsigned). </summary>
        public const string MachineId = "Cabinet.MachineId";

        /// <summary> The properties manager area key. </summary>
        public const string Area = "Cabinet.Area";

        /// <summary> The properties manager zone key. </summary>
        public const string Zone = "Cabinet.ZoneName";

        /// <summary> The properties manager bank id key. </summary>
        public const string Bank = "Cabinet.BankId";

        /// <summary> The properties manager egm position key. </summary>
        public const string Position = "Cabinet.EgmPosition";

        /// <summary> The properties manager egm location key. </summary>
        public const string Location = "Cabinet.MachineLocation";

        /// <summary> The properties manager egm calculated device name key. </summary>
        public const string CalculatedDeviceName = "Cabinet.CalculatedDeviceName";

        /// <summary> Property Manager key for operating hours. </summary>
        public const string OperatingHours = "Cabinet.OperatingHours";

        /// <summary> The properties manager currency Id. </summary>
        public const string CurrencyId = "Cabinet.CurrencyId";

        /// <summary> The properties manager currency description. </summary>
        public const string CurrencyDescription = "Cabinet.CurrencyDescription";

        /// <summary> The properties manager egm position key. </summary>
        public const string Currencies = "Cabinet.Currencies";

        /// <summary> The properties manager key for paper in chute sound file path. </summary>
        public const string PaperInChuteSoundKey = "Cabinet.PaperInChuteSound";

        /// <summary> The properties manager key for out of printer error sound file path. </summary>
        public const string PrinterErrorSoundKey = "Cabinet.PrinterErrorSound";

        /// <summary> The properties manager key for printer warning sound file path. </summary>
        public const string PrinterWarningSoundKey = "Cabinet.PrinterWarningSound";

        /// <summary> The properties manager key for note acceptor (BNA) error sound file path. </summary>
        public const string NoteAcceptorErrorSoundKey = "Cabinet.NoteAcceptorErrorSound";

        /// <summary> The properties manager key for Disk Space Monitor error sound file path. </summary>
        public const string DiskSpaceMonitorErrorSoundKey = "Cabinet.DiskSpaceMonitorErrorSound";

        /// <summary> The properties manager key for Firmware CRC error sound file path. </summary>
        public const string FirmwareCrcErrorSoundKey = "Cabinet.FirmwareCrcErrorSound";

        /// <summary> The properties manager key for Memory Monitor error sound file path. </summary>
        public const string MemoryMonitorErrorSoundKey = "Cabinet.MemoryMonitorErrorSound";

        /// <summary> The properties manager key for LiveAuthenticationFailed sound file path. </summary>
        public const string LiveAuthenticationFailedSoundKey = "Cabinet.LiveAuthenticationFailedSound";

        /// <summary> The properties manager key for touch sound file path. </summary>
        public const string TouchSoundKey = "Cabinet.TouchSound";

        /// <summary> The properties manager key for coin in sound file path. </summary>
        public const string CoinInSoundKey = "Cabinet.CoinInSound";

        /// <summary> The properties manager key for coin out sound file path. </summary>
        public const string CoinOutSoundKey = "Cabinet.CoinOutSound";

        /// <summary> The properties manager key for feature bell sound file path. </summary>
        public const string FeatureBellSoundKey = "Cabinet.FeatureBellSound";

        /// <summary> The properties manager key for collect sound file path. </summary>
        public const string CollectSoundKey = "Cabinet.CollectSound";

        /// <summary> The properties manager key for host offline sound file path. </summary>
        public const string HostOfflineSoundKey = "Cabinet.HostOfflineSound";

        /// <summary> The properties manager key for ding sound file path. </summary>
        public const string DingSoundKey = "Cabinet.DingSoundKey";

        /// <summary> The properties manager key for audit ticket mode. </summary>
        public const string TicketModeAuditKey = "Cabinet.TicketModeAudit";

        /// <summary> The key to access the cabinet style property. </summary>
        public const string CabinetStyle = "Cabinet.CabinetStyleString";

        /// <summary> The key to access the cabinet print identity property. </summary>
        public const string CabinetPrintIdentity = "Cabinet.PrintIdentity";

        /// <summary> The key to access the cabinet detailed audit ticket property. </summary>
        public const string DetailedAuditTickets = "Application.DetailedAuditTickets";

        /// <summary> The key to access the cabinet Property Id. </summary>
        public const string PropertyId = @"Cabinet.PropertyId";

        /// <summary> Maximum no. of line we can print on a ticket. </summary>
        public const string AuditTicketLineLimit = "Application.AuditTicketLineLimit";

        /// <summary> Maximum no. of event logs per page</summary>
        public const string AuditTicketEventsPerPage = "Application.AuditTicketEventsPerPage";

        /// <summary> Maximum number of characters per ticket line </summary>
        public const int AuditTicketMaxLineLength = 45;

        /// <summary> Test or blank </summary>
        public const string ActiveProtocol = "Active.Protocol";

        /// <summary> Property Manager key for IsInitialConfigurationComplete. </summary>
        public const string IsInitialConfigurationComplete = "IsInitialConfigurationComplete";

        /// <summary> Property Manager key for the selected Jurisdiction </summary>
        public const string JurisdictionKey = "System.Jurisdiction";

        /// <summary>
        ///     Property Manager key for AlertVolume.
        /// </summary>
        public const string AlertVolumeKey = "Application.AlertVolume";

        /// <summary>
        ///     Default volume level used if default is not specified in config files.
        /// </summary>
        public const byte DefaultVolumeLevel = 3;

        /// <summary>
        ///     Default for playing test alert sound on change.
        /// </summary>
        public const bool DefaultPlayTestAlertSound = false;

        /// <summary>
        ///     Default minimum volume for alerts if not specified in config files.
        /// </summary>
        public const byte AlertVolumeMinimum = 50;

        /// <summary>
        ///     Property Manager key for UseGameTypeVolume.
        /// </summary>
        public const string UseGameTypeVolumeKey = "Application.UseGameTypeVolume";

        /// <summary>
        ///     The default value for the UseGameTypeVolume flag.
        /// </summary>
        public const bool UseGameTypeVolume = true;

        /// <summary>
        ///     Property Manager key for LobbyVolumeScalar.
        /// </summary>
        public const string LobbyVolumeScalarKey = "Application.LobbyVolumeScalar";

        /// <summary>
        ///     The default value for the LobbyVolumeScalar property.
        /// </summary>
        public const byte LobbyVolumeScalar = 5;

        /// <summary>
        ///     Property Manager key for PlayerVolumeScalar.
        /// </summary>
        public const string PlayerVolumeScalarKey = "Application.PlayerVolumeScalar";

        /// <summary>
        ///     The default value for the PlayerVolumeScalar property.
        /// </summary>
        public const byte PlayerVolumeScalar = 5;

        /// <summary>
        ///     The key for the VolumeControlLocation property
        /// </summary>
        public const string VolumeControlLocationKey = "Application.VolumeControlLocation";

        /// <summary>
        ///     The default setting for the VolumeControlLocation property
        /// </summary>
        public const int VolumeControlLocationDefault = 2;

        /// <summary>
        ///     Property key for whether to enable show mode
        /// </summary>
        public const string ShowMode = "System.ShowMode";

        /// <summary>
        ///     Property key for whether to show game rules button in games
        /// </summary>
        public const string GameRules = "System.GameRules";

        /// <summary>
        ///     The maximum length of the Machine Id for G2S Protocol.
        /// </summary>
        public const int MaxMachineIdLengthG2S = 8;

        /// <summary>
        ///     The maximum length of the Machine Id for SAS Protocol.
        /// </summary>
        public const int MaxMachineIdLengthSAS = 10;

        /// <summary>
        ///     The maximum length of the location field.
        /// </summary>
        public const int MaxLocationLength = 16;

        /// <summary>
        ///     The maximum number of operating hour intervals (from the G2S spec).
        /// </summary>
        public const int MaxOperatingHours = 28;

        /// <summary>
        ///     The  CurrencyMultiplier key.
        /// </summary>
        public const string CurrencyMultiplierKey = "CurrencyMultiplier";

        /// <summary>
        ///     The default currency identifier.
        /// </summary>
        public const string DefaultCurrencyId = "USD";

        /// <summary>
        ///     The  Default Currency Multiplier.
        /// </summary>
        public const double DefaultCurrencyMultiplier = 100000D;

        /// <summary>
        ///     Gets the Property Key for the Role that was assigned before launching the Operator Menu.
        /// </summary>
        public const string RolePropertyKey = "OperatorMenu.Role";

        /// <summary>
        ///     Defines the default role that has basic permissions
        /// </summary>
        public const string DefaultRole = @"Administrator";

        /// <summary>
        ///     Defines a role that has elevated permissions
        /// </summary>
        public const string TechnicianRole = @"Technician";

        /// <summary>
        ///     NYL Property manager key for current operator ID
        /// </summary>
        public const string CurrentOperatorId = "CurrentOperatorId";

        /// <summary>
        ///     Mono.Addins extension path for Application's JurisdictionConfiguration
        /// </summary>
        public const string JurisdictionConfigurationExtensionPath = "/Application/JurisdictionConfiguration";

        /// <summary>
        ///     Jurisdiction extension string
        /// </summary>
        public const string Jurisdiction = "Jurisdiction";

        /// <summary>
        ///     Key for the jurisdictionId property in property manager.  The property value corresponding to
        ///     this key is set by the command line arguments.
        /// </summary>
        public const string JurisdictionId = "jurisdictionId";

        /// <summary>
        ///     Protocol extension string
        /// </summary>
        public const string Protocol = "Protocol";

        /// <summary>
        ///     The media player
        /// </summary>
        public const string MediaPlayers = @"MediaPlayer.Profiles";

        /// <summary>
        ///     Defines a property indicating whether or not a package should be deleted after installation is complete
        /// </summary>
        public const string DeletePackageAfterInstall = @"Application.DeletePackageAfterInstall";

        /// <summary>
        ///     Path Mapper path identifier for manifests
        /// </summary>
        public const string ManifestPath = "/Manifests";

        /// <summary>
        ///     Path Mapper path identifier for jurisdiction data
        /// </summary>
        public const string JurisdictionsPath = "/Jurisdictions";

        /// <summary>
        ///     Key used to get a value indicating whether e-Key smart card has been inserted and verified
        /// </summary>
        public const string EKeyVerified = "Hardware.EKeyVerified";

        /// <summary>
        ///     Key used to get the e-Key SD card drive path
        /// </summary>
        public const string EKeyDrive = "Hardware.EKeyDrive";

        /// <summary>
        ///     Gets the ISystemDisableManager key used when the EKey Device is verified.
        /// </summary>
        public static Guid EKeyVerifiedDisableKey => new Guid("{B237B7D0-8E96-4052-ACEE-9576BA79DE69}");

        /// <summary>
        ///     Gets the ISystemDisableManager key used when operating hours are enabled.
        /// </summary>
        public static Guid SystemDisableGuid => new Guid("{9705470A-33DA-4c7e-8F0A-886991B2AA4F}");

        /// <summary>
        ///     Gets the ISystemDisableManager key used when operating hours are enabled.
        /// </summary>
        public static Guid OperatingHoursDisableGuid => new Guid("{A5210158-059B-4523-B6CA-F1145EEA1DF1}");

        /// <summary>
        ///     Gets the ISystemDisableManager key used when the operator menu is launched.
        /// </summary>
        public static Guid OperatorMenuLauncherDisableGuid => new Guid("{38B2CB9B-3113-4B6A-9F3F-91356BD5B8F8}");

        /// <summary>
        ///     Key used to disable the operator menu during initialization
        /// </summary>
        /// <remarks>
        ///     The gaming layer (lobby) will typically be responsible for enabling the operator menu
        /// </remarks>
        public static Guid OperatorMenuInitializationKey => new Guid("{AE1D1DC3-517B-46B8-9798-DF6EB06F6729}");

        /// <summary>
        ///     Key used to disable the system when no games are enabled
        /// </summary>
        public static Guid NoGamesEnabledDisableKey => new Guid("{A2E017C8-4510-4520-AA28-8C12A6C4A60C}");

        /// <summary>
        ///     Key used to disable the system when the number of connected mechanical reels
        ///     doesn't match what the enabled game expects
        /// </summary>
        public static Guid ReelCountMismatchDisableKey => new("{0C45B33C-BA5A-4363-8337-935CB69F151F}");

        /// <summary>
        ///     Key used to disable the system when the reel controller is disconnected
        /// </summary>
        public static readonly Guid ReelControllerDisconnectedGuid = new Guid("{47402BA4-420B-4E89-936A-385D760B53C3}");

        /// <summary>
        ///     Whether HardMeter TickValue is Configurable or not.
        /// </summary>
        public const string HardMeterTickValueConfigurable = "HardMeterTickValueConfigurable";

        /// <summary>
        ///     Gets Default Hard Meter Tick Value
        /// </summary>
        public const string HardMeterTickValue = "HardMeterTickValue";

        /// <summary>
        ///     Gets Default Hard Meter Tick Value
        /// </summary>
        public const string HardMeterMapSelectionValue = "HardMeterMapSelectionValue";

        /// <summary> Property Manager key for HardMeterDefaultMeterMappingName. </summary>
        public const string HardMeterDefaultMeterMappingName = "Default";

        /// <summary>
        ///     Gets selected TowerLight Value
        /// </summary>
        public const string TowerLightTierTypeSelection = "TowerLightTierTypeSelection";

        /// <summary> The default value for determining whether or not the attract scene is shown </summary>
        public const bool DefaultAttractMode = false;

        /// <summary>Property manager key for LowMemoryThreshold flag.</summary>
        public const string LowMemoryThreshold = "Application.LowMemoryThreshold";

        /// <summary>Default memory threshold if none specified.</summary>
        public const long LowMemoryThresholdDefault = 209715200;

        /// <summary>Property manager key for ClockEnabled flag.</summary>
        public const string ClockEnabled = "Clock.Enabled";

        /// <summary>Property manager key for ClockFormat flag.</summary>
        public const string ClockFormat = "Clock.Format";

        /// <summary>Property manager key for CashoutClearWins flag.</summary>
        public const string CashoutClearWins = "Cashout.ClearWins";

        /// <summary>Property manager key for CommitStorageAfterCashout flag.</summary>
        public const string CommitStorageAfterCashout = "Cashout.CommitStorageAfterCashout";

        /// <summary>Property manager key multi-game DefaultBetAfterSwitch flag.</summary>
        public const string DefaultBetAfterSwitch = "MultiGame.DefaultBetAfterSwitch";

        /// <summary>Property manager key multi-game RestoreRebootStateAfterSwitch flag.</summary>
        public const string RestoreRebootStateAfterSwitch = "MultiGame.RestoreRebootStateAfterSwitch";

        /// <summary>Property manager key multi-game StateStorageLocation flag.</summary>
        public const string StateStorageLocation = "MultiGame.StateStorageLocation";

        /// <summary>
        ///     The default value for the DefaultBetInAttract flag.
        ///     If this setting is not equal to "disallowed" then switch to the initial bet option after 30 seconds in game idle state
        /// </summary>
        public const bool DefaultBetInAttract = true;

        /// <summary>The default DefaultAllowDenomPatch flag Value.</summary>
        public const bool DefaultAllowDenomPatch = true;

        /// <summary>The default DefaultDenomSelectionLobbyRequired value for DefaultDenomSelectionLobbyRequired flag.</summary>
        public const bool DefaultDenomSelectionLobbyRequired = false;

        /// <summary>Default DefaultGameDisabledUse flag Value.</summary>
        public const string DefaultGameDisabledUse = "disallowed";

        /// <summary>Default DefaultWinMeterResetOnBetLineDenomChanged flag Value. sets win meter to 0 and stop win increment once bet is changed</summary>
        public const bool DefaultWinMeterResetOnBetLineDenomChanged = true;

        /// <summary>Default DefaultConfirmDenomChange flag Value. Determine if a confirmation popup is required when changing denom.</summary>
        public const string DefaultConfirmDenomChange = "disallowed";

        /// <summary> Whether Bottom strip should be enabled or not </summary>
        public static string BottomStripEnabled = "BottomStripEnabled";

        /// <summary>
        ///     Key used to get the edge light role as a tower light is enabled or not.
        /// </summary>
        public static string EdgeLightAsTowerLightEnabled = "EdgeLightAsTowerLightEnabled";

        /// <summary>
        ///     Key used to get whether barkeeper functionality is enabled or not
        /// </summary>
        public static string BarkeeperEnabled = "BarkeeperEnabled";

        /// <summary>
        ///     Key used to get whether the Universal Interface Box is enabled or not
        /// </summary>
        public static string UniversalInterfaceBoxEnabled = "UniversalInterfaceBoxEnabled";

        /// <summary>
        ///     Key used to get whether the Harkey Reel Controller is enabled or not
        /// </summary>
        public static string HarkeyReelControllerEnabled = "HarkeyReelControllerEnabled";

        /// <summary>
        ///     Key used to get whether the Beagle Bones is enabled or not
        /// </summary>
        public static string BeagleBoneEnabled = "BeagleBoneEnabled";

        /// <summary>
        ///     used to get mask of enabled speakers
        /// </summary>
        public static string EnabledSpeakersMask = "EnabledSpeakersMask";

        /// <summary>
        /// used to get the MalfunctionMessage enabled or not.
        /// </summary>
        public static string EnabledMalfunctionMessage = "MalfunctionMessage";

        /// <summary>
        ///     Key used to disable the system when the display is disconnected
        /// </summary>
        public static Guid DisplayDisconnectedLockupKey = new Guid("{29AFB89B-80AD-4CAD-A410-53F26FB87DFD}");

        /// <summary>
        ///     Key used to disable the system when the touch display is disconnected
        /// </summary>
        public static Guid TouchDisplayDisconnectedLockupKey = new Guid("{34AFB79B-67BB-4CDC-A910-53F26FA87BFD}");

        /// <summary>
        ///     Key used to disable the system when the touch display is reconnected
        /// </summary>
        public static readonly Guid TouchDisplayReconnectedLockupKey = new Guid("{2189AFDD-E52E-40A8-9466-F711007D9862}");

        /// <summary>
        ///     Key used to disable the system when the button deck is disconnected
        /// </summary>
        public static readonly Guid LcdButtonDeckDisconnectedLockupKey = new Guid("{44B79BA8-08EF-4AED-9569-EEE0547A72D0}");

        /// <summary>
        ///     Key used to disable the system when a handpay is pending
        /// </summary>
        public static readonly Guid HandpayPendingDisableKey = new Guid("{9F4A85A2-F478-4AA2-A5FF-D387D74894F2}");

        /// <summary>
        ///     Key used to disable the system when a cashout failed
        /// </summary>
        public static readonly Guid HostCashOutFailedDisableKey = new Guid("{AD40EFCE-63C0-42E9-8FE4-68B222D2DC8D}");

        /// <summary>
        ///     Key used to disable the system when SAS Host 0 is disabled
        /// </summary>
        public static readonly Guid DisabledByHost0Key = new Guid("{145119C8-E099-4630-B1D9-B1FF85F7381D}");

        /// <summary>
        ///     Key used to disable the system when SAS Host 1 is disabled
        /// </summary>
        public static readonly Guid DisabledByHost1Key = new Guid("{9EC4368B-2FFE-4eda-999D-705577D5BFD2}");

        /// <summary>
        ///     Key used to disable the system when SAS Host 0 communications are offline
        /// </summary>
        public static readonly Guid Host0CommunicationsOfflineDisableKey = new Guid("{DA761F90-4C3B-4e14-9B73-11BB97A5C019}");

        /// <summary>
        ///     Key used to disable the system when SAS Host 1 communications are offline
        /// </summary>
        public static readonly Guid Host1CommunicationsOfflineDisableKey = new Guid("{52A2A637-FEBD-43e0-A798-A6FBFF7178A4}");

        /// <summary>
        ///     Key used to disable the system when Validation id is not set from SAS Host
        /// </summary>
        public static readonly Guid ValidationIdNeededGuid = new Guid("{13CAD5EC-F655-4049-B9B4-E9017BFA79F7}");


        /// <summary>
        ///     Key used to disable the system when Live Authentication is active
        /// </summary>
        public static readonly Guid LiveAuthenticationDisableKey = new Guid("{C7A33F9E-AEC9-472B-95A8-BD938208300C}");

        /// <summary>
        ///     System disable guid for excessive bill reject.
        /// </summary>
        public static readonly Guid ExcessiveDocumentRejectGuid = new Guid("{2142111C-CC15-4791-9748-29360A4CF9C7}");

        /// <summary>
        ///     Time zone
        /// </summary>
        public const string TimeZoneKey = "Cabinet.TimeZone";

        /// <summary>
        ///     Time zone offset
        /// </summary>
        public const string TimeZoneOffsetKey = "Cabinet.TimeZoneOffset";

        /// <summary>
        ///     Time zone bias
        /// </summary>
        public const string TimeZoneBias = "Cabinet.TimeZoneBias";

        /// <summary>
        ///     Visible status
        /// </summary>
        public const string Visible = "Visible";

        /// <summary>
        ///     Property manager key for NoteAcceptorDiagnostics
        /// </summary>
        public const string NoteAcceptorDiagnosticsKey = "NoteAcceptorDiagnostics";

        /// <summary>
        ///     Property manager key for DisabledByOperatorText.
        /// </summary>
        public const string DisabledByOperatorText = "Application.GeneralMessages.DisabledByOperatorText";

        /// <summary>
        ///     Property manager key for NoteAcceptorErrorBillJam.
        /// </summary>
        public const string NoteAcceptorErrorBillJamText = "Application.NoteAcceptorErrorMessages.BillJamText";

        /// <summary>
        ///     Property manager key for NoteAcceptorErrorBillStackerError.
        /// </summary>
        public const string NoteAcceptorErrorBillStackerErrorText =
            "Application.NoteAcceptorErrorMessages.BillStackerErrorText";

        /// <summary>
        ///     Property manager key for NoteAcceptorErrorBillStackerFull.
        /// </summary>
        public const string NoteAcceptorErrorBillStackerFullText =
            "Application.NoteAcceptorErrorMessages.BillStackerFullText";

        /// <summary>
        ///     Property manager key for NoteAcceptorErrorBillStackerJam.
        /// </summary>
        public const string NoteAcceptorErrorBillStackerJamText =
            "Application.NoteAcceptorErrorMessages.BillStackerJamText";

        /// <summary>
        ///     Property manager key for NoteAcceptorErrorBillUnexpectedError.
        /// </summary>
        public const string NoteAcceptorErrorBillUnexpectedErrorText =
            "Application.NoteAcceptorErrorMessages.BillUnexpectedErrorText";

        /// <summary>
        ///     Property manager key for NoteAcceptorErrorBillValidatorFault.
        /// </summary>
        public const string NoteAcceptorErrorBillValidatorFaultText =
            "Application.NoteAcceptorErrorMessages.BillValidatorFaultText";

        /// <summary>
        ///     Property manager key for NoteAcceptorErrorInvalidBill.
        /// </summary>
        public const string NoteAcceptorErrorInvalidBillText = "Application.NoteAcceptorErrorMessages.InvalidBillText";

        /// <summary>
        ///     Property manager key for NoteAcceptorErrorCashBoxRemoved.
        /// </summary>
        public const string NoteAcceptorErrorCashBoxRemovedText =
            "Application.NoteAcceptorErrorMessages.CashBoxRemovedText";

        /// <summary>
        ///     Property manager key for NoteAcceptorErrorGeneralFailure.
        /// </summary>
        public const string NoteAcceptorErrorGeneralFailureText =
            "Application.NoteAcceptorErrorMessages.GeneralFailureText";

        /// <summary>
        ///     Property manager key for the currency meter rollover value.
        /// </summary>
        public const string CurrencyMeterRolloverText = "Application.MeterRollover.Currency";

        /// <summary>
        ///     Property manager key for the occurrence meter rollover value.
        /// </summary>
        public const string OccurrenceMeterRolloverText = "Application.MeterRollover.Occurrence";

        /// <summary>
        ///     Property manager key for the initial bell ring value.
        /// </summary>
        public const string InitialBellRing = "Application.BellConfiguration.InitialValue";

        /// <summary>
        ///     Property manager key for the interval bell ring value.
        /// </summary>
        public const string IntervalBellRing = "Application.BellConfiguration.IntervalValue";

        /// <summary>
        ///     Property manager key for the max bell ring value.
        /// </summary>
        public const string MaxBellRing = "Application.BellConfiguration.MaxValue";

        /// <summary>
        ///     Property manager key for the stacker removed behavior value.
        /// </summary>
        public const string StackerRemovedBehaviorAutoClearPeriodMetersText = "Application.StackerRemovedBehavior";

        /// <summary>
        ///     Property manager key for the AutoClearPeriodMetersText value.
        /// </summary>
        public const string AutoClearPeriodMetersText = "Application.AutoClearPeriodMetersBehavior.AutoClearPeriodMeters";

        /// <summary>
        ///     Property manager key for the ClearLocalTimeOffsetHour value.
        /// </summary>
        public const string ClearClearPeriodOffsetHoursText = "Application.AutoClearPeriodMetersBehavior.ClearClearPeriodOffsetHours";

        /// <summary>
        ///     Property manager key for demonstration mode from jurisdiction config
        /// </summary>
        public const string DemonstrationModeEnabled = "Application.Demonstration.Enabled";

        /// <summary>
        ///     Property manager key for demonstration mode from Persistence
        /// </summary>
        public const string DemonstrationMode = "DemonstrationMode";

        /// <summary>
        ///     Default language.
        /// </summary>
        public const string DefaultLanguage = "en-US";

        /// <summary>
        ///     Default date format for player tickets.
        /// </summary>
        public const string DefaultPlayerTicketDateFormat = "yyyy-MM-dd";

        /// <summary>
        ///     Default date format.
        /// </summary>
        public const string DefaultDateFormat = "yyyy-MM-dd";

        /// <summary>
        ///     Default time format.
        /// </summary>
        public const string DefaultTimeFormat = "HH:mm:ss";

        /// <summary>
        ///     Default datetime format.
        /// </summary>
        public const string DefaultDateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        ///     Property manager key for LocalizationCurrentCulture
        /// </summary>
        public const string LocalizationCurrentCulture = "Localization.CurrentCulture";

        /// <summary>
        ///     Property manager key for LocalizationState
        /// </summary>
        public const string LocalizationState = "Localization.State";

        /// <summary>
        ///     Property manager key for LocalizationPlayerCurrentCulture.
        /// </summary>
        public const string LocalizationPlayerCurrentCulture = "Localization.Player.CurrentCulture";

        /// <summary>
        ///     Property manager key for LocalizationOperatorCurrentCulture.
        /// </summary>
        public const string LocalizationOperatorCurrentCulture = "Localization.Operator.CurrentCulture";

        /// <summary>
        ///     Property manager key for LocalizationOperatorTicketCurrentCulture.
        /// </summary>
        public const string LocalizationOperatorTicketCurrentCulture = "Localization.OperatorTicket.CurrentCulture";

        /// <summary>
        ///     Property manager key for LocalizationOperatorTicketLocale
        /// </summary>
        public const string LocalizationOperatorTicketLocale = "Localization.OperatorTicket.Locale";

        /// <summary>
        ///     Property manager key for LocalizationOperatorTicketDateFormat
        /// </summary>
        public const string LocalizationOperatorTicketDateFormat = "Localization.OperatorTicket.DateFormat";

        /// <summary>
        ///     Property manager key for LocalizationOperatorTicketSelectable
        /// </summary>
        public const string LocalizationOperatorTicketSelectable = "Localization.OperatorTicket.Selectable";

        /// <summary>
		///     Property manager key for LocalizationPlayerTicketCurrentCulture.
		/// </summary>
		public const string LocalizationPlayerTicketCurrentCulture = "Localization.PlayerTicket.CurrentCulture";

        /// <summary>
        ///     Property manager key for LocalizationPlayerTicketLocale
        /// </summary>
        public const string LocalizationPlayerTicketLocale = "Localization.PlayerTicket.Locale";

        /// <summary>
        ///     Property manager key for LocalizationPlayerTicketDateFormat
        /// </summary>
        public const string LocalizationPlayerTicketDateFormat = "Localization.PlayerTicket.DateFormat";

        /// <summary>
        ///     Property manager key for LocalizationPlayerTicketDefault
        /// </summary>
        public const string LocalizationPlayerTicketDefault = "Localization.PlayerTicket.Default";

        /// <summary>
        ///     Property manager key for LocalizationPlayerTicketSelectable
        /// </summary>
        public const string LocalizationPlayerTicketSelectable = "Localization.PlayerTicket.Selectable";

        /// <summary>
        ///     Property manager key for LocalizationPlayerTicketLanguageSettingVisible
        /// </summary>
        public const string LocalizationPlayerTicketLanguageSettingVisible = "Localization.PlayerTicket.LanguageSetting.Visible";

        /// <summary>
        ///     Property manager key for LocalizationPlayerTicketLanguageSettingShowCheckBox
        /// </summary>
        public const string LocalizationPlayerTicketLanguageSettingShowCheckBox = "Localization.PlayerTicket.LanguageSetting.ShowCheckBox";

        /// <summary>
        ///     Property manager key for LocalizationPlayerTicketOverride
        /// </summary>
        public const string LocalizationPlayerTicketOverride = "Localization.PlayerTicket.Override";

        /// <summary>
		///     Property manager key for LocalizationOperatorDefault.
		/// </summary>
		public const string LocalizationOperatorDefault = "Localization.Operator.Default";

        /// <summary>
        ///    Property manager key for LocalizationOperatorAvailable.
        /// </summary>
        public const string LocalizationOperatorAvailable = "Localization.Operator.Available";

        /// <summary>
        ///     Property manager key for LocalizationPlayerPrimary.
        /// </summary>
        public const string LocalizationPlayerPrimary = "Localization.Player.Primary";

        /// <summary>
        ///     Property manager key for LocalizationPlayerAvailable.
        /// </summary>
        public const string LocalizationPlayerAvailable = "Localization.Player.Available";

        /// <summary>
        ///     Property manager key for LocalizationOperatorDateFormat
        /// </summary>
        public const string LocalizationOperatorDateFormat = "Localization.Operator.DateFormat";

        /// <summary> Property Manager key for NoteAcceptorEnabled. </summary>
        public const string NoteAcceptorEnabled = "Cabinet.NoteAcceptorEnabled";

        /// <summary> Property Manager key for NoteAcceptorManufacturer. </summary>
        public const string NoteAcceptorManufacturer = "Cabinet.NoteAcceptorManufacturer";

        /// <summary> Property Manager key for PrinterEnabled. </summary>
        public const string PrinterEnabled = "Cabinet.PrinterEnabled";

        /// <summary> Property Manager key for PrinterManufacturer. </summary>
        public const string PrinterManufacturer = "Cabinet.PrinterManufacturer";

        /// <summary> Property Manager key for IdReaderEnabled. </summary>
        public const string IdReaderEnabled = "Cabinet.IdReaderEnabled";

        /// <summary> Property Manager key for IdReaderManufacturer. </summary>
        public const string IdReaderManufacturer = "Cabinet.IdReaderManufacturer";

        /// <summary> Property Manager key for Reel Controller Enabled. </summary>
        public const string ReelControllerEnabled = "Cabinet.ReelControllerEnabled";

        /// <summary> Property Manager key for Reel Controller Manufacturer. </summary>
        public const string ReelControllerManufacturer = "Cabinet.ReelControllerManufacturer";

        /// <summary> Property manager key for ConfigWizardMachineSetupConfigVisibility. </summary>
        public const string ConfigWizardMachineSetupConfigVisibility = "ConfigWizard.MachineSetupConfig.Visibility";

        /// <summary> Property manager key for ConfigWizardIOConfigPageUseSelectionEnabled. </summary>
        public const string ConfigWizardUseSelectionEnabled = "ConfigWizard.IOConfigPage.UseSelection.Enabled";

        /// <summary> Property manager key for ConfigWizardIdentityPageVisibility. </summary>
        public const string ConfigWizardIdentityPageVisibility = "ConfigWizard.IdentityPage.Visibility";

        /// <summary> Property manager key for ConfigWizardIdentityPageSerialNumberOverride. </summary>
        public const string ConfigWizardIdentityPageSerialNumberOverride = "ConfigWizard.IdentityPage.SerialNumberOverride";

        /// <summary> Property manager key for ConfigWizardIdentityPageAssetNumberOverride. </summary>
        public const string ConfigWizardIdentityPageAssetNumberOverride = "ConfigWizard.IdentityPage.AssetNumberOverride";

        /// <summary> Property manager key for ConfigWizardIdentityPageAreaOverride. </summary>
        public const string ConfigWizardIdentityPageAreaOverride = "ConfigWizard.IdentityPage.AreaOverride";

        /// <summary> Property manager key for ConfigWizardIdentityPageZoneOverride. </summary>
        public const string ConfigWizardIdentityPageZoneOverride = "ConfigWizard.IdentityPage.ZoneOverride";

        /// <summary> Property manager key for ConfigWizardIdentityPageBankOverride. </summary>
        public const string ConfigWizardIdentityPageBankOverride = "ConfigWizard.IdentityPage.BankOverride";

        /// <summary> Property manager key for ConfigWizardIdentityPagePositionOverride. </summary>
        public const string ConfigWizardIdentityPagePositionOverride = "ConfigWizard.IdentityPage.PositionOverride";

        /// <summary> Property manager key for ConfigWizardIdentityPageLocationOverride. </summary>
        public const string ConfigWizardIdentityPageLocationOverride = "ConfigWizard.IdentityPage.LocationOverride";

        /// <summary> Property manager key for ConfigWizardIdentityPageLocationOverride. </summary>
        public const string ConfigWizardIdentityPageDeviceNameOverride = "ConfigWizard.IdentityPage.DeviceNameOverride";

        /// <summary> Property manager key for ConfigWizardIdentityPagePrintIdentityVisibility. </summary>
        public const string ConfigWizardPrintIdentityEnabled = "ConfigWizard.IdentityPage.PrintIdentity.Visibility";

        /// <summary> Property manager key for ConfigWizardTimeConfigVisibility. </summary>
        public const string ConfigWizardCompletionPageShowGameSetupMessage = "ConfigWizard.CompletionPage.ShowGameSetupMessage";

        /// <summary> Property manager key for ConfigWizardHardMetersConfigConfigurable. </summary>
        public const string ConfigWizardHardMetersConfigConfigurable = "ConfigWizard.HardMetersConfig.Configurable";

        /// <summary> Property manager key for ConfigWizardHardMetersConfigCanReconfigure. </summary>
        public const string ConfigWizardHardMetersConfigCanReconfigure = "ConfigWizard.HardMetersConfig.CanReconfigure";

        /// <summary> Property manager key for ConfigWizardHardMetersConfigVisible. </summary>
        public const string ConfigWizardHardMetersConfigVisible = "ConfigWizard.HardMetersConfig.Visible";

        /// <summary> Property manager key for ConfigWizardDoorOpticsVisible. </summary>
        public const string ConfigWizardDoorOpticsVisible = "ConfigWizard.DoorOptics.Visible";

        /// <summary> Property manager key for ConfigWizardDoorOpticsConfigurable. </summary>
        public const string ConfigWizardDoorOpticsConfigurable = "ConfigWizard.DoorOptics.Configurable";

        /// <summary> Property manager key for ConfigWizardDoorOpticsEnabled. </summary>
        public const string ConfigWizardDoorOpticsEnabled = "ConfigWizard.DoorOptics.Enabled";

        /// <summary> Property manager key for ConfigWizardDoorOpticsCanReconfigure. </summary>
        public const string ConfigWizardDoorOpticsCanReconfigure = "ConfigWizard.DoorOptics.CanReconfigure";

        /// <summary> Property manager key for ConfigWizardBellyPanelDoorVisible. </summary>
        public const string ConfigWizardBellyPanelDoorVisible = "ConfigWizard.BellyPanelDoor.Visible";

        /// <summary> Property manager key for ConfigWizardBellyPanelDoorConfigurable. </summary>
        public const string ConfigWizardBellyPanelDoorConfigurable = "ConfigWizard.BellyPanelDoor.Configurable";

        /// <summary> Property manager key for ConfigWizardBellyPanelDoorEnabled. </summary>
        public const string ConfigWizardBellyPanelDoorEnabled = "ConfigWizard.BellyPanelDoor.Enabled";

        /// <summary> Property manager key for ConfigWizardBellyPanelDoorCanReconfigure. </summary>
        public const string ConfigWizardBellyPanelDoorCanReconfigure = "ConfigWizard.BellyPanelDoor.CanReconfigure";

        /// <summary> Property manager key for ConfigWizardBellVisible. </summary>
        public const string ConfigWizardBellVisible = "ConfigWizard.Bell.Visible";

        /// <summary> Property manager key for ConfigWizardBellConfigurable. </summary>
        public const string ConfigWizardBellConfigurable = "ConfigWizard.Bell.Configurable";

        /// <summary> Property manager key for ConfigWizardBellEnabled. </summary>
        public const string ConfigWizardBellEnabled = "ConfigWizard.Bell.Enabled";

        /// <summary> Property manager key for ConfigWizardBellCanReconfigure. </summary>
        public const string ConfigWizardBellCanReconfigure = "ConfigWizard.Bell.CanReconfigure";

        /// <summary> Property manager key for ConfigWizardLimitsPageEnabled. </summary>
        public const string ConfigWizardLimitsPageEnabled = "ConfigWizard.LimitsPage.Enabled";

        /// <summary> Property manager key for ConfigWizardLimitsPageEnabled. </summary>
        public const string ConfigWizardCreditLimitCheckboxEditable = "ConfigWizard.LimitsPage.CreditLimit.CheckboxEditable";

        /// <summary> Property manager key for ConfigWizardLimitsPageEnabled. </summary>
        public const string ConfigWizardHandpayLimitCheckboxEditable = "ConfigWizard.LimitsPage.HandpayLimit.CheckboxEditable";

        /// <summary> Property manager key for ConfigWizardHardwarePageRequirePrinter. </summary>
        public const string ConfigWizardHardwarePageRequirePrinter = "ConfigWizard.HardwarePage.RequirePrinter";

        /// <summary> Last selected index of initial setup wizard</summary>
        public const string ConfigWizardLastPageViewedIndex = "LastPageViewedIndex";

        /// <summary> Done with the selection pages in initial setup wizard</summary>
        public const string ConfigWizardSelectionPagesDone = "SelectionPagesDone";

        /// <summary> Property Manager key for MediaDisplay. </summary>
        public const string MediaDisplayEnabled = "Application.MediaDisplayEnabled";

        /// <summary>Property Manager key for machine settings imported flag.</summary>
        public const string MachineSettingsImported = "MachineSettings.Imported";

        /// <summary>Property Manager key for machine settings re-import flag.</summary>
        public const string MachineSettingsReimport = "MachineSettings.Reimport";

        /// <summary>Property Manager key for machine settings re-imported flag.</summary>
        public const string MachineSettingsReimported = "MachineSettings.Reimported";

        /// <summary> Fake </summary>
		public const string Fake = "Fake";

        /// <summary> GDS </summary>
        public const string GDS = "GDS";

        /// <summary> Serial Port Prefix (COM) </summary>
        public const string SerialPortPrefix = "COM";

        /// <summary> USB </summary>
        public const string USB = "USB";

        /// <summary> DeviceType </summary>
        public const string DeviceType = "DeviceType";

        /// <summary> Enabled </summary>
        public const string Enabled = "Enabled";

        /// <summary> Make </summary>
        public const string Make = "Make";

        /// <summary> Port </summary>
        public const string Port = "Port";

        /// <summary> Currency </summary>
        public const string Currency = "Currency";

        /// <summary> RequireZeroCredit </summary>
        public const string RequireZeroCredit = "RequireZeroCredit";

        /// <summary> SerialNumber </summary>
        public const string ConfigSerialNumber = "SerialNumber";

        /// <summary> MachineId </summary>
        public const string ConfigMachineId = "MachineId";

        /// <summary> IgnoreTouchCalibration </summary>
        public const string IgnoreTouchCalibration = "ignoreTouchCalibration";

        /// <summary> Property manager key for ScreenBrightness. </summary>
        public const string ScreenBrightness = "ScreenBrightness";

        /// <summary> The default screen brightness level for non-bar top cabinets </summary>
        public const int DefaultBrightness = 100;

        /// <summary> The default edge light minimum brightness level for non-bar top cabinets </summary>
        public const int DefaultEdgeLightingMinimumBrightness = 50;

        /// <summary> The default edge light maximum brightness level for non-bar top cabinets </summary>
        public const int DefaultEdgeLightingMaximumBrightness = 100;

        /// <summary> The excessive document reject default count.</summary>
        public const int ExcessiveDocumentRejectDefaultCount = -1;

        /// <summary> The property manager key for the Bottom Edge Lighting on/off switch </summary>
        public const string BottomEdgeLightingOnKey = "BottomEdgeLightingOn";

        /// <summary> The property manager key for the attract mode color override key </summary>
        public const string EdgeLightingAttractModeColorOverrideSelectionKey = "LightingOverrideSelection";

        /// <summary> The property manager key for the lobby mode color override key </summary>
        public const string EdgeLightingLobbyModeColorOverrideSelectionKey = "EdgeLightingLobbyModeOverrideSelection";

        /// <summary> The property manager key for the default color for edge lights </summary>
        public const string EdgeLightingDefaultStateColorOverrideSelectionKey = "EdgeLightingDefaultStateOverrideSelection";

        /// <summary> The property manager key for the Beagle Bone show override selection </summary>
        public const string ShowOverrideSelectionKey = "ShowOverrideSelection";

        /// <summary> The property manager key for the maximum allowed edge lighting value </summary>
        public const string MaximumAllowedEdgeLightingBrightnessKey = "MaximumAllowedEdgeLightingBrightness";

        /// <summary> The key to access the cabinet type property. </summary>
        public const string CabinetTypeKey = "CabinetType";

        /// <summary>Property manager key for MachineSetupConfigEnterOutOfServiceWithCreditsEnabled.</summary>
        public const string MachineSetupConfigEnterOutOfServiceWithCreditsEnabled =
            "ConfigWizard.MachineSetupConfig.EnterOutOfServiceWithCredits.Enabled";

        /// <summary>Property manager key for MachineSetupConfigEnterOutOfServiceWithCreditsEditable.</summary>
        public const string MachineSetupConfigEnterOutOfServiceWithCreditsEditable =
            "ConfigWizard.MachineSetupConfig.EnterOutOfServiceWithCredits.Editable";

        /// <summary>Property manager key for ExcessiveDocumentReject LockupType.</summary>
        public const string ExcessiveDocumentRejectLockupType = "Application.ExcessiveDocumentReject.LockupType";

        /// <summary>Property manager key for ExcessiveDocumentReject Count.</summary>
        public const string ExcessiveDocumentRejectCount = "Application.ExcessiveDocumentReject.Count";

        /// <summary>Property manager key for ExcessiveDocumentReject Count Default.</summary>
        public const string ExcessiveDocumentRejectCountDefault = "Application.ExcessiveDocumentReject.Count.Default";

        /// <summary>Property manager key for ExcessiveDocumentReject SoundFilePath, if this file is present then Audio is played when ExcessiveDocumentReject is satisfied.</summary>
        public const string ExcessiveDocumentRejectSoundFilePath = "Application.ExcessiveDocumentReject.SoundFilePath";

        /// <summary>Property manager key for ExcessiveDocumentReject ResetMethodKey, if this lockup is present then ResetMethodKey is used to clear it.</summary>
        public const string ExcessiveDocumentRejectResetMethodKey = "Application.ExcessiveDocumentReject.ResetMethodKey";

        /// <summary>Property manager key for CriticalMemoryIntegrityCheck Enabled.</summary>
        public const string PeriodicCriticalMemoryIntegrityCheckEnabled = "Application.CriticalMemoryIntegrityCheck.Enabled";

        /// <summary>Property manager key for CriticalMemoryIntegrityCheck Enabled.</summary>
        public const string PeriodicCriticalMemoryIntegrityCheckValue = "Application.CriticalMemoryIntegrityCheck.Value";

        /// <summary>Property manager key for CriticalMemoryIntegrityCheck SoundFilePath to be used for alerting when check fails.</summary>
        public const string PeriodicCriticalMemoryIntegrityCheckSoundFilePath = "Application.CriticalMemoryIntegrityCheck.SoundFilePath";

        /// <summary> If Cabinet Brightness control feature is enabled </summary>
        public const string CabinetBrightnessControlEnabled = "CabinetBrightnessControlEnabled";

        /// <summary> Cabinet Brightness control Minimum attribute </summary>
        public const string CabinetBrightnessControlMin = "CabinetBrightnessControlMin";

        /// <summary> Cabinet Brightness control Default attribute </summary>
        public const string CabinetBrightnessControlDefault = "CabinetBrightnessControlDefault";

        /// <summary> Cabinet Brightness control Maximum attribute </summary>
        public const string CabinetBrightnessControlMax = "CabinetBrightnessControlMax";

        /// <summary> If Edgelight Brightness control feature is enabled </summary>
        public const string EdgeLightingBrightnessControlEnabled = "EdgeLightingBrightnessControlEnabled";

        /// <summary> If Edgelight Brightness control Minimum attribute </summary>
        public const string EdgeLightingBrightnessControlMin = "EdgeLightingBrightnessControlMin";

        /// <summary> If Edgelight Brightness control Default attribute </summary>
        public const string EdgeLightingBrightnessControlDefault = "EdgeLightingBrightnessControlDefault";

        /// <summary> If Edgelight Brightness control Maximum attribute </summary>
        public const string EdgeLightingBrightnessControlMax = "EdgeLightingBrightnessControlMax";

        /// <summary> Platform Enhanced display feature for Cashout/Jackpot/CashWin </summary>
        public const string PlatformEnhancedDisplayEnabled = "Application.PlatformEnhancedDisplay.Enabled";

        /// <summary> Configuration requires control programs to be run from read only media </summary>
        public const string ReadOnlyMediaRequired = "Application.ReadOnlyMediaRequired";

        /// <summary>
        ///     Belly Door GUID
        /// </summary>
        public static readonly Guid BellyDoorGuid = new Guid("{FA5E990C-C069-4524-9F31-1AC35FABF840}");

        /// <summary>
        ///     Cash Door GUID
        /// </summary>
        public static readonly Guid CashDoorGuid = new Guid("{B81238B1-9575-4CAB-9482-B5A5288A1BB5}");

        /// <summary>
        ///     Logic Door GUID
        /// </summary>
        public static readonly Guid LogicDoorGuid = new Guid("{3ACE2A2C-2E01-4d67-8C96-D8330B54E1BE}");

        /// <summary>
        ///     Main Door GUID
        /// </summary>
        public static readonly Guid MainDoorGuid = new Guid("{CEA96AF8-A853-476c-A048-98F9AF88134F}");

        /// <summary>
        ///     Secondary Cash Door GUID
        /// </summary>
        public static readonly Guid SecondaryCashDoorGuid = new Guid("{537131AD-B401-4C1B-910A-0CC097A902C4}");

        /// <summary>
        ///     TopBox Door GUID
        /// </summary>
        public static readonly Guid TopBoxDoorGuid = new Guid("{7CE7E032-75A9-46ae-9737-B2A86D65C719}");

        /// <summary>
        ///     Drop Door GUID
        /// </summary>
        public static readonly Guid DropDoorGuid = new Guid("{FE821B58-4B6D-43ad-B8C0-B92EB62313F0}");

        /// <summary>
        ///     Mechanical Meter Door GUID
        /// </summary>
        public static readonly Guid MechanicalMeterDoorGuid = new Guid("{9374E8A1-CB29-4A32-A4B9-28783ADC2B24}");

        /// <summary>
        ///     Main Optic Door GUID
        /// </summary>
        public static readonly Guid MainOpticDoorGuid = new Guid("{72AAFAB5-FC3C-4152-931C-04A2CBFA6AAC}");

        /// <summary>
        ///     TopBox Optic Door GUID
        /// </summary>
        public static readonly Guid TopBoxOpticDoorGuid = new Guid("{7DA9CB7B-6A3D-4D89-96E2-7FE50CB6BAF8}");

        /// <summary>
        ///     Universal Interface Box Door GUID
        /// </summary>
        public static readonly Guid UniversalInterfaceBoxDoorGuid = new Guid("B45EC680-F81A-4E74-8ACE-36CBCDBB8A07");

        /// <summary>
        ///     NoteAcceptorDisconnectedGuid GUID
        /// </summary>
        public static readonly Guid NoteAcceptorDisconnectedGuid = new Guid("{488F0095-BD9E-4AC7-9AB8-13B5E8B7B62E}");

        /// <summary>
        ///     NoteAcceptorSelfTestFailedGuid GUID
        ///     would be used to create a error/lockup when self test for the BNA got failed.
        /// </summary>
        public static readonly Guid NoteAcceptorSelfTestFailedGuid = new Guid("{4D246D90-113D-426E-9CA2-95827BE96111}");

        /// <summary>
        ///     PrinterDisconnectedGuid GUID
        /// </summary>
        public static readonly Guid PrinterDisconnectedGuid = new Guid("{91B975FC-1EC9-4C24-9700-B3749FEF73C3}");

        /// <summary>
        ///     Battery 1 GUID
        /// </summary>
        public static readonly Guid Battery1Guid = new Guid("{22AFC79B-46CC-2AFC-E220-32F26FC87BAB}");

        /// <summary>
        ///      Battery 2 GUID
        /// </summary>
        public static readonly Guid Battery2Guid = new Guid("{32AFC79B-56CC-2EFC-E529-62F26FC87BAA}");

        /// <summary>
        ///      Progressive Update Timeout GUID
        /// </summary>
        public static readonly Guid ProgressiveUpdateTimeoutGuid = new Guid("{FA358CB0-4FCD-4028-BE64-8FD2FD9949E6}");

        /// <summary>
        ///      Progressive commit timeout GUID
        /// </summary>
        public static readonly Guid ProgressiveCommitTimeoutGuid = new Guid("{C153E5DA-4311-465A-8739-9B63F2F6705D}");

        /// <summary>
        ///      Progressive disconnected error GUID
        /// </summary>
        public static readonly Guid ProgressiveDisconnectErrorGuid = new Guid("{782B18C4-9C9C-43C6-93C8-B50BF783A944}");

        /// <summary>
        ///      Minimum threshold error GUID
        /// </summary>
        public static readonly Guid MinimumThresholdErrorGuid = new Guid("{e1904c57-648d-4d8d-9cdc-f2ac7edd1ddd}");

        /// <summary>
        ///      Excessive Meter Increment lockup GUID
        /// </summary>
        public static Guid ExcessiveMeterIncrementErrorGuid = new Guid("{444D0667-F0B1-4346-BB3D-C0AB5BA4763D}");

        /// <summary>
        ///      Self Audit Error lockup GUID
        /// </summary>
        public static Guid SelfAuditErrorGuid = new Guid("{7756BF42-FEBB-4CC2-B5CA-9313A493DD8D}");

        /// <summary>
        ///      Belly Door discrepency lockup GUID
        /// </summary>
        public static Guid BellyDoorDiscrepencyGuid = new Guid("{E3307468-9863-49E4-AF9E-C3982B869B27}");

        /// <summary> Property manager key for SoundConfigurationAlertVolumeMinimum </summary>
        public const string SoundConfigurationAlertVolumeMinimum = "SoundConfiguration.AlertVolume.Minimum";

        /// <summary> Property manager key for SoundConfigurationAlertVolumeConfigurable </summary>
        public const string SoundConfigurationAlertVolumeConfigurable = "SoundConfiguration.AlertVolume.Configurable";

        /// <summary> Property manager key for SoundConfigurationPlayTestAlertSound </summary>
        public const string SoundConfigurationPlayTestAlertSound = "SoundConfiguration.AlertVolume.PlayTestSound";

        /// <summary> Property manager key for SoundConfigurationLogicDoorFullVolumeAlert </summary>
        public const string SoundConfigurationLogicDoorFullVolumeAlert = "SoundConfiguration.AlertVolume.LogicDoorFullVolumeAlert";

        /// <summary> The default maximum allowable value when editing max credits in.</summary>
        public const decimal MaxCreditsInMax = 9_999_999_999.99M;

        /// <summary> The default minimum allowable value when editing max credits in.</summary>
        public const decimal MaxCreditsInMin = 1;

        /// <summary> The default max credits in.</summary>
        public const long DefaultMaxCreditsIn = 10_000_000_000;

        /// <summary>
        ///     Persistence key for saving if LCD button deck is expected on subsequent power ups after first power up.
        /// </summary>
        public const string LcdPlayerButtonExpected = "LcdPlayerButtonExpected";

        /// <summary> Property manager key for Bar code type</summary>
        public const string BarCodeType = "BarCodeType";

        /// <summary> Property manager key for validation length</summary>
        public const string ValidationLength = "ValidationLength";

        /// <summary> Property manager key for layout type</summary>
        public const string LayoutType = "LayoutType";

        /// <summary> Property manager key for Communications Online</summary>
        public const string CommunicationsOnline = "System.CommunicationsOnline";

        /// <summary>
        ///      Used for creating a secondary lockup, when system is disable, which requires operator reset
        /// </summary>
        public static readonly Guid OperatorResetRequiredDisableKey = new Guid("{144DC0DC-5940-471E-A3C0-4401E14494C4}");

        /// <summary> Property manager key for OperatorLockupResetEnabled </summary>
        public const string OperatorLockupResetEnabled = "OperatorLockupReset.Enabled";

        /// <summary> The key used to disable the system when currency with invalid ISO is entered</summary>
        public static readonly Guid CurrencyIsoInvalidDisableKey = new Guid("{097B50DD-ED1B-4761-A9EE-DAC4D3D88E91}");

        /// <summary> The key used to disable the system when hard meters disabled </summary>
        public static readonly Guid HardMeterDisabled = new Guid("{0B8A403E-FE8E-4190-8FB4-712285E8E461}");

        /// <summary> The key used to disable the system when the document check occurs.</summary>
        public static readonly Guid NoteAcceptorDocumentCheckDisableKey = new Guid("{50596087-C683-449C-9BAF-A54F39673987}");

        /// <summary> The key used to disable the system when the operator key is left in EGM.</summary>
        public static readonly Guid OperatorKeyNotRemovedDisableKey = new Guid("B047DA7E-1474-4727-993D-4219782334EF");

        /// <summary> The key used to disable the system when the game is disabled due to a fatal error.</summary>
        public static readonly Guid FatalGameErrorGuid = new Guid("{4BC3FC7F-CBE5-4D61-AF59-556B15FE25AB}");

        /// <summary> The key used to disable the system when the disk space falls below a threshold.</summary>
        public static readonly Guid DiskSpaceBelowThresholdDisableKey = new Guid("370AB562-A4E8-4368-865A-7ECB946EF8FC");

        /// <summary> The key used to disable the system when the disk space falls below a threshold.</summary>
        public static readonly Guid MemoryBelowThresholdDisableKey = new Guid("E980D852-3851-4246-9843-B2AA0DBD04C1");

        /// <summary> The key used to disable the system when an unrecoverable memory error is detected.</summary>
        public static readonly Guid StorageFaultDisableKey = new Guid("1cdf0037-5490-44f3-8505-7ceadaec2dff");

        /// <summary> Property manager key for logs to be displayed in operator menu</summary>
        public const string LogTypesAllowedForDisplayKey = "LogTypesAllowedForDisplayKey";

        /// <summary>
        ///     Key used to disable the system when secondary storage is connected but not supported
        /// </summary>
        public static Guid SecondaryStorageMediaConnectedKey => new Guid("{DB7CA371-8606-405C-9BBA-65C4219C0623}");

        /// <summary>
        ///     Key used to disable the system when secondary storage is supported but not connected
        /// </summary>
        public static Guid SecondaryStorageMediaNotConnectedKey => new Guid("{9512CE3D-FE74-4609-B2B4-7F74673B6F8A}");

        /// <summary> The key used to indicate that the cabinet type affects what is displayed to the screen. </summary>
        public static readonly string CabinetControlsDisplayElements = "CabinetControlsDisplayElements";

        /// <summary>
        ///     Key used to disable the system when there is a read-only configuration error
        /// </summary>
        public static readonly Guid ReadOnlyMediaDisableKey = new Guid("{322F9CF5-2143-4489-B806-837705DE63FA}");

        /// <summary>
        ///     Key used to disable the system when there is a Smart Card Removed error
        /// </summary>
        public static readonly Guid SmartCardRemovedDisableKey = new Guid("{27B87C06-2C02-49F0-9A17-85B65D74A31E}");

        /// <summary>
        ///     Key used to disable the system when there is a Smart Card Expired error
        /// </summary>
        public static readonly Guid SmartCardExpiredDisableKey = new Guid("{FDC7D7DA-9FB7-4EB1-8882-224C752C9C2C}");

        /// <summary>
        ///     Key used to disable the system when there is a Smart Card Not Present error
        /// </summary>
        public static readonly Guid SmartCardNotPresentDisableKey = new Guid("{556EC49E-9E06-471D-BF5F-6C6C13114085}");

        /// <summary>
        ///     Key used to disable the system when there is a License Error error
        /// </summary>
        public static readonly Guid LicenseErrorDisableKey = new Guid("{0D67A85A-EA03-4B79-9724-C739DA279A74}");

        /// <summary>
        ///     Key used to disable the system when Platform is verifying the firmware CRCs of peripherals
        /// </summary>
        public static readonly Guid MonitorVerifyingDisableKey = new Guid("{AE2B7D73-1C54-4CBE-9233-E94D60389A41}");

        /// <summary>
        ///     Key used to disable the system when Platform processed a firmware CRC signature mismatch
        /// </summary>
        public static readonly Guid MonitorSignatureMismatchDisableKey = new Guid("{5A0FF459-9EAD-4908-911C-BD81AA74D9B3}");

        /// <summary>
        ///     Key used for platform booted message.
        /// </summary>
        public static readonly Guid PlatformBootedKey = new Guid("{3E6C4270-46C5-41C2-B76D-1E4BCEBCD199}");

        /// <summary>
        ///     Key used to limit number of denom selections in audit
        /// </summary>
        public const int NumSelectableDenomsPerGameTypeInLobby = 12;

        /// <summary>
        ///     Indicates whether or not handpay receipts are to be printed.
        /// </summary>
        public const string HandpayReceiptPrintingEnabled = "Handpay.HandpayReceiptPrintingEnabled";

        /// <summary>
        ///     Indicates whether or not we have a game that talks to a Central Determination Server
        /// </summary>
        public const string CentralAllowed = "Application.CentralAllowed";

        /// <summary>
        ///     The file prefix for the jurisdiction package
        /// </summary>
        public const string JurisdictionPackagePrefix = @"ATI_Jurisdictions";

        /// <summary>
        ///     Key to disable via reserve lockup.
        /// </summary>
        public static readonly Guid ReserveDisableKey = new Guid("{55BE4308-0179-4D64-B57F-7F4A87868B8C}");

        /// <summary>
        ///     Key to disable while waiting for player input
        /// </summary>
        public static readonly Guid WaitingForInputDisableKey = new Guid("953C70C2-DBF2-4D5F-ADD1-4FE17DBD9A9D");

        /// <summary>
        ///     Property manager key for Reserve service allowed by jurisdiction.
        /// </summary>
        public const string ReserveServiceAllowed = "ReserveService.Allowed";

        /// <summary>
        ///     Property manager key for Reserve service support enabled by operator.
        /// </summary>
        public const string ReserveServiceEnabled = "ReserveService.Enabled";

        /// <summary>
        ///     Property manager key for Reserve service timeout duration in minutes.
        /// </summary>
        public const string ReserveServiceTimeoutInSeconds = "ReserveService.TimeoutInSeconds";

        /// <summary>
        ///     Property manager key for Reserve service pin.
        /// </summary>
        public const string ReserveServicePin = "ReserveService.Pin";

        /// <summary>
        ///     Property manager key for Reserve service pin.
        /// </summary>
        public const string ReserveServiceLockupPresent = "ReserveService.Lockup";

        /// <summary>
        ///     Property key for the remaining seconds of the reserved lockup
        /// </summary>y
        public const string ReserveServiceLockupRemainingSeconds = "ReserveService.LockupRemainingSeconds";

        /// <summary> Property manager key for protocols allowed for jurisdiction</summary>
        public const string ProtocolsAllowedKey = "Protocols.Allowed";

        /// <summary> Property manager key for mandatory protocol required for jurisdiction</summary>
        public const string MandatoryProtocol = "MandarotyProtocols";

        /// <summary>The original screen width the Platform UI was designed around</summary>
        public const int TargetResolutionWidth = 1920;

        /// <summary>The original screen height the Platform UI was designed around</summary>
        public const int TargetResolutionHeight = 1080;

        /// <summary>
        ///     Wait for the progressive initialization with the host.
        /// </summary>
        public const string WaitForProgressiveInitialization = @"Host.WaitForProgressiveInitialization";

        /// <summary>
        ///     Logic door has been opened and verification has not yet been received.
        /// </summary>
        public static readonly Guid LogicSealBrokenKey = new Guid("{3A6282CC-2321-4758-851B-4F1055984628}");

        /// <summary>
        ///     Display the Master Result (xor result of all individual hashes of components)
        /// </summary>
        public const string ShowMasterResult = @"SoftwareVerification.ShowMasterResult";

        /// <summary>
        ///     Flag that determines whether FirmwareCrcMonitor functionality is enabled or not
        /// </summary>
        public const string FirmwareCrcMonitorEnabled = @"FirmwareCrcMonitor.Enabled";

        /// <summary>
        ///     Flag value that provides the seed value for the CRC calculations
        /// </summary>
        public const string FirmwareCrcMonitorSeed = @"FirmwareCrcMonitor.Seed";

        /// <summary>
        ///     Flag indicate whether we need to run Signature verification at every reboot
        /// </summary>
        public const string RunSignatureVerificationAfterReboot = @"LiveAuthenticationManager.RunSignatureVerificationAfterReboot";

        /// <summary>
        ///     Show the wager information on LargeWin/JackpotWin presentation and Jackpot Receipt.
        /// </summary>
        public const string ShowWagerWithLargeWinInfo = @"LargeWinInfo.ShowWager";

        /// <summary>
        ///     Stores the last wager for the LargeWin/JackpotWin presentation and Jackpot Receipt.
        /// </summary>
        public const string LastWagerWithLargeWinInfo = @"LargeWinInfo.LastWager";

        /// <summary>
        ///     Do not require the EGM to be reconfigured if the Topper gets disconnected. Note Topper must be connected for initial configure.
        /// </summary>
        public const string TopperDisplayDisconnectNoReconfigure = @"Application.TopperDisplayDisconnectNoReconfigure";
    }
}
