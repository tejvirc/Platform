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
    public class PlayerBankBaseDecorator : IPlayerBank, ITransactionRequestor, IService, IDisposable
    {
        protected readonly PlayerBank PlayerBank;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Gaming.PlayerBank" /> class.
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
        public PlayerBankBaseDecorator(
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
            PlayerBank = new PlayerBank(bank, transactionCoordinator, transferOut, persistentStorage, meters, players, bus, history);
        }

        public void NotifyTransactionReady(Guid requestId)
        {
            PlayerBank.NotifyTransactionReady(requestId);
        }

        /// <inheritdoc />
        public long Balance => PlayerBank.Balance;

        /// <inheritdoc />
        public long Credits => PlayerBank.Credits;

        /// <inheritdoc />
        public Guid TransactionId => PlayerBank.TransactionId;

        /// <inheritdoc />
        public Guid RequestorGuid => PlayerBank.RequestorGuid;

        /// <inheritdoc />
        public virtual bool CashOut()
        {
            return PlayerBank.CashOut();
        }

        /// <inheritdoc />
        public bool CashOut(bool forcedCashout)
        {
            return PlayerBank.CashOut(forcedCashout);
        }

        /// <inheritdoc />
        public virtual bool CashOut(long amount, bool forcedCashout = false)
        {
            return PlayerBank.CashOut(amount, forcedCashout);
        }

        /// <inheritdoc />
        public bool CashOut(Guid traceId, bool forcedCashout, long associatedTransaction)
        {
            return PlayerBank.CashOut(traceId, forcedCashout, associatedTransaction);
        }

        /// <inheritdoc />
        public bool CashOut(
            Guid traceId,
            long amount,
            TransferOutReason reason,
            bool forcedCashout,
            long associatedTransaction)
        {
            return PlayerBank.CashOut(traceId, amount, reason, forcedCashout, associatedTransaction);
        }

        /// <inheritdoc />
        public bool ForceHandpay(Guid traceId, long amount, TransferOutReason reason, long associatedTransaction)
        {
            return PlayerBank.ForceHandpay(traceId, amount, reason, associatedTransaction);
        }

        /// <inheritdoc />
        public bool ForceVoucherOut(Guid traceId, long amount, TransferOutReason reason, long associatedTransaction)
        {
            return PlayerBank.ForceVoucherOut(traceId, amount, reason, associatedTransaction);
        }

        /// <inheritdoc />
        public void WaitForLock()
        {
            PlayerBank.WaitForLock();
        }

        /// <inheritdoc />
        public bool Lock()
        {
            return PlayerBank.Lock();
        }

        /// <inheritdoc />
        public bool Lock(TimeSpan timeout)
        {
            return PlayerBank.Lock(timeout);
        }

        /// <inheritdoc />
        public void Unlock()
        {
            PlayerBank.Unlock();
        }

        /// <inheritdoc />
        public void Wager(long amount)
        {
            PlayerBank.Wager(amount);
        }

        /// <inheritdoc />
        public void AddWin(long amount)
        {
            PlayerBank.AddWin(amount);
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IPlayerBank) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <summary>
        ///     Gets or sets a value indicating whether
        ///     this object has been disposed or not
        /// </summary>
        protected bool Disposed { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                Disposed = true;

                if (disposing)
                {
                    PlayerBank?.Dispose();
                }
            }
        }
    }
}