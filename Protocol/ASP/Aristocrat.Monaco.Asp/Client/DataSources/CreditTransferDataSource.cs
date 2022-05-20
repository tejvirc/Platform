namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Application.Contracts.Extensions;
    using Contracts;
    using Extensions;
    using Gaming.Contracts;
    using Gaming.Contracts.Bonus;
    using Gaming.Contracts.Diagnostics;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using HandlerMap = System.Collections.Generic.Dictionary<string, (System.Func<object> Getter, System.Action<object> Setter)>;

    public class CreditTransferDataSource : IDisposableDataSource, ITransaction
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public const string BonusCreditsWin = "BCredits_Win";

        public const string BonusReason = "BReason";

        private readonly IBonusHandler _bonusHandler;

        private readonly HandlerMap _handlers;

        private readonly Dictionary<string, object> _cachedMemberValue = new Dictionary<string, object>();

        private readonly IEventBus _eventBus;

        private IPersistentStorageAccessor _persistentStorageAccessor;

        private readonly IPropertiesManager _properties;

        private readonly IPersistentStorageManager _persistentStorageManager;

        private bool _disposed;

        public CreditTransferDataSource(IEventBus eventBus,
            IBonusHandler bonusHandler,
            IPropertiesManager properties,
            IPersistentStorageManager persistentStorageManager)
        {
            _bonusHandler = bonusHandler ?? throw new ArgumentNullException(nameof(bonusHandler));
            _handlers = GetMembersMap();

            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _eventBus.Subscribe<BonusAwardedEvent>(this, OnBonusAwarded);

            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _persistentStorageManager = persistentStorageManager ?? throw new ArgumentNullException(nameof(persistentStorageManager));

            InitializePersistentStorageAccessor(persistentStorageManager);
        }

        public IReadOnlyList<string> Members => _handlers.Keys.ToList();

        public string Name => "CREDIT_TRANSFER";

        public event EventHandler<Dictionary<string, object>> MemberValueChanged;

        public object GetMemberValue(string member)
        {
            try
            {
                return _handlers.GetValueOrDefault(member).Getter?.Invoke();
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(GetMemberValue)} threw trying to get {member} - default value of 0 returned.", ex);
                return 0;
            }
        }

        public void SetMemberValue(string member, object value) => _handlers.GetValueOrDefault(member).Setter?.Invoke(value);

        /// <inheritdoc />
        public void Begin(IReadOnlyList<string> dataMemberNames) => _cachedMemberValue.Clear();

        public void Commit()
        {
            var pairs = _persistentStorageAccessor.GetAll().FirstOrDefault().Value;
            if (pairs != null
                && pairs.ContainsKeys("Source_1stbyte", "Source_2ndbyte", "Source_3rdbyte", "Current_JP_Number")
                && _cachedMemberValue.ContainsKeys("Source_1stbyte", "Source_2ndbyte", "Source_3rdbyte", "Current_JP_Number")
                && (byte)pairs["Source_1stbyte"] == (byte)_cachedMemberValue["Source_1stbyte"]
                && (byte)pairs["Source_2ndbyte"] == (byte)_cachedMemberValue["Source_2ndbyte"]
                && (byte)pairs["Source_3rdbyte"] == (byte)_cachedMemberValue["Source_3rdbyte"]
                && (ushort)pairs["Current_JP_Number"] == (ushort)_cachedMemberValue["Current_JP_Number"])
            {
                throw new Exception("Duplicated credit transfer request.");
            }

            var creditWin = _cachedMemberValue.ContainsKey(BonusCreditsWin) ? (uint)_cachedMemberValue[BonusCreditsWin] : throw new ArgumentNullException(BonusCreditsWin);
            var cashableAmount = ((long)creditWin).CentsToMillicents();

            var bonusId = $"{Name}_{Guid.NewGuid()}";
            _cachedMemberValue["BonusID"] = bonusId;

            if (!_cachedMemberValue.TryGetValue("Bonus_Credit_Type", out var bonusCreditType) || !((byte)bonusCreditType).IsDefined<BonusCreditType>(out var creditType))
            {
                throw new Exception($"Unsupported 'Bonus Credit Type' {bonusCreditType ?? "Empty"}");
            }

            if (!_cachedMemberValue.TryGetValue(BonusReason, out var bonusReasonByte)
                || !((byte)bonusReasonByte).IsDefined<BonusReason>(out var bonusReason)
                || bonusReason == DataSources.BonusReason.Unknown)
            {
                throw new Exception($"Unsupported Bonus Reason({bonusReasonByte ?? "Empty"})");
            }

            if (!_properties.GetValue(GamingConstants.IsGameRunning, false))
            {
                throw new Exception("Credit Transfer is rejected as no game is running");
            }

            var gameDiagnostics = ServiceManager.GetInstance().GetService<IGameDiagnostics>() ?? throw new Exception("GameDiagnostics service is not found.");
            if (gameDiagnostics.IsActive && gameDiagnostics.Context is ICombinationTestContext)
            {
                throw new Exception("Credit Transfer is rejected as in combination test mode");
            }

            var blockName = typeof(LogicSealDataSource).ToString();
            var storageAccessor = _persistentStorageManager.BlockExists(blockName) ? _persistentStorageManager.GetBlock(blockName) : default;
            var logicSealStatus = storageAccessor?["LogicSealStatusField"];
            if (logicSealStatus != null && (LogicSealDataSource.LogicSealStatusEnum)logicSealStatus == LogicSealDataSource.LogicSealStatusEnum.Broken)
            {
                throw new Exception("Credit Transfer is rejected as 'Logic Seal' is broken");
            }

            Task.Run(() =>
            {
                try
                {
                    Persist(_cachedMemberValue);
                    var jackpotNumber = (ushort)_cachedMemberValue["Current_JP_Number"];
                    var formatString = "00#";
                    var sourceID = string.Concat(((byte)_cachedMemberValue["Source_1stbyte"]).ToString(formatString), ((byte)_cachedMemberValue["Source_2ndbyte"]).ToString(formatString), ((byte)_cachedMemberValue["Source_3rdbyte"]).ToString(formatString));
                    HandleCreditTransfer(cashableAmount, bonusId, creditType, bonusReason.GetDescription(), jackpotNumber, sourceID);
                }
                catch (Exception ex)
                {
                    Log.Error($"{nameof(Commit)} threw trying to persist the request and create a bonus transaction.", ex);
                }
            });
        }

        public void RollBack() => _cachedMemberValue.Clear();

        private IBonusRequest GetBonusRequest(
            long cashableAmount,
            string bonusId,
            BonusCreditType bonusCreditType,
            string reason,
            ushort jackpotNumber,
            string sourceID)
        {
            IBonusRequest request = null;
            if (bonusCreditType.IsAnyOf(BonusCreditType.Deductible, BonusCreditType.WagerMatch))
            {
                request = GetStandardBonus(bonusId, cashableAmount, BonusMode.Standard, reason, jackpotNumber, sourceID);
            }
            else if (bonusCreditType == BonusCreditType.NonDeductible)
            {
                request = GetStandardBonus(bonusId, cashableAmount, BonusMode.NonDeductible, reason, jackpotNumber, sourceID);
            }

            return request;
        }

        private void HandleCreditTransfer(long cashableAmount, string bonusId, BonusCreditType bonusCreditType, string reason, ushort jackpotNumber, string sourceID)
        {
            var request = GetBonusRequest(cashableAmount, bonusId, bonusCreditType, reason, jackpotNumber, sourceID) ?? throw new Exception("Empty BonusRequest");
            var bonusTransaction = _bonusHandler.Award(request);
            if (bonusTransaction?.Exception != (int)BonusException.None)
            {
                throw new Exception($"Failed credit transfer, {bonusTransaction?.ExceptionInformation}({bonusTransaction?.Exception}).");
            }
        }

        private StandardBonus GetStandardBonus(string bonusId, long cashableAmount, BonusMode mode, string reason, ushort jackpotNumber, string sourceID)
        {
            var request = new StandardBonus(
                bonusId,
                cashableAmount,
                0,
                0,
                PayMethod.Any,
                true,
                mode)
            {
                JackpotNumber = jackpotNumber,
                SourceID = sourceID,
                Message = reason,
                AllowedWhileInAuditMode = false,
            };

            return request;
        }

        private void OnBonusAwarded(BonusAwardedEvent bonusAwardedEvent)
        {
            var transaction = bonusAwardedEvent.Transaction;
            if (transaction != null && transaction.BonusId.Equals(GetValue("BonusID", null)?.ToString(), StringComparison.CurrentCultureIgnoreCase))
            {
                var sourceBytes = GetBytesFromSourceID(transaction.SourceID);

                var memberSnapshot = new Dictionary<string, object>
                {
                    { BonusCreditsWin, transaction.CashableAmount },
                    { BonusReason, GetEnumFromDescription(transaction.Message) },
                    { "Source_1stbyte", sourceBytes[0] },
                    { "Source_2ndbyte", sourceBytes[1] },
                    { "Source_3rdbyte", sourceBytes[2] },
                    { "Current_JP_Number", transaction.JackpotNumber },
                    { "Bonus_Credit_Type", GetMemberValue("Bonus_Credit_Type") }
                };

                MemberValueChanged?.Invoke(this, memberSnapshot);
            }
        }

        private void InitializePersistentStorageAccessor(IPersistentStorageManager storageManager)
        {
            var storageName = GetType().ToString();
            var blockExists = storageManager.BlockExists(storageName);
            _persistentStorageAccessor = blockExists
                ? storageManager.GetBlock(storageName)
                : storageManager.CreateBlock(PersistenceLevel.Transient, storageName, 1);
        }

        private void Persist(IEnumerable<KeyValuePair<string, object>> nameValues)
        {
            using (var transaction = _persistentStorageAccessor.StartTransaction())
            {
                foreach (var pair in nameValues)
                {
                    transaction[pair.Key] = pair.Value;
                }

                transaction.Commit();
            }
        }

        private HandlerMap GetMembersMap() => new HandlerMap
        {
            { BonusCreditsWin, (() => GetValue(BonusCreditsWin,0), o => { _cachedMemberValue[BonusCreditsWin] = o; })},
            { BonusReason, (() => GetValue(BonusReason,0), o =>{ _cachedMemberValue[BonusReason] = o; })},
            { "Source_1stbyte", (() => GetValue("Source_1stbyte",0), o => { _cachedMemberValue["Source_1stbyte"] = o; })},
            { "Source_2ndbyte", (() => GetValue("Source_2ndbyte",0), o => { _cachedMemberValue["Source_2ndbyte"] = o; })},
            { "Source_3rdbyte", (() => GetValue("Source_3rdbyte",0), o => { _cachedMemberValue["Source_3rdbyte"] = o;})},
            { "Current_JP_Number", (() => GetValue("Current_JP_Number",0), o => { _cachedMemberValue["Current_JP_Number"] = o;})},
            { "Bonus_Credit_Type", (() => GetValue("Bonus_Credit_Type",0), o => { _cachedMemberValue["Bonus_Credit_Type"] = o;})},
        };

        private object GetValue(string fieldName, object defaultValue)
        {
            var pairs = _persistentStorageAccessor.GetAll().FirstOrDefault().Value;
            if (pairs != null && pairs.ContainsKey(fieldName))
            {
                return pairs[fieldName];
            }

            return defaultValue;
        }

        private List<int> GetBytesFromSourceID(string sourceID)
        {
            var byteFormatLength = 3;
            var numberOfBytes = 3;

            var bytes = new List<int>();
            for (int i = 0; i < numberOfBytes; i++)
            {
                bytes.Add(int.Parse(sourceID.Substring(i * byteFormatLength, byteFormatLength)));
            }

            return bytes;
        }

        private BonusReason GetEnumFromDescription (string description)
        {
            var field = typeof(BonusReason).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => ((BonusReason)f.GetRawConstantValue()).GetDescription() == description)
                .SingleOrDefault();

            return field == null ? DataSources.BonusReason.Unknown : (BonusReason)field.GetRawConstantValue();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);
            }

            _disposed = true;
        }
    }
}