namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Application.Contracts.Extensions;
    using Application.UI.OperatorMenu;
    using Models;
    using Contracts.Barkeeper;
    using Kernel;
    using MVVM.Command;
    using Aristocrat.Monaco.Gaming.Contracts;

    public class BarkeeperConfigurationViewModel : OperatorMenuPageViewModelBase
    {
        private readonly IBarkeeperHandler _barkeeperHandler;
        private string _cashInSessionMeter;
        private string _coinInSessionMeter;
        private BarkeeperRewardLevels _rewardLevels;
        private List<CoinInRewardLevel> _coinInRewardLevels;
        private List<CashInRewardLevel> _cashInRewardLevels;
        private long _rewardLevelCoinInAmount;
        private bool _coinInRateEnabled;

        public BarkeeperConfigurationViewModel()
            : this(ServiceManager.GetInstance().GetService<IBarkeeperHandler>())
        {

            CoinInRateEnabledChangedCommand = new ActionCommand<object>(
                _ =>
                {
                    RaisePropertyChanged(nameof(RewardLevels));
                    RaisePropertyChanged(nameof(RewardLevels.CoinInStrategy.CoinInRate.Enabled));
                });

            CashInEnabledChangedCommand = new ActionCommand<object>(
                _ =>
                {
                    RaisePropertyChanged(nameof(CashInRewardLevel));
                    RaisePropertyChanged(nameof(CashInRewardLevel.Enabled));
                });
        }

        public ICommand CoinInRateEnabledChangedCommand { get; }

        public ICommand CashInEnabledChangedCommand { get; }

        public BarkeeperConfigurationViewModel(IBarkeeperHandler barkeeperHandler)
        {
            _barkeeperHandler = barkeeperHandler;
        }

        public string CashInSessionMeter
        {
            get => _cashInSessionMeter;
            set => SetProperty(ref _cashInSessionMeter, value, nameof(CashInSessionMeter));
        }

        public string CoinInSessionMeter
        {
            get => _coinInSessionMeter;
            set => SetProperty(ref _coinInSessionMeter, value, nameof(CoinInSessionMeter));
        }

        public long RewardLevelCoinInAmount
        {
            get => _rewardLevelCoinInAmount;
            set
            {
                if (SetProperty(ref _rewardLevelCoinInAmount, value, nameof(RewardLevelCoinInAmount)))
                {
                    ValidateCoinInRate();
                }
            }
        }

        public bool CoinInRateEnabled
        {
            get => _coinInRateEnabled;
            set
            {
                SetProperty(ref _coinInRateEnabled, value, nameof(CoinInRateEnabled));
                ValidateCoinInRate();
                RaisePropertyChanged(nameof(RewardLevelCoinInAmount));
            }
        }

        public BarkeeperRewardLevels RewardLevels
        {
            get => _rewardLevels;
            set
            {
                SetProperty(ref _rewardLevels, value, nameof(RewardLevels), nameof(RewardLevelsEnabled));

                CoinInRewardLevels = (from rewardLevel in _rewardLevels.RewardLevels
                    where
                        CoinInStrategyPredicate(rewardLevel)
                    select new CoinInRewardLevel()
                    {
                        Name = rewardLevel.Name,
                        Enabled = rewardLevel.Enabled,
                        ThresholdInCents = rewardLevel.ThresholdInCents,
                        Alert = rewardLevel.Alert,
                        Color = rewardLevel.Color
                    }).ToList();

                CashInRewardLevels = (from rewardLevel in _rewardLevels.RewardLevels
                    where
                        CashInStrategyPredicate(rewardLevel)
                    select new CashInRewardLevel()
                    {
                        Name = rewardLevel.Name,
                        Enabled = rewardLevel.Enabled,
                        ThresholdInCents = rewardLevel.ThresholdInCents,
                        Alert = rewardLevel.Alert,
                        Color = rewardLevel.Color
                    }).ToList();
            }
        }

        public bool RewardLevelsEnabled
        {
            get => _rewardLevels.Enabled;
            set
            {
                _rewardLevels.Enabled = value;
                RaisePropertyChanged(nameof(RewardLevels), nameof(RewardLevelsEnabled), nameof(CoinInRewardLevels),
                    nameof(CoinInRewardLevelsExist), nameof(CashInRewardLevels), nameof(CashInRewardLevelsExist));
            }
        }

        public List<CoinInRewardLevel> CoinInRewardLevels
        {
            get => _coinInRewardLevels;
            set => SetProperty(ref _coinInRewardLevels, value,
                nameof(CoinInRewardLevels), nameof(CoinInRewardLevelsExist));
        }

        public List<CashInRewardLevel> CashInRewardLevels
        {
            get => _cashInRewardLevels;
            set => SetProperty(ref _cashInRewardLevels, value,
                nameof(CashInRewardLevels), nameof(CashInRewardLevelsExist));
        }

        public bool CoinInRewardLevelsExist => _rewardLevels.Enabled && _coinInRewardLevels.Count > 0;

        public bool CashInRewardLevelsExist => _rewardLevels.Enabled && _cashInRewardLevels.Count > 0;

        protected override void OnLoaded()
        {
            base.OnLoaded();
            RewardLevels = new BarkeeperRewardLevels(_barkeeperHandler.RewardLevels);
            CashInSessionMeter =
                _barkeeperHandler.CreditsInDuringSession.MillicentsToDollars().FormattedCurrencyString();
            CoinInSessionMeter = _barkeeperHandler.CoinInDuringSession.MillicentsToDollars().FormattedCurrencyString();

            UpdateRewardLevels();
            RewardLevelCoinInAmount = _rewardLevels.CoinInStrategy.CoinInRate.Amount;
            CoinInRateEnabled = ValidateCoinInRate() && _rewardLevels.CoinInStrategy.CoinInRate.Enabled;
        }

        protected override void OnUnloaded()
        {
            OnCommitted();
            base.OnUnloaded();
        }

        protected override void OnCommitted()
        {
            base.OnCommitted();
            UpdateRewardLevels();

            RewardLevels.CoinInStrategy.CoinInRate.Enabled = CoinInRateEnabled && ValidateCoinInRate();

            if (RewardLevels.CoinInStrategy.CoinInRate.Enabled)
            {
                RewardLevels.CoinInStrategy.CoinInRate.Amount = RewardLevelCoinInAmount;
            }

            if (Committed || _barkeeperHandler.RewardLevels.Equals(RewardLevels))
            {
                return;
            }

            _barkeeperHandler.RewardLevels = RewardLevels;
        }

        protected override void SetError(string propertyName, string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                ClearErrors(propertyName);
            }
            else
            {
                base.SetError(propertyName, error);
            }
        }

        private bool ValidateCoinInRate()
        {
            var coinInRateValidate = ((decimal)RewardLevelCoinInAmount.CentsToDollars()).Validate(
                false,
                PropertiesManager.GetValue(GamingConstants.GambleWagerLimit, GamingConstants.DefaultGambleWagerLimit));

            SetError(nameof(RewardLevelCoinInAmount), coinInRateValidate);
            return string.IsNullOrEmpty(coinInRateValidate);
        }

        private void UpdateRewardLevels()
        {
            CoinInRewardLevels?.Where(lvl => !lvl.ValidateThresholdInCents() && lvl.Enabled).ToList()
                .ForEach(lvl => lvl.Enabled = false);
            CashInRewardLevels?.Where(lvl => !lvl.ValidateThresholdInCents() && lvl.Enabled).ToList()
                .ForEach(lvl => lvl.Enabled = false);

            foreach (var rewardLevel in RewardLevels.RewardLevels.Where(CoinInStrategyPredicate))
            {
                var coinInRewardLevel = _coinInRewardLevels.FirstOrDefault(x => x.Name == rewardLevel.Name);
                if (coinInRewardLevel is not null)
                {
                    if (!coinInRewardLevel.ThresholdError)
                    {
                            rewardLevel.ThresholdInCents = coinInRewardLevel.ThresholdInCents;
                    }
                    rewardLevel.Enabled = coinInRewardLevel.Enabled;
                    rewardLevel.Alert = coinInRewardLevel.Alert;
                    rewardLevel.Color = coinInRewardLevel.Color;
                }
            }

            foreach (var rewardLevel in RewardLevels.RewardLevels.Where(CashInStrategyPredicate))
            {
                var cashInRewardLevel = _cashInRewardLevels.FirstOrDefault(x => x.Name == rewardLevel.Name);
                if (cashInRewardLevel is not null)
                {
                    if (!cashInRewardLevel.ThresholdError)
                    {
                        rewardLevel.ThresholdInCents = cashInRewardLevel.ThresholdInCents;
                    }
                    rewardLevel.Enabled = cashInRewardLevel.Enabled;
                    rewardLevel.Alert = cashInRewardLevel.Alert;
                    rewardLevel.Color = cashInRewardLevel.Color;
                }
            }
        }

        private bool CoinInStrategyPredicate(RewardLevel rewardLevel)
        {
            return rewardLevel.TriggerStrategy == BarkeeperStrategy.CoinIn;
        }

        private bool CashInStrategyPredicate(RewardLevel rewardLevel)
        {
            return rewardLevel.TriggerStrategy == BarkeeperStrategy.CashIn;
        }
    }

    public class ColorOptionsProvider
    {
        public Array GetColorOptionsExceptBlack(Type enumType)
        {
            return Enum.GetValues(enumType).Cast<ColorOptions>().Except(new List<ColorOptions>
            {
                ColorOptions.Black
            }).ToArray();
        }
    }
}