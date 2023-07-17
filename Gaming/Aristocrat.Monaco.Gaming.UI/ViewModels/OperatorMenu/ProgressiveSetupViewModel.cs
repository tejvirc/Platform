namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Input;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.UI.OperatorMenu;
    using Contracts;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;
    using Contracts.Progressives.SharedSap;
    using Kernel;
    using Localization.Properties;
    using Models;
    using MVVM.Command;
    using PackageManifest.Models;
    using Progressives;

    /// <summary>
    ///     View model for Progressive Setup
    /// </summary>
    public class ProgressiveSetupViewModel : OperatorMenuSaveViewModelBase
    {
        private readonly IProgressiveConfigurationProvider _progressives;
        private readonly ILinkedProgressiveProvider _linkedProgressives;
        private readonly IGameService _gameService;
        private readonly IGameProvider _gameProvider;
        private readonly IPropertiesManager _propertiesManager;

        private readonly ReadOnlyGameConfiguration _selectedGame;
        private readonly IReadOnlyCollection<IViewableProgressiveLevel> _validProgressiveLevels;
        private readonly IReadOnlyCollection<IViewableSharedSapLevel> _configSharedSapLevels;
        private readonly IReadOnlyCollection<string> _configLinkedProgressiveNames;
        private readonly bool _isAssociatedSap;

        private int _progressiveGroupId;
        private int _originalProgressiveGroupId;

        private string _selectedGameInfo;
        private ObservableCollection<LevelModel> _levelModels;
        private bool _isSummaryView;
        private bool _isSelectable;
        private bool _isSap;
        private bool _isLP;

        private Dictionary<int, (int linkedGroupId, int linkedLevelId)> _configuredLinkedLevelIds;

        private List<(LevelModel.LevelDefinition SelectableLevel, string SelectableLevelType)> _originalNonSapProgressiveLevels;

        /// <summary>
        ///     Used to determine whether or not the game is fully setup, prevents saving in the AdvancedGameSetupViewModel if false.
        ///     (Currently only used if the Progressive Id is configurable, i.e. Vertex Progressives)
        /// </summary>
        public bool SetupCompleted = false;

        /// <summary>
        ///     Used to determine whether or not the progressive levels have been altered since the opening of the setup menu, prevents saving in the AdvancedGameSetupViewModel if false.
        ///     (Currently only used if the Progressive Id is configurable, i.e. Vertex Progressives)
        /// </summary>
        public bool ConfigurableProgressiveLevelsChanged = false;

        public ProgressiveSetupViewModel(
            ReadOnlyGameConfiguration selectedGame,
            BetOption betOption,
            bool isSummaryView,
            IReadOnlyCollection<IViewableProgressiveLevel> configProgressiveLevels,
            IReadOnlyCollection<IViewableSharedSapLevel> configSharedSapLevels,
            IReadOnlyCollection<string> configLinkedProgressiveNames)
        {
            if (configProgressiveLevels == null)
            {
                throw new ArgumentNullException(nameof(configProgressiveLevels));
            }

            _selectedGame = selectedGame ?? throw new ArgumentNullException(nameof(selectedGame));
            _configSharedSapLevels =
                configSharedSapLevels ?? throw new ArgumentNullException(nameof(configSharedSapLevels));
            _configLinkedProgressiveNames = configLinkedProgressiveNames ??
                                            throw new ArgumentNullException(nameof(configLinkedProgressiveNames));

            var container = ServiceManager.GetInstance().GetService<IContainerService>();
            _progressives = container.Container.GetInstance<IProgressiveConfigurationProvider>();
            _linkedProgressives = container.Container.GetInstance<ILinkedProgressiveProvider>();
            _gameService = ServiceManager.GetInstance().GetService<IGameService>();
            _gameProvider = ServiceManager.GetInstance().GetService<IGameProvider>();
            _propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            IsSummaryView = isSummaryView;
            SelectedGameInfo = $"{_selectedGame.ThemeName} | {selectedGame.PaytableId} | {_selectedGame.Denomination}";
            GenerateCSAPLevelsCommand = new ActionCommand<object>(GenerateCSAPLevelsPressed);

            var progressiveLevels = configProgressiveLevels.Any()
                ? configProgressiveLevels
                : _progressives.ViewProgressiveLevels();

            var viewableProgressiveLevels = progressiveLevels.ToList();
            _validProgressiveLevels = viewableProgressiveLevels
                .Where(
                    l => l.GameId == _selectedGame.Id &&
                         l.Denomination.Contains(_selectedGame.DenominationValue) &&
                         (l.BetOption is null || !l.BetOption.Any() || l.BetOption == betOption?.Name))
                .ToList();

            _isAssociatedSap = _validProgressiveLevels.Any(
                p => p.AssignedProgressiveId.AssignedProgressiveType == AssignableProgressiveType.AssociativeSap);

            IsConfigurableLinkedLevelId = _propertiesManager.GetValue(GamingConstants.ProgressiveConfigurableLinkedLeveId, false);
        }

        public ProgressiveSetupViewModel(
            ReadOnlyGameConfiguration selectedGame,
            BetOption betOption,
            bool isSummaryView = false)
            : this(
                selectedGame,
                betOption,
                isSummaryView,
                new List<IViewableProgressiveLevel>(),
                new List<IViewableSharedSapLevel>(),
                new List<string>())
        {
        }

        public ICommand GenerateCSAPLevelsCommand { get; }

        public bool GenerateCSAPLevelsAllowed => !IsSummaryView &&
                                                 ProgressiveLevels != null &&
                                                 ProgressiveLevels.Any(
                                                     p => p.LevelType == ProgressiveLevelType.Selectable &&
                                                          p.SelectableLevelType.Equals(Resources.StandaloneProgressive));

        public int ProgressiveGroupId
        {
            get => _progressiveGroupId;
            set
            {
                _progressiveGroupId = value;
                RaisePropertyChanged(nameof(ProgressiveGroupId));
            }
        }

        public override bool CanSave => (ProgressiveLevels?.All(x => x.CanSave) ?? true) && !OverMaximumAllowableProgressives;

        public bool IsSummaryView
        {
            get => _isSummaryView;
            set
            {
                _isSummaryView = value;
                RaisePropertyChanged(nameof(IsSummaryView));
                RaisePropertyChanged(nameof(ProgressiveTypeEditable));
                RaisePropertyChanged(nameof(ProgressiveTypeReadOnly));
                RaisePropertyChanged(nameof(ProgressiveLevelEditable));
                RaisePropertyChanged(nameof(ProgressiveLevelReadOnly));
                RaisePropertyChanged(nameof(InitialValueEditable));
                RaisePropertyChanged(nameof(InitialValueReadOnly));
                RaisePropertyChanged(nameof(ShowAssociatedSap));
                RaisePropertyChanged(nameof(OverflowValueEditable));
                RaisePropertyChanged(nameof(OverflowValueReadOnly));
            }
        }

        public bool IsSelectable
        {
            get => _isSelectable;
            set
            {
                _isSelectable = value;
                RaisePropertyChanged(nameof(IsSelectable));
                RaisePropertyChanged(nameof(IsSelectableOrLP));
                RaisePropertyChanged(nameof(ProgressiveTypeEditable));
                RaisePropertyChanged(nameof(ProgressiveTypeReadOnly));
                RaisePropertyChanged(nameof(ProgressiveLevelEditable));
                RaisePropertyChanged(nameof(ProgressiveLevelReadOnly));
                RaisePropertyChanged(nameof(ShowAssociatedSap));
            }
        }

        public bool IsSap
        {
            get => _isSap;
            set
            {
                _isSap = value;
                RaisePropertyChanged(nameof(IsSap));
                RaisePropertyChanged(nameof(ProgressiveTypeEditable));
                RaisePropertyChanged(nameof(ProgressiveTypeReadOnly));
                RaisePropertyChanged(nameof(ProgressiveLevelEditable));
                RaisePropertyChanged(nameof(ProgressiveLevelReadOnly));
                RaisePropertyChanged(nameof(InitialValueEditable));
                RaisePropertyChanged(nameof(InitialValueReadOnly));
                RaisePropertyChanged(nameof(ShowAssociatedSap));
                RaisePropertyChanged(nameof(OverflowValueEditable));
                RaisePropertyChanged(nameof(OverflowValueReadOnly));
            }
        }

        public bool IsLP
        {
            get => _isLP;
            set
            {
                _isLP = value;
                RaisePropertyChanged(nameof(IsLP));
                RaisePropertyChanged(nameof(IsSapOrLP));
                RaisePropertyChanged(nameof(IsSelectableOrLP));
                RaisePropertyChanged(nameof(ProgressiveTypeReadOnly));
                RaisePropertyChanged(nameof(ShowAssociatedSap));
            }
        }

        public bool IsSapOrLP => IsSap || IsLP;

        public bool IsSelectableOrLP => IsSelectable || IsLP;

        /// <summary>
        ///    G2S Vertex progressives aren't discoverable in advance. So they must be configured in the UI, then validated against Vertex.
        ///    This results in needing this UI to allows editing fields to configured the Progressive Group for all these levels to link to
        ///    and the specific vertex level id for each individual level. This property controls the logic related to these configurable ids
        /// </summary>
        public bool IsConfigurableLinkedLevelId { get; set; }

        public bool ProgressiveTypeEditable => !IsSummaryView && IsSelectable;

        public bool ProgressiveTypeReadOnly => IsSummaryView && IsSelectable || IsSapOrLP;

        public bool ProgressiveLevelEditable => !IsSummaryView && IsSelectable;

        public bool ProgressiveLevelReadOnly => IsSummaryView && IsSelectable;

        public bool InitialValueEditable => !IsSummaryView && IsSap;

        public bool InitialValueReadOnly => IsSummaryView && IsSap;

        public bool OverflowValueEditable => !IsSummaryView && IsSap;

        public bool OverflowValueReadOnly => IsSummaryView && IsSap;

        public bool ShowAssociatedSap => !IsSummaryView && IsSap && _isAssociatedSap;

        public string SelectedGameInfo
        {
            get => _selectedGameInfo;
            set
            {
                _selectedGameInfo = value;
                RaisePropertyChanged(nameof(SelectedGameInfo));
            }
        }

        public ObservableCollection<LevelModel> ProgressiveLevels
        {
            get => _levelModels;
            set
            {
                _levelModels = value;
                RaisePropertyChanged(nameof(_levelModels));
            }
        }

        public new string InputStatusText =>
            OverMaximumAllowableProgressives
                ? string.Format(
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OverMaximumAllowableProgressivesErrorText),
                    NumberOfEnabledProgressives,
                    MaxEnabledProgressivesAllowed)
                : "";

        /// <inheritdoc/>
        public override void Save()
        {
            SetupCompleted = true;

            if (IsProgressiveLevelsUnchanged())
            {
                return;
            }

            var levelAssignmentList = ProgressiveLevels.Select(
                    progressiveLevel => new ProgressiveLevelAssignment(
                        _selectedGame.GameDetail,
                        _selectedGame.DenominationValue,
                        progressiveLevel.AssociatedProgressiveLevel,
                        progressiveLevel.AssignedProgressiveInfo,
                        progressiveLevel.InitialValue.DollarsToMillicents(),
                        0,
                        progressiveLevel.OverflowValue.DollarsToMillicents()))
                .ToList();

            if (_gameService.Running &&
                PropertiesManager.GetValue(GamingConstants.SelectedGameId, 0) == _selectedGame.Id &&
                PropertiesManager.GetValue(GamingConstants.SelectedDenom, 0L) == _selectedGame.DenominationValue)
            {
                _gameService.ShutdownBegin();
            }
            _progressives.AssignLevelsToGame(levelAssignmentList);

            if (IsConfigurableLinkedLevelId)
            {
                Dictionary<int, (int linkedGroupId, int linkedLevelId)> configuredLevelIds = _propertiesManager.GetValue(GamingConstants.ProgressiveConfiguredLinkedLevelIds, new Dictionary<int, (int linkedGroupId, int linkedLevelId)>());
                foreach (var progLevel in ProgressiveLevels)
                {
                    (int linkedGroupId, int linkedLevelId) newConfig = (ProgressiveGroupId, progLevel.ConfigurableLinkedLevelId);

                    if (!configuredLevelIds.ContainsKey(progLevel.AssociatedProgressiveLevel.DeviceId))
                    {
                        ConfigurableProgressiveLevelsChanged = true;
                    }
                    else if (configuredLevelIds.TryGetValue(progLevel.AssociatedProgressiveLevel.DeviceId, out (int linkedGroupId, int linkedLevelId) levelConfig))
                    {
                        if (!ConfigurableProgressiveLevelsChanged)
                        {
                            ConfigurableProgressiveLevelsChanged = (newConfig != levelConfig);
                        }
                    }

                    configuredLevelIds[progLevel.AssociatedProgressiveLevel.DeviceId] = newConfig;
                }

                _propertiesManager.SetProperty(GamingConstants.ProgressiveConfiguredLinkedLevelIds, configuredLevelIds);
            }
        }

        /// <inheritdoc/>
        protected override void OnLoaded()
        {
            LoadConfigurableLinkedLevelsData();
            LoadData();
            base.OnLoaded();
        }

        protected override void DisposeInternal()
        {
            foreach (var model in _levelModels)
            {
                model.PropertyChanged -= LevelModel_PropertyChanged;
            }

            _levelModels.Clear();
            base.DisposeInternal();
        }

        private bool IsProgressiveLevelsUnchanged()
        {
            var nonSapLevels = ProgressiveLevels.Where(l => l.LevelType != ProgressiveLevelType.Sap).ToList();

            var configurableLevelIdsUnchanged = ProgressiveLevels.All(l => l.OriginalConfigurableLinkedLevelId == l.ConfigurableLinkedLevelId);

            return _selectedGame.ProgressiveSetupConfigured &&
                nonSapLevels.Count != 0 &&
                _originalNonSapProgressiveLevels.Count != 0 &&
                nonSapLevels.Select(x => (x.SelectableLevel, x.SelectableLevelType)).SequenceEqual(_originalNonSapProgressiveLevels) &&
                _progressiveGroupId == _originalProgressiveGroupId &&
                configurableLevelIdsUnchanged;
        }

        private void GenerateCSAPLevelsPressed(object obj)
        {
            var sharedSapProvider = ServiceManager.GetInstance().TryGetService<ISharedSapProvider>();
            if (sharedSapProvider is null)
            {
                return;
            }

            var newSharedSapLevels = sharedSapProvider.AutoGenerateLevels(
                _selectedGame.GameType,
                _selectedGame.DenominationValue,
                ProgressiveLevels.Where(
                        p => p.LevelType == ProgressiveLevelType.Selectable &&
                             p.SelectableLevelType.Equals(Resources.StandaloneProgressive))
                    .Select(l => l.AssociatedProgressiveLevel));

            if (newSharedSapLevels.Any())
            {
                foreach (var level in ProgressiveLevels)
                {
                    var customSapLevels = GenerateValidSharedSapLevels(level.AssociatedProgressiveLevel);
                    level.SetCSAPLevels(customSapLevels);
                }
            }
        }

        private void LoadConfigurableLinkedLevelsData()
        {
            if (!IsConfigurableLinkedLevelId) { return; }

            _configuredLinkedLevelIds = _propertiesManager.GetValue(
                GamingConstants.ProgressiveConfiguredLinkedLevelIds,
                new Dictionary<int, (int linkedGroupId, int linkedLevelId)>());

            //look to see if this set of levels has been configured previously. If so, initialize data to match.
            //otherwise, default to highest configured group + 1
            var firstValidProgressive = _validProgressiveLevels.First();
            if (_configuredLinkedLevelIds.TryGetValue(firstValidProgressive.DeviceId, out (int configuredGroupId, int _) ret))
            {
                ProgressiveGroupId = ret.configuredGroupId;
            }
            else
            {
                var maxConfiguredGroupId = _configuredLinkedLevelIds.Any()
                    ? _configuredLinkedLevelIds.Values.Select(val => val.linkedGroupId).Max()
                    : 0;
                ProgressiveGroupId = maxConfiguredGroupId + 1;
            }

            _originalProgressiveGroupId = ProgressiveGroupId;
        }

        private void LoadData()
        {
            ProgressiveLevels = new ObservableCollection<LevelModel>();
            _originalNonSapProgressiveLevels = new List<(LevelModel.LevelDefinition SelectableLevel, string SelectableLevelType)>();

            // Set view format based on progressive level type.
            // *NOTE* Mixed configurations are currently not supported.
            IsSelectable = _validProgressiveLevels.All(x => x.LevelType == ProgressiveLevelType.Selectable);
            IsSap = _validProgressiveLevels.All(x => x.LevelType == ProgressiveLevelType.Sap);
            IsLP = _validProgressiveLevels.All(x => x.LevelType == ProgressiveLevelType.LP);

            foreach (var level in _validProgressiveLevels)
            {
                var levelModel = CreateProgressiveLevelModel(level);
                levelModel.PropertyChanged += LevelModel_PropertyChanged;
                ProgressiveLevels.Add(levelModel);
                if (level.LevelType != ProgressiveLevelType.Sap) // Sap does not have selectable levels
                {
                    _originalNonSapProgressiveLevels.Add((levelModel.SelectableLevel, levelModel.SelectableLevelType));
                }
            }

            UpdateValidSelectableLevels();

            RaisePropertyChanged(nameof(ProgressiveLevels)); // required so the grid will update
            RaisePropertyChanged(nameof(GenerateCSAPLevelsAllowed));
        }

        private LevelModel CreateProgressiveLevelModel(IViewableProgressiveLevel level)
        {
            var customSapLevels = GenerateValidSharedSapLevels(level);
            var linkedLevels = GenerateValidLinkedLevels(level);
            var gameCount = 0;
            var sharedLevel = default(IViewableSharedSapLevel);

            if (_isAssociatedSap)
            {
                var associatedLevels = _progressives.ViewProgressiveLevels().Select(p => p).Where(
                    p => p.AssignedProgressiveId?.AssignedProgressiveKey != null &&
                         p.AssignedProgressiveId.AssignedProgressiveKey.Equals(
                             level.AssignedProgressiveId.AssignedProgressiveKey)).ToList();
                var gameThemes = new HashSet<string>();
                foreach (var associatedLevel in associatedLevels)
                {
                    gameThemes.Add(_gameProvider.GetGame(associatedLevel.GameId).ThemeId);
                }

                sharedLevel = _progressives.ViewSharedSapLevels().FirstOrDefault(
                    s => s.LevelAssignmentKey.Equals(level.AssignedProgressiveId.AssignedProgressiveKey));

                gameCount = gameThemes.Count;
            }

            var configurableLinkedLevelId = 0;
            if (IsConfigurableLinkedLevelId)
            {
                if (_configuredLinkedLevelIds.TryGetValue(level.DeviceId, out (int _, int configuredLevelId) ret))
                {
                    configurableLinkedLevelId = ret.configuredLevelId;
                }
                else
                {
                    //vertex Ids are 1-indexed, so try to provide a helpful default
                    configurableLinkedLevelId = level.LevelId + 1; 
                }
            }

            return new LevelModel(level, customSapLevels, linkedLevels, gameCount, sharedLevel, configurableLinkedLevelId);
        }

        private IReadOnlyCollection<IViewableSharedSapLevel> GenerateValidSharedSapLevels(
            IViewableProgressiveLevel targetLevel)
        {
            var validSharedSapLevels = new List<IViewableSharedSapLevel>();

            if (targetLevel.LevelType == ProgressiveLevelType.Sap &&
                (targetLevel.AssignedProgressiveId.AssignedProgressiveType != AssignableProgressiveType.CustomSap ||
                 targetLevel.AssignedProgressiveId.AssignedProgressiveType != AssignableProgressiveType.AssociativeSap) ||
                targetLevel.LevelType == ProgressiveLevelType.LP)
            {
                // We only generate levels for shared SAP if the level type is selectable
                return validSharedSapLevels;
            }

            var sapLevels = _configSharedSapLevels.Any() ? _configSharedSapLevels : _progressives.ViewSharedSapLevels();
            validSharedSapLevels.AddRange(sapLevels.Where(
                x =>
                    x.SupportedGameTypes.Any(gameType => _selectedGame.GameType == gameType) &&
                    x.ResetValue >= targetLevel.ResetValue &&
                    x.MaximumValue <= targetLevel.MaximumValue &&
                    x.CurrentValue >= targetLevel.ResetValue));

            return validSharedSapLevels;
        }

        private IReadOnlyCollection<IViewableLinkedProgressiveLevel> GenerateValidLinkedLevels(
            IViewableProgressiveLevel targetLevel)
        {
            var validLinkedProgressiveLevels = new List<IViewableLinkedProgressiveLevel>();

            if (targetLevel.LevelType == ProgressiveLevelType.Sap)
            {
                return validLinkedProgressiveLevels;
            }

            var linkedProgressives = _configLinkedProgressiveNames.Any()
                ? _progressives.ViewLinkedProgressiveLevels()
                    .Where(
                        x => x.Amount >= targetLevel.ResetValue.MillicentsToCents() &&
                             _configLinkedProgressiveNames.Any(linkedName => linkedName == x.LevelName))
                : _progressives.ViewLinkedProgressiveLevels()
                    .Where(x => x.Amount >= targetLevel.ResetValue.MillicentsToCents());

            // Linked progressive amounts are in cents
            validLinkedProgressiveLevels.AddRange(linkedProgressives);

            return validLinkedProgressiveLevels;
        }

        private void UpdateValidSelectableLevels()
        {
            var currentSelectedLevels = ProgressiveLevels.Where(
                    l => l.AssignedProgressiveInfo.AssignedProgressiveType != AssignableProgressiveType.None &&
                         l.SelectableLevel != null)
                .Select(
                    x => (Key: x.SelectableLevel.AssignmentKey, ProgressiveType: x.AssignedProgressiveInfo.AssignedProgressiveType))
                .ToList();

            foreach (var progressiveLevel in ProgressiveLevels.Where(
                x => x.AssociatedProgressiveLevel.LevelType != ProgressiveLevelType.Sap))
            {
                var assignedProgressiveKey = progressiveLevel.AssignedProgressiveInfo.AssignedProgressiveKey;
                var linkedLevels = GenerateValidLinkedLevels(progressiveLevel.AssociatedProgressiveLevel);
                var sharedSapLevels = GenerateValidSharedSapLevels(progressiveLevel.AssociatedProgressiveLevel);

                var filteredLinkedList = linkedLevels.Where(
                    l => l.LevelName.Equals(assignedProgressiveKey) ||
                         !currentSelectedLevels.Any(
                             x => x.ProgressiveType == AssignableProgressiveType.Linked &&
                                  x.Key == l.LevelName)).ToList();
                var filteredSapList = sharedSapLevels.Where(
                    l => l.LevelAssignmentKey == assignedProgressiveKey ||
                         !currentSelectedLevels.Any(
                             x => x.ProgressiveType == AssignableProgressiveType.CustomSap &&
                                  x.Key == l.LevelAssignmentKey)).ToList();

                progressiveLevel.PropertyChanged -= LevelModel_PropertyChanged;
                progressiveLevel.SetLinkedLevels(filteredLinkedList);
                progressiveLevel.SetCSAPLevels(filteredSapList);
                progressiveLevel.PropertyChanged += LevelModel_PropertyChanged;
            }
        }

        private void LevelModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(LevelModel.SelectableLevel):
                    UpdateValidSelectableLevels();
                    break;
                case nameof(LevelModel.SelectableLevelType):
                    RaisePropertyChanged(nameof(GenerateCSAPLevelsAllowed));
                    break;
                case nameof(LevelModel.CanSave):
                    RaisePropertyChanged(nameof(CanSave));
                    break;
            }
            RaisePropertyChanged(nameof(InputStatusText));
        }

        private int NumberOfEnabledProgressives => ProgressiveLevels?.Where(
            x => x.SelectableLevelType != Resources.NoProgressive).Count() ?? 0;

        private int MaxEnabledProgressivesAllowed =>
            _selectedGame.GameDetail.MaximumProgressivePerDenom ?? (ProgressiveLevels?.Count ?? 0);

        private bool OverMaximumAllowableProgressives => NumberOfEnabledProgressives > MaxEnabledProgressivesAllowed;
    }
}