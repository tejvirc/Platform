namespace Aristocrat.Monaco.Gaming.Class3
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Aristocrat.Monaco.Accounting.Contracts.HandCount;
    using Aristocrat.Monaco.Accounting.Contracts.TransferOut;
    using Aristocrat.Monaco.Kernel;
    using Contracts;
    using log4net;

    /// <summary>
    /// COAM-specific Decorator based on the interface <see cref="IPlayerBank" />
    /// </summary>
    public class PlayerBankCOAMDecorator : IPlayerBank, IService
    {
        /// <summary>Create a logger for use in this class.</summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPlayerBank _decorated;
        private readonly IHandCountService _handCountService;
        private readonly ICashOutAmountCalculator _cashOutAmountCalculator;
        public PlayerBankCOAMDecorator(IPlayerBank decorated, IHandCountService handCountService, ICashOutAmountCalculator cashOutAmountCalculator)
        {
            _decorated = decorated;
            _handCountService = handCountService;
            _cashOutAmountCalculator = cashOutAmountCalculator;
        }
        public long Balance => _decorated.Balance;

        public long Credits => _decorated.Credits;

        public Guid TransactionId => _decorated.TransactionId;

        public string Name => (_decorated as IService)?.Name;

        public ICollection<Type> ServiceTypes => (_decorated as IService)?.ServiceTypes;

        public void AddWin(long amount) => _decorated.AddWin(amount);

        public bool CashOut()
        {
            // Check if hand count calculations are active, and if so, fetch the calculator.
            if (_handCountService.HandCountServiceEnabled && _cashOutAmountCalculator != null)
            {
                var amountCashable = _cashOutAmountCalculator.GetCashableAmount(_decorated.Balance);

                if (amountCashable > 0)
                {
                    var res = _decorated.CashOut(amountCashable);

                    if (!res)
                    {
                        Logger.Error($"Player bank cashout ({amountCashable}) failed");
                    }

                    return res;
                }
                else
                {
                    // Do something to indicate we couldn't cash out anything?
                }
            }

            return _decorated.CashOut();
        }

        public bool CashOut(bool forcedCashout) => _decorated.CashOut(forcedCashout);

        public bool CashOut(long amount, bool forcedCashout = false) => _decorated.CashOut(amount, forcedCashout);

        public bool CashOut(Guid traceId, bool forcedCashout, long associatedTransaction) => _decorated.CashOut(traceId, forcedCashout, associatedTransaction);

        public bool CashOut(Guid traceId, long amount, TransferOutReason reason, bool forcedCashout, long associatedTransaction) => _decorated.CashOut(traceId, amount, reason, forcedCashout, associatedTransaction);

        public bool ForceHandpay(Guid traceId, long amount, TransferOutReason reason, long associatedTransaction) => _decorated.ForceHandpay(traceId, amount, reason, associatedTransaction);

        public bool ForceVoucherOut(Guid traceId, long amount, TransferOutReason reason, long associatedTransaction) => _decorated.ForceVoucherOut(traceId, amount, reason, associatedTransaction);

        public void Initialize() => (_decorated as IService)?.Initialize();

        public bool Lock() => _decorated.Lock();

        public bool Lock(TimeSpan timeout) => _decorated.Lock(timeout);

        public void Unlock() => _decorated.Unlock();

        public void Wager(long amount) => _decorated.Wager(amount);

        public void WaitForLock() => _decorated.WaitForLock();
    }
}
