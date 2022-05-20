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
        private readonly IGameService _gameService;
        private readonly IGameProvider _gameProvider;

        private readonly ReadOnlyGameConfiguration _selectedGame;
        private readonly IReadOnlyCollection<IViewableProgressiveLevel> _validProgressiveLevels;
        private readonly IReadOnlyCollection<IViewableSharedSapLevel> _configSharedSapLevels;
        private readonly IReadOnlyCollection<string> _configLinkedProgressiveNames;
        private readonly bool _isAssociatedSap;

        private string _selectedGameInfo;
        private ObservableCollection<LevelModel> _levelModels;
        private bool _isSummaryView;
        private bool _isSelectable;
        private bool _isSap;
        private bool _isLP;

        private List<(LevelModel.LevelDefinition SelectableLevel, string SelectableLevelType)> _originalNonSapProgressiveLevels;

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
            _gameService = ServiceManager.GetInstance().GetService<IGameService>();
            _gameProvider = ServiceManager.GetInstance().GetService<IGameProvider>();

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

        public bool ProgressiveTypeEditable => !IsSummaryView && IsSelectable;

        public bool ProgressiveTypeReadOnly => IsSummaryView && IsSelectable || IsSapOrLP;

        public bool ProgressiveLevelEditable => !IsSummaryView && IsSelectable;

        public bool ProgressiveLevelReadOnly => IsSummaryView && IsSelectable;

        public bool InitialValueEditable => !IsSummaryView && IsSap;

        public bool InitialValueReadOnly => IsSummaryView && IsSap;

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
                        progressiveLevel.InitialValue.DollarsToMillicents()))
                .ToList();

            if (_gameService.Running &&
                PropertiesManager.GetValue(GamingConstants.SelectedGameId, 0) == _selectedGame.Id &&
                PropertiesManager.GetValue(GamingConstants.SelectedDenom, 0L) == _selectedGame.DenominationValue)
            {
                _gameService.ShutdownBegin();
            }

            _progressives.AssignLevelsToGame(levelAssignmentList);
        }

        /// <inheritdoc/>
        protected override void OnLoaded()
        {
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

            return _selectedGame.ProgressiveSetupConfigured &&
                nonSapLevels.Count != 0 &&
                _originalNonSapProgressiveLevels.Count != 0 &&
                nonSapLevels.Select(x => (x.SelectableLevel, x.SelectableLevelType)).SequenceEqual(_originalNonSapProgressiveLevels);
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

            return new LevelModel(level, customSapLevels, linkedLevels, gameCount, sharedLevel);
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
