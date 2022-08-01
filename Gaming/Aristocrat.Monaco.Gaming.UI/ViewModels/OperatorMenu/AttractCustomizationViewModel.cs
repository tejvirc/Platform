namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Input;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.OperatorMenu;
    using Contracts;
    using Contracts.Models;
    using Kernel;
    using Localization.Properties;
    using Monaco.UI.Common.Extensions;
    using MVVM;
    using MVVM.Command;
    using Vgt.Client12.Application.OperatorMenu;

    public class AttractCustomizationViewModel : OperatorMenuSaveViewModelBase
    {
        private readonly IAttractConfigurationProvider _attractProvider;
        private readonly IGameProvider _gameProvider;
        private readonly IPropertiesManager _propertiesManager;

        private List<IAttractInfo> _previousAttractInfo;
        private ObservableCollection<IAttractInfo> _configuredAttractInfo;

        private bool _slotAttractSelected;
        private bool _kenoAttractSelected;
        private bool _pokerAttractSelected;
        private bool _blackjackAttractSelected;
        private bool _rouletteAttractSelected;
        private bool _configuredAttractChanged;

        private IAttractInfo _selectedItem;

        public enum MoveBehavior
        {
            MoveUp,
            MoveDown,
            MoveToTop,
            MoveToBottom
        }

        public AttractCustomizationViewModel()
        {
            _attractProvider = ServiceManager.GetInstance().GetService<IAttractConfigurationProvider>();
            _gameProvider = ServiceManager.GetInstance().GetService<IGameProvider>();
            _propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            EventBus.Subscribe<OperatorMenuExitingEvent>(this, HandleEvent);

            CancelButtonText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Cancel);

            MoveUpCommand = new ActionCommand<object>(MoveUpButton_Clicked);
            MoveDownCommand = new ActionCommand<object>(MoveDownButton_Clicked);
            MoveToTopCommand = new ActionCommand<object>(MoveToTopButton_Clicked);
            MoveToBottomCommand = new ActionCommand<object>(MoveToBottomButton_Clicked);

            RestoreDefaultCommand = new ActionCommand<object>(RestoreDefaultButton_Clicked);

            Init(true);
        }

        private void Init(bool skipRaisePropertyChanged = false)
        {
            _selectedItem = null;
            _configuredAttractChanged = false;

            var configuredSequence = _attractProvider.GetAttractSequence().ToList();
            _previousAttractInfo = configuredSequence.Select(o => (IAttractInfo)o.Clone()).ToList();

            _configuredAttractInfo = new ObservableCollection<IAttractInfo>(configuredSequence);
            if (!skipRaisePropertyChanged)
            {
                RaisePropertyChanged(nameof(ConfiguredAttractInfo));
            }
            
            AddAttractItemSelectedHandler();

            var slotOptionText = PropertiesManager.GetValue(GamingConstants.OverridenSlotGameTypeText, string.Empty);
            if (string.IsNullOrEmpty(slotOptionText))
            {
                SlotTextLabel = Localizer.For(CultureFor.Operator)
                    .GetString(ResourceKeys.Slot);
            }
            else
            {
                var localizedText = Localizer.For(CultureFor.Operator).GetString(slotOptionText);
                SlotTextLabel = string.IsNullOrEmpty(localizedText)
                    ? Localizer.For(CultureFor.Operator)
                        .GetString(ResourceKeys.Slot)
                    : localizedText;
            }

            UpdateGameTypeSelection(skipRaisePropertyChanged);
        }

        public ObservableCollection<IAttractInfo> ConfiguredAttractInfo
        {
            get => _configuredAttractInfo;
            private set
            {
                _configuredAttractInfo = value;
                RaisePropertyChanged(nameof(ConfiguredAttractInfo));
            }
        }

        public IAttractInfo SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                RaisePropertyChanged(nameof(ChangeOrderButtonsEnabled));
            }
        }

        public bool ChangeOrderButtonsEnabled => SelectedItem != null;

        public bool SlotAttractSelected
        {
            get => _slotAttractSelected;
            set
            {
                if (_slotAttractSelected != value)
                {
                    _slotAttractSelected = value;
                    AttractSelectionChanged(GameType.Slot, _slotAttractSelected);
                }
            }
        }

        public bool KenoAttractSelected
        {
            get => _kenoAttractSelected;
            set
            {
                if (_kenoAttractSelected != value)
                {
                    _kenoAttractSelected = value;
                    AttractSelectionChanged(GameType.Keno, _kenoAttractSelected);
                }
            }
        }

        public bool PokerAttractSelected
        {
            get => _pokerAttractSelected;
            set
            {
                if (_pokerAttractSelected != value)
                {
                    _pokerAttractSelected = value;
                    AttractSelectionChanged(GameType.Poker, _pokerAttractSelected);
                }
            }
        }

        public bool BlackjackAttractSelected
        {
            get => _blackjackAttractSelected;
            set
            {
                if (_blackjackAttractSelected != value)
                {
                    _blackjackAttractSelected = value;
                    AttractSelectionChanged(GameType.Blackjack, _blackjackAttractSelected);
                }
            }
        }

        public bool RouletteAttractSelected
        {
            get => _rouletteAttractSelected;
            set
            {
                if (_rouletteAttractSelected != value)
                {
                    _rouletteAttractSelected = value;
                    AttractSelectionChanged(GameType.Roulette, _rouletteAttractSelected);
                }
            }
        }

        public bool SlotAttractOptionEnabled =>
            _gameProvider.GetAllGames().Any(a => a.GameType == GameType.Slot && a.Enabled);

        public bool KenoAttractOptionEnabled =>
            _gameProvider.GetAllGames().Any(a => a.GameType == GameType.Keno && a.Enabled);

        public bool PokerAttractOptionEnabled =>
            _gameProvider.GetAllGames().Any(a => a.GameType == GameType.Poker && a.Enabled);

        public bool BlackjackAttractOptionEnabled =>
            _gameProvider.GetAllGames().Any(a => a.GameType == GameType.Blackjack && a.Enabled);

        public bool RouletteAttractOptionEnabled =>
            _gameProvider.GetAllGames().Any(a => a.GameType == GameType.Roulette && a.Enabled);

        public string SlotTextLabel { get; private set; }

        public ICommand MoveUpCommand { get; set; }

        public ICommand MoveDownCommand { get; set; }

        public ICommand MoveToTopCommand { get; set; }

        public ICommand MoveToBottomCommand { get; set; }

        public ICommand RestoreDefaultCommand { get; set; }

        public override bool HasChanges()
        {
            if (!_propertiesManager.GetValue(GamingConstants.DefaultAttractSequenceOverridden, false)
                || !_previousAttractInfo.SequenceEqual(
                    ConfiguredAttractInfo.ToList(),
                    new AttractInfoComparer()))
            {
                return true;
            }

            return false;
        }

        public override void Save()
        {
            _attractProvider.SaveAttractSequence(_configuredAttractInfo);
            _previousAttractInfo.Clear();
            _previousAttractInfo = _configuredAttractInfo.Select(o => (IAttractInfo)o.Clone()).ToList();

            _configuredAttractChanged = true;

            DialogResult = true;

            _propertiesManager.SetProperty(GamingConstants.DefaultAttractSequenceOverridden, true);
        }

        private void HandleAttractItemSelected(object sender, PropertyChangedEventArgs e)
        {
            if (sender is IAttractInfo ai)
            {
                if (ai.IsSelected)
                {
                    // If all items of a game type are selected, then we check the game type as well
                    if (!ConfiguredAttractInfo.Any(g => g.GameType == ai.GameType && !g.IsSelected))
                    {
                        switch (ai.GameType)
                        {
                            case GameType.Slot:
                                _slotAttractSelected = true;
                                RaisePropertyChanged(nameof(SlotAttractSelected));

                                break;
                            case GameType.Keno:
                                _kenoAttractSelected = true;
                                RaisePropertyChanged(nameof(KenoAttractSelected));

                                break;
                            case GameType.Poker:
                                _pokerAttractSelected = true;
                                RaisePropertyChanged(nameof(PokerAttractSelected));

                                break;
                            case GameType.Blackjack:
                                _blackjackAttractSelected = true;
                                RaisePropertyChanged(nameof(BlackjackAttractSelected));

                                break;
                            case GameType.Roulette:
                                _rouletteAttractSelected = true;
                                RaisePropertyChanged(nameof(RouletteAttractSelected));

                                break;
                        }
                    }
                }
                else
                {
                    // If any item of a game type is unselected, then we uncheck the game type as well
                    if (ConfiguredAttractInfo.Any(g => g.GameType == ai.GameType && !g.IsSelected))
                    {
                        switch (ai.GameType)
                        {
                            case GameType.Slot:
                                _slotAttractSelected = false;
                                RaisePropertyChanged(nameof(SlotAttractSelected));

                                break;
                            case GameType.Keno:
                                _kenoAttractSelected = false;
                                RaisePropertyChanged(nameof(KenoAttractSelected));

                                break;
                            case GameType.Poker:
                                _pokerAttractSelected = false;
                                RaisePropertyChanged(nameof(PokerAttractSelected));

                                break;
                            case GameType.Blackjack:
                                _blackjackAttractSelected = false;
                                RaisePropertyChanged(nameof(BlackjackAttractSelected));

                                break;
                            case GameType.Roulette:
                                _rouletteAttractSelected = false;
                                RaisePropertyChanged(nameof(RouletteAttractSelected));

                                break;
                        }
                    }
                }
            }

            RaisePropertyChanged(nameof(ConfiguredAttractInfo));
            RaisePropertyChanged(nameof(CanSave));
        }

        public override bool CanSave => HasChanges();

        private void UpdateGameTypeSelection(bool skipPropertyChanged = false)
        {
            _slotAttractSelected = !ConfiguredAttractInfo.Any(g => g.GameType == GameType.Slot && !g.IsSelected);

            _kenoAttractSelected = !ConfiguredAttractInfo.Any(g => g.GameType == GameType.Keno && !g.IsSelected);

            _pokerAttractSelected = !ConfiguredAttractInfo.Any(g => g.GameType == GameType.Poker && !g.IsSelected);

            _blackjackAttractSelected =
                !ConfiguredAttractInfo.Any(g => g.GameType == GameType.Blackjack && !g.IsSelected);

            _rouletteAttractSelected =
                !ConfiguredAttractInfo.Any(g => g.GameType == GameType.Roulette && !g.IsSelected);

            if (!skipPropertyChanged)
            {
                RaisePropertyChanged(nameof(SlotAttractSelected));
                RaisePropertyChanged(nameof(KenoAttractSelected));
                RaisePropertyChanged(nameof(PokerAttractSelected));
                RaisePropertyChanged(nameof(BlackjackAttractSelected));
                RaisePropertyChanged(nameof(RouletteAttractSelected));
            }
            
        }

        private void AttractSelectionChanged(GameType gameType, bool selected)
        {
            switch (gameType)
            {
                case GameType.Slot:
                    RaisePropertyChanged(nameof(SlotAttractSelected));

                    break;
                case GameType.Keno:
                    RaisePropertyChanged(nameof(KenoAttractSelected));

                    break;
                case GameType.Poker:
                    RaisePropertyChanged(nameof(PokerAttractSelected));

                    break;
                case GameType.Blackjack:
                    RaisePropertyChanged(nameof(BlackjackAttractSelected));

                    break;
                case GameType.Roulette:
                    RaisePropertyChanged(nameof(RouletteAttractSelected));

                    break;
            }

            foreach (var attractItem in _configuredAttractInfo)
            {
                if (attractItem.GameType == gameType)
                {
                    attractItem.IsSelected = selected;
                }
            }

            RaisePropertyChanged(nameof(ConfiguredAttractInfo));
            RaisePropertyChanged(nameof(CanSave));
        }

        private void MoveUpButton_Clicked(object o)
        {
            if (SelectedItem != null)
            {
                MoveAttractItem(MoveBehavior.MoveUp);
            }
        }

        private void MoveDownButton_Clicked(object o)
        {
            if (SelectedItem != null)
            {
                MoveAttractItem(MoveBehavior.MoveDown);
            }
        }

        private void MoveToTopButton_Clicked(object o)
        {
            if (SelectedItem != null)
            {
                MoveAttractItem(MoveBehavior.MoveToTop);
            }
        }

        private void MoveToBottomButton_Clicked(object o)
        {
            if (SelectedItem != null)
            {
                MoveAttractItem(MoveBehavior.MoveToBottom);
            }
        }

        private void MoveAttractItem(MoveBehavior behavior)
        {
            var itemIndex = _configuredAttractInfo.IndexOf(SelectedItem);
            var newIndex = -1;

            switch (behavior)
            {
                case MoveBehavior.MoveUp:
                    newIndex = itemIndex - 1;

                    break;
                case MoveBehavior.MoveDown:
                    newIndex = itemIndex + 1;

                    break;
                case MoveBehavior.MoveToTop:
                    newIndex = 0;

                    break;
                case MoveBehavior.MoveToBottom:
                    newIndex = _configuredAttractInfo.Count - 1;

                    break;
            }

            if (itemIndex != newIndex && newIndex >= 0 && newIndex < _configuredAttractInfo.Count)
            {
                _configuredAttractInfo.Move(itemIndex, newIndex);
                RaisePropertyChanged(nameof(CanSave));
            }
        }

        private void RestoreDefaultButton_Clicked(object o)
        {
            var defaultSequence = _attractProvider.GetDefaultSequence().ToList();

            if (defaultSequence.SequenceEqual(ConfiguredAttractInfo.ToList(), new AttractInfoComparer()))
            {
                return;
            }

            ConfiguredAttractInfo.Clear();
            ConfiguredAttractInfo.AddRange(defaultSequence);
            AddAttractItemSelectedHandler();

            UpdateGameTypeSelection();
        }

        protected override void OnLoaded()
        {
            var operatorMenuLauncher = ServiceManager.GetInstance().TryGetService<IOperatorMenuLauncher>();
            operatorMenuLauncher?.PreventExit();

            base.OnLoaded();
        }

        protected override void OnUnloaded()
        {
            var operatorMenuLauncher = ServiceManager.GetInstance().TryGetService<IOperatorMenuLauncher>();
            operatorMenuLauncher?.AllowExit();

            if (_configuredAttractChanged)
            {
                EventBus.Publish(new AttractConfigurationChangedEvent());
            }

            base.OnUnloaded();
        }

        private void HandleEvent(OperatorMenuExitingEvent theEvent)
        {
            MvvmHelper.ExecuteOnUI(Cancel);
        }

        private void AddAttractItemSelectedHandler()
        {
            foreach (var ai in ConfiguredAttractInfo)
            {
                ai.PropertyChanged += HandleAttractItemSelected;
            }
        }

        private sealed class AttractInfoComparer : IEqualityComparer<IAttractInfo>
        {
            public bool Equals(IAttractInfo x, IAttractInfo y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                return x.GameType == y.GameType && x.IsSelected == y.IsSelected && x.ThemeId == y.ThemeId
                       && x.SequenceNumber == y.SequenceNumber;
            }

            public int GetHashCode(IAttractInfo obj)
            {
                return (obj.GameType, obj.ThemeId, obj.IsSelected, obj.SequenceNumber).GetHashCode();
            }
        }
    }
}