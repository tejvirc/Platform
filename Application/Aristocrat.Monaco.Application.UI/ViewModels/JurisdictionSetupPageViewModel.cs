namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Extensions.CommunityToolkit;
    using CommunityToolkit.Mvvm.Input;
    using ConfigWizard;
    using Contracts;
    using Contracts.Drm;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Contracts.Settings;
    using Kernel;
    using Kernel.Contracts;
    using Models;
    using Monaco.Localization.Properties;
    using Monaco.UI.Common.Extensions;
    using OperatorMenu;
    using Views;

    [CLSCompliant(false)]
    public class JurisdictionSetupPageViewModel : ConfigWizardViewModelBase
    {
        private const string ShowModeJurisdictionId = "JUR000033";
        private const string ShowMode = "ShowMode";
        private const string GameRules = "GameRules";

        private string _selectedJurisdiction;
        private bool _isShowModeChecked;
        private bool _isGameRulesChecked;
        private bool _gameRulesEditable;

        private bool _isEKeyVerified;
        private bool _isEKeyDriveFound;
        private bool _isSuccess;
        private bool _isError;
        private bool _isImporting;
        private bool _isJurisdictionSelectionEnabled;
        private string _importSettingLabel;
        private readonly IConfigurationSettingsManager _settingsManager;
        private readonly IDialogService _dialogService;
        private CancellationTokenSource _cancellation;
        private bool _gameRulesVisible;

        public JurisdictionSetupPageViewModel() : base(true)
        {
            var jurisdictions = MonoAddinsHelper.GetSelectableConfigurationAddinNodes(ApplicationConstants.Jurisdiction);

            // If a jurisdiction ID was specified in license info, curtail the jurisdiction
            // selections to those that match the license data.
            var licensedJurisdictionId = string.Empty;
            var serviceManager = ServiceManager.GetInstance();
            var digitalRights = serviceManager.GetService<IDigitalRights>();
            licensedJurisdictionId = digitalRights.JurisdictionId;
            Logger.Debug($"JurisdictionId from DRM: '{licensedJurisdictionId}'");

            if (!string.IsNullOrEmpty(licensedJurisdictionId))
            {
                jurisdictions = jurisdictions.Where(j => j.Id.Contains(licensedJurisdictionId)).ToList();
            }

            Jurisdictions = new List<string>(jurisdictions.Select(j => j.Name).OrderBy(o => o));

            var jurisdiction = PropertiesManager.GetValue(ApplicationConstants.JurisdictionKey, string.Empty);

            if (!string.IsNullOrWhiteSpace(jurisdiction))
            {
                _selectedJurisdiction = Jurisdictions.FirstOrDefault(x => x == jurisdiction);
            }

            _selectedJurisdiction = _selectedJurisdiction ?? Jurisdictions.FirstOrDefault();

#if !(RETAIL)
            ShowModeVisible = true;
#else
            ShowModeVisible = licensedJurisdictionId == ShowModeJurisdictionId;
#endif
            _isShowModeChecked = licensedJurisdictionId == ShowModeJurisdictionId;
            _gameRulesVisible = ShowModeVisible;
            _isGameRulesChecked = !_isShowModeChecked;
            _isJurisdictionSelectionEnabled = true;

            _settingsManager = serviceManager.GetService<IConfigurationSettingsManager>();
            ImportCommand = new RelayCommand<object>(_ => Import(), _ => IsEKeyVerified && IsEKeyDriveFound && IsMachineConfigFound && !IsImporting);
            _importSettingLabel = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportMachineSettingsInsertEKey);

            _dialogService = serviceManager.GetService<IDialogService>();
        }

        protected override void Loaded()
        {
            EventBus.Subscribe<PropertyChangedEvent>(
                this,
                Handle,
                evt =>
                    evt.PropertyName == ApplicationConstants.EKeyVerified ||
                    evt.PropertyName == ApplicationConstants.EKeyDrive);

            EventBus.Subscribe<ConfigurationSettingsImportedEvent>(this, Handle);
            EventBus.Subscribe<ConfigurationSettingsSummaryEvent>(this, Handle);

            IsEKeyVerified = PropertiesManager.GetValue(ApplicationConstants.EKeyVerified, false);
            IsEKeyDriveFound = PropertiesManager.GetValue<string>(ApplicationConstants.EKeyDrive, null) != null;

            _cancellation = new CancellationTokenSource();

            if (IsWizardPage &&
                PropertiesManager.GetValue(ApplicationConstants.MachineSettingsReimport, false) &&
                !PropertiesManager.GetValue(ApplicationConstants.MachineSettingsReimported, false))
            {
                if (CanImport)
                {
                    ReimportMachineSettings();
                }
                else
                {
                    var viewModel = new ConfigurationSavePopupViewModel(
                        this,
                        () => CanImport,
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportSettingsLabel),
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportMachineSettingsReImport),
                        true
                    );

                    var result = _dialogService.ShowDialog<ConfigurationSavePopupPage>(
                        this,
                        viewModel,
                        string.Empty,
                        DialogButton.Save | DialogButton.Cancel,
                        new DialogButtonCustomText
                        {
                            { DialogButton.Save, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Import) },
                            { DialogButton.Cancel, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Cancel) }
                        });

                    if (!result ?? false)
                    {
                        EventBus.Publish(new ExitRequestedEvent(ExitAction.Restart));
                        return;
                    }

                    ReimportMachineSettings();
                }
            }
        }

        public List<string> Jurisdictions { get; }

        public string SelectedJurisdiction
        {
            get => _selectedJurisdiction;
            set
            {
                _selectedJurisdiction = value;
                OnPropertyChanged(nameof(SelectedJurisdiction));
            }
        }

        public bool ShowModeVisible { get; }

        public bool IsShowModeChecked
        {
            get => _isShowModeChecked;
            set
            {
                _isShowModeChecked = value;
                GameRulesEditable = _isShowModeChecked;
                OnPropertyChanged(nameof(IsShowModeChecked));

                PropertiesManager.SetProperty(ApplicationConstants.ShowMode, _isShowModeChecked);
            }
        }

        /// <summary>
        /// Whether or not the game rules should be shown.
        /// </summary>
        public bool GameRulesVisible
        {
            get => _gameRulesVisible;

            set
            {
                _gameRulesVisible = value;
                OnPropertyChanged(nameof(GameRulesVisible));
            }
        }

        /// <summary>
        /// Whether or not the game rules should be allowed to be edited.
        /// </summary>
        public bool GameRulesEditable
        {
            get => _gameRulesEditable;

            set
            {
                if (value && IsJurisdictionSelectionEnabled)
                {
                    _gameRulesEditable = true;
                    IsGameRulesChecked = false;
                }
                else
                {
                    _gameRulesEditable = false;
                    IsGameRulesChecked = true;
                }
                OnPropertyChanged(nameof(GameRulesEditable));
            }
        }

        /// <summary>
        /// The game rules should be enabled if show mode is off.
        /// </summary>
        public bool IsGameRulesChecked
        {
            get => _isGameRulesChecked;
            set
            {
                _isGameRulesChecked = value;
                OnPropertyChanged(nameof(IsGameRulesChecked));

                PropertiesManager.SetProperty(ApplicationConstants.GameRules, _isGameRulesChecked);
            }
        }

        public bool IsJurisdictionSelectionEnabled
        {
            get => _isJurisdictionSelectionEnabled;
            set
            {
                _isJurisdictionSelectionEnabled = value;
                OnPropertyChanged(nameof(IsJurisdictionSelectionEnabled));
            }
        }

        protected override void SaveChanges()
        {
            SetAddinConfigProperty(ApplicationConstants.Jurisdiction, SelectedJurisdiction);

            Logger.InfoFormat($"Jurisdiction={SelectedJurisdiction}");

            PropertiesManager.SetProperty(ApplicationConstants.JurisdictionKey, SelectedJurisdiction);
            PropertiesManager.SetProperty(ApplicationConstants.ShowMode, IsShowModeChecked);
            PropertiesManager.SetProperty(ApplicationConstants.GameRules, IsGameRulesChecked);
        }

        protected override void SetupNavigation()
        {
            WizardNavigator.CanNavigateBackward = false;
            WizardNavigator.CanNavigateForward = Jurisdictions.Any();
        }

        protected override void LoadAutoConfiguration()
        {
            var jurisdiction = string.Empty;
            AutoConfigurator.GetValue(ApplicationConstants.Jurisdiction, ref jurisdiction);
            SelectedJurisdiction = jurisdiction;

            string value = null;
            AutoConfigurator.GetValue(GameRules, ref value);
            if (value != null && bool.TryParse(value, out var gameRules))
            {
                IsGameRulesChecked = gameRules;
            }

            if (ShowModeVisible)
            {
                value = null;
                AutoConfigurator.GetValue(ShowMode, ref value);
                if (value != null && bool.TryParse(value, out var showMode))
                {
                    IsShowModeChecked = showMode;
                }
            }

            base.LoadAutoConfiguration();
        }

        public string ImportSettingLabel
        {
            get => _importSettingLabel;
            set
            {
                _importSettingLabel = value;
                OnPropertyChanged(nameof(ImportSettingLabel));
            }
        }

        public ObservableCollection<ConfigurationSetting> ConfigurationSettings { get; } = new ObservableCollection<ConfigurationSetting>();

        public RelayCommand<object> ImportCommand { get; }

        /// <summary>
        ///     Gets a value that indicates if the import is in progress.
        /// </summary>
        public bool IsImporting
        {
            get => _isImporting;

            set
            {
                SetProperty(ref _isImporting, value);
                ImportCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Gets a value that indicates whether the import was successful.
        /// </summary>
        public bool IsSuccess
        {
            get => _isSuccess;

            set => SetProperty(ref _isSuccess, value);
        }

        /// <summary>
        ///     Gets a value that indicates whether the import failed.
        /// </summary>
        public bool IsError
        {
            get => _isError;

            set => SetProperty(ref _isError, value);
        }

        /// <summary>
        ///     Gets a value that indicates whether the EKey has been verified.
        /// </summary>
        public bool IsEKeyVerified
        {
            get => _isEKeyVerified;
            set
            {
                SetProperty(ref _isEKeyVerified, value);
                ImportCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Gets a value that indicates whether the EKey drive has been found.
        /// </summary>
        public bool IsEKeyDriveFound
        {
            get => _isEKeyDriveFound;

            set
            {
                SetProperty(ref _isEKeyDriveFound, value);
                ImportCommand.NotifyCanExecuteChanged();
            }
        }

        public bool IsMachineConfigFound => _settingsManager.IsConfigurationImportFilePresent(ConfigurationGroup.Machine);

        private bool CanImport => IsEKeyVerified && IsEKeyDriveFound && IsMachineConfigFound && !IsImporting;

        private void Import()
        {
            var viewModel = new ConfigurationSavePopupViewModel(
                this,
                () => CanImport,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportSettingsLabel),
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportMachineSettings),
                true
                );

            var result = _dialogService.ShowDialog<ConfigurationSavePopupPage>(
                this,
                viewModel,
                string.Empty,
                DialogButton.Save | DialogButton.Cancel,
                new DialogButtonCustomText
                {
                    { DialogButton.Save, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Yes) },
                    { DialogButton.Cancel, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.No) }
                });

            if (!result ?? false)
            {
                return;
            }

            PropertiesManager.SetProperty(ApplicationConstants.MachineSettingsReimport, false);
            IsJurisdictionSelectionEnabled = false;
            IsImporting = true;
            IsError = false;
            ConfigurationSettings.Clear();

            Task.Run(async () => await _settingsManager.Import(ConfigurationGroup.Machine), _cancellation.Token)
                .ContinueWith(
                    task => task.Exception?.Handle(
                        ex =>
                        {
                            IsJurisdictionSelectionEnabled = true;
                            IsImporting = false;
                            IsError = true;
                            IsSuccess = false;
                            Logger.Error(
                                $"Machine settings import failed, {ex.InnerException?.Message ?? ex.Message}",
                                ex);
                            ImportResult(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportFailed),
                                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportFailedMachineSettings));
                            return true;
                        }),
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted,
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void ReimportMachineSettings()
        {
            IsImporting = true;
            IsError = false;

            Task.Run(async () => await _settingsManager.Import(ConfigurationGroup.Machine), _cancellation.Token)
                .ContinueWith(
                    task => task.Exception?.Handle(
                        ex =>
                        {
                            IsImporting = false;
                            IsError = true;
                            Logger.Error(
                                $"Machine settings re-import failed, {ex.InnerException?.Message ?? ex.Message}",
                                ex);
                            ImportResult(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportFailed),
                                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportFailedMachineSettings));
                            return true;
                        }),
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted,
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void Handle(PropertyChangedEvent obj)
        {
            Execute.OnUIThread(
                () =>
                {
                    IsEKeyVerified = PropertiesManager.GetValue(ApplicationConstants.EKeyVerified, false);
                    IsEKeyDriveFound = PropertiesManager.GetValue<string>(ApplicationConstants.EKeyDrive, null) != null;
                    ImportSettingLabel = IsEKeyVerified && IsEKeyDriveFound && IsMachineConfigFound
                        ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportMachineSettingsDetected)
                        : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportMachineSettingsInsertEKey);
                });
        }

        private void Handle(ConfigurationSettingsImportedEvent evt)
        {
            Execute.OnUIThread(
                () =>
                {
                    IsImporting = false;

                    if (!PropertiesManager.GetValue(ApplicationConstants.MachineSettingsReimport, false))
                    {
                        if (IsError)
                        {
                            IsSuccess = false;
                            ImportResult(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportFailed),
                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportFailedMachineSettings));
                        }
                        else
                        {
                            PropertiesManager.SetProperty(ApplicationConstants.MachineSettingsReimport, true);
                            PropertiesManager.SetProperty(ApplicationConstants.MachineSettingsReimported, true);
                            RequestConfigurationSettingSummary();
                        }
                    }
                    else
                    {
                        IsJurisdictionSelectionEnabled = false;
                        PropertiesManager.SetProperty(ApplicationConstants.MachineSettingsReimported, true);

                        _isShowModeChecked = PropertiesManager.GetValue(ApplicationConstants.ShowMode, false);
                        OnPropertyChanged(nameof(IsShowModeChecked));

                        _isGameRulesChecked = PropertiesManager.GetValue(ApplicationConstants.GameRules, true);
                        OnPropertyChanged(nameof(IsGameRulesChecked));
                    }
                });
        }

        private void RequestConfigurationSettingSummary()
        {
            ConfigurationSettings.Clear();
            Task.Run(async () => await _settingsManager.Summary(ConfigurationGroup.Machine), _cancellation.Token)
                .ContinueWith(
                    task => task.Exception?.Handle(
                        ex =>
                        {
                            Logger.Error(
                                $"Machine settings Summary failed, {ex.InnerException?.Message ?? ex.Message}",
                                ex);
                            return true;
                        }),
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted,
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void Handle(ConfigurationSettingsSummaryEvent evt)
        {
            Execute.OnUIThread(
                () =>
                {
                    ConfigurationSettings.AddRange(
                        evt.Settings.Select(x => new ConfigurationSetting { Name = x.Key, Settings = x.Value }));

                    IsSuccess = true;

                    var jurisdiction = PropertiesManager.GetValue(ApplicationConstants.JurisdictionKey, string.Empty);
                    if (!string.IsNullOrWhiteSpace(jurisdiction))
                    {
                        SelectedJurisdiction = Jurisdictions.FirstOrDefault(x => x == jurisdiction) ?? Jurisdictions.FirstOrDefault();
                    }

                    _isShowModeChecked = PropertiesManager.GetValue(ApplicationConstants.ShowMode, IsShowModeChecked);
                    OnPropertyChanged(nameof(IsShowModeChecked));

                    _isGameRulesChecked = PropertiesManager.GetValue(ApplicationConstants.GameRules, IsGameRulesChecked);
                    OnPropertyChanged(nameof(IsGameRulesChecked));

                    var viewModel = new ConfigurationSettingSummaryPopupViewModel(
                        ConfigurationSettings,
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportCompleteText),
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportMachineSettingsOverwrite),
                        true
                    );

                    _dialogService.ShowDialog<ConfigurationSettingSummaryPopupView>(
                        this,
                        viewModel,
                        string.Empty,
                        DialogButton.Cancel,
                        new DialogButtonCustomText
                        {
                            { DialogButton.Cancel, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Close) }
                        });
                });
        }
    }
}
