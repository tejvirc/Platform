namespace Aristocrat.Monaco.Gaming
{
    using Accounting.Contracts;
    using Application.Contracts;
    using Contracts;
    using Contracts.Session;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Accounting.Contracts.TransferOut;
    using Aristocrat.Monaco.Accounting.Contracts.HandCount;
    using Aristocrat.Monaco.Hardware.Contracts.Button;
    using System.Threading.Tasks;

    /// <summary>
    ///     An <see cref="IPlayerBank" /> implementation.
    /// </summary>
    public class CoamPlayerBank : IPlayerBank, ITransactionRequestor, IService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly PlayerBank _playerBank;
        private readonly IEventBus _bus;
        private readonly ICashOutAmountCalculator _cashOutAmountCalculator;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly long _handCountPayoutLimit;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerBank" /> class.
        /// </summary>
        /// <param name="bank">An <see cref="IBank" /> implementation.</param>
        /// <param name="transactionCoordinator">An <see cref="ITransactionCoordinator" /> implementation.</param>
        /// <param name="transferOut">An <see cref="ITransferOutHandler" /> instance.</param>
        /// <param name="persistentStorage">An <see cref="IPersistentStorageManager" /> instance.</param>
        /// <param name="meters">An <see cref="IMeterManager" /> instance</param>
        /// <param name="players">An <see cref="IPlayerService" /> instance</param>
        /// <param name="bus">An <see cref="IEventBus" /> instance.</param>
        /// <param name="properties"></param>
        /// <param name="history">An <see cref="IGameHistory"/> instance.</param>
        /// <param name="cashOutAmountCalculator"></param>
        /// <param name="systemDisableManager"></param>
        public CoamPlayerBank(
            IBank bank,
            ITransactionCoordinator transactionCoordinator,
            ITransferOutHandler transferOut,
            IPersistentStorageManager persistentStorage,
            IMeterManager meters,
            IPlayerService players,
            IEventBus bus,
            IPropertiesManager properties,
            IGameHistory history,
            ICashOutAmountCalculator cashOutAmountCalculator,
            ISystemDisableManager systemDisableManager
            )
        {
            _playerBank = new PlayerBank(bank, transactionCoordinator, transferOut, persistentStorage, meters, players, bus, history);
            _bus = bus;
            _cashOutAmountCalculator = cashOutAmountCalculator;
            _systemDisableManager = systemDisableManager;

            _handCountPayoutLimit = properties.GetValue<long>(AccountingConstants.HandCountPayoutLimit, 0);
        }

        private async Task CheckLargePayoutAsync(long amount)
        {
            if (amount > _handCountPayoutLimit)
            {
                var keyOff = Initiate();
                await keyOff.Task;

                _systemDisableManager.Enable(ApplicationConstants.LargePayoutDisableKey);
            }
        }

        private TaskCompletionSource<object> Initiate()
        {
            var keyOff = new TaskCompletionSource<object>();

            _bus.Subscribe<DownEvent>(
                this,
                _ =>
                {
                    keyOff.TrySetResult(null);
                },
                evt => evt.LogicalId == (int)ButtonLogicalId.Button30);

            _systemDisableManager.Disable(
                ApplicationConstants.LargePayoutDisableKey,
                SystemDisablePriority.Immediate,
                () => "COLLECT LIMIT REACHED. SEE ATTENDANT.",
                true,
                () => "COLLECT LIMIT REACHED. SEE ATTENDANT.");

            return keyOff;
        }

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _playerBank.Dispose();
            }

            _disposed = true;
        }

        public void NotifyTransactionReady(Guid requestId)
        {
            _playerBank.NotifyTransactionReady(requestId);
        }

        /// <inheritdoc />
        public long Balance => _playerBank.Balance;

        /// <inheritdoc />
        public long Credits => _playerBank.Credits;

        /// <inheritdoc />
        public Guid TransactionId => _playerBank.TransactionId;

        /// <inheritdoc />
        public Guid RequestorGuid => _playerBank.RequestorGuid;

        /// <inheritdoc />
        public bool CashOut()
        {
            var amountCashable = _cashOutAmountCalculator.GetCashableAmount(_playerBank.Balance);
            if (amountCashable > 0)
            {
                CheckLargePayoutAsync(amountCashable).Wait();

                _bus.Publish(new CashOutStartedEvent(false, true));

                var success = _playerBank.CashOut(amountCashable);

                if (!success)
                {
                    _bus.Publish(new CashOutAbortedEvent());
                }

                return success;
            }

            return true;
        }

        /// <inheritdoc />
        public bool CashOut(bool forcedCashout)
        {
            return _playerBank.CashOut(forcedCashout);
        }

        /// <inheritdoc />
        public bool CashOut(long amount, bool forcedCashout = false)
        {
            return _playerBank.CashOut(amount, forcedCashout);
        }

        /// <inheritdoc />
        public bool CashOut(Guid traceId, bool forcedCashout, long associatedTransaction)
        {
            return _playerBank.CashOut(traceId, forcedCashout, associatedTransaction);
        }

        /// <inheritdoc />
        public bool CashOut(
            Guid traceId,
            long amount,
            TransferOutReason reason,
            bool forcedCashout,
            long associatedTransaction)
        {
            return _playerBank.CashOut(traceId, amount, reason, forcedCashout, associatedTransaction);
        }

        /// <inheritdoc />
        public bool ForceHandpay(Guid traceId, long amount, TransferOutReason reason, long associatedTransaction)
        {
            return _playerBank.ForceHandpay(traceId, amount, reason, associatedTransaction);
        }

        /// <inheritdoc />
        public bool ForceVoucherOut(Guid traceId, long amount, TransferOutReason reason, long associatedTransaction)
        {
            return _playerBank.ForceVoucherOut(traceId, amount, reason, associatedTransaction);
        }

        /// <inheritdoc />
        public void WaitForLock()
        {
            _playerBank.WaitForLock();
        }

        /// <inheritdoc />
        public bool Lock()
        {
            return _playerBank.Lock();
        }

        /// <inheritdoc />
        public bool Lock(TimeSpan timeout)
        {
            return _playerBank.Lock(timeout);
        }

        /// <inheritdoc />
        public void Unlock()
        {
            _playerBank.Unlock();
        }

        /// <inheritdoc />
        public void Wager(long amount)
        {
            _playerBank.Wager(amount);
        }

        /// <inheritdoc />
        public void AddWin(long amount)
        {
            _playerBank.AddWin(amount);
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IPlayerBank) };

        /// <inheritdoc />
        public void Initialize()
        {
        }
    }
}