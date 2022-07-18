namespace Aristocrat.Monaco.Gaming.Barkeeper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Timers;
    using System.Xml.Serialization;
    using Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Barkeeper;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;
    using log4net;
    using Newtonsoft.Json;
    using Stateless;
    using Common;

    public class BarkeeperHandler : IBarkeeperHandler, IService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEdgeLightingController _edgeLighting;
        private readonly IGamePlayState _gamePlay;
        private readonly IPlayerBank _bank;
        private readonly IPropertiesManager _propertiesManager;

        private readonly object _lockObject = new object();

        private readonly StateMachine<BarkeeperStates, BarkeeperCashInTriggers> _cashInStateMachine;
        private readonly StateMachine<BarkeeperStates, BarkeeperCoinInTrigger> _coinInStateMachine;
        private readonly Timer _rateOfPlayTimer = new Timer { Enabled = false, AutoReset = false };

        private readonly Stopwatch _barkeeperSession = new Stopwatch();

        private readonly PatternParameters _auditHaloPattern = new SolidColorPatternParameters
        {
            Color = Color.White,
            Strips = new List<int> { (int)StripIDs.BarkeeperStrip4Led },
            Priority = StripPriority.AuditMenu
        };

        private readonly PatternParameters _auditButtonPattern = new SolidColorPatternParameters
        {
            Color = Color.Black,
            Strips = new List<int> { (int)StripIDs.BarkeeperStrip1Led },
            Priority = StripPriority.AuditMenu
        };

        private PatternParameters _idleCashInPattern;
        private PatternParameters _idleCoinInPattern;
        private PatternParameters _disabledCashInPattern;
        private PatternParameters _disabledCoinInPattern;

        private Timer _zeroBalanceTimer;
        private StateMachine<BarkeeperStates, BarkeeperCoinInTrigger>.TriggerWithParameters<RewardLevel> _coinInTrigger;

        private List<RewardLevel> _coinInRewardLevels;

        private List<RewardLevel> _cashInRewardLevel;

        private RewardLevel _lastAward;
        private RewardLevel _maxAwardLevel;
        private IEdgeLightToken _haloLightToken;
        private IEdgeLightToken _buttonLightToken;
        private readonly IList<IEdgeLightToken> _auditMenuLightToken = new List<IEdgeLightToken>();
        private long _coinIn;
        private long _cashIn;
        private bool _enabled;
        private bool _disposed;
        private long _rateOfPlayElapsedTimeFromPreviousSession;
        private BarkeeperRewardLevels _rewardLevels;

        public BarkeeperHandler(
            IEdgeLightingController edgeLighting,
            IPropertiesManager propertiesManager,
            IGamePlayState gamePlay,
            IPlayerBank bank)
        {
            _edgeLighting = edgeLighting ?? throw new ArgumentNullException(nameof(edgeLighting));
            _gamePlay = gamePlay ?? throw new ArgumentNullException(nameof(gamePlay));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));

            InitializeBarkeeperLevels();
            _cashInStateMachine = CreateCashInStateMachine(
                _enabled && _cashInRewardLevel.Any(x => x.Enabled) ? BarkeeperStates.Idle : BarkeeperStates.Disabled);
            _coinInStateMachine = CreateCoinInStateMachine(
                _enabled && _coinInRewardLevels.Any(x => x.Enabled) ? BarkeeperStates.Idle : BarkeeperStates.Disabled);
            _rateOfPlayTimer.Elapsed += CoinInTimerElapsed;
        }

        public BarkeeperRewardLevels RewardLevels
        {
            get => _rewardLevels;
            set
            {
                _propertiesManager.SetProperty(GamingConstants.BarkeeperRewardLevels, ToXml(value));
                InitializeBarkeeperLevels();
                BarkeeperReconfigured();
            }
        }

        public long CreditsInDuringSession
        {
            get
            {
                lock (_lockObject)
                {
                    return _cashIn;
                }
            }
            private set
            {
                lock (_lockObject)
                {
                    _cashIn = value;
                }
            }
        }

        public long CoinInDuringSession
        {
            get
            {
                lock (_lockObject)
                {
                    return _coinIn;
                }
            }
            private set
            {
                lock (_lockObject)
                {
                    _coinIn = value;
                }
            }
        }

        public void OnAuditEntered()
        {
            lock (_lockObject)
            {
                ClearAuditMenuLighting();
                _auditMenuLightToken.Add(
                    _edgeLighting.AddEdgeLightRenderer(
                        _auditHaloPattern,
                        GamingConstants.BarkeeperCashInIdleBrightness));
                _auditMenuLightToken.Add(
                    _edgeLighting.AddEdgeLightRenderer(
                        _auditButtonPattern,
                        GamingConstants.BarkeeperDefaultBrightness));
            }
        }

        public void OnAuditExited()
        {
            lock (_lockObject)
            {
                ClearAuditMenuLighting();
                _edgeLighting.ClearStripBrightnessForPriority(
                    (int)StripIDs.BarkeeperStrip1Led,
                    StripPriority.AuditMenu);
                _edgeLighting.ClearStripBrightnessForPriority(
                    (int)StripIDs.BarkeeperStrip4Led,
                    StripPriority.AuditMenu);
            }
        }

        public void OnBalanceUpdate(long newBalance)
        {
            lock (_lockObject)
            {
                if (newBalance != 0)
                {
                    return;
                }

                _cashInStateMachine.Fire(BarkeeperCashInTriggers.BalanceCleared);
            }
        }

        public void OnCashOutCompleted()
        {
            lock (_lockObject)
            {
                if (_bank.Balance != 0)
                {
                    return;
                }

                _coinInStateMachine.Fire(BarkeeperCoinInTrigger.PlayerCashedOut);
            }
        }

        public void OnCreditsInserted(long total)
        {
            lock (_lockObject)
            {
                _zeroBalanceTimer.Stop();

                if (!_enabled || !_cashInRewardLevel.Any(x => x.Enabled))
                {
                    return;
                }

                CreditsInDuringSession += total;
                UpdateCreditsIn();
                PersistSession();
            }
        }

        public void CreditsWagered(long wageredAmount)
        {
            lock (_lockObject)
            {
                if (!_enabled || !_coinInRewardLevels.Any(x => x.Enabled))
                {
                    return;
                }

                CoinInDuringSession += wageredAmount;
                UpdateCoinInRate();
                PersistSession();
            }
        }

        public void BarkeeperButtonPressed()
        {
            _coinInStateMachine.Fire(BarkeeperCoinInTrigger.BarkeeperButtonPressed);
        }

        public void GameEnded(IGameHistoryLog gameHistory)
        {
            lock (_lockObject)
            {
                if (!_enabled)
                {
                    return;
                }

                if (gameHistory.EndCredits != 0)
                {
                    return;
                }

                ResetZeroBalanceTimer();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Name => typeof(BarkeeperHandler).ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(IBarkeeperHandler) };

        public void Initialize()
        {
            if (!_enabled)
            {
                _haloLightToken = _edgeLighting.AddEdgeLightRenderer(_disabledCashInPattern);
                _buttonLightToken = _edgeLighting.AddEdgeLightRenderer(_disabledCoinInPattern);
                return;
            }

            if (_cashInRewardLevel.Any(x => x.Enabled))
            {
                _haloLightToken = _edgeLighting.AddEdgeLightRenderer(
                    _idleCashInPattern,
                    _rewardLevels.CashInStrategy.IdleBrightness);
                UpdateCreditsIn();
            }
            else
            {
                _haloLightToken = _edgeLighting.AddEdgeLightRenderer(_disabledCashInPattern);
            }

            if (_coinInRewardLevels.Any(x => x.Enabled))
            {
                _buttonLightToken = _edgeLighting.AddEdgeLightRenderer(
                    _idleCoinInPattern,
                    _rewardLevels.CoinInStrategy.IdleBrightness);
                RestoreCoinSession();
            }
            else
            {
                _buttonLightToken = _edgeLighting.AddEdgeLightRenderer(_disabledCoinInPattern);
            }

            OnBalanceUpdate(_bank.Balance);
        }

        public string ToXml(BarkeeperRewardLevels rewardLevels)
        {
            var xmlSerializer = new XmlSerializer(typeof(BarkeeperRewardLevels));
            using (var stream = new StringWriter())
            {
                xmlSerializer.Serialize(stream, rewardLevels);
                return stream.ToString();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                ClearButtonLighting();
                ClearHaloLighting();
                _rateOfPlayTimer.Elapsed -= CoinInTimerElapsed;
                _rateOfPlayTimer.Dispose();
                _zeroBalanceTimer.Elapsed -= ZeroBalanceElapsed;
                _zeroBalanceTimer.Dispose();
            }

            _disposed = true;
        }

        private void PersistSession()
        {
            _propertiesManager.SetProperty(
                GamingConstants.BarkeeperCoinIn,
                CoinInDuringSession,
                true);

            _propertiesManager.SetProperty(
                GamingConstants.BarkeeperCashIn,
                CreditsInDuringSession,
                true);

            PersistActiveCoinInReward();
        }

        private void PersistActiveCoinInReward()
        {
            _propertiesManager.SetProperty(
                GamingConstants.BarkeeperActiveCoinInReward,
                JsonConvert.SerializeObject(_lastAward),
                true);
        }

        private void PersistRateOfPlayElapsedTime(long elapsedTime)
        {
            _propertiesManager.SetProperty(
                GamingConstants.BarkeeperRateOfPlayElapsedMilliseconds,
                CoinInRateOfPlayEnabled ? elapsedTime : 0,
                true
            );
        }

        private void RestoreCoinSession()
        {
            _rateOfPlayElapsedTimeFromPreviousSession = RestoreRateOfPlayElapsedTime();
            _coinIn = (long)_propertiesManager.GetProperty(GamingConstants.BarkeeperCoinIn, 0L);
            _cashIn = (long)_propertiesManager.GetProperty(GamingConstants.BarkeeperCashIn, 0L);

            ResetRateOfPlay();
            if (_lastAward != null && !_lastAward.Awarded)
            {
                AwardRewardLevel(_lastAward);
            }

            if ((_lastAward != null || CoinInDuringSession > 0) &&
                _bank.Balance == 0 &&
                _gamePlay.UncommittedState == PlayState.Idle)
            {
                ResetZeroBalanceTimer();
            }
        }

        private long RestoreRateOfPlayElapsedTime()
        {
            if (CoinInRateOfPlayEnabled)
            {
                return (long)_propertiesManager.GetProperty(
                    GamingConstants.BarkeeperRateOfPlayElapsedMilliseconds,
                    0L);
            }
            else
            {
                return 0L;
            }
        }

        private void ResetZeroBalanceTimer()
        {
            _zeroBalanceTimer.Stop();
            _zeroBalanceTimer.Start();
        }

        private void UpdateCreditsIn()
        {
            if (_cashInRewardLevel.Count(
                x => x.Enabled && x.ThresholdInCents <= CreditsInDuringSession.MillicentsToCents()) > 0)
            {
                _cashInStateMachine.Fire(BarkeeperCashInTriggers.CashInserted);
            }
        }

        private void UpdateCoinInRate()
        {
            ResetRateOfPlay();
            var rewardLevel = _coinInRewardLevels
                .Where(x => x.Enabled && x.ThresholdInCents <= CoinInDuringSession.MillicentsToCents())
                .OrderByDescending(x => x.ThresholdInCents)
                .FirstOrDefault();
            if (rewardLevel == null || _lastAward == rewardLevel)
            {
                return;
            }

            // Last reward given is still active so don't reset it to lower level RewardLevel
            if (_lastAward != null && !_lastAward.Awarded && rewardLevel.ThresholdInCents < _lastAward.ThresholdInCents)
            {
                return;
            }

            AwardRewardLevel(rewardLevel);
        }

        private void AwardRewardLevel(RewardLevel rewardLevel)
        {
            // Clone it to avoid setting Awarded flag in the RewardLevels object directly
            _lastAward = rewardLevel.DeepClone();
            _coinInStateMachine.Fire(_coinInTrigger, rewardLevel);
        }

        private void ResetRateOfPlay()
        {
            if (!CoinInRateOfPlayEnabled)
            {
                return;
            }

            var coinInRate = RewardLevels.CoinInStrategy.CoinInRate;

            _rateOfPlayTimer.Stop();
            // Elapsed time is equal to current elapsed time and elapsed time from previous session if any
            long elapsedTime = _barkeeperSession.ElapsedMilliseconds + _rateOfPlayElapsedTimeFromPreviousSession;
            var resetTime = coinInRate.SessionRateInMs * ((double)CoinInDuringSession.MillicentsToCents() / coinInRate.Amount) - elapsedTime;
            Logger.Debug($"Setting reset time to {resetTime}.  The current amount in is {CoinInDuringSession}");
            PersistRateOfPlayElapsedTime(elapsedTime);
            if (resetTime > 0 && !double.IsInfinity(resetTime) && !double.IsNaN(resetTime))
            {
                _rateOfPlayTimer.Interval = resetTime;
                _rateOfPlayTimer.Start();

                if (!_barkeeperSession.IsRunning)
                {
                    _barkeeperSession.Reset();
                    _barkeeperSession.Start();
                }
            }
        }

        private bool CoinInRateOfPlayEnabled => RewardLevels.CoinInStrategy.CoinInRate.Enabled;

        private void ClearAuditMenuLighting()
        {
            lock (_lockObject)
            {
                foreach (var token in _auditMenuLightToken)
                {
                    _edgeLighting.RemoveEdgeLightRenderer(token);
                }

                _auditMenuLightToken.Clear();
            }
        }

        private void Reset()
        {
            lock (_lockObject)
            {
                CoinInDuringSession = 0;
                _rateOfPlayTimer.Stop();
                _barkeeperSession.Reset();
                _lastAward = null;
                PersistSession();
                PersistRateOfPlayElapsedTime(0);
                _rateOfPlayElapsedTimeFromPreviousSession = 0;
            }
        }

        private void BarkeeperReconfigured()
        {
            lock (_lockObject)
            {
                _cashInStateMachine.Fire(BarkeeperCashInTriggers.BarkeeperReconfigured);
                _coinInStateMachine.Fire(BarkeeperCoinInTrigger.BarkeeperReconfigured);
            }
        }

        private void InitializeBarkeeperLevels()
        {
            _rewardLevels = BarkeeperRewardLevelHelper.ToRewards(
                (string)_propertiesManager.GetProperty(GamingConstants.BarkeeperRewardLevels, string.Empty));
            _cashInRewardLevel = RewardLevels.RewardLevels.Where(x => x.TriggerStrategy == BarkeeperStrategy.CashIn)
                .ToList();
            _coinInRewardLevels = RewardLevels.RewardLevels.Where(x => x.TriggerStrategy == BarkeeperStrategy.CoinIn)
                .ToList();
            _enabled = RewardLevels.Enabled;
            _lastAward = JsonConvert.DeserializeObject<RewardLevel>((string)_propertiesManager.GetProperty(GamingConstants.BarkeeperActiveCoinInReward, string.Empty));

            if (_coinInRewardLevels.Any(x => x.Enabled))
            {
                _maxAwardLevel = _coinInRewardLevels.Find(x => x.Enabled);
                foreach (var rewardLevel in _coinInRewardLevels.FindAll(lvl => lvl.Enabled))
                {
                    if (rewardLevel.ThresholdInCents > _maxAwardLevel.ThresholdInCents)
                    {
                        _maxAwardLevel = rewardLevel;
                    }
                }
            }
            else
            {
                _maxAwardLevel = null;
            }

            _zeroBalanceTimer?.Stop();
            _zeroBalanceTimer = new Timer
            {
                Enabled = false, AutoReset = false, Interval = RewardLevels.CoinInStrategy.SessionTimeout
            };
            _zeroBalanceTimer.Elapsed += ZeroBalanceElapsed;

            _idleCoinInPattern = BarkeeperRewardLevelHelper.GetPattern(
                RewardLevels.CoinInStrategy.IdleColor,
                BarkeeperLed.Button,
                BarkeeperAlertOptions.LightOn);
            _idleCashInPattern = BarkeeperRewardLevelHelper.GetPattern(
                RewardLevels.CashInStrategy.IdleColor,
                BarkeeperLed.Halo,
                BarkeeperAlertOptions.LightOn);
            _disabledCoinInPattern = BarkeeperRewardLevelHelper.GetPattern(
                RewardLevels.CoinInStrategy.DisabledColor,
                BarkeeperLed.Button,
                BarkeeperAlertOptions.LightOn);
            _disabledCashInPattern = BarkeeperRewardLevelHelper.GetPattern(
                RewardLevels.CashInStrategy.DisabledColor,
                BarkeeperLed.Halo,
                BarkeeperAlertOptions.LightOn);
        }

        private StateMachine<BarkeeperStates, BarkeeperCoinInTrigger> CreateCoinInStateMachine(
            BarkeeperStates startingState)
        {
            var stateMachine = new StateMachine<BarkeeperStates, BarkeeperCoinInTrigger>(startingState);
            _coinInTrigger = stateMachine.SetTriggerParameters<RewardLevel>(BarkeeperCoinInTrigger.CoinInReached);

            stateMachine.Configure(BarkeeperStates.Disabled)
                .OnEntry(HandleCoinInDisabled)
                .PermitIf(
                    BarkeeperCoinInTrigger.BarkeeperReconfigured,
                    BarkeeperStates.Idle,
                    () => _enabled && _coinInRewardLevels.Any(x => x.Enabled));
            stateMachine.Configure(BarkeeperStates.Idle)
                .OnEntry(HandleCoinInIdle)
                .Permit(BarkeeperCoinInTrigger.Disable, BarkeeperStates.Disabled)
                .PermitReentry(BarkeeperCoinInTrigger.PlayerCashedOut)
                .PermitReentryIf(
                    BarkeeperCoinInTrigger.BarkeeperReconfigured,
                    () => _enabled && _coinInRewardLevels.Any(x => x.Enabled))
                .PermitIf(
                    BarkeeperCoinInTrigger.BarkeeperReconfigured,
                    BarkeeperStates.Disabled,
                    () => !_enabled || !_coinInRewardLevels.Any(x => x.Enabled))
                .PermitReentry(BarkeeperCoinInTrigger.TimedOut)
                .PermitDynamic(_coinInTrigger, _ => BarkeeperStates.Active);
            stateMachine.Configure(BarkeeperStates.Active)
                .OnEntryFrom(_coinInTrigger, HandleCoinActive)
                .Permit(BarkeeperCoinInTrigger.Disable, BarkeeperStates.Disabled)
                .Permit(BarkeeperCoinInTrigger.PlayerCashedOut, BarkeeperStates.Idle)
                .Permit(BarkeeperCoinInTrigger.TimedOut, BarkeeperStates.Idle)
                .PermitDynamic(_coinInTrigger, _ => BarkeeperStates.Active)
                .PermitIf(
                    BarkeeperCoinInTrigger.BarkeeperReconfigured,
                    BarkeeperStates.Idle,
                    () => _enabled && _coinInRewardLevels.Any(x => x.Enabled))
                .PermitIf(
                    BarkeeperCoinInTrigger.BarkeeperReconfigured,
                    BarkeeperStates.Disabled,
                    () => !_enabled || !_coinInRewardLevels.Any(x => x.Enabled))
                .InternalTransitionIf(
                    BarkeeperCoinInTrigger.BarkeeperButtonPressed,
                    (_) => _lastAward?.ThresholdInCents != _maxAwardLevel?.ThresholdInCents,
                    HandleBarKeeperButtonPressed)
                .PermitIf(
                    BarkeeperCoinInTrigger.BarkeeperButtonPressed,
                    BarkeeperStates.Idle,
                    () => _lastAward?.ThresholdInCents == _maxAwardLevel?.ThresholdInCents);
            stateMachine.OnTransitioned(
                transition => Logger.Info(
                    $"Transitioned from {transition.Source} to {transition.Destination} with the trigger {transition.Trigger}"));
            stateMachine.OnUnhandledTrigger(
                (state, trigger) => Logger.Info($"Handling an unknown trigger {trigger} while in the state {state}"));

            stateMachine.Activate();
            return stateMachine;
        }

        private StateMachine<BarkeeperStates, BarkeeperCashInTriggers> CreateCashInStateMachine(
            BarkeeperStates startingState)
        {
            var stateMachine = new StateMachine<BarkeeperStates, BarkeeperCashInTriggers>(startingState);
            stateMachine.Configure(BarkeeperStates.Disabled)
                .OnEntry(HandleCashInDisabled)
                .PermitIf(
                    BarkeeperCashInTriggers.BarkeeperReconfigured,
                    BarkeeperStates.Idle,
                    () => _enabled && _cashInRewardLevel.Any(x => x.Enabled));
            stateMachine.Configure(BarkeeperStates.Idle)
                .OnEntry(HandleCashInIdle)
                .Permit(BarkeeperCashInTriggers.CashInserted, BarkeeperStates.Active)
                .PermitReentry(BarkeeperCashInTriggers.BalanceCleared)
                .PermitIf(
                    BarkeeperCashInTriggers.BarkeeperReconfigured,
                    BarkeeperStates.Disabled,
                    () => !_enabled || !_cashInRewardLevel.Any(x => x.Enabled))
                .PermitReentryIf(
                    BarkeeperCashInTriggers.BarkeeperReconfigured,
                    () => _enabled && _cashInRewardLevel.Any(x => x.Enabled))
                .Permit(BarkeeperCashInTriggers.Disable, BarkeeperStates.Disabled);
            stateMachine.Configure(BarkeeperStates.Active)
                .OnEntry(HandleCashInActive)
                .Permit(BarkeeperCashInTriggers.Disable, BarkeeperStates.Disabled)
                .PermitIf(
                    BarkeeperCashInTriggers.BarkeeperReconfigured,
                    BarkeeperStates.Disabled,
                    () => !_enabled || !_cashInRewardLevel.Any(x => x.Enabled))
                .PermitIf(
                    BarkeeperCashInTriggers.BarkeeperReconfigured,
                    BarkeeperStates.Idle,
                    () => _enabled && _cashInRewardLevel.Any(x => x.Enabled))
                .Permit(BarkeeperCashInTriggers.BalanceCleared, BarkeeperStates.Idle);
            stateMachine.OnTransitioned(
                transition => Logger.Info(
                    $"Transitioned from {transition.Source} to {transition.Destination} with the trigger {transition.Trigger}"));
            stateMachine.OnUnhandledTrigger(
                (state, trigger) => Logger.Info($"Handling an unknown trigger {trigger} while in the state {state}"));
            return stateMachine;
        }

        private void ClearHaloLighting()
        {
            if (_haloLightToken == null)
            {
                return;
            }

            _edgeLighting.RemoveEdgeLightRenderer(_haloLightToken);
            _haloLightToken = null;
        }

        private void ClearButtonLighting()
        {
            if (_buttonLightToken == null)
            {
                return;
            }

            _edgeLighting.RemoveEdgeLightRenderer(_buttonLightToken);
            _buttonLightToken = null;
        }

        private void HandleCoinInDisabled()
        {
            lock (_lockObject)
            {
                ClearButtonLighting();
                _buttonLightToken = _edgeLighting.AddEdgeLightRenderer(
                    _disabledCoinInPattern,
                    _rewardLevels.CoinInStrategy.IdleBrightness);
            }
        }

        private void HandleCoinActive(RewardLevel reward)
        {
            lock (_lockObject)
            {
                ClearButtonLighting();
                _buttonLightToken = _edgeLighting.AddEdgeLightRenderer(
                    reward,
                    _rewardLevels.CoinInStrategy.ActiveBrightness);
            }
        }

        private void HandleBarKeeperButtonPressed()
        {
            lock (_lockObject)
            {
                if (_lastAward != null)
                { 
                    _lastAward.Awarded = true;
                    PersistActiveCoinInReward();
                }
                ClearButtonLighting();
                _buttonLightToken = _edgeLighting.AddEdgeLightRenderer(
                    _idleCoinInPattern,
                    _rewardLevels.CoinInStrategy.IdleBrightness);
            }
        }

        private void HandleCoinInIdle()
        {
            lock (_lockObject)
            {
                Reset();
                ClearButtonLighting();
                _buttonLightToken = _edgeLighting.AddEdgeLightRenderer(
                    _idleCoinInPattern,
                    _rewardLevels.CoinInStrategy.IdleBrightness);
            }
        }

        private void HandleCashInDisabled()
        {
            lock (_lockObject)
            {
                ClearHaloLighting();
                _haloLightToken = _edgeLighting.AddEdgeLightRenderer(
                    _disabledCashInPattern,
                    _rewardLevels.CashInStrategy.IdleBrightness);
            }
        }

        private void HandleCashInActive()
        {
            lock (_lockObject)
            {
                var rewardLevel = _cashInRewardLevel.Where(
                        x => x.Enabled && x.ThresholdInCents <= CreditsInDuringSession.MillicentsToCents())
                    .OrderByDescending(x => x.ThresholdInCents)
                    .FirstOrDefault();
                ClearHaloLighting();
                _haloLightToken = _edgeLighting.AddEdgeLightRenderer(
                    rewardLevel,
                    _rewardLevels.CashInStrategy.ActiveBrightness);
            }
        }

        private void HandleCashInIdle()
        {
            lock (_lockObject)
            {
                CreditsInDuringSession = 0;
                PersistSession();
                ClearHaloLighting();
                _haloLightToken = _edgeLighting.AddEdgeLightRenderer(
                    _idleCashInPattern,
                    _rewardLevels.CashInStrategy.IdleBrightness);
            }
        }

        private void CoinInTimerElapsed(object sender, ElapsedEventArgs e)
        {
            lock (_lockObject)
            {
                CoinInDuringSession = 0;
                PersistSession();
                PersistRateOfPlayElapsedTime(0);
                _rateOfPlayElapsedTimeFromPreviousSession = 0;
                _barkeeperSession.Reset();
            }
        }

        private void ZeroBalanceElapsed(object sender, ElapsedEventArgs e)
        {
            _coinInStateMachine.Fire(BarkeeperCoinInTrigger.TimedOut);
        }

        private enum BarkeeperStates
        {
            Disabled,
            Idle,
            Active
        }

        private enum BarkeeperCashInTriggers
        {
            Disable,
            BalanceCleared,
            CashInserted,
            BarkeeperReconfigured
        }

        private enum BarkeeperCoinInTrigger
        {
            Disable,
            PlayerCashedOut,
            CoinInReached,
            TimedOut,
            BarkeeperButtonPressed,
            BarkeeperReconfigured
        }
    }
}