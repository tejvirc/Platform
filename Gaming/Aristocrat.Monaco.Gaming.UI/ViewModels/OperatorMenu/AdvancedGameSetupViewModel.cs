namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Drm;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.Contracts.Settings;
    using Application.UI.OperatorMenu;
    using Commands;
    using Common;
    using Contracts;
    using Contracts.Configuration;
    using Contracts.Events.OperatorMenu;
    using Contracts.Meters;
    using Contracts.Models;
    using Contracts.Progressives;
    using Contracts.Progressives.SharedSap;
    using Kernel;
    using Localization.Properties;
    using Microsoft.Expression.Interactivity.Core;
    using Models;
    using MVVM;
    using MVVM.Command;
    using Progressives;
    using Settings;
    using Views.OperatorMenu;

    public class AdvancedGameSetupViewModel : OperatorMenuSaveViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IGameProvider _gameProvider;
        private readonly IProgressiveConfigurationProvider _progressiveConfiguration;
        private readonly IGameConfigurationProvider _gameConfiguration;
        private readonly IConfigurationProvider _restrictionProvider;
        private readonly ILinkedProgressiveProvider _linkedProgressiveProvider;
        private readonly IConfigurationSettingsManager _settingsManager;
        private readonly IGameService _gameService;
        private readonly IDigitalRights _digitalRights;

        private readonly double _denomMultiplier;
        private readonly bool _enableRtpScaling;

        private readonly IDictionary<string, EditableGameProfile> _editableGames;
        private readonly IDictionary<GameType, List<EditableGameProfile>> _gamesMapping;
        private readonly object _gamesMappingLock = new object();
        private readonly IDictionary<GameType, List<EditableGameConfiguration>> _editableGameConfigByGameTypeMapping;

        private readonly IDictionary<(GameType gameType, string subType), List<EditableGameConfiguration>>
            _editableGameConfigBySubGameTypeMapping;

        private readonly IDictionary<GameType, HashSet<long>> _gameTypeToActiveDenomMapping;

        private readonly IDictionary<(GameType gameType, string subType), HashSet<long>>
            _subGameTypeToActiveDenomMapping;

        private readonly List<IViewableProgressiveLevel> _cachedConfigProgressiveLevels;

        private readonly List<(IViewableSharedSapLevel assignedLevel, ProgressiveSharedLevelSettings settings)>
            _cachedConfigSharedSapLevels;

        private bool _isError;
        private bool _isInProgress;
        private CancellationTokenSource _cancellation;
        private EditableGameConfiguration _selectedConfig;
        private EditableGameProfile _selectedGame;
        private ProgressiveSettings _progressiveSettings;
        private GameType _selectedGameType;
        private long _topAwardValue;
        private bool _gameOptionsGridEnabled;
        private bool _canEdit;
        private string _readOnlyStatus;
        private bool _resetScrollIntoView;
        private IDictionary<string, object> _pendingImportSettings = new Dictionary<string, object>();
        private ObservableCollection<EditableGameProfile> _games = new();
        private long _maxBetLimit;

        private string _saveWarningText = string.Empty; 

        public AdvancedGameSetupViewModel()
        {
            if (!InDesigner)
            {
                _dialogService = ServiceManager.GetInstance().GetService<IDialogService>();
            }

            ImportCommand = new ActionCommand<object>(
                _ => Import(),
                _ => CanExecuteImportCommand);
            ExportCommand = new ActionCommand<object>(_ => Export(), _ => CanExecuteExportCommand);
            ConfigCommand = new ActionCommand(EnterConfig);

            ProgressiveSetupCommand = new ActionCommand<object>(ProgressiveSetup);
            ProgressiveViewCommand = new ActionCommand<object>(ProgressiveView);
            ShowRtpSummaryCommand = new ActionCommand(ShowRtpSummary);
            ShowProgressiveSummaryCommand = new ActionCommand(ShowProgressiveSummary);

            ImportExportVisible = GetConfigSetting(OperatorMenuSetting.AllowImportExport, false);
            GlobalOptionsVisible = GetConfigSetting(OperatorMenuSetting.ShowGlobalOptions, false);
            _enableRtpScaling = GetConfigSetting(OperatorMenuSetting.EnableRtpScaling, false);
            ShowGameRtpAsRange = GetGlobalConfigSetting(OperatorMenuSetting.ShowGameRtpAsRange, true);
            _gameProvider = ServiceManager.GetInstance().GetService<IGameProvider>();
            _gameService = ServiceManager.GetInstance().GetService<IGameService>();
            _denomMultiplier = PropertiesManager.GetValue(ApplicationConstants.CurrencyMultiplierKey, 1d);
            _gamesMapping = new Dictionary<GameType, List<EditableGameProfile>>();
            _editableGames = new Dictionary<string, EditableGameProfile>();
            _editableGameConfigByGameTypeMapping = new Dictionary<GameType, List<EditableGameConfiguration>>();
            _editableGameConfigBySubGameTypeMapping =
                new Dictionary<(GameType gameType, string subType), List<EditableGameConfiguration>>();
            _gameTypeToActiveDenomMapping = new Dictionary<GameType, HashSet<long>>();
            _subGameTypeToActiveDenomMapping = new Dictionary<(GameType gameType, string subType), HashSet<long>>();
            _progressiveConfiguration = ServiceManager.GetInstance().GetService<IProgressiveConfigurationProvider>();
            _linkedProgressiveProvider = ServiceManager.GetInstance().GetService<ILinkedProgressiveProvider>();
            _gameConfiguration = ServiceManager.GetInstance().GetService<IGameConfigurationProvider>();
            _restrictionProvider = ServiceManager.GetInstance().GetService<IConfigurationProvider>();

            _digitalRights = ServiceManager.GetInstance().GetService<IDigitalRights>();

            _cachedConfigProgressiveLevels = new List<IViewableProgressiveLevel>();
            _cachedConfigSharedSapLevels = new List<(IViewableSharedSapLevel, ProgressiveSharedLevelSettings)>();

            var games = _gameProvider.GetGames().OrderBy(g => g.ThemeName);

            GameTypes = new List<GameType>(
                games.Select(g => g.GameType).OrderBy(g => g.GetDescription(typeof(GameType))).Distinct());
            SelectedGameType = GameTypes.FirstOrDefault();
            _settingsManager = ServiceManager.GetInstance().GetService<IConfigurationSettingsManager>();

            CancelButtonText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExitConfigurationText);
        }

        public ICommand ShowRtpSummaryCommand { get; }

        public ICommand ShowProgressiveSummaryCommand { get; }

        public ICommand ConfigCommand { get; }

        public ActionCommand<object> ImportCommand { get; }

        public ActionCommand<object> ExportCommand { get; }

        public ICommand ProgressiveSetupCommand { get; }

        public ICommand ProgressiveViewCommand { get; }

        public string ReadOnlyStatus
        {
            get => _readOnlyStatus;
            set => SetProperty(ref _readOnlyStatus, value, nameof(ReadOnlyStatus), nameof(ThemePlusOptions));
        }

        public bool IsInProgress
        {
            get => _isInProgress;
            set
            {
                SetProperty(ref _isInProgress, value);
                MvvmHelper.ExecuteOnUI(
                    () =>
                    {
                        ExportCommand.RaiseCanExecuteChanged();
                        ImportCommand.RaiseCanExecuteChanged();

                        RaisePropertyChanged(nameof(CanExecuteImportCommand));
                        RaisePropertyChanged(nameof(CanExecuteExportCommand));
                    });
            }
        }

        public bool IsError
        {
            get => _isError;

            set => SetProperty(ref _isError, value);
        }

        public bool InitialConfigComplete => PropertiesManager.GetValue(GamingConstants.OperatorMenuGameConfigurationInitialConfigComplete, false);

        public override bool CanSave => !HasErrors && InputEnabled && !Committed &&
                                        (HasChanges() || !InitialConfigComplete) && !IsEnabledGamesLimitExceeded &&
                                        !_editableGames.Any(g => g.Value.HasErrors);

        public bool ShowSaveButtonOverride => ShowSaveButton && IsInEditMode;

        public bool ConfigureVisible => !ShowSaveButtonOverride && _canEdit;

        public bool ShowCancelButtonOverride => ShowCancelButton && IsInEditMode && InitialConfigComplete;

        public bool IsInEditMode { get; private set; }

        public bool ShowSummaryButtons => !IsInEditMode;

        public bool ImportExportVisible { get; }

        public bool ImportVisibleOverride => ImportExportVisible && GameOptionsGridEnabled;

        public bool ExportVisibleOverride => ImportExportVisible && !GameOptionsGridEnabled;

        public bool HasMultipleGames => GameTypes.Count() > 1 || Games.Count > 1;

        public bool IsRouletteGameSelected => SelectedGameType == GameType.Roulette;

        public bool IsPokerGameSelected => SelectedGameType == GameType.Poker;

        public bool OptionColumnVisible => GlobalOptionsVisible && !IsRouletteGameSelected;

        // ReSharper disable once MemberCanBePrivate.Global - used by xaml
        public bool GlobalOptionsVisible { get; }

        public bool GambleOptionVisible => GlobalOptionsVisible && SelectedGameType != GameType.Blackjack &&
                                           SelectedGameType != GameType.Keno && !IsRouletteGameSelected;

        public bool LetItRideOptionVisible => GlobalOptionsVisible && SelectedGameType == GameType.Blackjack;

        public bool CanExecuteImportCommand => CanExecuteExportCommand && _settingsManager.IsConfigurationImportFilePresent(ConfigurationGroup.Game);

        public bool CanExecuteExportCommand => !IsInProgress && FieldAccessEnabled && (FieldAccessRestriction == OperatorMenuAccessRestriction.None);

        public bool ShowGameRtpAsRange { get; }

        public IEnumerable<GameType> GameTypes { get; }

        public GameType SelectedGameType
        {
            get => _selectedGameType;
            set
            {
                lock (_gamesMappingLock)
                {
                    if (!_gamesMapping.ContainsKey(value) || !_gamesMapping[value].Any())
                    {
                        return;
                    }
                }

                SetProperty(ref _selectedGameType, value);
                ApplySelectedGameType();
                ResetScrollIntoView = true;
                ResetScrollIntoView = false;
                UpdateInputStatusText();
            }
        }

        public ObservableCollection<EditableGameProfile> Games
        {
            get => _games;
            set => SetProperty(ref _games, value, nameof(Games), nameof(HasMultipleGames));
        }

        public EditableGameConfiguration SelectedConfig
        {
            get => _selectedConfig;
            set => SetProperty(ref _selectedConfig, value);
        }

        public EditableGameProfile SelectedGame
        {
            get => _selectedGame;
            set
            {
                if (_selectedGame == value)
                {
                    return;
                }

                _selectedGame = value;
                UpdateInputStatusText();
                RaisePropertyChanged(nameof(SelectedGame), nameof(GameConfigurations), nameof(ThemePlusOptions), nameof(SelectedDenoms));
                if (_selectedGame == null)
                {
                    return;
                }

                UpdateRestrictions();
                ApplyGameOptionsEnabled();
                ResetScrollIntoView = true;
                ResetScrollIntoView = false;
            }
        }

        public IEnumerable<EditableGameConfiguration> GameConfigurations =>
            SelectedGame?.GameConfigurations
            .Where(c => c.Active)
            .OrderBy(c => c.Denom);

        public string ThemePlusOptions => $"{SelectedGame?.ThemeName} {Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameOptions)} {ReadOnlyStatus}";

        public bool HasTopAward => _topAwardValue > 0;

        public long TopAwardValue
        {
            get => _topAwardValue;
            set => SetProperty(ref _topAwardValue, value, nameof(TopAwardValue), nameof(HasTopAward));
        }

        public bool GameOptionsGridEnabled
        {
            get => _gameOptionsGridEnabled;
            set => SetProperty(
                ref _gameOptionsGridEnabled,
                value,
                nameof(GameOptionsGridEnabled),
                nameof(GameOptionsEnabled));
        }

        public bool GameOptionsEnabled => GameOptionsGridEnabled && InputEnabled;

        public IEnumerable<IConfigurationRestriction> ConfiguredRestrictions =>
            SelectedGame?.Restrictions ?? Enumerable.Empty<IConfigurationRestriction>();

        public IEnumerable<IConfigurationRestriction> ValidRestrictions =>
            SelectedGame?.ValidRestrictions ?? Enumerable.Empty<IConfigurationRestriction>();

        public IConfigurationRestriction SelectedRestriction
        {
            get => SelectedGame?.SelectedRestriction;
            set
            {
                if (SelectedGame is null)
                {
                    return;
                }

                SelectedGame.SelectedRestriction = value;
                RaisePropertyChanged(nameof(SelectedRestriction));

                SetRestriction(value);

                RaisePropertyChanged(nameof(GameConfigurations));
                RaisePropertyChanged(nameof(CanSave));
            }
        }

        public bool ShowRestrictionChooser => ConfiguredRestrictions.Any() && !DenomSelectionLimitExists;

        public bool DenomSelectionLimitExists => ConfiguredRestrictions.Any() && ConfiguredRestrictions.Any(r => r.RestrictionDetails?.MaxDenomsEnabled != null);

        public int? DenomSelectionLimit => ConfiguredRestrictions.FirstOrDefault(r => r.RestrictionDetails?.MaxDenomsEnabled != null)?.RestrictionDetails.MaxDenomsEnabled;

        public string SelectedDenoms => $"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DenomsSelected)} {SelectedGame?.EnabledGameConfigurationsCount ?? 0} of {DenomSelectionLimit}";

        public bool ResetScrollIntoView
        {
            get => _resetScrollIntoView;
            set => SetProperty(ref _resetScrollIntoView, value);
        }

        public bool IsEnabledGamesLimitExceeded => TotalEnabledGames > _digitalRights.LicenseCount;

        public int TotalEnabledGames
        {
            get
            {
                lock (_gamesMappingLock)
                {
                    return _gamesMapping.Values
                        .SelectMany(m => m)
                        .Count(m => m.Enabled);
                }
            }
        }

        public string SaveWarningText
        {
            get => _saveWarningText;
            set => SetProperty(ref _saveWarningText, value);
        }
        public void HandlePropertyChangedEvent(PropertyChangedEvent eventObject)
        {
            MvvmHelper.ExecuteOnUI(
            () =>
            {
                ImportCommand.RaiseCanExecuteChanged();
                ExportCommand.RaiseCanExecuteChanged();

                RaisePropertyChanged(nameof(CanExecuteImportCommand));
                RaisePropertyChanged(nameof(CanExecuteExportCommand));
            });
        }

        public override bool HasChanges()
        {
            lock (_gamesMappingLock)
            {
                return _gamesMapping.Values.SelectMany(gameProfiles => gameProfiles)
                    .Any(gameProfile => gameProfile.HasChanges());
            }
        }

        public override void Save()
        {
            if (GameOptionsEnabled)
            {
                if (IsGameRecoveryNeeded()) // VLT-9241 happens only on a power reset recovery
                {
                    ShowPopup(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CannotChangeGameDuringPlay));
                    Logger.Info(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CannotChangeGameDuringPlay));
                    return;
                }

                var dialogService = ServiceManager.GetInstance().GetService<IDialogService>();
                var viewModel = new AdvancedGameConfigurationSavePopupViewModel(
                    this,
                    () => CanSave,
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameConfigurationSavePrompt),
                    GetWarningText());

                var result = dialogService.ShowDialog<AdvancedGameConfigurationSavePopupView>(
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

                dialogService.ShowDialog<AdvancedGameConfigurationSavingPopupView>(
                    this,
                    new AdvancedGameConfigurationSavingPopupViewModel(
                        () =>
                        {
                            SaveChanges();
                            CalculateTopAward();
                            IsInEditMode = false;
                            EventBus.Publish(new GameConfigurationSaveCompleteEvent());
                            _pendingImportSettings.Clear();
                            FieldAccessStatusText = string.Empty;
                            SetEditMode();
                            UpdateRestrictions();
                        },
                        dialogService),
                    string.Empty,
                    DialogButton.None);
            }
            else
            {
                SetEditMode();
            }
        }

        protected override void OnInputEnabledChanged()
        {
            RaisePropertyChanged(nameof(GameOptionsEnabled));
            base.OnInputEnabledChanged();
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            IsInProgress = false;

            _cancellation = new CancellationTokenSource();

            EventBus.Subscribe<ConfigurationSettingsImportedEvent>(this, _ => MvvmHelper.ExecuteOnUI(HandleImported));
            EventBus.Subscribe<ConfigurationSettingsExportedEvent>(this, _ => MvvmHelper.ExecuteOnUI(HandleExported));
            EventBus.Subscribe<PropertyChangedEvent>(
                this,
                HandlePropertyChangedEvent,
                evt =>
                    evt.PropertyName is ApplicationConstants.EKeyVerified or ApplicationConstants.EKeyDrive);

            _canEdit = GetConfigSetting(OperatorMenuSetting.EnableAdvancedConfig, false);
            IsInEditMode = _canEdit && !InitialConfigComplete;

            SetEditMode();
            lock (_gamesMapping)
            {
                AutoEnableGames();
            }
            UpdateSaveWarning();
        }

        protected override void InitializeData()
        {
            base.InitializeData();
            lock (_gamesMapping)
            {
                LoadGames();
            }
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();

            if (_pendingImportSettings.Any())
            {
                FieldAccessStatusText = string.Empty;
                ResetChanges();
                _pendingImportSettings.Clear();
            }
        }

        protected override void OnFieldAccessEnabledChanged()
        {
            base.OnFieldAccessEnabledChanged();
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    ImportCommand.RaiseCanExecuteChanged();
                    ExportCommand.RaiseCanExecuteChanged();

                    RaisePropertyChanged(nameof(CanExecuteImportCommand));
                    RaisePropertyChanged(nameof(CanExecuteExportCommand));
                });
        }

        protected override void DisposeInternal()
        {
            if (_cancellation != null)
            {
                _cancellation.Dispose();
                _cancellation = null;
            }

            ClearEditableGames();

            base.DisposeInternal();
        }

        protected override void OnInputStatusChanged()
        {
            UpdateInputStatusText();
            if (_selectedGame != null)
            {
                ApplyGameOptionsEnabled();
            }
        }

        protected override void Cancel()
        {
            if (!IsInEditMode)
            {
                return;
            }

            FieldAccessStatusText = string.Empty;
            ResetChanges();
            IsInEditMode = false;
            _pendingImportSettings.Clear();
            SetEditMode();
        }

        private static bool IsGameRecoveryNeeded()
        {
            return ServiceManager.GetInstance().TryGetService<IContainerService>()?.Container
                .GetInstance<IGameHistory>().IsRecoveryNeeded ?? false;
        }

        private static string BuildImportConfirmationText()
        {
            var text = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportGameSettingsDetected)
                       + Environment.NewLine + Environment.NewLine
                       + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportGameSettingsOverwrite)
                           .Replace("\\r\\n", Environment.NewLine);

            return text;
        }

        private void LoadGames()
        {
            _maxBetLimit = ((long)PropertiesManager.GetProperty(
                    AccountingConstants.MaxBetLimit,
                    AccountingConstants.DefaultMaxBetLimit))
                .MillicentsToCents();

            lock (_gamesMappingLock)
            {
                _gamesMapping.Clear();
            }

            ClearEditableGames();

            Logger.Debug("Loading games...");

            var grouping = _gameProvider.GetGames()
                .SelectMany(game => game.Denominations.Select(denom => (Game: game, Denom: denom)))
                .GroupBy(x => new GamesGrouping(x.Denom.Value, x.Game.GameType, x.Game.ThemeId, x.Game.ThemeName), x => x.Game);

            foreach (var group in grouping)
            {
                var groupKey = group.Key;
                var gameProfiles = group.ToList();
                SetupPaytableOptions(gameProfiles, groupKey);
            }

            // Apply valid restrictions to all game profiles
            foreach (var gameProfile in _editableGames.Values.Where(g => g.Restrictions.Any()))
            {
                gameProfile.UpdateValidRestrictions();
                SetGameRestriction(gameProfile, gameProfile.SelectedRestriction);
            }

            foreach (var entry in _editableGameConfigByGameTypeMapping)
            {
                CheckForMaximumDenominations(entry.Value, _gameTypeToActiveDenomMapping[entry.Key]);
            }

            foreach (var entry in _editableGameConfigBySubGameTypeMapping)
            {
                var key = _subGameTypeToActiveDenomMapping.Keys.SingleOrDefault(
                    x => x.gameType == entry.Key.gameType && x.subType == entry.Key.subType);

                CheckForMaximumDenominations(entry.Value, _subGameTypeToActiveDenomMapping[key]);
            }

            SelectedGameType = GameTypes.FirstOrDefault();
            CalculateTopAward();
            ScaleEnabledRtpValues();

            // This must be done after the _gamesMapping setup is finished in SetupPaytableOptions
            AutoEnableGames();
        }

        private void CheckForMaximumDenominations(List<EditableGameConfiguration> gameConfigs, ICollection<long> activeDenoms)
        {
            if (activeDenoms.Count >= ApplicationConstants.NumSelectableDenomsPerGameTypeInLobby)
            {
                foreach (var config in gameConfigs)
                {
                    if (!config.Enabled && !activeDenoms.Contains(config.BaseDenom))
                    {
                        config.MaxDenomEntriesReached = true;
                    }
                }
            }
        }

        private string GetWarningText()
        {
            var reqEKeyDevGameReConfig = Configuration.GetSetting(
                OperatorMenuSetting.RequireEKeyDeviceGameReconfiguration,
                false);
            return reqEKeyDevGameReConfig && InitialConfigComplete
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReqEKeyDeviceGameReConfigWarningLabel)
                : null;
        }

        private void EnterConfig()
        {
            EventBus.Subscribe<GameStatusChangedEvent>(this, OnGameStatusChanged);
            IsInEditMode = true;
            SetEditMode();
        }

        private void SetEditMode()
        {
            ReadOnlyStatus = IsInEditMode
                ? string.Empty
                : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReadOnlyModeText);

            if (IsInEditMode)
            {
                PreventOperatorMenuExit();
            }
            else
            {
                AllowOperatorMenuExit();
            }

            UpdateInputStatusText();

            GameOptionsGridEnabled = IsInEditMode;

            ApplyGameOptionsEnabled();
            RaisePropertyChanged(
                nameof(GameOptionsEnabled),
                nameof(ShowSaveButtonOverride),
                nameof(ShowCancelButtonOverride),
                nameof(ShowSummaryButtons),
                nameof(IsInEditMode),
                nameof(CanSave),
                nameof(ExportVisibleOverride),
                nameof(ImportVisibleOverride),
                nameof(ConfigureVisible));

            UpdateRestrictions();
        }

        private void UpdateInputStatusText()
        {
            if (InputEnabled)
            {
                InputStatusText = string.Empty;
                ClearErrors(nameof(InputStatusText));

                if (SelectedGame?.GameConfigurations.Count == 0)
                {
                    InputStatusText = string.Format(
                        CultureInfo.CurrentCulture,
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoGameOptionsAtMaxBetLimit),
                        SelectedGame.ThemeName,
                        _maxBetLimit.CentsToDollars().FormattedCurrencyString());
                }
                else if (GameConfigurations != null)
                {
                    // We need to check all the denominations, not just the current one. If we find one that's invalid,
                    // prevent saving the config.
                    foreach (var gameConfig in GameConfigurations)
                    {
                        gameConfig.SetWarningText();
                        // If the warning is due to max denoms reached, we can still Save and don't need to set error for this page
                        if (!string.IsNullOrEmpty(gameConfig.WarningText) && (!gameConfig.MaxDenomEntriesReached || !gameConfig.EnabledByHost))
                        {
                            SetError(nameof(InputStatusText), InputStatusText);
                            break;
                        }
                    }
                }

                RaisePropertyChanged(nameof(InputStatusText), nameof(CanSave), nameof(HasErrors));
            }
        }

        private void UpdateSaveWarning()
        {
            if (IsEnabledGamesLimitExceeded)
            {
                SaveWarningText = string.Format(
                    CultureInfo.CurrentCulture,
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EnabledGamesLimitExceeded),
                    TotalEnabledGames,
                    _digitalRights.LicenseCount);
            }
            else
            {
                SaveWarningText = string.Empty;
            }
        }

        private void UpdateRestrictions()
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    SelectedGame?.UpdateValidRestrictions();
                    RaisePropertyChanged(
                        nameof(SelectedRestriction),
                        nameof(ValidRestrictions),
                        nameof(ShowRestrictionChooser),
                        nameof(DenomSelectionLimit),
                        nameof(DenomSelectionLimitExists),
                        nameof(GameConfigurations),
                        nameof(CanSave));
                    SetRestriction(SelectedRestriction);
                });
        }

        private void SetRestriction(IConfigurationRestriction restriction)
        {
            if (SelectedGame is null)
            {
                return;
            }

            if (DenomSelectionLimitExists)
            {
                ProcessOperatorModeRules();
                return;
            }

            SetGameRestriction(SelectedGame, restriction);
        }

        private void SetGameRestriction(EditableGameProfile profile, IConfigurationRestriction restriction)
        {
            // Process mappings (Single Game Multi Denom, or "Player Selectable Denoms")
            foreach (var game in profile.GameConfigurations)
            {
                // If there are configured restrictions but none is chosen or none were valid, then
                // we don't want to show any denoms for configuration.
                if (profile.Restrictions.Any() && restriction is null)
                {
                    game.RestrictedToReadOnly = true;
                    game.Enabled = false;
                    game.Active = false;
                    continue;
                }

                // From here on, if the restriction is null, then we can't do anything, so we
                // should just move on.
                if (restriction is null)
                {
                    continue;
                }

                var mapping = restriction.RestrictionDetails.Mapping?.FirstOrDefault(c => game.BaseDenom == c.Denomination);
                if (mapping is null)
                {
                    game.RestrictedToReadOnly = true;
                    game.Enabled = false;
                    game.Active = false;
                    continue;
                }

                var mappedGame = game.AvailableGames.FirstOrDefault(g => g.VariationId == mapping.VariationId);
                if (mappedGame is null)
                {
                    game.Active = false;
                    continue;
                }

                game.Active = true;
                game.Game = mappedGame;
                game.RestrictedToReadOnly = !restriction.RestrictionDetails.Editable || !mapping.Editable;

                game.Enabled = mapping.EnabledByDefault;

                var betLinePreset = game.Game.BetLinePresetList.FirstOrDefault(b => b.Id == mapping.DefaultBetLinePresetId);
                if (betLinePreset is not null)
                {
                    var betOption = game.BetOptions.FirstOrDefault(b => b.Name == betLinePreset.BetOption.Name);
                    if (betOption is not null)
                    {
                        Logger.Debug($"Restricted bet option to {mapping.DefaultBetLinePresetId} from {string.Join(",", mapping.BetLinePresets)}");
                        game.SelectedBetOption = betOption;
                    }

                    var lineOption = game.LineOptions.FirstOrDefault(l => l.Name == betLinePreset.LineOption.Name);
                    if (lineOption is not null)
                    {
                        game.SelectedLineOption = lineOption;
                    }
                }

                Logger.Debug($"{restriction.Name} Restriction set for {game.Game.ThemeId} - " +
                             $"Name:{restriction.RestrictionDetails.Name} " +
                             $"MinRtp:{restriction.RestrictionDetails.MinimumPaybackPercent} " +
                             $"MaxRtp:{restriction.RestrictionDetails.MaximumPaybackPercent}");
            }

            CheckForRestrictionMismatch(profile, restriction);
        }

        private void CheckForRestrictionMismatch(EditableGameProfile profile, IConfigurationRestriction restriction)
        {
            if (!profile.ValidRestrictions.Any())
            {
                return;
            }

            var gamesWithRestrictions = Games.Where(g => g.ValidRestrictions != null && g.ValidRestrictions.Any()).ToList();
            var restrictionMismatch = restriction != null &&
                                      gamesWithRestrictions.Any(g => g.SelectedRestriction?.Name != restriction.Name);
            foreach (var game in gamesWithRestrictions)
            {
                game.SetRestrictionError(restrictionMismatch);
            }
        }

        /// <summary>
        ///     When Operator Mode is set, denom selection is only done by the Operator, not the player. Currently "Operator Mode"
        ///     limits denom selection to one denom only. This could change and the groundwork is in-place; to allow the operator
        ///     to select more than one denom, up to <see cref="DenomSelectionLimit" />.
        /// </summary>
        private void ProcessOperatorModeRules()
        {
            if (!DenomSelectionLimit.HasValue)
            {
                return;
            }

            // Process limited denom selection only ("Operator Selectable Denoms")
            if (SelectedGame.EnabledGameConfigurationsCount < DenomSelectionLimit.Value)
            {
                foreach (var game in SelectedGame.GameConfigurations)
                {
                    game.RestrictedToReadOnly = false;
                }
            }
            else if (SelectedGame.EnabledGameConfigurationsCount == DenomSelectionLimit.Value)
            {
                // Restrict all but the ones enabled
                foreach (var game in SelectedGame.GameConfigurations)
                {
                    game.RestrictedToReadOnly = !game.Enabled;
                }
            }
            else
            {
                // Too many denoms enabled. Reset by disabling all denoms
                foreach (var game in SelectedGame.GameConfigurations)
                {
                    game.RestrictedToReadOnly = false;
                    game.Enabled = false;
                }
            }
        }

        private Dictionary<long, long> GetCountByDenomAndGameType(GameType gameType)
        {
            var editableConfigsCountByDenom = new Dictionary<long, long>();
            if (_editableGameConfigByGameTypeMapping.ContainsKey(gameType))
            {
                var configList = _editableGameConfigByGameTypeMapping[gameType].Where(gc => gc.Enabled).ToList();
                foreach (var config in configList)
                {
                    if (editableConfigsCountByDenom.ContainsKey(config.BaseDenom))
                    {
                        editableConfigsCountByDenom[config.BaseDenom]++;
                    }
                    else
                    {
                        editableConfigsCountByDenom.Add(config.BaseDenom, 1);
                    }
                }
            }

            return editableConfigsCountByDenom;
        }

        private Dictionary<long, long> GetCountByDenomAndSubGameType(GameType gameType, string subGameType)
        {
            var editableConfigsCountByDenom = new Dictionary<long, long>();

            if (_editableGameConfigBySubGameTypeMapping.Keys.Any(
                x => x.gameType == gameType && x.subType == subGameType))
            {
                var configList = _editableGameConfigBySubGameTypeMapping
                    .SingleOrDefault(x => x.Key.gameType == gameType && x.Key.subType == subGameType)
                    .Value
                    .Where(gc => gc.Enabled)
                    .ToList();

                foreach (var config in configList)
                {
                    if (editableConfigsCountByDenom.ContainsKey(config.BaseDenom))
                    {
                        editableConfigsCountByDenom[config.BaseDenom]++;
                    }
                    else
                    {
                        editableConfigsCountByDenom.Add(config.BaseDenom, 1);
                    }
                }
            }

            return editableConfigsCountByDenom;
        }

        private void SetupPaytableOptions(IReadOnlyList<IGameDetail> gameProfiles, GamesGrouping groupKey)
        {
            var editableConfig = new EditableGameConfiguration(groupKey.Denom, gameProfiles, ShowGameRtpAsRange);
            var maxBet = editableConfig.BetMaximum.DollarsToCents();

            if (maxBet > _maxBetLimit)
            {
                return;
            }

            if (_editableGames.ContainsKey(groupKey.ThemeName))
            {
                _editableGames[groupKey.ThemeName]
                    .AddConfigs(new List<EditableGameConfiguration> { editableConfig });
            }
            else
            {
                var gameProfile = new EditableGameProfile(
                    groupKey.ThemeId,
                    groupKey.ThemeName,
                    new List<EditableGameConfiguration> { editableConfig },
                    _enableRtpScaling,
                    _restrictionProvider,
                    _gameConfiguration);
                _editableGames.Add(groupKey.ThemeName, gameProfile);
                lock (_gamesMappingLock)
                {
                    if (_gamesMapping.ContainsKey(groupKey.GameType))
                    {
                        _gamesMapping[groupKey.GameType].Add(gameProfile);
                    }
                    else
                    {
                        _gamesMapping.Add(groupKey.GameType, new List<EditableGameProfile> { gameProfile });
                    }
                }
            }

            if (!string.IsNullOrEmpty(editableConfig.SubGameType))
            {
                if (_editableGameConfigBySubGameTypeMapping.Keys.Any(
                    x => x.gameType == groupKey.GameType && x.subType == editableConfig.SubGameType))
                {
                    var key = _editableGameConfigBySubGameTypeMapping.Keys
                        .SingleOrDefault(
                            x => x.gameType == groupKey.GameType && x.subType == editableConfig.SubGameType);

                    _editableGameConfigBySubGameTypeMapping[key].Add(editableConfig);

                    if (editableConfig.Enabled)
                    {
                        _subGameTypeToActiveDenomMapping[key].Add(editableConfig.BaseDenom);
                    }
                }
                else
                {
                    var newEntry = (groupKey.GameType, editableConfig.SubGameType);
                    _editableGameConfigBySubGameTypeMapping.Add(
                        newEntry,
                        new List<EditableGameConfiguration> { editableConfig });

                    _subGameTypeToActiveDenomMapping.Add(newEntry, new HashSet<long>());
                    if (editableConfig.Enabled)
                    {
                        _subGameTypeToActiveDenomMapping[newEntry].Add(editableConfig.BaseDenom);
                    }
                }
            }
            else
            {
                if (_editableGameConfigByGameTypeMapping.ContainsKey(groupKey.GameType))
                {
                    _editableGameConfigByGameTypeMapping[groupKey.GameType].Add(editableConfig);

                    if (editableConfig.Enabled)
                    {
                        _gameTypeToActiveDenomMapping[groupKey.GameType].Add(editableConfig.BaseDenom);
                    }
                }
                else
                {
                    var gameType = groupKey.GameType;

                    _editableGameConfigByGameTypeMapping.Add(
                        gameType,
                        new List<EditableGameConfiguration> { editableConfig });

                    _gameTypeToActiveDenomMapping.Add(gameType, new HashSet<long>());
                    if (editableConfig.Enabled)
                    {
                        _gameTypeToActiveDenomMapping[gameType].Add(editableConfig.BaseDenom);
                    }
                }
            }

            editableConfig.PropertyChanged += OnSubPropertyChanged;
            RaisePropertyChanged(nameof(editableConfig.DenomString));
        }

        private void SaveChanges()
        {
            var hasChanges = HasChanges();
            var progressiveLevels = _progressiveConfiguration.ViewProgressiveLevels()
                .Where(x => x.CanEdit && x.LevelType == ProgressiveLevelType.Sap).ToList();

            var updatedLevels = new List<IViewableProgressiveLevel>();
            foreach (var game in _editableGames.Where(e => e.Value.HasChanges()))
            {
                if (game.Value.HasRestrictionChanges())
                {
                    foreach (var mapping in game.Value.OriginalRestriction.RestrictionDetails.Mapping)
                    {
                        var gameDetail = _gameProvider.GetGames().FirstOrDefault(g => g.VariationId == mapping.VariationId);

                        if (gameDetail is not null)

                        {
                            ClearGameData(gameDetail, null);
                        }
                    }
                }

                ResetGameStorage(game.Value);

                foreach (var id in game.Value.GameConfigurations
                    .SelectMany(c => c.AvailableGames.Select(g => g.Id))
                    .Distinct())
                {
                    // We are already grouped by themeId so just remove all the games we have for this themeId
                    _gameProvider.SetActiveDenominations(id, Enumerable.Empty<IDenomination>());
                }

                var updates = game.Value.GameConfigurations
                    .Where(c => c.Game != null)
                    .Select(gameConfig => (gameId: gameConfig.Game.Id, config: gameConfig))
                    .GroupBy(x => x.gameId, (id, group) => (id, group.Select(x => x.config)));
                foreach (var (gameId, configurations) in updates)
                {
                    updatedLevels.AddRange(SaveGameConfiguration(gameId, configurations, progressiveLevels));
                }

                if (game.Value.SelectedRestriction != null)
                {
                    _gameConfiguration.Apply(game.Value.ThemeId, game.Value.SelectedRestriction);
                }
            }

            if (updatedLevels.Any())
            {
                _progressiveConfiguration.LockProgressiveLevels(updatedLevels);
            }

            var progressiveMeterManager = ServiceManager.GetInstance().GetService<IProgressiveMeterManager>();
            progressiveMeterManager.UpdateLPCompositeMeters();

            if (_pendingImportSettings.Any())
            {
                SaveImportSettings();
            }
            else if (hasChanges)
            {
                var currentGame = _gameProvider.GetGame(PropertiesManager.GetValue(GamingConstants.SelectedGameId, 0));

                if (currentGame != null)
                {
                    var activeDenomination = _gameProvider.GetEnabledGames().Where(g => g.ThemeId == currentGame.ThemeId)
                        .SelectMany(g => g.Denominations.Where(d => d.Active)).OrderBy(d => d.Value).FirstOrDefault();

                    PropertiesManager.SetProperty(GamingConstants.SelectedDenom, activeDenomination?.Value ?? 0L);
                }

                FinishSaveChanges();
            }
        }

        private void ResetGameStorage(EditableGameProfile value)
        {
            var configList = value.GameConfigurations.ToList();
            configList.ForEach(
                configuration =>
                {
                    if (!configuration.Enabled &&
                        (configuration.Game?.ActiveDenominations.Contains(configuration.BaseDenom) ??
                         false))
                    {
                        ClearGameData(configuration.Game, configuration.BaseDenom);
                    }
                });
        }

        private void ClearGameData(IGameDetail game, long? denom)
        {
            Logger.Debug($"ClearGameData for {game.ThemeId} -- variation {game.VariationId}");

            var handlerFactory = ServiceManager.GetInstance().TryGetService<IContainerService>()?.Container.GetInstance<ICommandHandlerFactory>();
            handlerFactory?.Create<ClearGameLocalSessionData>().Handle(new ClearGameLocalSessionData(game, denom));
        }

        private IEnumerable<IViewableProgressiveLevel> SaveGameConfiguration(
            int gameId,
            IEnumerable<EditableGameConfiguration> configurations,
            IEnumerable<IViewableProgressiveLevel> progressiveLevels)
        {
            var denominations = (
                from configuration in configurations
                let denomination = configuration.ResolveDenomination()
                where denomination != null
                select new Denomination(denomination.Id, denomination.Value, configuration.Enabled)
                {
                    BetOption = configuration.SelectedBetOption?.Name,
                    LineOption = configuration.SelectedLineOption?.Name,
                    BonusBet = configuration.SelectedBonusBet,
                    SecondaryAllowed = configuration.Gamble,
                    LetItRideAllowed = configuration.LetItRide,
                    MinimumWagerCredits = configuration.MinimumWagerCredits,
                    MaximumWagerCredits = configuration.MaximumWagerCredits,
                    MaximumWagerOutsideCredits = configuration.MaximumWagerOutsideCredits
                }).ToList();

            _gameProvider.SetActiveDenominations(gameId, denominations.Where(d => d.Active));
            return progressiveLevels.Where(
                x => x.GameId == gameId && denominations.Any(
                    d => x.Denomination.Contains(d.Value) && d.Active &&
                         (string.IsNullOrEmpty(x.BetOption) || d.BetOption == x.BetOption)));
        }

        private void SaveImportSettings()
        {
            IsInProgress = true;
            ResetConfigImportSettings();

            var settings = new Dictionary<string, object>(_pendingImportSettings);

            var importTask = new Task(
                async () => await _settingsManager.Import(ConfigurationGroup.Game, settings),
                _cancellation.Token);

            importTask.ContinueWith(
                task => task.Exception?.Handle(
                    ex =>
                    {
                        IsError = true;
                        Logger.Error("Game settings import failed", ex);
                        ShowClosableDialog(
                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportFailed),
                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportFailedGameSettings));
                        return true;
                    }),
                TaskContinuationOptions.OnlyOnFaulted);

            importTask.RunSynchronously();
        }

        private void FinishSaveChanges()
        {
            EventBus.Publish(new OperatorMenuSettingsChangedEvent());

            EventBus.Publish(new GameConfiguringEvent());

            _gameService.ShutdownBegin();
            foreach (var game in _games)
            {
                game.OnSave();
            }

            RaisePropertyChanged(nameof(CanSave));
        }

        private void ResetChanges()
        {
            _gameTypeToActiveDenomMapping.Clear();
            _subGameTypeToActiveDenomMapping.Clear();

            ResetConfigImportSettings();

            foreach (var game in _editableGames.Select(x => x.Value))
            {
                game.Reset();
                foreach (var config in game.GameConfigurations)
                {
                    config.ResetChanges();

                    if (!config.Enabled)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(config.SubGameType))
                    {
                        if (!_subGameTypeToActiveDenomMapping.Keys.Any(
                                x => x.gameType == config.Game?.GameType && x.subType == config.SubGameType))
                        {
                            _subGameTypeToActiveDenomMapping.Add(
                                (config.Game.GameType, config.SubGameType),
                                new HashSet<long> { config.BaseDenom });
                        }
                        else
                        {
                            var key = _subGameTypeToActiveDenomMapping.Keys
                                .Single(x => x.gameType == config.Game?.GameType && x.subType == config.SubGameType);

                            _subGameTypeToActiveDenomMapping[key].Add(config.BaseDenom);
                        }
                    }
                    else if (config.Game != null)
                    {
                        if (!_gameTypeToActiveDenomMapping.ContainsKey(config.Game.GameType))
                        {
                            _gameTypeToActiveDenomMapping.Add(config.Game.GameType, new HashSet<long> { config.BaseDenom });
                        }
                        else
                        {
                            _gameTypeToActiveDenomMapping[config.Game.GameType].Add(config.BaseDenom);
                        }
                    }
                    else
                    {
                        Logger.Debug($"Invalid game for denom {config.Denom}");
                    }
                }
            }

            foreach (var config in _editableGames.SelectMany(x => x.Value.GameConfigurations))
            {
                if (config.Enabled)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(config.SubGameType))
                {
                    if (_subGameTypeToActiveDenomMapping.Keys.Any(x => x.gameType == config.Game?.GameType && x.subType == config.SubGameType))
                    {
                        var key = _subGameTypeToActiveDenomMapping.Keys
                            .Single(x => x.gameType == config.Game.GameType && x.subType == config.SubGameType);

                        if (!_subGameTypeToActiveDenomMapping[key].Contains(config.BaseDenom) &&
                            _subGameTypeToActiveDenomMapping[key].Count >= ApplicationConstants.NumSelectableDenomsPerGameTypeInLobby)
                        {
                            config.MaxDenomEntriesReached = true;
                        }
                    }
                }
                else if (config.Game != null)
                {
                    if (_gameTypeToActiveDenomMapping.ContainsKey(config.Game.GameType) &&
                        !_gameTypeToActiveDenomMapping[config.Game.GameType].Contains(config.BaseDenom) &&
                        _gameTypeToActiveDenomMapping[config.Game.GameType].Count >= ApplicationConstants.NumSelectableDenomsPerGameTypeInLobby)
                    {
                        config.MaxDenomEntriesReached = true;
                    }
                }
            }
        }

        private void OnSubPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(CanSave));

            if (sender is not EditableGameConfiguration editableConfig)
            {
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(editableConfig.ForcedMinBet) or nameof(editableConfig.ForcedMaxBet)
                    or nameof(editableConfig.ForcedMaxBetOutside):
                    UpdateInputStatusText();
                    break;
                case nameof(editableConfig.SelectedPaytable):
                    ScaleEnabledRtpValues();
                    return;
            }

            if (e.PropertyName != nameof(editableConfig.Enabled) || editableConfig.Game == null)
            {
                return;
            }

            Dictionary<long, long> editableConfigsCountByDenom;
            List<EditableGameConfiguration> configList;

            if (!string.IsNullOrEmpty(editableConfig.SubGameType))
            {
                editableConfigsCountByDenom = GetCountByDenomAndSubGameType(
                    editableConfig.Game.GameType,
                    editableConfig.SubGameType);
                configList = _editableGameConfigBySubGameTypeMapping.SingleOrDefault(
                        x => x.Key.gameType == editableConfig.Game.GameType &&
                             x.Key.subType == editableConfig.SubGameType)
                    .Value;
            }
            else
            {
                editableConfigsCountByDenom = GetCountByDenomAndGameType(editableConfig.Game.GameType);
                configList = _editableGameConfigByGameTypeMapping[editableConfig.Game.GameType];
            }

            var overLimit = editableConfigsCountByDenom.Count >= ApplicationConstants.NumSelectableDenomsPerGameTypeInLobby;
            foreach (var config in configList)
            {
                if (overLimit)
                {
                    if (!config.Enabled && !editableConfigsCountByDenom.ContainsKey(config.BaseDenom))
                    {
                        config.MaxDenomEntriesReached = true;
                    }
                }
                else
                {
                    config.MaxDenomEntriesReached = false;
                }
            }

            if (DenomSelectionLimitExists)
            {
                ProcessOperatorModeRules();
            }

            UpdateInputStatusText();
            UpdateSaveWarning();
            RaisePropertyChanged(nameof(CanSave));
        }

        private void ScaleEnabledRtpValues()
        {
            if (!_enableRtpScaling || SelectedGame == null || SelectedRestriction != null)
            {
                return;
            }

            var enabledConfigs = SelectedGame.GameConfigurations.Where(g => g.Enabled).ToList();
            foreach (var config in SelectedGame.GameConfigurations)
            {
                var enabledLowerDenoms = enabledConfigs.Where(g => g.Denom < config.Denom).ToList();
                var closestEnabledLowerDenom = enabledLowerDenoms.FirstOrDefault(g => g.Denom == enabledLowerDenoms.Max(d => d.Denom));
                var enabledHigherDenoms = enabledConfigs.Where(g => g.Denom > config.Denom).ToList();
                var closestEnabledHigherDenom = enabledHigherDenoms.FirstOrDefault(g => g.Denom == enabledHigherDenoms.Min(d => d.Denom));

                config.SetAllowedRtpRange(
                    closestEnabledLowerDenom?.Game?.MinimumPaybackPercent,
                    closestEnabledHigherDenom?.Game?.MinimumPaybackPercent);
            }
        }

        private void OnGameStatusChanged(GameStatusChangedEvent obj)
        {
            foreach (var game in _editableGames.Values)
            {
                var config = game.GameConfigurations.FirstOrDefault(g => g.Game?.Id == obj.GameId);
                if (config == null)
                {
                    continue;
                }

                config.RaiseEnabledByHostChanged();
                if (!config.EnabledByHost)
                {
                    config.Enabled = false;
                }
            }
        }

        private void ApplySelectedGameType()
        {
            lock (_gamesMappingLock)
            {
                Games = new ObservableCollection<EditableGameProfile>(
                    _gamesMapping.TryGetValue(SelectedGameType, out var gameProfiles)
                        ? gameProfiles.OrderBy(g => g.ThemeName)
                        : Enumerable.Empty<EditableGameProfile>());
            }

            SelectedGame = Games.FirstOrDefault();

            ApplyGameOptionsEnabled();
            RaisePropertyChanged(nameof(LetItRideOptionVisible), nameof(GambleOptionVisible),
                nameof(OptionColumnVisible), nameof(IsRouletteGameSelected), nameof(IsPokerGameSelected));
        }

        private void ApplyGameOptionsEnabled()
        {
            if (SelectedGame == null)
            {
                return;
            }

            foreach (var gameConfiguration in SelectedGame.GameConfigurations)
            {
                gameConfiguration.GameOptionsEnabled = GameOptionsEnabled;
            }
        }

        private void CalculateTopAward()
        {
            // TODO Recalculate after certain changes?
            if (!Games.Any())
            {
                TopAwardValue = 0;
            }
            else
            {
                // TopAward is the max of all enabled game top award values across EGM
                TopAwardValue = _editableGames
                    .SelectMany(g => g.Value.GameConfigurations)
                    .Where(c => c.Enabled).MaxOrDefault(x => x.TopAwardValue, 0);
            }
        }

        private void Import()
        {
            if (!FieldAccessEnabled)
            {
                ShowPopup(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameSetupEKeyImportVerified));
            }
            else if (!_settingsManager.IsConfigurationImportFilePresent(ConfigurationGroup.Game))
            {
                ShowPopup(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameSetupImportFileMissing));
            }
            else
            {
                FieldAccessStatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.KeepEKeyInsertedText);
                var viewModel = new AdvancedGameConfigurationSavePopupViewModel(
                    this,
                    () => !IsInProgress
                          && FieldAccessEnabled
                          && _settingsManager.IsConfigurationImportFilePresent(ConfigurationGroup.Game),
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportSettingsLabel),
                    BuildImportConfirmationText());

                var result = ShowSavableDialog<AdvancedGameConfigurationSavePopupView>(viewModel, string.Empty);

                if (!result ?? false)
                {
                    FieldAccessStatusText = string.Empty;
                    return;
                }

                MvvmHelper.ExecuteOnUI(
                    () =>
                    {
                        IsInProgress = true;
                        IsError = false;

                        var errorMessage = PreviewSettings();

                        IsInProgress = false;

                        if (string.IsNullOrEmpty(errorMessage))
                        {
                            ShowClosableDialog(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportCompleteText),
                                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportGameSettingsComplete));
                        }
                        else
                        {
                            _pendingImportSettings.Clear();
                            ResetConfigImportSettings();

                            ShowClosableDialog(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportFailed), errorMessage);
                        }
                    });

                SelectedGame.Refresh();
            }
        }

        private string PreviewSettings()
        {
            try
            {
                var (_, values) = _settingsManager.Preview(ConfigurationGroup.Game).FirstOrDefault();

                if (!values.TryGetValue("Gaming", out var gamingSettingsValue)
                    || !(gamingSettingsValue is GamingSettings gamingSettings))
                {
                    Logger.Error("Failed to preview settings, invalid game settings");
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportFailedGameSettingsInvalid);
                }

                if (!values.TryGetValue("Progressive", out var progressiveSettingsValue)
                    || !(progressiveSettingsValue is ProgressiveSettings progressiveSettings))
                {
                    Logger.Error("Failed to preview settings, invalid progressive settings");
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportFailedProgressiveSettingsInvalid);
                }

                var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
                var jurisdiction = propertiesManager.GetValue(ApplicationConstants.JurisdictionKey, string.Empty);

                if (!jurisdiction.Equals(gamingSettings.Jurisdiction))
                {
                    Logger.Error($"Failed to preview settings, can not import game settings jurisdiction {gamingSettings.Jurisdiction} for jurisdiction {jurisdiction}");
                    return string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportFailedGameSettingsJurisdictionInvalid), gamingSettings.Jurisdiction, jurisdiction);
                }

                ResetConfigImportSettings();

                _progressiveSettings = progressiveSettings;
                var sharedSapLevels = _progressiveConfiguration.ViewSharedSapLevels();
                foreach (var sapLevel in progressiveSettings.CustomSapLevels)
                {
                    var (errorMessageKey, level) = sapLevel.TryCreateSharedSapLevel(sharedSapLevels);
                    if (!string.IsNullOrEmpty(errorMessageKey))
                    {
                        Logger.Error("Failed to import settings.");
                        return Localizer.For(CultureFor.Operator).GetString(errorMessageKey);
                    }

                    _cachedConfigSharedSapLevels.Add((level, sapLevel));
                }

                var linkedProgressives = _linkedProgressiveProvider.ViewLinkedProgressiveLevels();

                var gameConfigurations = _editableGames.Values.SelectMany(p => p.GameConfigurations);

                foreach (var configuration in gameConfigurations)
                {
                    var (denomination, gameSetting) = gamingSettings.Games
                        .SelectMany(g => g.Denominations.Select(d => (denom: d, game: g)))
                        .FirstOrDefault(
                            x => x.denom.Active &&
                                 x.denom.Value == configuration.ResolveDenomination().Value &&
                                 configuration.AvailableGames.Any(
                                     g => g.ThemeId == x.game.ThemeId && g.PaytableId == x.game.PaytableId));

                    if (denomination is null || gameSetting is null)
                    {
                        configuration.Enabled = false;
                        continue;
                    }

                    configuration.Game = configuration.AvailableGames.FirstOrDefault(
                        g => g.PaytableId == gameSetting.PaytableId && g.ThemeId == gameSetting.ThemeId);

                    if (configuration.Game is null)
                    {
                        continue;
                    }

                    var gameProfile = _editableGames.Values.FirstOrDefault(g => g.ThemeId.Equals(configuration.Game.ThemeId));

                    if (gameProfile != null)
                    {
                        var restriction = GetRestrictionFromVariationId(configuration.Game.VariationId, gameProfile);
                        if (restriction != null)
                        {
                            gameProfile.SelectedRestriction = restriction;
                        }
                    }

                    configuration.Enabled = denomination.Active;
                    configuration.SelectedBetOption = string.IsNullOrEmpty(denomination.BetOption)
                        ? null
                        : configuration.BetOptions?.FirstOrDefault(o => o.Name == denomination.BetOption) ??
                          configuration.BetOptions?.FirstOrDefault();
                    configuration.SelectedLineOption = string.IsNullOrEmpty(denomination.LineOption)
                        ? null
                        : configuration.LineOptions?.FirstOrDefault(o => o.Name == denomination.LineOption) ??
                          configuration.LineOptions?.FirstOrDefault();
                    configuration.SelectedBonusBet = denomination.BonusBet;
                    configuration.Gamble = denomination.SecondaryAllowed;
                    configuration.ForcedMinBet = denomination.MinimumWagerCredits * configuration.Denom;
                    configuration.ForcedMaxBet = denomination.MaximumWagerCredits * configuration.Denom;
                    configuration.ForcedMaxBetOutside = denomination.MaximumWagerOutsideCredits * configuration.Denom;

                    var (errorMessageKey, levels) = _progressiveSettings.TryCreateProgressiveLevels(
                        _cachedConfigSharedSapLevels,
                        linkedProgressives,
                        _progressiveConfiguration,
                        configuration.Game,
                        denomination.Value,
                        denomination.BetOption);

                    if (!string.IsNullOrEmpty(errorMessageKey))
                    {
                        IsInProgress = false;
                        Logger.Error("Failed to import settings.");
                        return Localizer.For(CultureFor.Operator).GetString(errorMessageKey);
                    }

                    var hasConfigProgressives = levels.Count > 0;
                    if (hasConfigProgressives)
                    {
                        _cachedConfigProgressiveLevels.AddRange(levels);
                        configuration.LoadConfiguredProgressiveLevels(levels);
                    }
                }

                _pendingImportSettings = new Dictionary<string, object>(values);

                SelectedGameType = GameTypes.FirstOrDefault();
            }
            catch (Exception e)
            {
                IsInProgress = false;
                Logger.Error("Failed to preview settings", e);
                return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportFailedGameSettings);
            }

            return string.Empty;
        }

        private void Export()
        {
            if (!FieldAccessEnabled)
            {
                ShowPopup(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameSetupEKeyExportVerified));
            }
            else
            {
                var viewModel = new AdvancedGameConfigurationSavePopupViewModel(
                    this,
                    () => !IsInProgress && FieldAccessEnabled,
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExportSettingsLabel),
                    _settingsManager.IsConfigurationImportFilePresent(ConfigurationGroup.Game)
                        ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExportGameDialog1)
                        : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExportGameDialog3));

                var result = ShowSavableDialog<AdvancedGameConfigurationSavePopupView>(viewModel, string.Empty);

                if (!result ?? false)
                {
                    return;
                }

                if (result == true && FieldAccessEnabled)
                {
                    MvvmHelper.ExecuteOnUI(
                        () =>
                        {
                            IsInProgress = true;
                            IsError = false;
                        });

                    Task.Run(async () => await _settingsManager.Export(ConfigurationGroup.Game), _cancellation.Token)
                        .ContinueWith(
                            task => task.Exception?.Handle(
                                ex =>
                                {
                                    IsError = true;
                                    Logger.Error(
                                        $"Game settings export failed,  {ex.InnerException?.Message ?? ex.Message}",
                                        ex);

                                    ShowClosableDialog(
                                        Localizer.For(CultureFor.Operator)
                                            .GetString(ResourceKeys.ExportCompleteErrorText));

                                    return true;
                                }),
                            CancellationToken.None,
                            TaskContinuationOptions.OnlyOnFaulted,
                            TaskScheduler.FromCurrentSynchronizationContext());

                    MvvmHelper.ExecuteOnUI(() => IsInProgress = true);
                }
            }
        }

        private void ResetConfigImportSettings()
        {
            _progressiveSettings = null;
            _cachedConfigProgressiveLevels.Clear();
            _cachedConfigSharedSapLevels.Clear();
        }

        private void ShowClosableDialog(string windowTitle, string windowInfoText = null)
        {
            _dialogService.ShowDialog(
                this,
                windowTitle,
                DialogButton.Cancel,
                new DialogButtonCustomText
                {
                    { DialogButton.Cancel, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Close) }
                },
                windowInfoText);
        }

        private bool? ShowSavableDialog<T>(
            IModalDialogSaveViewModel viewModel,
            string windowTitle,
            string windowInfoText = null) where T : IOperatorMenuPage
        {
            return _dialogService.ShowDialog<T>(
                this,
                viewModel,
                windowTitle,
                DialogButton.Save | DialogButton.Cancel,
                new DialogButtonCustomText
                {
                    { DialogButton.Save, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Yes) },
                    { DialogButton.Cancel, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.No) }
                },
                windowInfoText);
        }

        private void HandleExported()
        {
            if (IsError)
            {
                ShowPopup(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExportCompleteErrorText));
            }
            else
            {
                ShowClosableDialog(
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExportCompleteText),
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExportGameDialog2));
            }

            IsInProgress = false;
        }

        private void HandleImported()
        {
            if (IsError)
            {
                ShowClosableDialog(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportFailed),
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportFailedGameSettings));
            }
            else
            {
                FinishSaveChanges();
            }

            IsInProgress = false;
        }

        private void ClearEditableGames()
        {
            if (_editableGames.Any())
            {
                foreach (var game in _editableGames)
                {
                    game.Value.PropertyChanged -= OnSubPropertyChanged;
                    game.Value.Dispose();
                }

                _editableGames.Clear();
            }

            _editableGameConfigByGameTypeMapping.Clear();
            _editableGameConfigBySubGameTypeMapping.Clear();
            _gameTypeToActiveDenomMapping.Clear();
            _subGameTypeToActiveDenomMapping.Clear();
        }

        private void ProgressiveSetup(object configObject) // pops the Progressive Setup dialog
        {
            if (!(configObject is EditableGameConfiguration gameConfig) || gameConfig.Game == null)
            {
                return;
            }

            var readOnlyConfig = new ReadOnlyGameConfiguration(
                gameConfig.Game,
                gameConfig.ResolveDenomination().Value,
                _denomMultiplier,
                gameConfig.ProgressiveSetupConfigured);

            var viewModel = new ProgressiveSetupViewModel(readOnlyConfig, gameConfig.SelectedBetOption);

            _dialogService.ShowDialog<ProgressiveSetupView>(
                this,
                viewModel,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProgressiveSetupDialogCaption));

            ApplyGameOptionsEnabled();
        }

        private void ProgressiveView(object configObject)
        {
            if (!(configObject is EditableGameConfiguration gameConfig) ||
                !(CreateSetupViewModel(gameConfig, true) is ProgressiveSetupViewModel viewModel))
            {
                return;
            }

            _dialogService.ShowInfoDialog<ProgressiveSetupView>(
                this,
                viewModel,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProgressiveViewDialogCaption));
        }

        private ProgressiveSetupViewModel CreateSetupViewModel(EditableGameConfiguration gameConfig, bool isSummaryView)
        {
            if (gameConfig?.Game is null)
            {
                return null;
            }

            var denomValue = gameConfig.ResolveDenomination().Value;

            var readOnlyConfig = new ReadOnlyGameConfiguration(
                gameConfig.Game,
                denomValue,
                _denomMultiplier,
                gameConfig.ProgressiveSetupConfigured);

            var customSaps = new List<IViewableSharedSapLevel>();
            var levels = new List<IViewableProgressiveLevel>();
            var linkedLevelNames = new List<string>();

            if (_progressiveSettings != null)
            {
                levels.AddRange(_cachedConfigProgressiveLevels
                    .Where(
                        level => level.GameId == gameConfig.Game.Id
                                 && level.Denomination.Contains(denomValue)));
                customSaps.AddRange(_cachedConfigSharedSapLevels.Select(x => x.assignedLevel));
                linkedLevelNames.AddRange(_progressiveSettings.LinkedProgressiveLevelNames);
            }

            return new ProgressiveSetupViewModel(
                readOnlyConfig,
                gameConfig.SelectedBetOption,
                isSummaryView,
                levels,
                customSaps,
                linkedLevelNames);
        }

        private void ShowRtpSummary()
        {
            var viewModel = new GameRtpSummaryViewModel(_gameProvider.GetGames(), _denomMultiplier);
            _dialogService.ShowInfoDialog<GameRtpSummaryView>(
                this,
                viewModel,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameSummaryTitle));
        }

        private void ShowProgressiveSummary()
        {
            var viewModel = new ProgressiveSummaryViewModel();
            _dialogService.ShowInfoDialog<ProgressiveSummaryView>(
                this,
                viewModel,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProgressiveSummaryTitle));
        }

        private void AutoEnableGames()
        {
            lock (_gamesMappingLock)
            {
                foreach (var key in _gamesMapping.Keys.ToList())
                {
                    _gamesMapping[key].ForEach(AutoEnableGame);
                }
            }
        }

        private void AutoEnableGame(EditableGameProfile gameProfile)
        {
            if (!(gameProfile.OriginalRestriction?.RestrictionDetails.Editable ?? false))
            {
                return;
            }

            foreach (var config in gameProfile.GameConfigurations)
            {
                config.Enabled = true;
            }
        }

        private IConfigurationRestriction GetRestrictionFromVariationId(string variationId, EditableGameProfile gameProfile)
        {
            return gameProfile.ValidRestrictions.FirstOrDefault(
                v => v.RestrictionDetails.Mapping.Any(
                    v2 => v2.VariationId.Equals(variationId)));
        }

        private class GamesGrouping
        {
            public GamesGrouping(long denom, GameType gameType, string themeId, string themeName)
            {
                Denom = denom;
                GameType = gameType;
                ThemeId = themeId;
                ThemeName = themeName;
            }

            public long Denom { get; }

            public GameType GameType { get; }

            public string ThemeId { get; }

            public string ThemeName { get; }

            public static bool operator ==(GamesGrouping left, GamesGrouping right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(GamesGrouping left, GamesGrouping right)
            {
                return !Equals(left, right);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = Denom.GetHashCode();
                    hashCode = (hashCode * 397) ^ (int)GameType;
                    hashCode = (hashCode * 397) ^
                               (ThemeName != null ? StringComparer.InvariantCulture.GetHashCode(ThemeName) : 0);
                    return hashCode;
                }
            }

            public override bool Equals(object obj)
            {
                return !(obj is null) &&
                       (ReferenceEquals(this, obj) ||
                        obj.GetType() == GetType() &&
                        Equals((GamesGrouping)obj));
            }

            private bool Equals(GamesGrouping other)
            {
                return Denom == other.Denom && GameType == other.GameType && string.Equals(
                    ThemeName,
                    other.ThemeName,
                    StringComparison.InvariantCulture);
            }
        }
    }
}
