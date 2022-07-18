namespace Aristocrat.Monaco.Sas.Eft
{
    using System;
    using System.Reflection;
    using Application.Contracts.Extensions;
    using Hardware.Contracts.Persistence;
    using Aristocrat.Monaco.Accounting.Contracts.Handpay;
    using Aristocrat.Sas.Client;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Sas.Client.Eft;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Provides logic to handle player initiated cashout on EGM.
    /// </summary>
    public class PlayerInitiatedCashoutProvider : IPlayerInitiatedCashoutProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string BlockFieldName = "PlayerLastCashoutAmount";
        private static readonly string BlockName = typeof(PlayerInitiatedCashoutProvider).ToString();

        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly IEventBus _eventBus;
        private readonly IPersistentStorageAccessor _persistentStorageAccessor;
        private readonly object _lock = new object();

        /// <summary>
        ///     Constructor to initiate the instance
        /// </summary>
        /// <param name="storage">the storage instance</param>
        /// <param name="eventBus">the event bus instance</param>
        /// <param name="exceptionHandler">SAS Exception Handler instance</param>        
        public PlayerInitiatedCashoutProvider(
            IPersistentStorageManager storage,
            IEventBus eventBus,
            ISasExceptionHandler exceptionHandler)
        {
            _persistentStorage = storage ?? throw new ArgumentNullException(nameof(storage));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));

            if (!_persistentStorage.BlockExists(BlockName)) //create the block storage
            {
                _persistentStorageAccessor = _persistentStorage.CreateBlock(PersistenceLevel.Static, BlockName, 1);
                Logger.Info($"Created storage block ({BlockName}) complete.");
            }
            else
            {
                _persistentStorageAccessor = _persistentStorage.GetBlock(BlockName);
            }
            SubscribeEvents();
        }

        /// <summary>
        ///     Subscribes to events to clear/save cashout amount and report exception 26
        /// </summary>
        private void SubscribeEvents()
        {
            _eventBus.Subscribe<VoucherIssuedEvent>(this, (theEvent) => UpdateCashoutAmount(theEvent.Transaction.TransactionAmount.MillicentsToCents()));
            _eventBus.Subscribe<HandpayCompletedEvent>(this, (theEvent) => UpdateCashoutAmount(theEvent.Transaction.TransactionAmount.MillicentsToCents()));
            _eventBus.Subscribe<TransferOutStartedEvent>(this, (_) => UpdateCashoutAmount(0));
            _eventBus.Subscribe<TransferOutCompletedEvent>(this, (_) => _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.PlayerInitiatedCashout)));
        }

        /// get the last cashout amount
        public ulong GetCashoutAmount()
        { 
            return (ulong)_persistentStorageAccessor[BlockFieldName];
        }

        /// clear the last cashout amount
        public void ClearCashoutAmount()
        {
            UpdateCashoutAmount(0);
        }

        private void UpdateCashoutAmount(long theAmount)
        {
            lock (_lock)
            {
                using (var transaction = _persistentStorageAccessor.StartTransaction())
                {
                    transaction[BlockFieldName] = (ulong)theAmount;
                    transaction.Commit();
                }
            }
        }
    }
}