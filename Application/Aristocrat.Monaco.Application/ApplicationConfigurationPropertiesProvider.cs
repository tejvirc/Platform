namespace Aristocrat.Monaco.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Aristocrat.Monaco.Hardware.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Audio;
    using Contracts;
    using Contracts.Localization;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Gds;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Kernel.Contracts;
    using log4net;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     Definition of the ApplicationConfigurationPropertiesProvider class
    /// </summary>
    public class ApplicationConfigurationPropertiesProvider : IPropertyProvider
    {
        private const PersistenceLevel Level = PersistenceLevel.Static;
        private const int DefaultReserveTimeoutInMinutes = 5;
        private const long DefaultBellValueInMillicents = 1000000;
        private const long DefaultMaxBellValueInMillicents = 10000000000;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<string, Tuple<object, string, bool>> _properties;
        private readonly IPersistentStorageManager _storageManager;
        private readonly bool _blockExists;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationConfigurationPropertiesProvider" /> class.
        /// </summary>
        public ApplicationConfigurationPropertiesProvider()
        {
            _storageManager = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();

            _blockExists = _storageManager.BlockExists(GetBlockName());

            var configuration = ConfigurationUtilities.GetConfiguration(
                ApplicationConstants.JurisdictionConfigurationExtensionPath,
                () =>
                    new ApplicationConfiguration
                    {
                        AllowedLocales = new string[0],
                        AutoClearPeriodMetersBehavior = new ApplicationConfigurationAutoClearPeriodMetersBehavior
                        {
                            AutoClearPeriodMeters = false,
                            ClearClearPeriodOffsetHours = 0,
                        },
                        BillsAccepted = new ApplicationConfigurationBillsAccepted
                        {
                            Bills = 255
                        },
                        SoftwareInstall = new ApplicationConfigurationSoftwareInstall
                        {
                            DeletePackageAfter = false
                        },
                        ExcessiveDocumentReject = new ApplicationConfigurationExcessiveDocumentReject
                        {
                            LockupType = ExcessiveDocumentRejectLockupType.Soft,
                            ConsecutiveRejectsBeforeLockup = -1
                        },
                        OperatorLockupReset = new ApplicationConfigurationOperatorLockupReset
                        {
                            Enabled = false
                        },
                        FirmwareCrcMonitor = new ApplicationConfigurationFirmwareCrcMonitor()
                        {
                            Enabled = false
                        },
                        SecondaryStorageMedia = new ApplicationConfigurationSecondaryStorageMedia(),
                        ReadOnlyMedia = new ApplicationConfigurationReadOnlyMedia(),
                        HandpayReceiptPrinting = new ApplicationConfigurationHandpayReceiptPrinting(),
                        EdgeLightConfiguration = new ApplicationConfigurationEdgeLightConfiguration(),
                    });

            var deletePackageAfterInstall = configuration.SoftwareInstall?.DeletePackageAfter ?? false;
            var mediaDisplayEnabled = configuration.MediaDisplay?.Enabled ?? false;
            var defaultVolumeLevel = configuration.SoundConfiguration?.DefaultVolumeLevel ?? ApplicationConstants.DefaultVolumeLevel;
            var defaultVolumeControlLocation = configuration.SoundConfiguration?.VolumeControl?.Location ??
                                               (VolumeControlLocation)ApplicationConstants.VolumeControlLocationDefault;
            var excessiveDocumentRejectCount = configuration.ExcessiveDocumentReject?.ConsecutiveRejectsBeforeLockup ??
                                               ApplicationConstants.ExcessiveDocumentRejectDefaultCount;
            var excessiveDocumentRejectSoundFilePath = configuration.ExcessiveDocumentReject?.SoundFilePath ?? string.Empty;
            var barCodeType = configuration.BarcodeType;
            var validationLength = configuration.ValidationLength;
            var layoutType = configuration.LayoutType;
            var attractModeColorOverride = configuration.EdgeLightConfiguration?.AttractModeColor ??
                                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LightingOverrideTransparent);
            var bottomEdgeLightingOn = false;
            var reserveServiceEnabled = configuration.ReserveService?.Enabled ?? true;
            var reserveServiceLockupPresent = false;
            var reserveServicePin = string.Empty;
            var reserveServiceTimeoutInSeconds = configuration.ReserveService?.TimeoutInSeconds ??
                                                 (int)TimeSpan.FromMinutes(DefaultReserveTimeoutInMinutes).TotalSeconds;
            var reserveServiceLockupRemainingTime = 0;

            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var machineSettingsImported = propertiesManager.GetValue(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None);
            if (!_blockExists && machineSettingsImported != ImportMachineSettings.None)
            {
                deletePackageAfterInstall = propertiesManager.GetValue(ApplicationConstants.DeletePackageAfterInstall, false);
                if (configuration.MediaDisplay != null)
                {
                    mediaDisplayEnabled = propertiesManager.GetValue(ApplicationConstants.MediaDisplayEnabled, false);
                }

                defaultVolumeLevel = propertiesManager.GetValue(PropertyKey.DefaultVolumeLevel, ApplicationConstants.DefaultVolumeLevel);
                defaultVolumeControlLocation = (VolumeControlLocation)propertiesManager.GetValue(
                    ApplicationConstants.VolumeControlLocationKey,
                    ApplicationConstants.VolumeControlLocationDefault);
                excessiveDocumentRejectCount = propertiesManager.GetValue(
                    ApplicationConstants.ExcessiveDocumentRejectCount,
                    ApplicationConstants.ExcessiveDocumentRejectDefaultCount);
                excessiveDocumentRejectSoundFilePath = propertiesManager.GetValue(
                    ApplicationConstants.ExcessiveDocumentRejectSoundFilePath,
                    string.Empty);
                barCodeType = propertiesManager.GetValue(ApplicationConstants.BarCodeType, BarcodeTypeOptions.Interleave2of5);
                validationLength = propertiesManager.GetValue(ApplicationConstants.ValidationLength, ValidationLengthOptions.System);
                layoutType = propertiesManager.GetValue(ApplicationConstants.LayoutType, LayoutTypeOptions.ExtendedLayout);
                attractModeColorOverride = propertiesManager.GetValue(
                    ApplicationConstants.EdgeLightingAttractModeColorOverrideSelectionKey,
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LightingOverrideTransparent));
                bottomEdgeLightingOn = propertiesManager.GetValue(ApplicationConstants.BottomEdgeLightingOnKey, false);
                reserveServiceEnabled = propertiesManager.GetValue(ApplicationConstants.ReserveServiceEnabled, true);
                reserveServiceLockupPresent = propertiesManager.GetValue(
                    ApplicationConstants.ReserveServiceLockupPresent,
                    false);
                reserveServicePin = propertiesManager.GetValue(ApplicationConstants.ReserveServicePin, string.Empty);
                reserveServiceTimeoutInSeconds = propertiesManager.GetValue(
                    ApplicationConstants.ReserveServiceTimeoutInSeconds,
                    (int)TimeSpan.FromMinutes(DefaultReserveTimeoutInMinutes).TotalSeconds);
                reserveServiceLockupRemainingTime = propertiesManager.GetValue(
                    ApplicationConstants.ReserveServiceLockupRemainingSeconds,
                    0);
            }



            // The Tuple is structured as value (Item1), Key (Item2), IsPersistent (Item3)
            _properties = new Dictionary<string, Tuple<object, string, bool>>
            {
                {
                    ApplicationConstants.DisabledByOperatorText,
                    Tuple.Create(
                        (object)configuration.GeneralMessages?.DisabledByOperator.Message,
                        ApplicationConstants.DisabledByOperatorText,
                        false)
                },
                {
                    ApplicationConstants.NoteAcceptorErrorBillJamText,
                    Tuple.Create(
                        (object)configuration.NoteAcceptorErrorMessages?.BillJam.Message,
                        ApplicationConstants.NoteAcceptorErrorBillJamText,
                        false)
                },
                {
                    ApplicationConstants.NoteAcceptorErrorBillStackerErrorText,
                    Tuple.Create(
                        (object)configuration.NoteAcceptorErrorMessages?.BillStackerError.Message,
                        ApplicationConstants.NoteAcceptorErrorBillStackerErrorText,
                        false)
                },
                {
                    ApplicationConstants.NoteAcceptorErrorBillStackerFullText,
                    Tuple.Create(
                        (object)configuration.NoteAcceptorErrorMessages?.BillStackerFull.Message,
                        ApplicationConstants.NoteAcceptorErrorBillStackerFullText,
                        false)
                },
                {
                    ApplicationConstants.NoteAcceptorErrorBillStackerJamText,
                    Tuple.Create(
                        (object)configuration.NoteAcceptorErrorMessages?.BillStackerJam.Message,
                        ApplicationConstants.NoteAcceptorErrorBillStackerJamText,
                        false)
                },
                {
                    ApplicationConstants.NoteAcceptorErrorBillUnexpectedErrorText,
                    Tuple.Create(
                        (object)configuration.NoteAcceptorErrorMessages?.BillUnexpectedError.Message,
                        ApplicationConstants.NoteAcceptorErrorBillUnexpectedErrorText,
                        false)
                },
                {
                    ApplicationConstants.NoteAcceptorErrorBillValidatorFaultText,
                    Tuple.Create(
                        (object)configuration.NoteAcceptorErrorMessages?.BillValidatorFault.Message,
                        ApplicationConstants.NoteAcceptorErrorBillValidatorFaultText,
                        false)
                },
                {
                    ApplicationConstants.NoteAcceptorErrorInvalidBillText,
                    Tuple.Create(
                        (object)configuration.NoteAcceptorErrorMessages?.InvalidBill.Message,
                        ApplicationConstants.NoteAcceptorErrorInvalidBillText,
                        false)
                },
                {
                    ApplicationConstants.NoteAcceptorErrorCashBoxRemovedText,
                    Tuple.Create(
                        (object)configuration.NoteAcceptorErrorMessages?.CashBoxRemoved.Message,
                        ApplicationConstants.NoteAcceptorErrorCashBoxRemovedText,
                        false)
                },
                {
                    ApplicationConstants.NoteAcceptorErrorGeneralFailureText,
                    Tuple.Create(
                        (object)configuration.NoteAcceptorErrorMessages?.GeneralFailure.Message,
                        ApplicationConstants.NoteAcceptorErrorGeneralFailureText,
                        false)
                },
                {
                    ApplicationConstants.CurrencyMeterRolloverText,
                    Tuple.Create(
                        (object)configuration.MeterRollover?.Currency,
                        ApplicationConstants.CurrencyMeterRolloverText,
                        false)
                },
                {
                    ApplicationConstants.OccurrenceMeterRolloverText,
                    Tuple.Create(
                        (object)configuration.MeterRollover?.Occurrence,
                        ApplicationConstants.OccurrenceMeterRolloverText,
                        false)
                },
                {
                    ApplicationConstants.InitialBellRing,
                    Tuple.Create(
                        (object)InitFromStorage(
                            ApplicationConstants.InitialBellRing,
                            configuration.BellConfiguration?.InitialValue ?? DefaultBellValueInMillicents),
                        ApplicationConstants.InitialBellRing,
                        true)
                },
                {
                    ApplicationConstants.IntervalBellRing,
                    Tuple.Create(
                        (object)InitFromStorage(
                            ApplicationConstants.IntervalBellRing,
                            configuration.BellConfiguration?.IntervalValue ?? DefaultBellValueInMillicents),
                        ApplicationConstants.IntervalBellRing,
                        true)
                },
                {
                    ApplicationConstants.MaxBellRing,
                    Tuple.Create(
                        (object)configuration.BellConfiguration?.MaxBellValue ?? DefaultMaxBellValueInMillicents,
                        ApplicationConstants.MaxBellRing,
                        false)
                },
                {
                    ApplicationConstants.StackerRemovedBehaviorAutoClearPeriodMetersText,
                    Tuple.Create(
                        (object)configuration.StackerRemoveBehavior?.AutoClearPeriodMeters,
                        ApplicationConstants.StackerRemovedBehaviorAutoClearPeriodMetersText,
                        false)
                },
                {
                    ApplicationConstants.AutoClearPeriodMetersText,
                    Tuple.Create(
                        (object)configuration.AutoClearPeriodMetersBehavior?.AutoClearPeriodMeters,
                        ApplicationConstants.AutoClearPeriodMetersText,
                        false)
                },
                {
                    ApplicationConstants.ClearClearPeriodOffsetHoursText,
                    Tuple.Create(
                        (object)configuration.AutoClearPeriodMetersBehavior?.ClearClearPeriodOffsetHours,
                        ApplicationConstants.ClearClearPeriodOffsetHoursText,
                        false)
                },
                {
                    ApplicationConstants.AlertVolumeKey,
                    Tuple.Create(
                        (object)InitFromStorage(
                            ApplicationConstants.AlertVolumeKey,
                            configuration.SoundConfiguration?.AlertVolume?.Value ?? 100),
                        ApplicationConstants.AlertVolumeKey,
                        true)
                },
                {
                    ApplicationConstants.SoundConfigurationAlertVolumeMinimum,
                    Tuple.Create(
                        (object)configuration.SoundConfiguration?.AlertVolume?.Minimum ?? ApplicationConstants.AlertVolumeMinimum,
                         ApplicationConstants.SoundConfigurationAlertVolumeMinimum,
                         false)
                },
                {
                    ApplicationConstants.SoundConfigurationAlertVolumeConfigurable,
                    Tuple.Create(
                        (object)configuration.SoundConfiguration?.AlertVolume?.Configurable ?? false,
                         ApplicationConstants.SoundConfigurationAlertVolumeConfigurable,
                        false)
                },
                {
                    ApplicationConstants.SoundConfigurationPlayTestAlertSound,
                    Tuple.Create(
                        (object)configuration.SoundConfiguration?.AlertVolume?.PlayTestSound ?? false,
                         ApplicationConstants.SoundConfigurationPlayTestAlertSound,
                        false)
                },
                {
                    ApplicationConstants.SoundConfigurationLogicDoorFullVolumeAlert,
                    Tuple.Create(
                        (object)configuration.SoundConfiguration?.AlertVolume?.LogicDoorFullVolumeAlert ?? false,
                         ApplicationConstants.SoundConfigurationLogicDoorFullVolumeAlert,
                        false)
                },
                {
                    PropertyKey.DefaultVolumeLevel,
                    Tuple.Create(
                        (object)InitFromStorage(
                            PropertyKey.DefaultVolumeLevel,
                            defaultVolumeLevel),
                        PropertyKey.DefaultVolumeLevel,
                        true)
                },
                {
                    ApplicationConstants.UseGameTypeVolumeKey,
                    Tuple.Create(
                        (object)configuration.SoundConfiguration?.UseGameTypeVolume ?? ApplicationConstants.UseGameTypeVolume,
                        ApplicationConstants.UseGameTypeVolumeKey,
                        false)
                },
                {
                    ApplicationConstants.LobbyVolumeScalarKey,
                    Tuple.Create(
                        (object)InitFromStorage(
                            ApplicationConstants.LobbyVolumeScalarKey,
                            configuration.SoundConfiguration?.LobbyVolumeScalar ?? ApplicationConstants.LobbyVolumeScalar),
                        ApplicationConstants.LobbyVolumeScalarKey,
                        true)
                },
                {
                    ApplicationConstants.PlayerVolumeScalarKey,
                    Tuple.Create(
                        (object)InitFromStorage(
                            ApplicationConstants.PlayerVolumeScalarKey,
                            configuration.SoundConfiguration?.PlayerVolumeScalar ?? ApplicationConstants.PlayerVolumeScalar),
                        ApplicationConstants.PlayerVolumeScalarKey,
                        true)
                },
                {
                    ApplicationConstants.VolumeControlLocationKey,
                    Tuple.Create(
                        (object)InitFromStorage(
                            ApplicationConstants.VolumeControlLocationKey,
                            (int)defaultVolumeControlLocation),
                        ApplicationConstants.VolumeControlLocationKey,
                        true)
                },
                {
                    ApplicationConstants.DeletePackageAfterInstall,
                    Tuple.Create(
                        (object)deletePackageAfterInstall,
                        ApplicationConstants.DeletePackageAfterInstall,
                        true)
                },
                {
                    ApplicationConstants.BottomEdgeLightingOnKey,
                    Tuple.Create(
                        (object)InitFromStorage(
                            ApplicationConstants.BottomEdgeLightingOnKey,
                            bottomEdgeLightingOn),
                        ApplicationConstants.BottomEdgeLightingOnKey,
                        true)
                },
                {
                    ApplicationConstants.EdgeLightingAttractModeColorOverrideSelectionKey,
                    Tuple.Create(
                        (object)InitFromStorage(
                            ApplicationConstants.EdgeLightingAttractModeColorOverrideSelectionKey,
                            attractModeColorOverride),
                        ApplicationConstants.EdgeLightingAttractModeColorOverrideSelectionKey,
                        true)
                },
                {
                    ApplicationConstants.EdgeLightingLobbyModeColorOverrideSelectionKey,
                    Tuple.Create(
                        (object)configuration.EdgeLightConfiguration?.LobbyModeColor?? "Blue",
                        ApplicationConstants.EdgeLightingLobbyModeColorOverrideSelectionKey,
                        false)
                },
                {
                    ApplicationConstants.EdgeLightingDefaultStateColorOverrideSelectionKey,
                    Tuple.Create(
                        (object)configuration.EdgeLightConfiguration?.DefaultColor?? "Blue",
                        ApplicationConstants.EdgeLightingDefaultStateColorOverrideSelectionKey,
                        false)
                },
                {
                    ApplicationConstants.ExcessiveDocumentRejectLockupType,
                    Tuple.Create(
                        (object)configuration.ExcessiveDocumentReject?.LockupType ?? ExcessiveDocumentRejectLockupType.Soft,
                        ApplicationConstants.ExcessiveDocumentRejectLockupType,
                        false)
                },
                {
                    ApplicationConstants.ExcessiveDocumentRejectCount,
                    Tuple.Create(
                        InitFromStorage(
                            ApplicationConstants.ExcessiveDocumentRejectCount,
                            (object)excessiveDocumentRejectCount),
                        ApplicationConstants.ExcessiveDocumentRejectCount,
                        true)
                },
                {
                    ApplicationConstants.ExcessiveDocumentRejectCountDefault,
                    Tuple.Create(
                        (object)configuration.ExcessiveDocumentReject?.ConsecutiveRejectsBeforeLockup ?? -1,
                        ApplicationConstants.ExcessiveDocumentRejectCountDefault,
                        false)
                },
                {
                    ApplicationConstants.ExcessiveDocumentRejectSoundFilePath,
                    Tuple.Create(
                            (object)excessiveDocumentRejectSoundFilePath,
                        ApplicationConstants.ExcessiveDocumentRejectSoundFilePath,
                        false)
                },
                {
                    ApplicationConstants.ExcessiveDocumentRejectResetMethodKey,
                    Tuple.Create(
                        (object)configuration.ExcessiveDocumentReject?.ResetMethodKey ?? ResetMethodKeyType.MainDoor,
                        ApplicationConstants.ExcessiveDocumentRejectResetMethodKey,
                        false)
                },
                {
                    ApplicationConstants.PeriodicCriticalMemoryIntegrityCheckEnabled,
                    Tuple.Create(
                        (object)configuration.CriticalMemoryIntegrityCheck?.Enabled ?? false,
                        ApplicationConstants.PeriodicCriticalMemoryIntegrityCheckEnabled,
                        false)
                },
                {
                    ApplicationConstants.PeriodicCriticalMemoryIntegrityCheckValue,
                    Tuple.Create(
                        (object)configuration.CriticalMemoryIntegrityCheck?.Value ?? 86400,
                        ApplicationConstants.PeriodicCriticalMemoryIntegrityCheckValue,
                        false)
                },
                {
                    ApplicationConstants.PeriodicCriticalMemoryIntegrityCheckSoundFilePath,
                    Tuple.Create(
                        (object)configuration.CriticalMemoryIntegrityCheck?.SoundFilePath ?? string.Empty,
                        ApplicationConstants.PeriodicCriticalMemoryIntegrityCheckSoundFilePath,
                        false)
                },
                {
                    ApplicationConstants.PaperInChuteSoundKey,
                    Tuple.Create(
                        (object)configuration.PaperInChuteSound?.FilePath,
                        ApplicationConstants.PaperInChuteSoundKey,
                        false)
                },
                {
                    ApplicationConstants.PrinterErrorSoundKey,
                    Tuple.Create(
                        (object)configuration.PrinterErrorSound?.FilePath,
                        ApplicationConstants.PrinterErrorSoundKey,
                        false)
                },
                {
                    ApplicationConstants.PrinterWarningSoundKey,
                    Tuple.Create(
                        (object)configuration?.PrinterWarningSound?.FilePath,
                        ApplicationConstants.PrinterWarningSoundKey,
                        false)
                },
                {
                    ApplicationConstants.NoteAcceptorErrorSoundKey,
                    Tuple.Create(
                        (object)configuration.NoteAcceptorErrorSound?.FilePath,
                        ApplicationConstants.NoteAcceptorErrorSoundKey,
                        false)
                },
                {
                    ApplicationConstants.DiskSpaceMonitorErrorSoundKey,
                    Tuple.Create(
                        (object)configuration.DiskSpaceMonitorErrorSound?.FilePath,
                        ApplicationConstants.DiskSpaceMonitorErrorSoundKey,
                        false)
                },
                {
                    ApplicationConstants.FirmwareCrcErrorSoundKey,
                    Tuple.Create(
                        (object)configuration.FirmwareCrcErrorSound?.FilePath,
                        ApplicationConstants.FirmwareCrcErrorSoundKey,
                        false)
                },
                {
                    ApplicationConstants.LiveAuthenticationFailedSoundKey,
                    Tuple.Create(
                        (object)configuration?.LiveAuthenticationFailedSound?.FilePath,
                        ApplicationConstants.LiveAuthenticationFailedSoundKey,
                        false)
                },
                {
                    ApplicationConstants.TouchSoundKey,
                    Tuple.Create(
                        (object)configuration.TouchSound?.FilePath,
                        ApplicationConstants.TouchSoundKey,
                        false)
                },
                {
                    ApplicationConstants.CoinInSoundKey,
                    Tuple.Create(
                        (object)configuration.CoinInSound?.FilePath,
                        ApplicationConstants.CoinInSoundKey,
                        false)
                },
                {
                    ApplicationConstants.CoinOutSoundKey,
                    Tuple.Create(
                        (object)configuration.CoinOutSound?.FilePath,
                        ApplicationConstants.CoinOutSoundKey,
                        false)
                },
                {
                    ApplicationConstants.FeatureBellSoundKey,
                    Tuple.Create(
                        (object)configuration.FeatureBellSound?.FilePath,
                        ApplicationConstants.FeatureBellSoundKey,
                        false)
                },
                {
                    ApplicationConstants.CollectSoundKey,
                    Tuple.Create(
                        (object)configuration.CollectSound?.FilePath,
                        ApplicationConstants.CollectSoundKey,
                        false)
                },
                {
                    ApplicationConstants.HostOfflineSoundKey,
                    Tuple.Create(
                        (object)configuration.HostOfflineSound?.FilePath,
                        ApplicationConstants.HostOfflineSoundKey,
                        false)
                },
                {
                    ApplicationConstants.DingSoundKey,
                    Tuple.Create(
                        (object)configuration.DingSound?.FilePath,
                        ApplicationConstants.DingSoundKey,
                        false)
                },
                {
                    ApplicationConstants.TicketModeAuditKey,
                    Tuple.Create(
                        (object)configuration.TicketMode?.Audit ?? TicketModeAuditBehavior.Audit,
                        ApplicationConstants.TicketModeAuditKey,
                        false)
                },
                {
                    ApplicationConstants.OperatorLockupResetEnabled,
                    Tuple.Create(
                        (object)configuration.OperatorLockupReset?.Enabled ?? false,
                        ApplicationConstants.OperatorLockupResetEnabled,
                        false)
                },
                {
                    ApplicationConstants.ClockEnabled,
                    Tuple.Create(
                        (object)configuration.Clock?.Enabled ?? false,
                        ApplicationConstants.ClockEnabled,
                        false)
                },
                {
                    ApplicationConstants.ClockFormat,
                    Tuple.Create(
                        (object)configuration.Clock?.Format ?? 12,
                        ApplicationConstants.ClockFormat,
                        false)
                },
                {
                    ApplicationConstants.CashoutClearWins,
                    Tuple.Create(
                        (object)configuration.Cashout?.ClearWins ?? true,
                        ApplicationConstants.CashoutClearWins,
                        false)
                },
                {
                    ApplicationConstants.CommitStorageAfterCashout,
                    Tuple.Create(
                        (object)configuration.Cashout?.CommitStorageAfterCashout ?? false,
                        ApplicationConstants.CommitStorageAfterCashout,
                        false)
                },
                {
                    ApplicationConstants.DefaultBetAfterSwitch,
                    Tuple.Create(
                        (object)configuration.MultiGame?.DefaultBetAfterSwitch ?? true,
                        ApplicationConstants.DefaultBetAfterSwitch,
                        false)
                },
                {
                    ApplicationConstants.RestoreRebootStateAfterSwitch,
                    Tuple.Create(
                        (object)configuration.MultiGame?.RestoreRebootStateAfterSwitch ?? true,
                        ApplicationConstants.RestoreRebootStateAfterSwitch,
                        false)
                },
                {
                    ApplicationConstants.StateStorageLocation,
                    Tuple.Create(
                        (object)configuration.MultiGame?.StateStorageLocation ?? "gamePlayerSession",
                        ApplicationConstants.StateStorageLocation,
                        false)
                },
                {
                    ApplicationConstants.PlatformEnhancedDisplayEnabled,
                    Tuple.Create(
                        (object)configuration.PlatformEnhancedDisplay?.Enabled ?? true,
                        ApplicationConstants.PlatformEnhancedDisplayEnabled,
                        false)
                },
                {
                    SecondaryStorageConstants.SecondaryStorageRequired,
                    Tuple.Create(
                        (object)configuration.SecondaryStorageMedia?.Required ?? false,
                        SecondaryStorageConstants.SecondaryStorageRequired,
                        false)
                },
                {
                    ApplicationConstants.ReadOnlyMediaRequired,
                    Tuple.Create(
                        (object)configuration.ReadOnlyMedia?.Required ?? false,
                        ApplicationConstants.ReadOnlyMediaRequired,
                        false)
                },
                {
                    ApplicationConstants.CabinetTypeKey,
                    Tuple.Create(
                        (object)InitFromStorage(
                            ApplicationConstants.CabinetTypeKey,
                            (int)ServiceManager.GetInstance().GetService<ICabinetDetectionService>().Type),
                        ApplicationConstants.CabinetTypeKey,
                        true)
                },
                {
                    ApplicationConstants.BarCodeType,
                    Tuple.Create(
                        (object)InitFromStorage(
                            ApplicationConstants.BarCodeType, (int)barCodeType),
                        ApplicationConstants.BarCodeType,
                        true)
                },
                {
                    ApplicationConstants.HandpayReceiptPrintingEnabled,
                    Tuple.Create(
                        (object)configuration.HandpayReceiptPrinting?.Enabled ?? false,
                        ApplicationConstants.HandpayReceiptPrintingEnabled,
                        false)
                },
                {
                    ApplicationConstants.ValidationLength,
                    Tuple.Create(
                        (object)InitFromStorage(
                            ApplicationConstants.ValidationLength, (int)validationLength),
                        ApplicationConstants.ValidationLength,
                        true)
                },
                {
                    ApplicationConstants.LayoutType,
                    Tuple.Create(
                        (object)InitFromStorage(
                            ApplicationConstants.LayoutType, (int)layoutType),
                        ApplicationConstants.LayoutType,
                        true)
                },
                {
                    ApplicationConstants.ReserveServiceLockupPresent,
                    Tuple.Create(
                        (object)InitFromStorage(
                            ApplicationConstants.ReserveServiceLockupPresent,
                            reserveServiceLockupPresent),
                        ApplicationConstants.ReserveServiceLockupPresent,
                        true)
                },
                {
                    ApplicationConstants.ReserveServicePin,
                    Tuple.Create(
                        (object)InitFromStorage(
                            ApplicationConstants.ReserveServicePin,
                            reserveServicePin),
                        ApplicationConstants.ReserveServicePin,
                        true)
                },
                {
                    ApplicationConstants.ReserveServiceTimeoutInSeconds,
                    Tuple.Create(
                        (object)InitFromStorage(
                            ApplicationConstants.ReserveServiceTimeoutInSeconds,
                            reserveServiceTimeoutInSeconds),
                        ApplicationConstants.ReserveServiceTimeoutInSeconds,
                        true)
                },
                {
                    ApplicationConstants.ReserveServiceEnabled,
                    Tuple.Create(
                        (object)InitFromStorage(
                            ApplicationConstants.ReserveServiceEnabled,
                            reserveServiceEnabled),
                        ApplicationConstants.ReserveServiceEnabled,
                        true)
                },
                {
                    ApplicationConstants.ReserveServiceAllowed,
                    Tuple.Create(
                        (object)configuration.ReserveService?.Allowed ?? true,
                        ApplicationConstants.ReserveServiceAllowed,
                        true)
                },
                {
                    ApplicationConstants.ReserveServiceLockupRemainingSeconds,
                    Tuple.Create(
                        (object)InitFromStorage(
                            ApplicationConstants.ReserveServiceLockupRemainingSeconds,
                            reserveServiceLockupRemainingTime),
                        ApplicationConstants.ReserveServiceLockupRemainingSeconds,
                        false)
                },
                {
                    ApplicationConstants.WaitForProgressiveInitialization,
                    Tuple.Create(
                        (object)configuration.WaitForProgressiveInitialization?.Enabled ?? false,
                        ApplicationConstants.WaitForProgressiveInitialization,
                        false)
                },
                {
                    ApplicationConstants.ShowMasterResult,
                    Tuple.Create(
                        (object)configuration.SoftwareVerification?.ShowMasterResult ?? false,
                        ApplicationConstants.ShowMasterResult,
                        false)
                },
                {
                    ApplicationConstants.FirmwareCrcMonitorEnabled,
                    Tuple.Create(
                        (object)configuration.FirmwareCrcMonitor?.Enabled ?? false,
                        ApplicationConstants.FirmwareCrcMonitorEnabled,
                        false)
                },
                {
                    ApplicationConstants.FirmwareCrcMonitorSeed,
                    Tuple.Create(
                        (object)configuration.FirmwareCrcMonitor?.Seed ?? GdsConstants.DefaultSeed,
                        ApplicationConstants.FirmwareCrcMonitorSeed,
                        false)
                },
                {
                    ApplicationConstants.RunSignatureVerificationAfterReboot,
                    Tuple.Create(
                        (object)configuration.LiveAuthenticationManager?.RunSignatureVerificationAfterReboot ?? false,
                        ApplicationConstants.RunSignatureVerificationAfterReboot,
                        false)
                },
                {
                    ApplicationConstants.LowMemoryThreshold,
                    Tuple.Create(
                        (object)configuration.MemoryMonitor?.LowMemoryThreshold ?? ApplicationConstants.LowMemoryThresholdDefault,
                        ApplicationConstants.LowMemoryThreshold,
                        false)
                },
                {
                    ApplicationConstants.TopperDisplayDisconnectNoReconfigure,
                    Tuple.Create(
                        (object)configuration.DisplayDisconnectNoReconfigure?.Topper ?? false,
                        ApplicationConstants.TopperDisplayDisconnectNoReconfigure,
                        false)
                }
            };

            if (configuration.MediaDisplay != null)
            {
                _properties.Add(
                    ApplicationConstants.MediaDisplayEnabled,
                    Tuple.Create(
                        (object)mediaDisplayEnabled,
                        ApplicationConstants.MediaDisplayEnabled,
                        true));
            }

            if (configuration.DetailedAuditTickets != null)
            {
                _properties.Add(
                    ApplicationConstants.DetailedAuditTickets,
                    Tuple.Create(
                        (object)(configuration?.DetailedAuditTickets.Enabled ?? false),
                        ApplicationConstants.DetailedAuditTickets,
                        true));
            }

            if (configuration.Demonstration != null)
            {
                _properties.Add(
                    ApplicationConstants.DemonstrationModeEnabled,
                    Tuple.Create(
                        (object)configuration.Demonstration.Enabled,
                        ApplicationConstants.DemonstrationModeEnabled,
                        false));
            }

            if (configuration.LogTypesAllowedForDisplay != null)
            {
                _properties.Add(ApplicationConstants.LogTypesAllowedForDisplayKey,
                    Tuple.Create(
                        (object)configuration.LogTypesAllowedForDisplay,
                        ApplicationConstants.LogTypesAllowedForDisplayKey,
                        false));
            }

            if (configuration.MasterVolumeSettings != null)
            {
                _properties.Add(HardwareConstants.VolumePreset,
                    Tuple.Create(
                        (object)LoadVolumeLevel(configuration.MasterVolumeSettings),
                        HardwareConstants.VolumePreset,
                        false));
            }

            if (configuration.VolumeScalarSettings != null)
            {
                _properties.Add(HardwareConstants.VolumeScalarPreset,
                    Tuple.Create(
                        (object)LoadVolumeScalar(configuration.VolumeScalarSettings),
                        HardwareConstants.VolumeScalarPreset,
                        false));
            }


            SetPrinterLineLimits(configuration.AuditTicket);

            propertiesManager.AddPropertyProvider(this);

            if (!_blockExists && machineSettingsImported != ImportMachineSettings.None)
            {
                machineSettingsImported |= ImportMachineSettings.ApplicationConfigurationPropertiesLoaded;
                propertiesManager.SetProperty(ApplicationConstants.MachineSettingsImported, machineSettingsImported);
            }
        }

        private Dictionary<byte, Tuple<string,float>> LoadVolumeLevel(ApplicationConfigurationVolumeNode[] masterVolumeSettings)
        {
            var result = new Dictionary<byte, Tuple<string,float>>();
            foreach (var i in masterVolumeSettings)
            {
                result.Add(i.Key, new Tuple<string, float>(i.Description, i.Value));    
            }

            return result;
        }

        private Dictionary<VolumeScalar, float> LoadVolumeScalar(ApplicationConfigurationScalarNode[] VolumeScalarSettings)
        {
            var result = new Dictionary<VolumeScalar, float>();
            foreach (var i in VolumeScalarSettings)
            {
                result.Add((VolumeScalar)i.Key, i.Value); 
            }
            return result;
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

            var errorMessage = "Unknown application property: " + propertyName;
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

            if (value.Item1 != propertyValue)
            {
                if (!string.IsNullOrEmpty(value.Item2))
                {
                    Logger.Debug(
                        $"setting property {propertyName} to {propertyValue}. Type is {propertyValue.GetType()}");
                }

                //Item3 indicates persistence
                if (value.Item3)
                {
                    var accessor = GetAccessor();
                    accessor[value.Item2] = propertyValue;
                }

                _properties[propertyName] = Tuple.Create(propertyValue, value.Item2, value.Item3);
            }
        }

        private IPersistentStorageAccessor GetAccessor(string name = null, int blockSize = 1)
        {
            var blockName = GetBlockName(name);

            return _storageManager.BlockExists(blockName)
                ? _storageManager.GetBlock(blockName)
                : _storageManager.CreateBlock(Level, blockName, blockSize);
        }

        private string GetBlockName(string name = null)
        {
            var baseName = GetType().ToString();
            return string.IsNullOrEmpty(name) ? baseName : $"{baseName}.{name}";
        }

        private T InitFromStorage<T>(string propertyName, T defaultValue = default(T))
        {
            var accessor = GetAccessor();
            if (!_blockExists)
            {
                accessor[propertyName] = defaultValue;
            }

            return (T)accessor[propertyName];
        }

        private void SetPrinterLineLimits(ApplicationConfigurationAuditTicket auditTicket)
        {
            int eventsPerPage = 6;
            int lineLimit = 36;

            if (auditTicket?.PrinterProtocol != null)
            {
                if (auditTicket.PrinterProtocol == ServiceManager.GetInstance().TryGetService<IPrinter>()?.ServiceProtocol)
                {
                    lineLimit = auditTicket.LineLimit;
                    eventsPerPage = auditTicket.EventsPerPage;
                }
            }

            _properties.Add(ApplicationConstants.AuditTicketLineLimit, Tuple.Create((object)lineLimit, ApplicationConstants.AuditTicketLineLimit, false));
            _properties.Add(ApplicationConstants.AuditTicketEventsPerPage, Tuple.Create((object)eventsPerPage, ApplicationConstants.AuditTicketEventsPerPage, false));
        }
    }
}
