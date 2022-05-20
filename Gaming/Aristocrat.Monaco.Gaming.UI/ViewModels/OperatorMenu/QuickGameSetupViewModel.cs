namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.OperatorMenu;
    using Contracts;
    using Contracts.Events.OperatorMenu;
    using Contracts.Models;
    using Kernel;
    using Localization.Properties;
    using Models;
    using MVVM.Command;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Input;

    public class QuickGameSetupViewModel : OperatorMenuSaveViewModelBase
    {
        private readonly IGameProvider _gameProvider;

        private long _topAwardValue;

        public QuickGameSetupViewModel()
        {
            AllOffCommand = new ActionCommand<object>(AllOffClicked);
            AllLowCommand = new ActionCommand<object>(AllLowClicked);
            AllMediumCommand = new ActionCommand<object>(AllMediumClicked);
            AllHighCommand = new ActionCommand<object>(AllHighClicked);
            AdvancedSetupCommand = new ActionCommand<object>(AdvancedSetupClicked);

            _gameProvider = ServiceManager.GetInstance().GetService<IGameProvider>();
            var games = _gameProvider.GetAllGames();
            CalculateTopAward(games.Where(game => game.Enabled).ToList());

            GameHoldPercentages = new List<HoldPercentage>();

            var gameTypes = games.Select(g => g.GameType).Distinct();
            foreach (var type in gameTypes)
            {
                if (GameHoldPercentages.All(hp => hp.GameType != type))
                {
                    var hp = new HoldPercentage(type);
                    hp.PropertyChanged += HoldPercentage_PropertyChanged;
                    GameHoldPercentages.Add(hp);
                }
            }

            AdvancedSetupButtonEnabled = GetConfigSetting(
                typeof(GameConfigurationViewModel),
                OperatorMenuSetting.EnableAdvancedConfig,
                false);
        }

        public ICommand AllOffCommand { get; }

        public ICommand AllLowCommand { get; }

        public ICommand AllMediumCommand { get; }

        public ICommand AllHighCommand { get; }

        public ICommand AdvancedSetupCommand { get; }

        public bool AdvancedSetupButtonEnabled { get; }

        public List<HoldPercentage> GameHoldPercentages { get; }

        public long TopAwardValue
        {
            get => _topAwardValue;
            set
            {
                _topAwardValue = value;
                RaisePropertyChanged(nameof(TopAwardValue));
            }
        }

        public override bool HasChanges()
        {
            return GameHoldPercentages.Any(p => p.SelectedOption != null);
        }

        public override void Save()
        {
            SaveChanges();
            EventBus.Publish(new GameSetupFinishedEvent());
        }

        private void SaveChanges()
        {
            // TODO Apply changes to game configuration on save
            var games = _gameProvider.GetAllGames().ToList();
            foreach (var hp in GameHoldPercentages)
            {
                if (hp.SelectedOption != null)
                {
                    foreach (var game in games.Where(g => g.GameType == hp.GameType))
                    {
                        // what do these options mean?
                        //_gameProvider.Configure(game.Id, new GameOptionConfigValues());
                    }

                    hp.SelectedOption = null;
                }
            }
        }

        protected override void Cancel()
        {
            ResetChanges();
            EventBus.Publish(new GameSetupFinishedEvent());
        }

        private void ResetChanges()
        {
            GameHoldPercentages.ForEach(p => p.SelectedOption = null);
        }

        private void CalculateTopAward(IReadOnlyCollection<IGameDetail> games)
        {
            // TODO Recalculate after selecting percentage options
            if (!games.Any())
            {
                TopAwardValue = 0;
            }
            else
            {
                TopAwardValue = games
                    .SelectMany(game => game.Denominations.Where(denom => denom.Active)
                        .Select(denom => game.TopAward(denom))).Max();
            }
        }

        private void HoldPercentage_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(CanSave));
        }

        private void AllOffClicked(object o)
        {
            GameHoldPercentages.ForEach(p => p.SelectedOption = GameHoldPercentageType.Off);
        }

        private void AllLowClicked(object o)
        {
            GameHoldPercentages.ForEach(p => p.SelectedOption = GameHoldPercentageType.Low);
        }

        private void AllMediumClicked(object o)
        {
            GameHoldPercentages.ForEach(p => p.SelectedOption = GameHoldPercentageType.Medium);
        }

        private void AllHighClicked(object o)
        {
            GameHoldPercentages.ForEach(p => p.SelectedOption = GameHoldPercentageType.High);
        }

        private void AdvancedSetupClicked(object o)
        {
            if (HasChanges())
            {
                var dialogService = ServiceManager.GetInstance().GetService<IDialogService>();
                var result = dialogService.ShowYesNoDialog(this, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SavePrompt));

                if (result == true)
                {
                    SaveChanges();
                }
                else if (result == false)
                {
                    ResetChanges();
                }
            }

            EventBus.Publish(new AdvancedSetupEvent());
        }

        protected override void DisposeInternal()
        {
            foreach (var hp in GameHoldPercentages)
            {
                hp.PropertyChanged -= HoldPercentage_PropertyChanged;
            }

            base.DisposeInternal();
        }
    }
}
