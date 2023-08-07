namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts;
    using Contracts.Session;
    using Hardware.Contracts;
    using Hardware.Contracts.IdReader;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    public class PlayerSessionHistory : IPlayerSessionHistory, IService
    {
        private const string BlockIndexField = @"BlockIndex";
        private const string TransactionIdField = @"TransactionId";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IIdProvider _idProvider;
        private readonly IPersistentStorageAccessor _indexAccessor;
        private readonly List<IPlayerSessionLog> _playerSessionHistory;
        private readonly IPersistentStorageAccessor _playerSessionHistoryAccessor;
        private readonly object _sync = new object();

        private int? _blockIndex;

        /// <summary>
        ///     Handles all player session history and current session
        /// </summary>
        public PlayerSessionHistory(IIdProvider idProvider, IPersistentStorageManager storageManager)
        {
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));

            if (storageManager == null)
            {
                throw new ArgumentNullException(nameof(storageManager));
            }

            var indexBlockName = GetType() + ".Index";
            _indexAccessor = storageManager.GetAccessor(PersistenceLevel.Critical, indexBlockName);

            var dataBlockName = GetType() + ".Data";
            _playerSessionHistoryAccessor = storageManager.GetAccessor(
                PersistenceLevel.Critical,
                dataBlockName,
                MaxEntries);

            _playerSessionHistory = new List<IPlayerSessionLog>(MaxEntries);

            LoadPlayerSessionHistory();
        }

        private int BlockIndex
        {
            get => (int)(_blockIndex ?? (_blockIndex = (int)_indexAccessor[BlockIndexField]));

            set => _indexAccessor[BlockIndexField] = _blockIndex = value;
        }

        /// <inheritdoc />
        public int TotalEntries => _playerSessionHistory.Count;

        /// <inheritdoc />
        public long LogSequence => _idProvider.GetCurrentLogSequence<IPlayerSessionLog>();

        /// <inheritdoc />
        public IPlayerSessionLog CurrentLog => _playerSessionHistory.Count == 0
            ? null
            : _playerSessionHistory.ElementAtOrDefault(BlockIndex == 0 ? MaxEntries - 1 : BlockIndex - 1);

        /// <inheritdoc />
        public int MaxEntries => 35; // TODO: pull value from config

        /// <inheritdoc />
        public IEnumerable<IPlayerSessionLog> GetHistory()
        {
            return _playerSessionHistory;
        }

        public IPlayerSessionLog GetByTransactionId(long transactionId)
        {
            return _playerSessionHistory.SingleOrDefault(l => l.TransactionId == transactionId);
        }

        /// <inheritdoc />
        public void AddLog(IPlayerSessionLog playerSessionLog)
        {
            var sequenceNumber = _idProvider.GetNextLogSequence<IPlayerSessionLog>();

            lock (_sync)
            {
                using (var transaction = _playerSessionHistoryAccessor.StartTransaction())
                {
                    ResetBlockIndexIfLastPlayerDidNotPlay();

                    MapToBlockValues(BlockIndex, playerSessionLog, transaction, sequenceNumber);

                    transaction.Commit();

                    AddToHistory(playerSessionLog);

                    Logger.Debug($"Player session updated {BlockIndex}  transactionId:{playerSessionLog.TransactionId}");

                    BlockIndex++;
                    if (BlockIndex == MaxEntries)
                    {
                        BlockIndex = 0;
                    }
                }
            }
        }

        public void UpdateLog(IPlayerSessionLog playerSessionLog)
        {
            lock (_sync)
            {
                using (var transaction = _playerSessionHistoryAccessor.StartTransaction())
                {
                    var index = _playerSessionHistory.FindIndex(a => a.TransactionId == playerSessionLog.TransactionId);

                    Logger.Debug($"before update log: _playerSessionHistory count: {_playerSessionHistory.Count} .index: {index} .transactionId:{playerSessionLog.TransactionId}");

                    MapToBlockValues(index, playerSessionLog, transaction, playerSessionLog.LogSequence);

                    transaction.Commit();

                    _playerSessionHistory[index] = playerSessionLog;

                    Logger.Debug($"After update log: _playerSessionHistory count: {_playerSessionHistory.Count} .index {index} .transactionId:{playerSessionLog.TransactionId}");
                }
            }
        }

        /// <inheritdoc />
        public string Name => typeof(PlayerSessionHistory).ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IPlayerSessionHistory) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        private void ResetBlockIndexIfLastPlayerDidNotPlay()
        {
            if (_playerSessionHistory.Count == 0)
            {
                return;
            }

            var lastIndex = BlockIndex == 0 ? MaxEntries - 1 : BlockIndex - 1;

            var lastLog = _playerSessionHistory[lastIndex];

            if (lastLog.TiedCount + lastLog.WonCount + lastLog.LostCount == 0) //if last player did not play 
            {
                BlockIndex = lastIndex; //override it with last index
            }
        }

        private void LoadPlayerSessionHistory()
        {
            _playerSessionHistory.Clear();

            var results = _playerSessionHistoryAccessor.GetAll();

            for (var index = 0; index < MaxEntries; ++index)
            {
                if (results.TryGetValue(index, out var values))
                {
                    var data = MapFromValues(values);
                    if (data != null)
                    {
                        _playerSessionHistory.Insert(index, data);
                    }
                }
            }
        }

        private static IPlayerSessionLog MapFromValues(IDictionary<string, object> values)
        {
            var transactionId = (long)values[TransactionIdField];

            return transactionId == 0
                ? null
                : new PlayerSessionLog
                {
                    TransactionId = transactionId,
                    LogSequence = (long)values[@"LogSequence"],
                    PlayerId = (string)values[@"PlayerId"],
                    IdNumber = (string)values[@"IdNumber"],
                    StartDateTime = (DateTime)values[@"StartDateTime"],
                    EndDateTime = (DateTime)values[@"EndDateTime"],
                    BasePointAward = (long)values[@"BasePointAward"],
                    CurrentHotLevel = (int)values[@"CurrentHotLevel"],
                    HighestHotLevel = (int)values[@"HighestHotLevel"],
                    EgmPaidBonusNonWonAmount = (long)values[@"EgmPaidBonusNonWonAmount"],
                    EgmPaidBonusWonAmount = (long)values[@"EgmPaidBonusWonAmount"],
                    EgmPaidGameWonAmount = (long)values[@"EgmPaidGameWonAmount"],
                    EgmPaidProgWonAmount = (long)values[@"EgmPaidProgWonAmount"],
                    DenomId = (long)values[@"DenomId"],
                    TheoreticalHoldAmount = (long)values[@"TheoreticalHoldAmount"],
                    TheoreticalPaybackAmount = (long)values[@"TheoreticalPaybackAmount"],
                    WonCount = (long)values[@"WonCount"],
                    TiedCount = (long)values[@"TiedCount"],
                    LostCount = (long)values[@"LostCount"],
                    Exception = (int)values[@"Exception"],
                    HandPaidBonusNonWonAmount = (long)values[@"HandPaidBonusNonWonAmount"],
                    HandPaidBonusWonAmount = (long)values[@"HandPaidBonusWonAmount"],
                    HandPaidGameWonAmount = (long)values[@"HandPaidGameWonAmount"],
                    HandPaidProgWonAmount = (long)values[@"HandPaidProgWonAmount"],
                    WageredNonCashAmount = (long)values[@"WageredNonCashAmount"],
                    WageredPromoAmount = (long)values[@"WageredPromoAmount"],
                    WageredCashableAmount = (long)values[@"WageredCashableAmount"],
                    SessionCarryOver = (long)values[@"SessionCarryOver"],
                    HostCarryOver = (long)values[@"HostCarryOver"],
                    HostPointAward = (long)values[@"HostPointAward"],
                    PlayerPointAward = (long)values[@"PlayerPointAward"],
                    OverridePointAward = (long)values[@"OverridePointAward"],
                    PaytableId = (string)values[@"PaytableId"],
                    ThemeId = (string)values[@"ThemeId"],
                    OverrideId = (long)values[@"OverrideId"],
                    PlayerSessionState = (PlayerSessionState)values[@"PlayerSessionState"],
                    IdReaderType = (IdReaderTypes)values[@"IdReaderType"],
                    SessionMeters = StorageUtilities.GetListFromByteArray<SessionMeter>((byte[])values[@"SessionMeters"])
                };
        }

        private static void MapToBlockValues(
            int blockIndex,
            IPlayerSessionLog playerSessionLog,
            IPersistentStorageTransaction transaction,
            long sequenceNumber)
        {
            transaction[blockIndex, @"LogSequence"] = playerSessionLog.LogSequence = sequenceNumber;
            transaction[blockIndex, @"PlayerId"] = playerSessionLog.PlayerId;
            transaction[blockIndex, @"IdNumber"] = playerSessionLog.IdNumber;
            transaction[blockIndex, @"StartDateTime"] = playerSessionLog.StartDateTime;
            transaction[blockIndex, @"EndDateTime"] = playerSessionLog.EndDateTime;
            transaction[blockIndex, @"BasePointAward"] = playerSessionLog.BasePointAward;
            transaction[blockIndex, @"CurrentHotLevel"] = playerSessionLog.CurrentHotLevel;
            transaction[blockIndex, @"HighestHotLevel"] = playerSessionLog.HighestHotLevel;
            transaction[blockIndex, @"EgmPaidBonusNonWonAmount"] = playerSessionLog.EgmPaidBonusNonWonAmount;
            transaction[blockIndex, @"EgmPaidBonusWonAmount"] = playerSessionLog.EgmPaidBonusWonAmount;
            transaction[blockIndex, @"EgmPaidGameWonAmount"] = playerSessionLog.EgmPaidGameWonAmount;
            transaction[blockIndex, @"EgmPaidProgWonAmount"] = playerSessionLog.EgmPaidProgWonAmount;
            transaction[blockIndex, @"DenomId"] = playerSessionLog.DenomId;
            transaction[blockIndex, @"TheoreticalHoldAmount"] = playerSessionLog.TheoreticalHoldAmount;
            transaction[blockIndex, @"TheoreticalPaybackAmount"] = playerSessionLog.TheoreticalPaybackAmount;
            transaction[blockIndex, @"WonCount"] = playerSessionLog.WonCount;
            transaction[blockIndex, @"TiedCount"] = playerSessionLog.TiedCount;
            transaction[blockIndex, @"LostCount"] = playerSessionLog.LostCount;
            transaction[blockIndex, @"Exception"] = playerSessionLog.Exception;
            transaction[blockIndex, @"HandPaidBonusNonWonAmount"] = playerSessionLog.HandPaidBonusNonWonAmount;
            transaction[blockIndex, @"HandPaidBonusWonAmount"] = playerSessionLog.HandPaidBonusWonAmount;
            transaction[blockIndex, @"HandPaidGameWonAmount"] = playerSessionLog.HandPaidGameWonAmount;
            transaction[blockIndex, @"HandPaidProgWonAmount"] = playerSessionLog.HandPaidProgWonAmount;
            transaction[blockIndex, @"WageredNonCashAmount"] = playerSessionLog.WageredNonCashAmount;
            transaction[blockIndex, @"WageredPromoAmount"] = playerSessionLog.WageredPromoAmount;
            transaction[blockIndex, @"WageredCashableAmount"] = playerSessionLog.WageredCashableAmount;
            transaction[blockIndex, @"SessionCarryOver"] = playerSessionLog.SessionCarryOver;
            transaction[blockIndex, @"HostCarryOver"] = playerSessionLog.HostCarryOver;
            transaction[blockIndex, @"HostPointAward"] = playerSessionLog.HostPointAward;
            transaction[blockIndex, @"PlayerPointAward"] = playerSessionLog.PlayerPointAward;
            transaction[blockIndex, @"OverridePointAward"] = playerSessionLog.OverridePointAward;
            transaction[blockIndex, @"PaytableId"] = playerSessionLog.PaytableId;
            transaction[blockIndex, @"ThemeId"] = playerSessionLog.ThemeId;
            transaction[blockIndex, @"OverrideId"] = playerSessionLog.OverrideId;
            transaction[blockIndex, @"PlayerSessionState"] = playerSessionLog.PlayerSessionState;
            transaction[blockIndex, @"IdReaderType"] = playerSessionLog.IdReaderType;
            transaction.UpdateList(blockIndex, @"SessionMeters", playerSessionLog.SessionMeters);
            transaction[blockIndex, TransactionIdField] = playerSessionLog.TransactionId;
        }

        private void AddToHistory(IPlayerSessionLog playerSessionLog)
        {
            if (BlockIndex >= _playerSessionHistory.Count)
            {
                _playerSessionHistory.Insert(BlockIndex, playerSessionLog);
            }
            else
            {
                _playerSessionHistory[BlockIndex] = playerSessionLog;
            }
        }
    }
}
