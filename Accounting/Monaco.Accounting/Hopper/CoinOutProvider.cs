namespace Aristocrat.Monaco.Accounting.Hopper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Hopper;
    using Contracts.Transactions;
    using Contracts.TransferOut;
    using Hardware.Contracts;
    using Hardware.Contracts.Hopper;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using HardwareFaultEvent = Hardware.Contracts.Hopper.HardwareFaultEvent;

    /// <summary>
    ///     An <see cref="ITransferOutProvider" /> for coins
    /// </summary>
    [CLSCompliant(false)]
    public class CoinOutProvider : ICoinOutProvider, IDisposable
    {

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Guid RequestorId = new Guid("{DBC83E60-7914-475D-9A79-9473A173BF49}");
        private const int DeviceId = 1;

        private readonly IBank _bank;
        private readonly IEventBus _bus;
        private readonly IIdProvider _idProvider;
        private readonly IMeterManager _meters;
        private readonly IPropertiesManager _properties;
        private readonly IPersistentStorageManager _storage;
        private readonly ITransactionHistory _transactions;
        private readonly IHopper _hopperService;
        private readonly ManualResetEvent _transferOutEvent = new ManualResetEvent(false);
        private const int _transferOutEventTimeout = 100; //milli seconds
        private readonly IMessageDisplay _messageDisplay;
        private long _tokenValue;
        private Guid _transactionGuid = Guid.Empty;
        private bool _disposed;

        public CoinOutProvider()
            : this(
                ServiceManager.GetInstance().GetService<IBank>(),
                ServiceManager.GetInstance().GetService<ITransactionHistory>(),
                ServiceManager.GetInstance().GetService<IMeterManager>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IIdProvider>(),
                ServiceManager.GetInstance().TryGetService<IHopper>(),
                ServiceManager.GetInstance().GetService<IMessageDisplay>())
        {
        }

        public CoinOutProvider(
            IBank bank,
            ITransactionHistory transactions,
            IMeterManager meters,
            IPersistentStorageManager storage,
            IEventBus bus,
            IPropertiesManager properties,
            IIdProvider idProvider,
            IHopper hopperService,
            IMessageDisplay messageDisplay)
        {
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
            _messageDisplay = messageDisplay ?? throw new ArgumentNullException(nameof(messageDisplay));
            _hopperService = hopperService;
        }

        public string Name => typeof(CoinOutProvider).ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(ICoinOutProvider) };

        public bool Active { get; private set; }

        public void Initialize()
        {
            _bus.Subscribe<HardwareFaultEvent>(this, Handle);
            _bus.Subscribe<CoinOutEvent>(this, Handle);
            _bus.Subscribe<SystemDisabledEvent>(this, Handle);

            _tokenValue = _properties.GetValue(HardwareConstants.CoinValue, AccountingConstants.DefaultTokenValue);
        }

        /// <summary>
        ///     Transfer an amount of credits from a specific account in the bank out of the system.
        /// </summary>
        /// <param name="transactionId">The transaction Id for the transfer.</param>
        /// <param name="cashableAmount">The cashable amount requiring a handpay</param>
        /// <param name="promoAmount">The promotional amount requiring a handpay</param>
        /// <param name="nonCashAmount">The non-cashable amount requiring a handpay</param>
        /// <param name="associatedTransactions">An optional list of associated transactions</param>
        /// <param name="reason">The reason for transfer out.</param>
        /// <param name="traceId">A reference Id that should be associated with the cash out</param>
        /// <param name="cancellationToken">A cancellation token used to end the transfer</param>
        /// <returns>a <see cref="TransferResult"/></returns>
        public async Task<TransferResult> Transfer(
            Guid transactionId,
            long cashableAmount,
            long promoAmount,
            long nonCashAmount,
            IReadOnlyCollection<long> associatedTransactions,
            TransferOutReason reason,
            Guid traceId,
            CancellationToken cancellationToken)
        {
            _transferOutEvent.Reset();
            _messageDisplay.RemoveMessage(RequestorId);
            if (!_properties.GetValue(ApplicationConstants.HopperEnabled, false))
            {
                Logger.Info($"Coin out from hopper not supported - {transactionId}");
                return TransferResult.Failed;
            }

            if (!CheckHopperPayOutLimit(cashableAmount))
            {
                Logger.Info($"Credit is beyond hopper limit - {transactionId}");
                return TransferResult.Failed;
            }


            if (cancellationToken.IsCancellationRequested)
            {
                return TransferResult.Failed;
            }

            try
            {
                Active = true;
                _transactionGuid = transactionId;
                var transaction = new CoinOutTransaction(
                    DeviceId,
                    DateTime.UtcNow,
                    cashableAmount,
                    reason)
                {
                    BankTransactionId = transactionId,
                    AuthorizedCashableAmount = GetHopperPayOut(cashableAmount),
                    AssociatedTransactions = associatedTransactions,
                    TraceId = traceId
                };

                using (var scope = _storage.ScopedTransaction())
                {
                    if (transactionId == Guid.Empty)
                    {
                        Logger.Error("Coin out Event : Failed to acquire a transaction.");
                        return TransferResult.Failed;
                    }

                    // Unique log sequence number assigned by the EGM; a series that strictly increases by 1 (one) starting at 1 (one).
                    transaction.LogSequence = _idProvider.GetNextLogSequence<CoinOutTransaction>();
                    _transactions.AddTransaction(transaction);

                    scope.Complete();
                }

                await Transfer(transaction.AuthorizedCashableAmount);
                transaction = _transactions.RecallTransactions<CoinOutTransaction>().FirstOrDefault(t => t.BankTransactionId == _transactionGuid);
                _bus.Publish(new HopperPayOutCompletedEvent(transaction.TransferredCashableAmount));
                return new TransferResult(transaction.TransferredCashableAmount, 0L, 0L, transaction.Exception);
            }
            finally
            {
                Active = false;
                _transactionGuid = Guid.Empty;
            }
        }

        public bool CanRecover(Guid transactionId) => false;

        public async Task<bool> Recover(IRecoveryTransaction transaction, CancellationToken cancellationToken)
        {
            // There is nothing to recover for coins
            return await Task.FromResult(false);
        }

        private async Task<bool> Transfer(long amount)
        {
            if(_hopperService == null)
            {
                Logger.Info($"Hopper is not connected");
                return false;
            }

            _hopperService.SetMaxCoinoutAllowed((int)amount / (int)_tokenValue);
            _hopperService.StartHopperMotor();
            var success = _transferOutEvent.WaitOne();
            return await Task.FromResult(success);
        }

        /// <summary>
        ///     Checking whether hopper successfully transferred the amount.
        ///     Checking whether Split is on and transferred amount is equal to threshold
        ///     Condition 2
        ///     Checking whether Split is off and remaining amount is less than token value
        ///     means hopper pay out is successful.
        /// </summary>
        /// <param name="transferredAmount"></param>
        /// <param name="remainingAmount"></param>
        /// <returns></returns>
        public bool CheckCoinOutException(long transferredAmount, long remainingAmount)
        {

            return !((_properties.GetValue(AccountingConstants.HopperTicketSplit, false) &&
                      transferredAmount == _properties.GetValue(
                          AccountingConstants.HopperTicketThreshold,
                          0L))
                     || (!_properties.GetValue(AccountingConstants.HopperTicketSplit, false)
                         && remainingAmount < _tokenValue));
        }

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
                _bus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        /// <summary>
        ///     Check if amount is not greater than voucher limit and
        ///     hopper collect limit must be greater than token value as this can be 0
        ///     Total amount also should be greater than token value.
        /// </summary>
        /// <param name="cashableAmount"></param>
        /// <returns></returns>
        private bool CheckHopperPayOutLimit(long cashableAmount)
        {
            if (cashableAmount > _properties.GetValue(AccountingConstants.VoucherOutLimit, 0L))
            {
                return false;
            }

            var maxCashoutCoinLimit = _properties.GetValue(AccountingConstants.HopperTicketSplit, false)
                ? _properties.GetValue(AccountingConstants.HopperTicketThreshold, 0L)
                : _properties.GetValue(AccountingConstants.HopperCollectLimit, 0L);


            return maxCashoutCoinLimit >= _tokenValue
                   && cashableAmount >= _tokenValue
                   && (_properties.GetValue(AccountingConstants.HopperTicketSplit, false) || maxCashoutCoinLimit >= cashableAmount);
        }


        /// <summary>
        ///     Evaluating Cashable leaving the residue.
        /// </summary>
        /// <param name="cashableAmount"></param>
        /// <returns></returns>
        private long GetHopperPayOut(long cashableAmount)
        {
            var maxCashoutCoinLimit = _properties.GetValue(AccountingConstants.HopperTicketSplit, false)
                ? _properties.GetValue(AccountingConstants.HopperTicketThreshold, 0L)
                : _properties.GetValue(AccountingConstants.HopperCollectLimit, 0L);

            cashableAmount = (cashableAmount / _tokenValue) * _tokenValue;
            maxCashoutCoinLimit = (maxCashoutCoinLimit / _tokenValue) * _tokenValue;

            return maxCashoutCoinLimit >= cashableAmount ? cashableAmount : maxCashoutCoinLimit;
        }



        private void Handle(HardwareFaultEvent evt)
        {
            if (evt.Fault == HopperFaultTypes.IllegalCoinOut)
            {
                HandleIllegalCoin();
            }
        }

        private void Handle(SystemDisabledEvent evt)
        {
            if (_transactionGuid.Equals(Guid.Empty))
            {
                return;
            }

            _hopperService.StopHopperMotor();
            var transaction = _transactions.RecallTransactions<CoinOutTransaction>()
                .FirstOrDefault(t => t.BankTransactionId == _transactionGuid);
            if (transaction is null)
            {
                return;
            }

            try
            {
                using (var scope = _storage.ScopedTransaction())
                {
                    transaction.Exception = true;
                    _transactions.UpdateTransaction(transaction);
                    scope.Complete();
                }

            }
            catch (Exception)
            {
                Logger.Error($"Hopper Coin Out Stopped : {evt.GetType()}");
            }
            finally
            {
                // In case of any lockup we are holding transfer for 100ms to avoid illegal coin out.
                Task.Delay(_transferOutEventTimeout).ContinueWith(_ => _transferOutEvent.Set());
            }
        }

        private void Handle(CoinOutEvent evt)
        {
            if (_transactionGuid.Equals(Guid.Empty))
            {
                return;
            }

            var transaction = _transactions.RecallTransactions<CoinOutTransaction>()
                .FirstOrDefault(t => t.BankTransactionId == _transactionGuid);
            if (transaction is null)
            {
                return;
            }

            void StopHopperMotor()
            {
                _transferOutEvent.Set();
                _hopperService.StopHopperMotor();
            }

            try
            {
                using (var scope = _storage.ScopedTransaction())
                {
                    if (transaction.TransferredCashableAmount < transaction.AuthorizedCashableAmount)
                    {
                        _bank.Withdraw(AccountType.Cashable, evt.Coin.Value, transaction.BankTransactionId);
                        transaction.TransferredCashableAmount += evt.Coin.Value;
                    }

                    _meters.GetMeter(AccountingMeters.TrueCoinOutCount).Increment(1);
                    _transactions.UpdateTransaction(transaction);
                    scope.Complete();
                }

                if (transaction.AuthorizedCashableAmount <= transaction.TransferredCashableAmount
                    || transaction.Exception)
                {
                    StopHopperMotor();
                }
                else
                {
                    DisplayMessage(transaction.TransferredCashableAmount);
                }
            }
            catch (BankException)
            {
                StopHopperMotor();
                // In case of bank exception, bank withdraw of coin didn't completed successfully so coin is illegal.
                _bus.Publish(new HardwareFaultEvent(HopperFaultTypes.IllegalCoinOut));
            }
            catch (Exception)
            {
                StopHopperMotor();
                Logger.Error("Failed to update transfer of coin out");
            }
        }

        private void HandleIllegalCoin()
        {
            if (_transactionGuid.Equals(Guid.Empty))
            {
                return;
            }

            _hopperService.StopHopperMotor();
            var transaction = _transactions.RecallTransactions<CoinOutTransaction>()
                .FirstOrDefault(t => t.BankTransactionId == _transactionGuid);
            if (transaction is null)
            {
                return;
            }

            try
            {
                using (var scope = _storage.ScopedTransaction())
                {
                    _meters.GetMeter(AccountingMeters.ExcessCoinOutCount).Increment(1);
                    transaction.Exception = true;
                    _transactions.UpdateTransaction(transaction);
                    scope.Complete();
                }
            }
            catch (Exception)
            {
                Logger.Error($"Hopper Coin Out Stopped due to Illegal Coin Out");
            }
            finally
            {
                _transferOutEvent.Set();
            }
        }

        private void DisplayMessage(long acceptedAmount)
        {
            var displayableMessage = new DisplayableMessage(
                //TBC () => Localizer.For(CultureFor.Player).FormatString(ResourceKeys.HopperPayOut) +
                () => "Hopper Pay Out {0}" +
                      " " + acceptedAmount.MillicentsToDollars().FormattedCurrencyString(),
                DisplayableMessageClassification.Informative,
                DisplayableMessagePriority.Immediate,
                typeof(CoinOutEvent), RequestorId);

            _messageDisplay.DisplayMessage(displayableMessage, _transferOutEventTimeout);
        }
    }
}
