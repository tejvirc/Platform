namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using Application.UI.OperatorMenu;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.Application.Contracts.OperatorMenu;
    using Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Models;
    using Aristocrat.Monaco.Gaming.UI.Views.OperatorMenu;
    using Kernel;
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows.Input;
    using Application.Contracts.Localization;
    using Contracts.Progressives;
    using Contracts.Progressives.SharedSap;
    using Localization.Properties;
    using Progressives;
    using CommunityToolkit.Mvvm.Input;

    /// <summary>
    ///     Defines the CustomSAPViewModel class
    /// </summary>
    [CLSCompliant(false)]
    public class CustomSAPViewModel : OperatorMenuPageViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly ISharedSapProvider _sharedSapProvider;
        private readonly IProgressiveConfigurationProvider _configurationProvider;

        private GameType _gameType = GameType.Poker;
        private string _localInputStatusText;

        public ObservableCollection<SharedLevelDisplay> LevelDetails { get; } =
            new ObservableCollection<SharedLevelDisplay>();

        public CustomSAPViewModel()
        {
            _dialogService = ServiceManager.GetInstance().GetService<IDialogService>();
            _sharedSapProvider = ServiceManager.GetInstance().GetService<ISharedSapProvider>();
            _configurationProvider = ServiceManager.GetInstance().GetService<IProgressiveConfigurationProvider>();

            AddSAPLevelCommand = new RelayCommand<object>(AddLevelPressed);
            DeleteSAPLevelCommand = new RelayCommand<string>(DeleteLevelPressed);
            EditSAPLevelCommand = new RelayCommand<string>(EditLevelPressed);

            RefreshLevelDetails();
        }

        public ICommand AddSAPLevelCommand { get; }

        public ICommand DeleteSAPLevelCommand { get; }

        public ICommand EditSAPLevelCommand { get; }

        public bool GameTypeSelect
        {
            get => _gameType == GameType.Keno;
            set
            {
                _gameType = value ? GameType.Keno : GameType.Poker;
                OnPropertyChanged(nameof(_gameType));
                RefreshLevelDetails();
            }
        }

        public string LocalInputStatusText
        {
            get => _localInputStatusText;
            set
            {
                _localInputStatusText = value;
                OnPropertyChanged(nameof(LocalInputStatusText));
            }
        }

        protected override void OnLoaded()
        {
            ClearValidationOnUnload = true;
            SetInputStatusText();
            RefreshLevelDetails();
        }

        protected override void OnFieldAccessEnabledChanged()
        {
            SetInputStatusText();
        }

        private void AddLevelPressed(object obj)
        {
            var viewModel = new AddSAPLevelViewModel(_gameType, _sharedSapProvider);
            _dialogService.ShowDialog<AddSAPLevelView>(
                this,
                viewModel,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LevelCreationTittle));

            RefreshLevelDetails();
        }

        private void DeleteLevelPressed(string levelName)
        {
            var levelToRemove =
                _sharedSapProvider.ViewSharedSapLevels().Where(level => level.Name == levelName);

            var removedLevels =
                _sharedSapProvider.RemoveSharedSapLevel(levelToRemove);

            if (removedLevels.All(level => level.Name != levelName))
            {
                // TODO: Failed to remove. Notify user...
            }

            RefreshLevelDetails();
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        private void EditLevelPressed(string levelName)
        {
            var levelToEdit =
                _sharedSapProvider.ViewSharedSapLevels().FirstOrDefault(level => level.Name == levelName);

            if (levelToEdit is null)
            {
                // TODO: If this operation fails the level was not found
                return;
            }

            var viewModel = new AddSAPLevelViewModel(
                _gameType,
                _sharedSapProvider,
                levelToEdit,
                _configurationProvider.ViewConfiguredProgressiveLevels().Any(
                    x => x.AssignedProgressiveId.AssignedProgressiveType == AssignableProgressiveType.CustomSap &&
                         x.AssignedProgressiveId.AssignedProgressiveKey == levelToEdit.LevelAssignmentKey));

            _dialogService.ShowDialog<AddSAPLevelView>(
                this,
                viewModel,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LevelCreationTittle));
            RefreshLevelDetails();
        }

        private void RefreshLevelDetails()
        {
            LevelDetails.Clear();

            var levels = _sharedSapProvider.ViewSharedSapLevels()
                .Where(level => level.SupportedGameTypes.Contains(_gameType));
            var assignedCustomSapLevels = _configurationProvider.ViewConfiguredProgressiveLevels().Where(
                x => x.AssignedProgressiveId.AssignedProgressiveType == AssignableProgressiveType.CustomSap).ToList();

            // Level index starts at 1.  This is continually refreshed as items are added and removed.
            var levelIndex = 1; 
            foreach (var level in levels.OrderBy(l => l.CreatedDateTime.Ticks))
            {
                LevelDetails.Add(
                    new SharedLevelDisplay(
                        level,
                        levelIndex++,
                        assignedCustomSapLevels.Any(
                            x => x.AssignedProgressiveId.AssignedProgressiveKey == level.LevelAssignmentKey)));
            }

            OnPropertyChanged(nameof(InputEnabled));
        }

        private void SetInputStatusText()
        {
            if (!InputEnabled)
            {
                LocalInputStatusText = InputStatusText;
            }
            else if (!FieldAccessEnabled)
            {
                LocalInputStatusText = FieldAccessRestriction == OperatorMenuAccessRestriction.LogicDoor
                    ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OpenLogicDoor)
                    : InputStatusText;
            }
            else
            {
                LocalInputStatusText = string.Empty;
            }
        }

        public class SharedLevelDisplay
        {
            private readonly IViewableSharedSapLevel _sharedLevel;
            private readonly bool _assigned;

            public SharedLevelDisplay(IViewableSharedSapLevel level, int levelIndex, bool assigned)
            {
                _sharedLevel = level ?? throw new ArgumentNullException(nameof(level));
                LevelId = levelIndex;
                _assigned = assigned;
            }

            /// <summary>
            ///     The current value of the level
            /// </summary>
            public string CurrentValue => _sharedLevel.CurrentValue.MillicentsToDollars().FormattedCurrencyString(true);

            /// <summary>
            ///     The overflow amount
            /// </summary>
            public string OverflowValue => _sharedLevel.Overflow.MillicentsToDollars().FormattedCurrencyString(true);

            /// <summary>
            ///     The initial value of level
            /// </summary>
            public string InitialValue => _sharedLevel.InitialValue.MillicentsToDollarsNoFraction().FormattedCurrencyString();

            /// <summary>
            ///     The reset value of level
            /// </summary>
            public string ResetValue => _sharedLevel.ResetValue.MillicentsToDollarsNoFraction().FormattedCurrencyString();

            /// <summary>
            ///     The maximum value of level
            /// </summary>
            public string MaximumValue =>
                _sharedLevel.MaximumValue == 0 ? string.Empty :
                    _sharedLevel.MaximumValue.MillicentsToDollarsNoFraction().FormattedCurrencyString();

            /// <summary>
            ///     The increment rate of level
            /// </summary>
            public double IncrementRate => (double)_sharedLevel.IncrementRate / GamingConstants.PercentageConversion;

            /// <summary>
            ///     The name of level
            /// </summary>
            public string Name => _sharedLevel.Name;

            /// <summary>
            ///     The editable status of level
            /// </summary>
            public bool IsEditable => _sharedLevel.CanEdit;

            /// <summary>
            ///     Gets whether or not the level can be deleted
            /// </summary>
            public bool CanDelete => IsEditable && !_assigned;

            /// <summary>
            ///     The id of level
            /// </summary>
            public int LevelId { get; }
        }
    }
}
