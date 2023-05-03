namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Contracts;
    using Contracts.Configuration;
    using Contracts.Models;
    using Contracts.Progressives;
    using Contracts.Rtp;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using MVVM.ViewModel;
    using PackageManifest.Models;
    using Progressives;

    public class EditableGameConfiguration : BaseViewModel
    {
        private new static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly IProgressiveConfigurationProvider _progressives;
        private readonly IPropertiesManager _properties;
        private readonly IRtpService _rtpService;
        private readonly List<IViewableProgressiveLevel> _assignedLevels = new List<IViewableProgressiveLevel>();
        private readonly IDictionary<int, IDenomination> _denominationMapping;
        private readonly decimal _denomMultiplier;
        private readonly bool _allowEditHostDisabled;

        private bool _active = true;
        private bool _enabled;
        private BetOption _selectedBetOption;
        private bool _betOptionAvailable;
        private LineOption _selectedLineOption;
        private bool _lineOptionAvailable;
        private int _selectedBonusBet;
        private bool _bonusBetAvailable;
        private bool _gamble;
        private bool _letItRide;
        private bool _progressivesEnabled;
        private string _warningText;
        private decimal _forcedMinBet;
        private decimal _betMinimum;
        private decimal _betMaximum;
        private decimal _forcedMaxBet;
        private decimal _forcedMaxBetOutside;
        private ObservableCollection<int> _bonusBets = new ObservableCollection<int>();
        private ObservableCollection<BetOption> _betOptions = new ObservableCollection<BetOption>();
        private ObservableCollection<LineOption> _lineOptions = new ObservableCollection<LineOption>();
        private ObservableCollection<decimal> _minBetOptions;
        private bool _progressiveSetupConfigured;
        private bool _maxDenomEntriesReached;
        private bool _restrictedToReadOnly;
        private decimal _lowestAllowedMinimumRtp;
        private decimal _highestAllowedMinimumRtp;
        private long _topAwardValue;
        private bool _progressivesEditable;
        private bool _gameOptionsEnabled;
        private bool _showGameRtpAsRange;
        private IReadOnlyList<PaytableDisplay> _availablePaytables;
        private PaytableDisplay _selectedPaytable;

        public EditableGameConfiguration(long denom, IReadOnlyList<IGameDetail> games, bool showGameRtpAsRange)
        {
            var serviceManager = ServiceManager.GetInstance();
            _properties = serviceManager.GetService<IPropertiesManager>();
            _rtpService = serviceManager.GetService<IRtpService>();
            _progressives = serviceManager.GetService<IProgressiveConfigurationProvider>();

            _denomMultiplier = (decimal)_properties.GetValue(ApplicationConstants.CurrencyMultiplierKey, 1d);

            BaseDenom = denom;
            AvailableGames = games;
            _showGameRtpAsRange = showGameRtpAsRange;

            // Initial list should include all available RTPs
            _lowestAllowedMinimumRtp = LowestAvailableMinimumRtp = AvailableGames.Min(g => g.MinimumPaybackPercent);
            _highestAllowedMinimumRtp = HighestAvailableMinimumRtp = AvailableGames.Max(g => g.MinimumPaybackPercent);

            _gamble = _properties.GetValue(GamingConstants.GambleAllowed, false) && _properties.GetValue(GamingConstants.GambleEnabled, false);
            _letItRide = _properties.GetValue(GamingConstants.LetItRideEnabled, false);

            _allowEditHostDisabled = _properties.GetValue(GamingConstants.AllowEditHostDisabled, false);
            _denominationMapping = AvailableGames.ToDictionary(
                x => x.Id,
                x => x.Denominations.FirstOrDefault(d => d.Value == BaseDenom));

            SetAvailableGamesAndDenom();
            SetWarningText();
            ResetChanges();
        }

        /// <summary>
        ///     Game can be null if the configuration is invalid, so take appropriate precautions when using
        /// </summary>
        public IGameDetail Game
        {
            get => SelectedPaytable?.GameDetail;
            set => SelectedPaytable = value is null
                ? null
                : AvailablePaytables.FirstOrDefault(x => x.GameDetail.Id == value.Id);
        }

        public PaytableDisplay SelectedPaytable
        {
            get => _selectedPaytable;
            set
            {
                if (!SetProperty(ref _selectedPaytable, value))
                {
                    return;
                }

                RaisePropertyChanged(nameof(Game));
                LoadBetOptions();
                LoadLineOptions();
                LoadBonusBets(SelectedBetOption);
                SetProgressivesConfigured();
                TopAwardValue = RecalculateTopAward();
                SetWarningText();
            }
        }

        public long TopAwardValue
        {
            get => _topAwardValue;
            private set => SetProperty(ref _topAwardValue, value);
        }

        public IReadOnlyList<IGameDetail> AvailableGames { get; }

        public IReadOnlyList<PaytableDisplay> AvailablePaytables
        {
            get => _availablePaytables;
            private set => SetProperty(ref _availablePaytables, value);
        }

        public decimal HighestAvailableMinimumRtp { get; }

        public decimal LowestAvailableMinimumRtp { get; }

        public string WarningText
        {
            get => _warningText;
            set => SetProperty(
                ref _warningText,
                value,
                nameof(WarningText),
                nameof(CanEdit));
        }

        public long BaseDenom { get; }

        public ObservableCollection<BetOption> BetOptions
        {
            get => _betOptions;
            set => SetProperty(ref _betOptions, value, nameof(BetOptions));
        }

        public BetOption SelectedBetOption
        {
            get => _selectedBetOption;
            set
            {
                if (_selectedBetOption == value)
                {
                    return;
                }

                _selectedBetOption = value;
                RaisePropertyChanged(nameof(SelectedBetOption));
                LoadBonusBets(SelectedBetOption);
                ConfigurationMinBet();
                SetProgressivesConfigured();
                TopAwardValue = RecalculateTopAward();
            }
        }

        public bool BetOptionEnabled => CanEdit && BetOptionAvailable && Enabled && !RestrictedToReadOnly;

        public bool BetOptionAvailable
        {
            get => _betOptionAvailable;
            set => SetProperty(ref _betOptionAvailable, value, nameof(BonusBetAvailable), nameof(BetOptionEnabled));
        }

        public ObservableCollection<LineOption> LineOptions
        {
            get => _lineOptions;
            set => SetProperty(ref _lineOptions, value, nameof(LineOptions));
        }

        public LineOption SelectedLineOption
        {
            get => _selectedLineOption;
            set
            {
                if (_selectedLineOption == value)
                {
                    return;
                }

                _selectedLineOption = value;
                RaisePropertyChanged(nameof(SelectedLineOption));
                ConfigurationMinBet();
                TopAwardValue = RecalculateTopAward();
            }
        }

        public bool LineOptionEnabled => CanEdit && LineOptionAvailable && Enabled && LineOptions.Count > 1 && !RestrictedToReadOnly;

        public bool LineOptionAvailable
        {
            get => _lineOptionAvailable;
            set => SetProperty(ref _lineOptionAvailable, value, nameof(LineOptionAvailable), nameof(LineOptionEnabled));
        }

        public ObservableCollection<int> BonusBets
        {
            get => _bonusBets;
            set => SetProperty(ref _bonusBets, value, nameof(BonusBets));
        }

        public int SelectedBonusBet
        {
            get => _selectedBonusBet;
            set => SetProperty(ref _selectedBonusBet, value);
        }

        public bool BonusBetEnabled => CanEdit && BonusBetAvailable && Enabled;

        public bool BonusBetAvailable
        {
            get => _bonusBetAvailable;
            set => SetProperty(ref _bonusBetAvailable, value, nameof(BonusBetAvailable), nameof(BonusBetAvailable));
        }

        public bool Active
        {
            get => _active && (Game?.Active ?? false);
            set => _active = value;
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value)
                {
                    return;
                }

                if (!value)
                {
                    Gamble = false;
                }

                _enabled = value;
                RaisePropertyChanged(nameof(Enabled));
                RaisePropertyChanged(nameof(CanEdit));
                RaisePropertyChanged(nameof(CanEditAndEnabled));
                RaisePropertyChanged(nameof(CanEditAndEnableGamble));
                RaisePropertyChanged(nameof(CanEditAndEnableLetItRide));
                RaisePropertyChanged(nameof(ProgressivesAllowed));
                RaisePropertyChanged(nameof(ProgressivesEnabled));
                RaisePropertyChanged(nameof(ProgressiveSetupEnabled));
                RaisePropertyChanged(nameof(ProgressiveSetupVisibility));
                RaisePropertyChanged(nameof(ProgressiveViewVisibility));
                RaisePropertyChanged(nameof(BetOptionEnabled));
                RaisePropertyChanged(nameof(LineOptionEnabled));
                RaisePropertyChanged(nameof(BonusBetEnabled));
                RaisePropertyChanged(nameof(SelectedPaytable));
            }
        }

        public bool ProgressivesEnabled
        {
            get => _progressivesEnabled;
            set => SetProperty(ref _progressivesEnabled, value);
        }

        public bool ProgressiveSetupEnabled => CanEdit && Enabled && ProgressivesEnabled;

        public string ProgressiveSetupText => ProgressiveSetupConfigured
            ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Edit)
            : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProgressiveSetupButtonText);

        public bool ProgressiveSetupConfigured
        {
            get => _progressiveSetupConfigured;
            set => SetProperty(
                ref _progressiveSetupConfigured,
                value,
                nameof(ProgressiveSetupConfigured),
                nameof(ProgressiveSetupText),
                nameof(ProgressiveViewVisibility));
        }

        public bool ProgressiveSetupVisibility => ProgressivesEnabled && !ProgressiveViewVisibility;

        public bool ProgressiveViewVisibility => ProgressivesEnabled && ProgressiveSetupConfigured &&
                                                 (!ProgressivesEditable || UseImportedLevels || !GameOptionsEnabled);

        public bool GameOptionsEnabled
        {
            get => _gameOptionsEnabled;
            set
            {
                if (!SetProperty(ref _gameOptionsEnabled, value))
                {
                    return;
                }

                RaisePropertyChanged(
                    nameof(CanEditAndEnabled),
                    nameof(CanEditAndEnableGamble),
                    nameof(CanEditAndEnableLetItRide),
                    nameof(BetOptionEnabled),
                    nameof(LineOptionEnabled),
                    nameof(BonusBetEnabled),
                    nameof(ProgressiveSetupEnabled),
                    nameof(CanEdit),
                    nameof(CanToggleEnabled),
                    nameof(ProgressiveViewVisibility),
                    nameof(ProgressiveSetupVisibility),
                    nameof(GameOptionsEnabled));
                SetProgressivesConfigured();
            }
        }

        public bool UseImportedLevels => _assignedLevels.Any(l => l.GameId == Game?.Id);

        public bool ProgressivesEditable
        {
            get => _progressivesEditable;
            private set => SetProperty(
                ref _progressivesEditable,
                value,
                nameof(ProgressivesEditable),
                nameof(ProgressiveSetupVisibility),
                nameof(ProgressiveViewVisibility));
        }

        public bool MaxDenomEntriesReached
        {
            get => _maxDenomEntriesReached;
            set
            {
                if (_maxDenomEntriesReached == value)
                {
                    return;
                }

                _maxDenomEntriesReached = value;
                RaisePropertyChanged(
                    nameof(CanToggleEnabled),
                    nameof(CanEditAndEnabled),
                    nameof(CanEditAndEnableGamble),
                    nameof(CanEditAndEnableLetItRide),
                    nameof(CanEdit));
                SetWarningText();
            }
        }

        public bool ProgressivesAllowed => false; // This should be disabled until we support "bolt-on" progressives

        public bool EnabledByHost => Game?.Status == GameStatus.None;

        // VLT-12434 : prevent en/disabling games when credits are on the machine
        public bool CanToggleEnabled => (EnabledByHost || _allowEditHostDisabled) && AvailablePaytables.Any() &&
                                        GameOptionsEnabled &&
                                        !MaxDenomEntriesReached &&
                                        !RestrictedToReadOnly;

        public bool CanEdit => GameOptionsEnabled;

        public bool CanEditAndEnabled => CanEdit && Enabled && !RestrictedToReadOnly;

        public bool CanEditAndEnableGamble =>
            CanEdit && Enabled && !RestrictedToReadOnly && (_properties.GetValue(GamingConstants.GambleAllowed, true));

        public bool CanEditAndEnableLetItRide =>
            CanEdit && Enabled && !RestrictedToReadOnly && (_properties.GetValue(GamingConstants.LetItRideAllowed, true));

        /// <summary>
        ///     Some games provide "MultiGame Packages" which are pre-defined configuration templates. The Platform equivalent for
        ///     a Package is <see cref="IConfigurationRestriction" />. If the value of this property is <c>true</c>, then the
        ///     currently selected <see cref="IConfigurationRestriction" /> defines this <see cref="EditableGameConfiguration" />
        ///     as read-only.
        /// </summary>
        /// <value>
        ///     <c>true</c> if restricted; otherwise, <c>false</c>.
        /// </value>
        public bool RestrictedToReadOnly
        {
            get => _restrictedToReadOnly;
            set => SetProperty(
                ref _restrictedToReadOnly,
                value,
                nameof(RestrictedToReadOnly),
                nameof(CanToggleEnabled),
                nameof(CanEditAndEnableGamble),
                nameof(CanEditAndEnableLetItRide),
                nameof(CanEditAndEnabled),
                nameof(BetOptionEnabled),
                nameof(LineOptionEnabled));
        }

        public decimal Denom => BaseDenom / _denomMultiplier;

        public string DenomString => $"{Denom.FormattedCurrencyString()}";

        public string MaxBet => $"{BetMaximum.FormattedCurrencyString()}";

        public bool Gamble
        {
            get => _gamble;
            set
            {
                if (!SetProperty(ref _gamble, value))
                {
                    return;
                }

                _properties.SetProperty(GamingConstants.GambleEnabled, _gamble);
            }
        }

        public bool LetItRide
        {
            get => _letItRide;
            set
            {
                if (!SetProperty(ref _letItRide, value))
                {
                    return;
                }

                _properties.SetProperty(GamingConstants.LetItRideEnabled, _letItRide);
            }
        }

        public decimal ForcedMinBet
        {
            get => _forcedMinBet;
            set
            {
                if (!SetProperty(ref _forcedMinBet, value))
                {
                    return;
                }

                SetWarningText();
            }
        }

        public int MinBetWidth => GetBetWidth(ForcedMinBet);

        public int MaxBetWidth => GetBetWidth(ForcedMaxBet);

        public decimal BetMinimum
        {
            get => _betMinimum;
            private set => SetProperty(ref _betMinimum, value);
        }

        public ObservableCollection<decimal> MinBetOptions
        {
            get => _minBetOptions;
            set
            {
                if (SetProperty(ref _minBetOptions, value, nameof(MinBetOptions)) && _minBetOptions != null)
                {
                    ForcedMinBet = _minBetOptions.FirstOrDefault();
                }
            }
        }

        public decimal BetMaximum
        {
            get => _betMaximum;
            set => SetProperty(ref _betMaximum, value, nameof(BetMaximum), nameof(MaxBet));
        }

        public decimal ForcedMaxBet
        {
            get => _forcedMaxBet;
            set
            {
                if (!SetProperty(ref _forcedMaxBet, value))
                {
                    return;
                }

                SetWarningText();
            }
        }

        // For when we have a higher maximum bet for low-odds bets, for example betting in roulette on just red/black or odd/even.
        public decimal ForcedMaxBetOutside
        {
            get => _forcedMaxBetOutside;
            set
            {
                SetProperty(ref _forcedMaxBetOutside, value);
                SetWarningText();
            }
        }

        public int MinimumWagerCredits => (int)(ForcedMinBet * _denomMultiplier / BaseDenom);

        public int MaximumWagerCredits => (int)(ForcedMaxBet * _denomMultiplier / BaseDenom);

        public int MaximumWagerOutsideCredits => (int)(ForcedMaxBetOutside * _denomMultiplier / BaseDenom);

        public string SubGameType => Game?.GameSubtype;

        private IEnumerable<IGameDetail> FilteredAvailableGames => AvailableGames.Where(
            g => g.MinimumPaybackPercent <= _highestAllowedMinimumRtp &&
                 g.MinimumPaybackPercent >= _lowestAllowedMinimumRtp).ToList();

        private IEnumerable<IViewableProgressiveLevel> ViewProgressiveLevels => Game == null || UseImportedLevels
            ? _assignedLevels.Where(l => l.GameId == Game?.Id)
            : _progressives.ViewProgressiveLevels(Game.Id, BaseDenom).Where(
                p => string.IsNullOrEmpty(p.BetOption) || p.BetOption == SelectedBetOption?.Name);

        public void LoadConfiguredProgressiveLevels(IReadOnlyCollection<IViewableProgressiveLevel> levels)
        {
            _assignedLevels.Clear();
            if (!(levels?.Any() ?? false))
            {
                return;
            }

            _assignedLevels.AddRange(levels);
            SetProgressivesConfigured();
        }

        public IDenomination ResolveDenomination()
        {
            return SelectedPaytable != null && _denominationMapping.TryGetValue(SelectedPaytable.GameDetail.Id, out var denom)
                ? denom
                : null;
        }

        public bool HasChanges()
        {
            var denomination = ResolveDenomination();

            return Game?.ActiveDenominations.Contains(BaseDenom) != Enabled ||
                   denomination.MinimumWagerCredits * BaseDenom / _denomMultiplier != _forcedMinBet ||
                   denomination.MaximumWagerCredits * BaseDenom / _denomMultiplier != _forcedMaxBet ||
                   denomination.MaximumWagerOutsideCredits * BaseDenom / _denomMultiplier != _forcedMaxBetOutside ||
                   denomination.BetOption != SelectedBetOption?.Name ||
                   denomination.LineOption != SelectedLineOption?.Name ||
                   denomination.BonusBet != SelectedBonusBet ||
                   denomination.SecondaryAllowed != Gamble ||
                   denomination.LetItRideAllowed != LetItRide;
        }

        public void ResetChanges()
        {
            var game = FilteredAvailableGames.FirstOrDefault(x => x.ActiveDenominations.Contains(BaseDenom));
            Game = game ?? FilteredAvailableGames.FirstOrDefault();

            var denomination = ResolveDenomination();

            _assignedLevels?.Clear();

            MaxDenomEntriesReached = false;
            Enabled = denomination?.Active ?? false;
            Gamble = _properties.GetValue(GamingConstants.GambleAllowed, false) && (denomination?.SecondaryAllowed ?? false);
            LetItRide = denomination?.LetItRideAllowed ?? false;
            ForcedMinBet = denomination?.MinimumWagerCredits * Denom ?? BetMinimum;
            ForcedMaxBet = denomination?.MaximumWagerCredits * Denom ?? BetMaximum;
            ForcedMaxBetOutside = denomination?.MaximumWagerOutsideCredits * Denom ?? BetMaximum;
            SelectedBetOption = string.IsNullOrEmpty(denomination?.BetOption)
                ? null
                : BetOptions?.FirstOrDefault(o => o.Name == denomination.BetOption) ?? BetOptions?.FirstOrDefault();
            SelectedLineOption = string.IsNullOrEmpty(denomination?.LineOption)
                ? null
                : LineOptions?.FirstOrDefault(o => o.Name == denomination.LineOption) ?? LineOptions?.FirstOrDefault();
            SelectedBonusBet = denomination?.BonusBet ?? BonusBets.FirstOrDefault();
        }

        public void RaiseEnabledByHostChanged()
        {
            RaisePropertyChanged(nameof(EnabledByHost));
            RaisePropertyChanged(nameof(CanEdit));
            RaisePropertyChanged(nameof(CanEditAndEnabled));
            RaisePropertyChanged(nameof(CanEditAndEnableGamble));
            RaisePropertyChanged(nameof(CanEditAndEnableLetItRide));
        }

        public void SetAllowedRtpRange(decimal? lowestAllowed, decimal? highestAllowed)
        {
            var lowest = lowestAllowed ?? LowestAvailableMinimumRtp;
            var highest = highestAllowed ?? HighestAvailableMinimumRtp;
            if (_lowestAllowedMinimumRtp == lowest && _highestAllowedMinimumRtp == highest)
            {
                return;
            }

            _lowestAllowedMinimumRtp = lowest;
            _highestAllowedMinimumRtp = highest;

            SetAvailableGamesAndDenom();

            if (!AvailablePaytables.Contains(SelectedPaytable))
            {
                // Reset selected RTP if the one that was selected previously is no longer available
                SelectedPaytable = AvailablePaytables.FirstOrDefault();
            }

            RaisePropertyChanged(nameof(CanToggleEnabled));

            Logger.Debug(
                AvailablePaytables.Aggregate(
                    $"Denom {Denom} available minimum RTPs: ",
                    (current, rtp) => current + $"{rtp.GameDetail.MinimumPaybackPercent}  ")
                + $" (Allowed Range: {_lowestAllowedMinimumRtp} - {_highestAllowedMinimumRtp})");
        }

        private bool IsProgressivesEnabled()
        {
            var restrictedType = _properties.GetValue(
                GamingConstants.RestrictedProgressiveTypes,
                new List<ProgressiveLevelType>());
            return ViewProgressiveLevels.Any(x => !restrictedType.Contains(x.LevelType));
        }

        private void ConfigurationMinBet()
        {
            var minCredits = Game?.MinimumWagerCredits(SelectedBetOption, SelectedLineOption) ?? 0;
            var maxCredits = Game?.MaximumWagerCredits(SelectedBetOption, SelectedLineOption) ?? 0;
            BetMinimum = minCredits * Denom;
            BetMaximum = maxCredits * Denom;
            MinBetOptions = new ObservableCollection<decimal>(
                Game?.GetBetAmounts(SelectedBetOption, SelectedLineOption, Denom, Game.GameType) ?? new List<decimal>());
        }

        private void SetProgressivesConfigured()
        {
            ProgressivesEnabled = IsProgressivesEnabled();

            if (!ProgressivesEnabled)
            {
                return;
            }

            ProgressiveSetupConfigured = ViewProgressiveLevels
                .Any(p => p.CurrentState != ProgressiveLevelState.Init);
            ProgressivesEditable = ViewProgressiveLevels.Any(p => p.CanEdit) && (!_assignedLevels?.Any() ?? false);
            RaisePropertyChanged(nameof(UseImportedLevels));
        }

        public void SetWarningText()
        {
            if (!EnabledByHost && !_allowEditHostDisabled)
            {
                WarningText = string.Empty;

                if (Game != null)
                {
                    foreach (GameStatus status in Enum.GetValues(typeof(GameStatus)))
                    {
                        if ((Game.Status & status) != 0)
                        {
                            var statusString = "GameStatus_" + status;
                            WarningText += " " + Localizer.For(CultureFor.Operator).GetString(statusString);
                        }
                    }
                }

                if (string.IsNullOrEmpty(WarningText))
                {
                    WarningText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameDisabled);
                }
            }
            else if (MaxDenomEntriesReached)
            {
                // Include game sub type that has reached the max for user clarity
                var gameType = !string.IsNullOrEmpty(Game.GameSubtype)
                    ? Game.GameSubtype + " " + Game.GameType
                    : Game.GameType.ToString();
                WarningText = string.Format(CultureInfo.CurrentCulture, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MaxEnabledDenomsReached), gameType);
            }
            else if (BetRangesInvalid())
            {
                WarningText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InvalidBetRangeForDenom);
            }
            else if (ForcedMinBet.DollarsToMillicents() % Denom.DollarsToMillicents() != 0 ||
                     ForcedMaxBet.DollarsToMillicents() % Denom.DollarsToMillicents() != 0 ||
                     ForcedMaxBetOutside.DollarsToMillicents() % Denom.DollarsToMillicents() != 0)
            {
                WarningText = string.Format(
                    CultureInfo.CurrentCulture,
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InvalidBetAmountForDenom),
                    Denom.FormattedCurrencyString());
            }
            else
            {
                WarningText = string.Empty;
            }
        }

        private bool BetRangesInvalid()
        {
            // It turns out that this maths is invalid for other games like poker or slots, as the relationship between the
            // MaximumWagerCredits and MaxBet is not simple. Hence we only perform this check on Roulette for now.
            var game = FilteredAvailableGames.FirstOrDefault();
            if (game?.GameType == GameType.Roulette)
            {
                return (ForcedMaxBet < ForcedMinBet || ForcedMaxBetOutside < ForcedMinBet || ForcedMaxBetOutside < ForcedMaxBet);
            }

            return false;
        }

        private void LoadBetOptions()
        {
            BetOptionAvailable = false;

            var denomination = ResolveDenomination();
            if (denomination == null)
            {
                return;
            }

            if (Game?.BetOptionList == null)
            {
                SelectedBetOption = null;
                return;
            }

            BetOptions = new ObservableCollection<BetOption>(Game?.BetOptionList);

            SelectedBetOption = (string.IsNullOrEmpty(denomination.BetOption)
                                    ? null
                                    : BetOptions.FirstOrDefault(o => o.Name == denomination.BetOption))
                                ?? Game?.ActiveBetOption
                                ?? BetOptions?.FirstOrDefault();

            BetOptionAvailable = BetOptions.Any();
        }

        private void LoadLineOptions()
        {
            LineOptionAvailable = false;

            var denomination = ResolveDenomination();
            if (denomination == null)
            {
                return;
            }

            if (Game?.LineOptionList == null)
            {
                SelectedLineOption = null;
                return;
            }

            LineOptions = new ObservableCollection<LineOption>(Game?.LineOptionList);

            SelectedLineOption = (string.IsNullOrEmpty(denomination.LineOption)
                                     ? null
                                     : LineOptions?.FirstOrDefault(o => o.Name == denomination.LineOption))
                                 ?? Game?.ActiveLineOption
                                 ?? LineOptions?.FirstOrDefault();

            LineOptionAvailable = LineOptions?.Any() ?? false;
        }

        private void LoadBonusBets(BetOption betOption)
        {
            BonusBetAvailable = false;

            IDenomination denomination;

            if (betOption != null && Game?.GameType == GameType.Poker && (denomination = ResolveDenomination()) != null)
            {
                BonusBets = new ObservableCollection<int>(betOption.BonusBets);

                if (BonusBets.Count > 0)
                {
                    BonusBetAvailable = true;
                    SelectedBonusBet = BonusBets.Contains(denomination.BonusBet)
                        ? denomination.BonusBet
                        : BonusBets.FirstOrDefault();
                }
            }

            // Raise this in case we change variation while editing and we have different bonus bets. Note
            // that if someone actually does want to do this, you need to enforce that the game's various
            // ati:betOption manifest entries have unique names. Otherwise they just get coalesced.
            RaisePropertyChanged(nameof(SelectedBonusBet), nameof(BonusBets), nameof(BonusBetAvailable));
        }

        private long RecalculateTopAward()
        {
            return Game?.TopAward(ResolveDenomination(), SelectedBetOption, SelectedLineOption) ?? 0;
        }

        private void SetAvailableGamesAndDenom()
        {
            AvailablePaytables = FilteredAvailableGames.OrderByDescending(g => g.VariationId == "99")
                .ThenBy(g => Convert.ToInt32(g.VariationId))
                .Select(g =>
                {
                    var rtp = _rtpService.GetTotalRtp(g);
                    var paytableDisplay = new PaytableDisplay(g, rtp, _showGameRtpAsRange);
                    return paytableDisplay;
                }).ToList();
        }

        private static int GetBetWidth(decimal betAmount)
        {
            const int defaultBetWidth = 162;

            const int currencyDigitWidth = 4;

            var format = CurrencyExtensions.CurrencyCultureInfo?.NumberFormat;

            if (format == null)
            {
                return defaultBetWidth;
            }

            var padding = format.CurrencyDecimalDigits +
                          format.CurrencyDecimalSeparator.Length +
                          format.CurrencySymbol.Length;

            var textLength =
                betAmount.FormattedCurrencyString().Length - padding;

            return defaultBetWidth + Math.Max(textLength, 1) * currencyDigitWidth;
        }
    }
}