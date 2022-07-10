namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Accounting.Contracts;
    using Accounting.Contracts.TransferOut;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.Monaco.Accounting.Contracts.Handpay;
    using Aristocrat.Monaco.Accounting.Contracts.Wat;
    using Contracts;
    using Contracts.Central;
    using Contracts.Configuration;
    using Contracts.Models;
    using Contracts.Progressives;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Localization.Properties;
    using log4net;

    /// <summary>
    ///     An implementation of <see cref="IGameHistory" />
    /// </summary>
    public class GameHistory : IGameHistory, IService
    {
        private const string GameHistoryKey = @"GameHistory";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IBank _bank;
        private readonly ICurrencyInContainer _currencyHandler;
        private readonly IGameDiagnostics _gameDiagnostics;
        private readonly IGameProvider _gameProvider;
        private readonly IIdProvider _idProvider;
        private readonly IPersistentBlock _persistentBlock;
        private readonly List<GameHistoryLog> _logs;
        private readonly IPropertiesManager _properties;
        private readonly ILoggedEventContainer _loggedEventContainer;
        private readonly ITransactionHistory _transactionHistory;
        private readonly IGameRoundMeterSnapshotProvider _meterSnapshotProvider;
        private readonly IGameConfigurationProvider _gameConfigurationProvider;
        private GameHistoryLog _currentLog;
        private readonly bool _keepGameRoundMeterSnapshots;
        private readonly object _logsLock = new();

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameHistory" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="properties">An <see cref="IPropertiesManager" /> instance.</param>
        /// <param name="bank">An <see cref="IBank" /> instance.</param>
        /// <param name="gameDiagnostics">An <see cref="IGameDiagnostics" /> instance.</param>
        /// <param name="gameProvider">An <see cref="IGameProvider" /> instance.</param>
        /// <param name="idProvider">An <see cref="IIdProvider" /> instance.</param>
        /// <param name="systemDisable">An <see cref="ISystemDisableManager" /> instance.</param>
        /// <param name="currencyHandler">The currency handler.</param>
        /// <param name="persistenceProvider">The persistence provider.</param>
        /// <param name="loggedEventContainer">The logged events</param>
        /// <param name="transactionHistory">An <see cref="ITransactionHistory"/> instance</param>
        /// <param name="meterSnapshotProvider">An <see cref="IGameRoundMeterSnapshotProvider"/> instance</param>
        /// <param name="gameConfigurationProvider">An <see cref="IGameConfigurationProvider"/> instance</param>
        public GameHistory(
            IPropertiesManager properties,
            IBank bank,
            IGameDiagnostics gameDiagnostics,
            IGameProvider gameProvider,
            IIdProvider idProvider,
            ISystemDisableManager systemDisable,
            ICurrencyInContainer currencyHandler,
            IPersistenceProvider persistenceProvider,
            ILoggedEventContainer loggedEventContainer,
            ITransactionHistory transactionHistory,
            IGameRoundMeterSnapshotProvider meterSnapshotProvider,
            IGameConfigurationProvider gameConfigurationProvider)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _gameDiagnostics = gameDiagnostics ?? throw new ArgumentNullException(nameof(gameDiagnostics));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
            var provider = persistenceProvider ?? throw new ArgumentNullException(nameof(persistenceProvider));
            _persistentBlock = provider.GetOrCreateBlock(GameHistoryKey, PersistenceLevel.Critical);
            _loggedEventContainer = loggedEventContainer ?? throw new ArgumentNullException(nameof(loggedEventContainer));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _meterSnapshotProvider = meterSnapshotProvider ?? throw new ArgumentNullException(nameof(meterSnapshotProvider));
            _gameConfigurationProvider = gameConfigurationProvider ?? throw new ArgumentNullException(nameof(gameConfigurationProvider));
            _keepGameRoundMeterSnapshots = properties.GetValue(GamingConstants.KeepGameRoundMeterSnapshots, true);

            if (systemDisable == null)
            {
                throw new ArgumentNullException(nameof(systemDisable));
            }

            _currencyHandler = currencyHandler ?? throw new ArgumentNullException(nameof(currencyHandler));
            // Get the maximum game history
            // TODO: This property needs to be set by jurisdiction
            MaxEntries = _properties.GetValue(GamingConstants.MaxGameHistory, GamingConstants.DefaultMaxGameHistory);

            _logs = new List<GameHistoryLog>(MaxEntries);

            LoadGameHistory();
            if (!IsGameFatalError)
            {
                return;
            }

            var log = _currentLog;

            systemDisable.Disable(GamingConstants.FatalGameErrorGuid, SystemDisablePriority.Immediate,
                () => GameErrorCode.LiabilityLimit == log?.ErrorCode
                    ? Localizer.For(CultureFor.Player).GetString(ResourceKeys.LiabilityCheckFailed)
                    : Localizer.For(CultureFor.Player).GetString(ResourceKeys.LegitimacyCheckFailed));
        }

        private int CurrentLogIndex { get; set; }

        /// <inheritdoc />
        public long LogSequence => _idProvider.GetCurrentLogSequence<IGameHistory>();

        /// <inheritdoc />
        public long PendingCurrencyIn => _currencyHandler.AmountIn;

        /// <inheritdoc />
        public IGameHistoryLog CurrentLog => _currentLog;

        /// <inheritdoc />
        public int TotalEntries
        {
            get
            {
                lock (_logsLock)
                {
                    return _logs.Count;
                }
            }
        }

        /// <inheritdoc />
        public int MaxEntries { get; }

        /// <inheritdoc />
        public bool IsRecoveryNeeded
        {
            get
            {
                if (_currentLog == null)
                {
                    return false;
                }

                return _currentLog.StartDateTime != DateTime.MinValue && _currentLog.PlayState != PlayState.Idle;
            }
        }

        /// <inheritdoc />
        public bool HasPendingCashOut => CurrentLog?.CashOutInfo.Any(i => !i.Complete) ?? false;

        /// <inheritdoc />
        public bool IsGameFatalError => _currentLog?.PlayState == PlayState.FatalError;

        /// <inheritdoc />
        public PlayState LastPlayState => _currentLog.PlayState;

        /// <inheritdoc />
        public bool IsDiagnosticsActive => _gameDiagnostics?.IsActive ?? false;

        /// <inheritdoc />
        public void Escrow(long initialWager, byte[] data)
        {
            InitializeLog(PlayState.PrimaryGameEscrow, initialWager, data, Enumerable.Empty<Jackpot>());

            Logger.Debug(
                $"[Game Escrow {CurrentLogIndex}] Game Id {_currentLog.GameId} Denom Id {_currentLog.DenomId} Start Credits {_currentLog.StartCredits} Wager {initialWager} Start {_currentLog.StartDateTime}");
        }

        /// <inheritdoc />
        public void Fail()
        {
            if (_gameDiagnostics.IsActive || _currentLog == null)
            {
                return;
            }

            UpdateTransactions(_currentLog, true);

            _currentLog.EndCredits = _bank.QueryBalance();
            _currentLog.PlayState = PlayState.Idle;
            _currentLog.FinalWager = 0;
            _currentLog.LastUpdate = DateTime.UtcNow;
            _currentLog.EndDateTime = DateTime.UtcNow;

            Logger.Debug($"[Finalizing Failed Game Log {CurrentLogIndex}]");

            if (_properties.GetValue(GamingConstants.KeepFailedGameOutcomes, true))
            {
                AddMeterSnapshot();
                Persist(_currentLog);
                IncrementLogIndex();
            }
            else
            {
                Persist(_currentLog);
            }
            Logger.Debug($"[Failed {CurrentLogIndex}]");
        }

        /// <inheritdoc />
        public void AppendOutcomes(IEnumerable<Outcome> outcomes)
        {
            if (_gameDiagnostics.IsActive)
            {
                return;
            }

            _currentLog.Outcomes = _currentLog.Outcomes.Concat(outcomes);
            _currentLog.LastUpdate = DateTime.UtcNow;

            Persist(_currentLog);

            Logger.Debug($"[Appended outcomes {CurrentLogIndex}]");
        }

        /// <inheritdoc />
        public void Start(long initialWager, byte[] data, IEnumerable<Jackpot> jackpotSnapshot)
        {
            if (_gameDiagnostics.IsActive)
            {
                return;
            }

            if (_currentLog?.PlayState != PlayState.PrimaryGameEscrow)
            {
                InitializeLog(PlayState.PrimaryGameStarted, initialWager, data, jackpotSnapshot);
            }
            else
            {
                _currentLog.PlayState = PlayState.PrimaryGameStarted;
                _currentLog.RecoveryBlob = data;
                _currentLog.JackpotSnapshot = jackpotSnapshot?.Select(item => new Jackpot(item)).ToList() ??
                                              Enumerable.Empty<Jackpot>();
                _currentLog.LastUpdate = DateTime.UtcNow;
                Persist(_currentLog);
            }

            AddMeterSnapshot();

            Logger.Debug(
                $"[Game Start {CurrentLogIndex}] Game Id {_currentLog!.GameId} Denom Id {_currentLog.DenomId} Start Credits {_currentLog.StartCredits} Wager {initialWager} Start {_currentLog.StartDateTime}");
        }

        public void AdditionalWager(long amount)
        {
            _currentLog.FinalWager += amount;

            Persist(_currentLog);
        }

        public void IncrementUncommittedWin(long win)
        {
            if (_gameDiagnostics.IsActive || win <= 0)
            {
                return;
            }

            _currentLog.UncommittedWin += win;
            Logger.Debug($"[Set Uncommitted Win {CurrentLogIndex}] Win Amount {win}");
        }

        public void CommitWin()
        {
            _currentLog.LastCommitIndex = 0;
            Persist(_currentLog);
        }

        /// <inheritdoc />
        public void EndPrimaryGame(long initialWin)
        {
            // Do not persist replays but do in game recovery.
            if (_gameDiagnostics.IsActive)
            {
                return;
            }

            var log = _currentLog;

            log.InitialWin = initialWin;
            log.PlayState = PlayState.PrimaryGameEnded;
            log.LastUpdate = DateTime.UtcNow;
            log.UncommittedWin = 0;

            Persist(log);

            Logger.Debug($"[Primary Game End {CurrentLogIndex}] Initial Win {initialWin}");
        }

        /// <inheritdoc />
        public void SecondaryGameChoice()
        {
            if (_gameDiagnostics.IsActive)
            {
                return;
            }

            var log = _currentLog;

            log.PlayState = PlayState.SecondaryGameChoice;
            log.LastUpdate = DateTime.UtcNow;

            Persist(log);

            Logger.Debug($"[Secondary Game Choice {CurrentLogIndex}]");
        }

        /// <inheritdoc />
        public void StartSecondaryGame(long stake)
        {
            if (_gameDiagnostics.IsActive)
            {
                return;
            }

            var log = _currentLog;

            log.SecondaryWager += stake;
            log.PlayState = PlayState.SecondaryGameStarted;
            log.LastUpdate = DateTime.UtcNow;
            Persist(log);

            Logger.Debug($"[Secondary Game Start {CurrentLogIndex}] Stake {stake}");
        }

        /// <inheritdoc />
        public void EndSecondaryGame(long win)
        {
            if (_gameDiagnostics.IsActive)
            {
                return;
            }

            var log = _currentLog;
            log.SecondaryPlayed++;
            log.SecondaryWin += win;
            log.PlayState = PlayState.SecondaryGameEnded;
            log.LastUpdate = DateTime.UtcNow;
            Persist(log);

            Logger.Debug($"[Secondary Game End {CurrentLogIndex}] Win {win}");
        }

        /// <inheritdoc />
        public void Results(long finalWin)
        {
            if (finalWin == 0 || _gameDiagnostics.IsActive)
            {
                return;
            }

            var log = _currentLog;

            log.FinalWin = finalWin;
            log.LastUpdate = DateTime.UtcNow;

            Persist(log);

            Logger.Debug($"[Game Results {CurrentLogIndex}] Final Win {finalWin}");
        }

        public void AddGameWinBonus(long win)
        {
            if (win == 0 || _gameDiagnostics.IsActive)
            {
                return;
            }

            var log = _currentLog;

            log.GameWinBonus = win;
            log.LastUpdate = DateTime.UtcNow;
            log.EndCredits = _bank.QueryBalance(); // Reset the ending credits
            Persist(log);

            Logger.Debug($"[Game Results {CurrentLogIndex}] Game Win Bonus {win}");
        }

        /// <inheritdoc />
        public void PayResults()
        {
            // Do not persist replays but do in game recovery.
            if (_gameDiagnostics.IsActive)
            {
                return;
            }

            var log = _currentLog;

            log.PlayState = PlayState.PayGameResults;
            log.LastUpdate = DateTime.UtcNow;

            Persist(log);

            Logger.Debug($"[Pay Results {CurrentLogIndex}]");
        }

        /// <inheritdoc />
        public void AppendGameRoundEventInfo(IList<string> newDescriptions)
        {
            // Do not persist replays but do in game recovery.
            if (newDescriptions.Count <= 0 || _gameDiagnostics.IsActive)
            {
                return;
            }

            _currentLog.GameRoundDescriptions += string.Join("\n", newDescriptions);

            Logger.Debug($"[AppendGameRoundEventInfo {CurrentLogIndex}]");
        }

        /// <inheritdoc />
        public void AppendJackpotInfo(JackpotInfo jackpot)
        {
            // Do not persist replays but do in game recovery.
            if (_gameDiagnostics.IsActive)
            {
                return;
            }

            _currentLog.Jackpots = new List<JackpotInfo>(_currentLog.Jackpots) { jackpot };
            _currentLog.PlayState = PlayState.ProgressivePending;
            _currentLog.LastUpdate = DateTime.UtcNow;

            Persist(_currentLog);

            Logger.Debug($"[AppendJackpotInfo {CurrentLogIndex}]");
        }

        /// <inheritdoc />
        public void LogGameRoundDetails(GameRoundDetails details)
        {
            _currentLog.GameRoundDetails = details;
        }

        /// <inheritdoc />
        public void ClearForRecovery()
        {
            if (_gameDiagnostics.IsActive)
            {
                return;
            }

            _currentLog.UncommittedWin = 0;
            _currentLog.GameRoundDescriptions = string.Empty;
            _currentLog.FreeGameIndex = 0;

            Persist(_currentLog);

            Logger.Debug($"[Clear Game Round Event Info {CurrentLogIndex}]");
        }

        /// <inheritdoc />
        public void LogFatalError(GameErrorCode errorCode)
        {
            // Do not persist replays but do in game recovery.
            if (_gameDiagnostics.IsActive)
            {
                return;
            }

            var log = _currentLog;

            log.PlayState = PlayState.FatalError;
            log.ErrorCode = errorCode;

            Persist(log);

            Logger.Debug($"[LogFatalError {CurrentLogIndex}]");
        }

        /// <inheritdoc />
        public void EndGame()
        {
            // Do not persist replays but do in game recovery.
            if (_gameDiagnostics.IsActive)
            {
                return;
            }

            var endTime = DateTime.UtcNow;

            var endCredits = _bank.QueryBalance();

            // Ensure the free game round is completed
            EndFreeGame();

            var log = _currentLog;

            log.EndCredits = endCredits;
            log.EndDateTime = endTime;
            log.EndTransactionId = _idProvider.CurrentTransactionId;
            log.PlayState = PlayState.GameEnded;
            log.LastUpdate = endTime;

            Persist(log);

            Logger.Debug($"[Game End {CurrentLogIndex}] End Time {endTime}");
        }

        /// <inheritdoc />
        public void End()
        {
            // Do not persist replays but do in game recovery.
            if (_gameDiagnostics.IsActive)
            {
                return;
            }

            var log = _currentLog;

            UpdateTransactions(log, true);
            AddMeterSnapshot();

            log.PlayState = PlayState.Idle;
            log.Events = _loggedEventContainer.HandOffEvents();
            log.LastUpdate = DateTime.UtcNow;

            Persist(log);

            Logger.Debug($"[Finalizing Game Log {CurrentLogIndex}]");

            IncrementLogIndex();

            Logger.Debug($"[End {CurrentLogIndex}]");
        }

        /// <inheritdoc />
        public bool LoadReplay(int replayIndex, out byte[] data)
        {
            lock (_logsLock)
            {
                data = _logs.ElementAtOrDefault(replayIndex)?.RecoveryBlob;
            }

            return data != null;
        }

        public bool LoadRecoveryPoint(out byte[] data)
        {
            return LoadReplay(CurrentLogIndex, out data);
        }

        /// <inheritdoc />
        public void AssociateTransactions(IEnumerable<TransactionInfo> transactions, bool applyToFreeGame = false)
        {
            if (CurrentLog is not GameHistoryLog log)
            {
                return;
            }

            var finalList = SetTransactionGameIndex(
                transactions,
                applyToFreeGame ? log.FreeGameIndex : 0,
                log.Transactions);

            if (applyToFreeGame)
            {
                var lastFreeGame = log.FreeGames.LastOrDefault(g => g.EndDateTime != DateTime.MinValue);
                if (lastFreeGame != null)
                {
                    using var persistentTransaction = _persistentBlock.Transaction();
                    log.Transactions = finalList;
                    _currencyHandler.Reset();
                    lastFreeGame.AmountOut =
                        finalList.Where(t => t.GameIndex == log.FreeGameIndex).Sum(t => t.Amount);
                    int index;
                    lock (_logsLock)
                    {
                        index = _logs.IndexOf(_currentLog);
                    }

                    persistentTransaction.SetValue(index, log);
                    persistentTransaction.Commit();

                    return;
                }
            }

            using (var persistentTransaction = _persistentBlock.Transaction())
            {
                log.Transactions = finalList;
                _currencyHandler.Reset();
                log.AmountOut = finalList.Where(t => t.GameIndex == 0).Sum(t => t.Amount);
                int index;
                lock (_logsLock)
                {
                    index = _logs.IndexOf(_currentLog);
                }

                persistentTransaction.SetValue(index, log);
                persistentTransaction.Commit();
            }

            Logger.Debug($"[AssociateTransactions {CurrentLogIndex}] {string.Join(",", finalList)}");
        }

        public void AppendCashOut(CashOutInfo cashOut)
        {
            if (CurrentLog is not GameHistoryLog log)
            {
                return;
            }

            var cashOutInfo = log.CashOutInfo.ToList();
            if (cashOutInfo.Any(c => c.Reason != TransferOutReason.BonusPay)
                && cashOut.Reason != TransferOutReason.BonusPay)
            {
                _currencyHandler.Reset();
            }

            cashOutInfo.Add(cashOut);
            log.CashOutInfo = cashOutInfo;

            Persist(log);

            Logger.Debug($"[Append Cash Out Info {CurrentLogIndex}] Info {cashOut.Reason} {cashOut.Amount} {cashOut.TraceId}");
        }

        public void CompleteCashOut(Guid referenceId)
        {
            if (CurrentLog is not GameHistoryLog log)
            {
                return;
            }

            var infos = log.CashOutInfo.ToList();
            var infoToComplete = infos.FirstOrDefault(c => c.TraceId == referenceId);
            if (infoToComplete == null)
            {
                return;
            }

            infoToComplete.Complete = true;

            log.CashOutInfo = infos;
            Persist(log);

            Logger.Debug($"[Complete Cash Out {CurrentLogIndex}] Info {infoToComplete.Reason} {infoToComplete.Amount} {infoToComplete.TraceId}");
        }

        /// <inheritdoc />
        public IGameHistoryLog GetByIndex(int index)
        {
            lock (_logsLock)
            {
                return _logs.ElementAtOrDefault(index == -1 ? MaxEntries : index);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IGameHistoryLog> GetGameHistory()
        {
            lock (_logsLock)
            {
                return _logs.ToArray();
            }
        }

        /// <inheritdoc />
        public void SaveRecoveryPoint(byte[] data)
        {
            // Do not persist replays but do in game recovery.
            if (_gameDiagnostics.IsActive)
            {
                return;
            }

            var log = _currentLog;
            log.RecoveryBlob = data;

            Persist(log);

            LogRecoveryData(data, "[RECOVERY POINT] ->");
        }

        /// <inheritdoc />
        public void LogRecoveryData(byte[] data, string header)
        {
            if (data == null)
            {
                return;
            }

            var encoded = Encoding.ASCII.GetString(data);

            Logger.Debug($"{header} {encoded.Length} bytes : {encoded}");
        }

        public void StartFreeGame()
        {
            // Do not persist replays but do in game recovery.
            if (_gameDiagnostics.IsActive)
            {
                return;
            }

            var log = _currentLog;

            var existing = log.FreeGames.ElementAtOrDefault(log.FreeGameIndex);
            if (log.FreeGameIndex < log.LastCommitIndex || existing != null)
            {
                if (existing is { Result: GameResult.None })
                {
                    existing.FinalWin = 0;
                }

                log.FreeGameIndex++;
                return;
            }

            UpdateTransactions(log, false);

            log.FreeGames = new List<FreeGame>(log.FreeGames)
            {
                new() { StartDateTime = DateTime.UtcNow, StartCredits = _bank.QueryBalance() }
            };

            log.FreeGameIndex++;

            Logger.Debug($"[StartFreeGame {CurrentLogIndex} ({log.FreeGameIndex})]");
        }

        public void FreeGameResults(long finalWin)
        {
            // Do not persist replays but do in game recovery.  finalWin == 0: Save ourselves some cycles if the win is 0
            if (_gameDiagnostics.IsActive || finalWin == 0)
            {
                return;
            }

            var log = _currentLog;
            var freeGames = log.FreeGames.ToList();
            if (log.FreeGameIndex <= log.LastCommitIndex)
            {
                return;
            }

            var freeGame = freeGames.ElementAt(log.FreeGameIndex - 1);
            if (freeGame is not { Result: GameResult.None })
            {
                return;
            }

            freeGame.FinalWin += finalWin;

            Logger.Debug($"[FreeGameResults {CurrentLogIndex} ({log.FreeGameIndex}): Win {finalWin}]");
        }

        public IFreeGameInfo EndFreeGame()
        {
            // Do not persist replays but do in game recovery.
            if (_gameDiagnostics.IsActive || _currentLog.FreeGames.IsNullOrEmpty())
            {
                return null;
            }

            var log = _currentLog;
            var freeGames = log.FreeGames.ToList();
            if (log.FreeGameIndex <= log.LastCommitIndex)
            {
                return null;
            }

            var freeGame = freeGames.LastOrDefault(g => g.Result == GameResult.None);
            if (freeGame == null)
            {
                return null;
            }

            freeGame.EndDateTime = DateTime.UtcNow;
            freeGame.EndCredits = _bank.QueryBalance();
            log.LastCommitIndex = log.FreeGameIndex;
            Persist(log);

            Logger.Debug($"[EndFreeGame {CurrentLogIndex} ({log.FreeGameIndex}): Final Win {freeGame.FinalWin}]");

            return freeGame;

        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IGameHistory) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        public void AddPromoWager(long amount)
        {
            var log = _currentLog;
            log.PromoWager += amount;
            Persist(log);
        }

        private static List<TransactionInfo> SetTransactionGameIndex(
            IEnumerable<TransactionInfo> transactions,
            int gameIndex,
            IEnumerable<TransactionInfo> existing = null)
        {
            return transactions.Select(
                o =>
                {
                    if (!(existing?.Any(e => e.TransactionId == o.TransactionId) ?? false))
                    {
                        o.GameIndex = gameIndex;
                    }

                    return o;
                }).ToList();
        }

        private void Persist(GameHistoryLog log)
        {
            int index;
            lock (_logsLock)
            {
                index = _logs.IndexOf(_currentLog);
            }

            _persistentBlock.SetValue(index > -1 ? index : CurrentLogIndex, log);
        }

        private void LoadGameHistory()
        {
            lock (_logsLock)
            {
                _logs.Clear();
            }

            for (var index = 0; index < MaxEntries; ++index)
            {
                var exists = _persistentBlock.GetValue(index, out GameHistoryLog result);
                if (exists)
                {
                    result.StorageIndex = index;
                    lock (_logsLock)
                    {
                        _logs.Insert(index, result);
                    }
                }
                else
                {
                    break;
                }
            }

            lock (_logsLock)
            {
                _currentLog = _logs.Any() ? _logs.OrderByDescending(e => e.TransactionId).FirstOrDefault() : null;
                CurrentLogIndex = _currentLog is null ? 0 : _logs.IndexOf(_currentLog);
            }

            var keepFailedGames = _properties.GetValue(GamingConstants.KeepFailedGameOutcomes, true);
            if (_currentLog?.PlayState == PlayState.Idle &&
                (keepFailedGames || _currentLog.Result != GameResult.Failed))
            {
                IncrementLogIndex();
            }
        }

        private void InitializeLog(
            PlayState state,
            long initialWager,
            byte[] data,
            IEnumerable<Jackpot> jackpotSnapshot)
        {
            if (_gameDiagnostics.IsActive)
            {
                return;
            }

            var log = new GameHistoryLog(CurrentLogIndex);

            var startTime = DateTime.UtcNow;

            // Moved the following out of the BeginTransaction block to support the scoped transaction
            var transactionId = _idProvider.GetNextTransactionId();
            var newLogSequence = _currentLog?.Result != GameResult.Failed || _currentLog?.StorageIndex != log.StorageIndex ||
                                 _properties.GetValue(GamingConstants.KeepFailedGameOutcomes, true);
            var sequenceNumber = newLogSequence ? _idProvider.GetNextLogSequence<IGameHistory>() : _currentLog.LogSequence;

            log.TransactionId = transactionId;
            log.LogSequence = sequenceNumber;
            log.StartDateTime = startTime;

            var (game, denomination) = _properties.GetSelectedGame();
            log.GameId = game.Id;
            log.DenomId = denomination.Value;
            log.StartCredits = _bank.QueryBalance();
            log.PlayState = state;
            log.InitialWager = initialWager;
            log.FinalWager = initialWager;
            log.PromoWager = 0L;
            log.RecoveryBlob = data;
            log.GameRoundDetails = null;

            log.DenomConfiguration = new GameConfiguration
            {
                BetOption = denomination.BetOption,
                LineOption = denomination.LineOption,
                SecondaryEnabled = denomination.SecondaryEnabled,
                LetItRideEnabled = denomination.LetItRideEnabled,
                BonusBet = denomination.BonusBet,
                MinimumWagerCredits = denomination.MinimumWagerCredits,
                MaximumWagerCredits = denomination.MaximumWagerCredits,
                MaximumWagerOutsideCredits = denomination.MaximumWagerOutsideCredits
            };

            log.LastUpdate = startTime;
            log.LastCommitIndex = -1;
            log.FreeGameIndex = 0;
            log.LocaleCode = _properties.GetValue(GamingConstants.SelectedLocaleCode, "en-us");
            log.GameConfiguration = _gameConfigurationProvider.GetActive(game.ThemeId)?.RestrictionDetails?.Mapping?.Any() ?? false
                ? _properties.GetValue(GamingConstants.GameConfiguration, string.Empty)
                : string.Empty;
            var transactions = newLogSequence
                ? _currencyHandler.Transactions
                : _currentLog.Transactions.Concat(_currencyHandler.Transactions);
            log.Transactions = SetTransactionGameIndex(transactions, -1);
            log.JackpotSnapshot = jackpotSnapshot?.Select(item => new Jackpot(item)).ToList() ??
                                  Enumerable.Empty<Jackpot>();

            log.GameRoundDescriptions = string.Empty;
            log.CashOutInfo = Enumerable.Empty<CashOutInfo>();
            log.Events = Enumerable.Empty<GameEventLogEntry>();
            log.FreeGames = Enumerable.Empty<FreeGame>();
            log.Jackpots = Enumerable.Empty<JackpotInfo>();
            log.Outcomes = Enumerable.Empty<Outcome>();
            log.MeterSnapshots = new List<GameRoundMeterSnapshot>();

            bool firstGamePlay;
            lock (_logsLock)
            {
                firstGamePlay = !_logs.Any();
            }

            using (var transaction = _persistentBlock.Transaction())
            {
                transaction.SetValue(CurrentLogIndex, log);

                _currencyHandler.Reset();

                transaction.Commit();

                lock (_logsLock)
                {
                    if (CurrentLogIndex >= _logs.Count)
                    {
                        _logs.Insert(CurrentLogIndex, log);
                    }
                    else
                    {
                        _logs[CurrentLogIndex] = log;
                    }
                }

                _currentLog = log;
            }

            if (firstGamePlay)
            {
                AddTransactionsToGameLog(transactionId);
            }
        }

        private void UpdateTransactions(GameHistoryLog log, bool postGame)
        {
            var indexModifier = postGame ? 0 : -1;

            var transactions = log.Transactions.ToList();
            var newTransactions = SetTransactionGameIndex(
                _currencyHandler.Transactions,
                log.FreeGameIndex + indexModifier);
            transactions.AddRange(newTransactions);
            log.Transactions = transactions;
            _currencyHandler.Reset();
        }

        private void IncrementLogIndex()
        {
            CurrentLogIndex++;
            if (CurrentLogIndex == MaxEntries)
            {
                CurrentLogIndex = 0;
            }
        }

        private void AddTransactionsToGameLog(long transactionId)
        {
            var trans = _transactionHistory.RecallTransactions(true);
            if (trans == null)
            {
                return;
            }

            var tranInfoList = new List<TransactionInfo>();
            long lastPlayTranId;
            lock (_logsLock)
            {
                lastPlayTranId = CurrentLogIndex > 0 ? _logs[CurrentLogIndex - 1].TransactionId : 0;
            }

            trans = trans.Where(t => t.TransactionId > lastPlayTranId)
                .OrderByDescending(t => t.TransactionId);
            foreach (var tran in trans)
            {
                var tranInfo = ConvertToTransactionInfo(tran);
                if (tranInfo.HasValue &&
                    _currentLog.Transactions.All(t => t.TransactionId != transactionId))
                {
                    tranInfoList.Add(tranInfo.Value);
                }
            }

            if (tranInfoList.Any())
            {
                _currentLog.Transactions = tranInfoList;
            }
        }

        private static TransactionInfo? ConvertToTransactionInfo(ITransaction transaction)
        {
            var info = new TransactionInfo
            {
                Time = transaction.TransactionDateTime,
                TransactionType = transaction.GetType(),
                TransactionId = transaction.TransactionId
            };

            switch (transaction)
            {
                case BillTransaction billTransaction:
                    info.Amount = billTransaction.Amount;
                    break;

                case VoucherInTransaction voucherInTransaction:
                    info.Amount = voucherInTransaction.Amount;
                    info.CashableAmount = voucherInTransaction.TypeOfAccount == AccountType.Cashable
                        ? voucherInTransaction.Amount
                        : 0;
                    info.CashablePromoAmount = voucherInTransaction.TypeOfAccount == AccountType.Promo
                        ? voucherInTransaction.Amount
                        : 0;
                    info.NonCashablePromoAmount = voucherInTransaction.TypeOfAccount == AccountType.NonCash
                        ? voucherInTransaction.Amount
                        : 0;
                    break;

                case HandpayTransaction handpayTransaction:

                    info.Amount = handpayTransaction.TransactionAmount;
                    info.CashableAmount = handpayTransaction.CashableAmount;
                    info.CashablePromoAmount = handpayTransaction.PromoAmount;
                    info.NonCashablePromoAmount = handpayTransaction.NonCashAmount;
                    info.HandpayType = handpayTransaction.HandpayType;
                    info.KeyOffType = handpayTransaction.KeyOffType;
                    break;

                case WatOnTransaction watOnTransaction:
                    info.Amount = watOnTransaction.TransactionAmount;
                    info.CashableAmount = watOnTransaction.TransferredCashableAmount;
                    info.CashablePromoAmount = watOnTransaction.TransferredPromoAmount;
                    info.NonCashablePromoAmount = watOnTransaction.TransferredNonCashAmount;
                    break;

                case VoucherOutTransaction voucherOutTransaction:
                    info.Amount = voucherOutTransaction.Amount;
                    break;

                case WatTransaction watTransaction:
                    info.Amount = watTransaction.TransactionAmount;
                    info.CashableAmount = watTransaction.TransferredCashableAmount;
                    info.CashablePromoAmount = watTransaction.TransferredPromoAmount;
                    info.NonCashablePromoAmount = watTransaction.TransferredNonCashAmount;
                    break;

                default:
                    return null;
            }

            return info;
        }

        private void AddMeterSnapshot()
        {
            if (!_keepGameRoundMeterSnapshots)
            {
                return;
            }
            _currentLog.MeterSnapshots.Add(
                _meterSnapshotProvider.GetSnapshot(_currentLog.PlayState)
            );
        }
    }
}