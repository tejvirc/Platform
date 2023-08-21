namespace Aristocrat.Monaco.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Contracts.Localization;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Gds;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Kernel.Contracts;
    using Kernel.MarketConfig;
    using Kernel.MarketConfig.Models.Application;
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

            var marketConfigManager = ServiceManager.GetInstance().GetService<IMarketConfigManager>();

            var configuration = marketConfigManager.GetMarketConfigForSelectedJurisdiction<ApplicationConfigSegment>();

            var deletePackageAfterInstall = configuration.DeletePackageAfterSoftwareInstall;
            var mediaDisplayEnabled = configuration.MediaDisplayEnabled;
            var defaultVolumeLevel = configuration.SoundConfiguration?.DefaultVolumeLevel;
            var defaultVolumeControlLocation = configuration.SoundConfiguration?.Location;
            var excessiveDocumentRejectCount = configuration.ExcessiveDocumentRejection?.ConsecutiveRejectsBeforeLockup ??
                                               ApplicationConstants.ExcessiveDocumentRejectDefaultCount;
            var excessiveDocumentRejectSoundFilePath = configuration.ExcessiveDocumentRejection?.SoundFilePath ?? string.Empty;
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
                mediaDisplayEnabled = propertiesManager.GetValue(ApplicationConstants.MediaDisplayEnabled, false);

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
                        (object)configuration.GeneralMessages.DisabledByOperator,
                        ApplicationConstants.DisabledByOperatorText,
                        false)
                },
                {
                    ApplicationConstants.LockupCulture,
                    Tuple.Create(
                        (object)configuration.GeneralMessages?.LockupCulture ?? CultureFor.Operator,
                        ApplicationConstants.LockupCulture,
                        false)
                },
                {
                    ApplicationConstants.NoteAcceptorErrorBillJamText,
                    Tuple.Create(
                        (object)configuration.NoteAcceptorErrorMessages?.BillJam,
                        ApplicationConstants.NoteAcceptorErrorBillJamText,
                        false)
                },
                {
                    ApplicationConstants.NoteAcceptorErrorBillStackerErrorText,
                    Tuple.Create(
                        (object)configuration.NoteAcceptorErrorMessages?.BillStackerError,
                        ApplicationConstants.NoteAcceptorErrorBillStackerErrorText,
                        false)
                },
                {
                    ApplicationConstants.NoteAcceptorErrorBillStackerFullText,
                    Tuple.Create(
                        (object)configuration.NoteAcceptorErrorMessages?.BillStackerFull,
                        ApplicationConstants.NoteAcceptorErrorBillStackerFullText,
                        false)
                },
                {
                    ApplicationConstants.NoteAcceptorErrorBillStackerJamText,
                    Tuple.Create(
                        (object)configuration.NoteAcceptorErrorMessages?.BillStackerJam,
                        ApplicationConstants.NoteAcceptorErrorBillStackerJamText,
                        false)
                },
                {
                    ApplicationConstants.NoteAcceptorErrorBillUnexpectedErrorText,
                    Tuple.Create(
                        (object)configuration.NoteAcceptorErrorMessages?.BillUnexpectedError,
                        ApplicationConstants.NoteAcceptorErrorBillUnexpectedErrorText,
                        false)
                },
                {
                    ApplicationConstants.NoteAcceptorErrorBillValidatorFaultText,
                    Tuple.Create(
                        (object)configuration.NoteAcceptorErrorMessages?.BillValidatorFault,
                        ApplicationConstants.NoteAcceptorErrorBillValidatorFaultText,
                        false)
                },
                {
                    ApplicationConstants.NoteAcceptorErrorInvalidBillText,
                    Tuple.Create(
                        (object)configuration.NoteAcceptorErrorMessages?.InvalidBill,
                        ApplicationConstants.NoteAcceptorErrorInvalidBillText,
                        false)
                },
                {
                    ApplicationConstants.NoteAcceptorErrorCashBoxRemovedText,
                    Tuple.Create(
                        (object)configuration.NoteAcceptorErrorMessages?.CashBoxRemoved,
                        ApplicationConstants.NoteAcceptorErrorCashBoxRemovedText,
                        false)
                },
                {
                    ApplicationConstants.NoteAcceptorErrorGeneralFailureText,
                    Tuple.Create(
                        (object)configuration.NoteAcceptorErrorMessages?.GeneralFailure,
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
                        (object)configuration.AutoClearPeriodMetersWhenRemovingStacker,
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
                        (object)configuration.AutoClearPeriodMetersBehavior?.MeterClearTime,
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
                        (object)configuration.ExcessiveDocumentRejection?.LockupType ?? ExcessiveDocumentRejectLockupType.Soft,
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
                        (object)configuration.ExcessiveDocumentRejection?.ConsecutiveRejectsBeforeLockup ?? -1,
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
                        (object)configuration.ExcessiveDocumentRejection?.ResetMethodKey ?? ResetMethodKeyType.MainDoor,
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
                        (object)configuration.PaperInChuteSoundFilePath,
                        ApplicationConstants.PaperInChuteSoundKey,
                        false)
                },
                {
                    ApplicationConstants.PrinterErrorSoundKey,
                    Tuple.Create(
                        (object)configuration.PrinterErrorSoundFilePath,
                        ApplicationConstants.PrinterErrorSoundKey,
                        false)
                },
                {
                    ApplicationConstants.PrinterWarningSoundKey,
                    Tuple.Create(
                        (object)configuration?.PrinterWarningSoundFilePath,
                        ApplicationConstants.PrinterWarningSoundKey,
                        false)
                },
                {
                    ApplicationConstants.NoteAcceptorErrorSoundKey,
                    Tuple.Create(
                        (object)configuration.NoteAcceptorErrorSoundFilePath,
                        ApplicationConstants.NoteAcceptorErrorSoundKey,
                        false)
                },
                {
                    ApplicationConstants.DiskSpaceMonitorErrorSoundKey,
                    Tuple.Create(
                        (object)configuration.DiskSpaceMonitorErrorSoundFilePath,
                        ApplicationConstants.DiskSpaceMonitorErrorSoundKey,
                        false)
                },
                {
                    ApplicationConstants.FirmwareCrcErrorSoundKey,
                    Tuple.Create(
                        (object)configuration.FirmwareCrcErrorSoundFilePath,
                        ApplicationConstants.FirmwareCrcErrorSoundKey,
                        false)
                },
                {
                    ApplicationConstants.LiveAuthenticationFailedSoundKey,
                    Tuple.Create(
                        (object)configuration?.LiveAuthenticationFailedSoundFilePath,
                        ApplicationConstants.LiveAuthenticationFailedSoundKey,
                        false)
                },
                {
                    ApplicationConstants.TouchSoundKey,
                    Tuple.Create(
                        (object)configuration.TouchSoundFilePath,
                        ApplicationConstants.TouchSoundKey,
                        false)
                },
                {
                    ApplicationConstants.CoinInSoundKey,
                    Tuple.Create(
                        (object)configuration.CoinInSoundFilePath,
                        ApplicationConstants.CoinInSoundKey,
                        false)
                },
                {
                    ApplicationConstants.CoinOutSoundKey,
                    Tuple.Create(
                        (object)configuration.CoinOutSoundFilePath,
                        ApplicationConstants.CoinOutSoundKey,
                        false)
                },
                {
                    ApplicationConstants.FeatureBellSoundKey,
                    Tuple.Create(
                        (object)configuration.FeatureBellSoundFilePath,
                        ApplicationConstants.FeatureBellSoundKey,
                        false)
                },
                {
                    ApplicationConstants.CollectSoundKey,
                    Tuple.Create(
                        (object)configuration.CollectSoundFilePath,
                        ApplicationConstants.CollectSoundKey,
                        false)
                },
                {
                    ApplicationConstants.HostOfflineSoundKey,
                    Tuple.Create(
                        (object)configuration.HostOfflineSoundFilePath,
                        ApplicationConstants.HostOfflineSoundKey,
                        false)
                },
                {
                    ApplicationConstants.DingSoundKey,
                    Tuple.Create(
                        (object)configuration.DingSoundFilePath,
                        ApplicationConstants.DingSoundKey,
                        false)
                },
                {
                    ApplicationConstants.TicketModeAuditKey,
                    Tuple.Create(
                        (object)configuration.TicketModeAuditBehavior,
                        ApplicationConstants.TicketModeAuditKey,
                        false)
                },
                {
                    ApplicationConstants.OperatorLockupResetEnabled,
                    Tuple.Create(
                        (object)configuration.OperatorLockupResetEnabled,
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
                        (object)configuration.PlatformEnhancedDisplayEnabled,
                        ApplicationConstants.PlatformEnhancedDisplayEnabled,
                        false)
                },
                {
                    SecondaryStorageConstants.SecondaryStorageRequired,
                    Tuple.Create(
                        (object)configuration.SecondaryStorageMediaRequired,
                        SecondaryStorageConstants.SecondaryStorageRequired,
                        false)
                },
                {
                    ApplicationConstants.ReadOnlyMediaRequired,
                    Tuple.Create(
                        (object)configuration.ReadOnlyMediaRequired,
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
                        (object)configuration.HandpayReceiptPrintingEnabled,
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
                        (object)configuration.WaitForProgressiveInitializationEnabled,
                        ApplicationConstants.WaitForProgressiveInitialization,
                        false)
                },
                {
                    ApplicationConstants.ShowMasterResult,
                    Tuple.Create(
                        (object)configuration.ShowMasterResultForSoftwareVerification,
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
                        (object)configuration.LiveAuthenticationManagerRunsSignatureVerificationAfterReboot,
                        ApplicationConstants.RunSignatureVerificationAfterReboot,
                        false)
                },
                {
                    ApplicationConstants.LowMemoryThreshold,
                    Tuple.Create(
                        (object)configuration.MemoryMonitorLowMemoryThreshold,
                        ApplicationConstants.LowMemoryThreshold,
                        false)
                },
                {
                    ApplicationConstants.ShowWagerWithLargeWinInfo,
                    Tuple.Create(
                        (object)configuration.LargeWinInfoShowsWager,
                        ApplicationConstants.ShowWagerWithLargeWinInfo,
                        false)
                },
                {
                    ApplicationConstants.TopperDisplayDisconnectNoReconfigure,
                    Tuple.Create(
                        (object)configuration.DisplayRatherThanRequireEgmReconfigureIfTopperDisconnected,
                        ApplicationConstants.TopperDisplayDisconnectNoReconfigure,
                        false)
                }
            };

            if (mediaDisplayEnabled)
            {
                _properties.Add(
                    ApplicationConstants.MediaDisplayEnabled,
                    Tuple.Create(
                        (object)mediaDisplayEnabled,
                        ApplicationConstants.MediaDisplayEnabled,
                        true));
            }

            if (configuration.DetailedAuditTicketsEnabled)
            {
                _properties.Add(
                    ApplicationConstants.DetailedAuditTickets,
                    Tuple.Create(
                        (object)(configuration.DetailedAuditTicketsEnabled),
                        ApplicationConstants.DetailedAuditTickets,
                        true));
            }

            if (configuration.DemonstrationEnabled)
            {
                _properties.Add(
                    ApplicationConstants.DemonstrationModeEnabled,
                    Tuple.Create(
                        (object)configuration.DemonstrationEnabled,
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

            SetPrinterLineLimits(configuration.AuditTicket);

            propertiesManager.AddPropertyProvider(this);

            if (!_blockExists && machineSettingsImported != ImportMachineSettings.None)
            {
                machineSettingsImported |= ImportMachineSettings.ApplicationConfigurationPropertiesLoaded;
                propertiesManager.SetProperty(ApplicationConstants.MachineSettingsImported, machineSettingsImported);
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

        private void SetPrinterLineLimits(AuditTicketFieldset auditTicket)
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
