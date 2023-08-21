namespace Aristocrat.Monaco.Sas.AftTransferProvider
{
    using Accounting.Contracts;
    using Application.Contracts;
    using Kernel;
    using log4net;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts.Localization;
    using Gaming.Contracts;
    using Localization.Properties;

    /// <summary>
    /// Definition of the HardCashOutLock class.
    /// This class is used to get a transaction from the coordinator and on a separate thread
    /// lock up and wait for a key-off. It can be set from the provider if an aft is coming through to
    /// unlock but not cash out.
    /// </summary>
    public sealed class HardCashOutLock : IHardCashOutLock, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private static readonly Guid RequestorId = new("{DEAE9B2D-C61D-471b-B63E-26EC52C9C7A5}");

        // what disables that shouldn't stop a host cash out key off
        private static readonly Guid[] ExcludeDisables =
        {
            ApplicationConstants.HostCashOutFailedDisableKey,
            ApplicationConstants.LiveAuthenticationDisableKey,
            GamingConstants.ReelsTiltedGuid,
            GamingConstants.ReelsNeedHomingGuid
        };

        private Guid _transactionId;
        private readonly object _lock = new();

        /// <summary>Indicates if the operator has keyed off the lockup.</summary>
        private bool _keyedOff;

        /// <summary>Indicates if the auto reset event was set by the aft off provider.</summary>
        private bool _aftIncoming;

        /// <summary>The auto reset event to wait for a key off.</summary>
        private readonly AutoResetEvent _autoResetEvent = new(false);

        private bool _disposed;
        private readonly ITransactionCoordinator _transactionCoordinator;
        private readonly IPropertiesManager _propertiesManager;
        private readonly ITransferOutHandler _transferOutHandler;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly IEventBus _eventBus;
        private readonly IBank _bank;

        /// <summary>
        ///     Initializes a new instance of the HardCashOutLock class.
        /// </summary>
        /// <param name="transactionCoordinator">The transaction coordinator</param>
        /// <param name="propertiesManager">The properties manager</param>
        /// <param name="transferOutHandler">The transfer out handler</param>
        /// <param name="systemDisableManager">The system disable manager</param>
        /// <param name="eventBus">The event bus</param>
        /// <param name="bank">The bank</param>
        public HardCashOutLock(
            ITransactionCoordinator transactionCoordinator,
            IPropertiesManager propertiesManager,
            ITransferOutHandler transferOutHandler,
            ISystemDisableManager systemDisableManager,
            IEventBus eventBus,
            IBank bank)
        {
            _transactionCoordinator = transactionCoordinator ?? throw new ArgumentNullException(nameof(transactionCoordinator));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _transferOutHandler = transferOutHandler ?? throw new ArgumentNullException(nameof(transferOutHandler));
            _systemDisableManager = systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
        }

        /// <inheritdoc />
        public bool Locked { get; set; }

        /// <inheritdoc />
        public bool WaitingForKeyOff => !_keyedOff && Locked;

        /// <inheritdoc />
        public bool CanCashOut => !DisablesPresentThatStopCashout();

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Dispose(true);
        }

        /// <inheritdoc />
        public bool Recover()
        {
            return false;
        }

        /// <inheritdoc />
        public bool LockupAndCashOut()
        {
            Logger.Debug("Start of LockupAndCashOut().");
            if (_transactionId == Guid.Empty)
            {
                _transactionId = _transactionCoordinator.RequestTransaction(RequestorId, 1000, TransactionType.Write);
                if (_transactionId == Guid.Empty)
                {
                    Logger.Debug("Unable to get transaction guid. Leaving.");
                    return false;
                }
            }

            // make sure we can't get into a permanently locked state.
            _keyedOff = false;
            _aftIncoming = false;

            Task.Run(
                () =>
                {
                    Logger.Debug("Start of hard cash-out thread.");
                    PresentLockup();

                    _autoResetEvent.WaitOne();
                    RemoveLockupPresentation();

                    Logger.Debug($"End of hard cash-out thread.  m_keyedOff={_keyedOff}, m_aftIncoming={_aftIncoming}");

                    // if this is false, then the system is shutting down and
                    // needs to come back into this state on boot up.
                    if (_keyedOff && !_aftIncoming)
                    {
                        _transactionCoordinator.ReleaseTransaction(_transactionId);
                        _transactionId = Guid.Empty;
                        _keyedOff = false;

                        Logger.Debug("Starting transfer out.");
                        _transferOutHandler.TransferOut();
                    }
                });

            Logger.Debug("End of LockupAndCashOut().");
            return true;
        }

        /// <inheritdoc />
        public void PresentLockup()
        {
            Locked = true;
            var cashAmountToKeyOff = _bank.QueryBalance(AccountType.Cashable);
            var nonCashAmountToKeyOff = _bank.QueryBalance(AccountType.NonCash);
            var promotionalAmountToKeyOff = _bank.QueryBalance(AccountType.Promo);

            var maximumVoucherLimit = (long)_propertiesManager.GetProperty(AftConstants.MaximumVoucherLimitPropertyName, AftConstants.DefaultVoucherLimit);
            if ((cashAmountToKeyOff + promotionalAmountToKeyOff >= maximumVoucherLimit) &&
                nonCashAmountToKeyOff > 0 &&
                nonCashAmountToKeyOff < maximumVoucherLimit)
            {
                // If combined cash-able and promotional are greater than the hand-pay limit and the
                // restricted are below the limit then first the playable ticket will process followed
                // by a canceled credits on the cash-able so only show the restricted first.
                cashAmountToKeyOff = 0;
                promotionalAmountToKeyOff = 0;
            }
            else if (((cashAmountToKeyOff + promotionalAmountToKeyOff) >= maximumVoucherLimit) ||
                (nonCashAmountToKeyOff >= maximumVoucherLimit))
            {
                // If the Combined Cash-able and Promotional are greater than the HandPay Limit
                // a HandPay will be required for the Combined Cash-able and Promotional and the
                // Restricted Credits will remain.
                nonCashAmountToKeyOff = 0;
            }

            // Post the HardCashLockOut Event to the Platform Event Bus
            _eventBus.Publish(new HardCashLockoutEvent(cashAmountToKeyOff, nonCashAmountToKeyOff, promotionalAmountToKeyOff));

            _systemDisableManager.Disable(
                ApplicationConstants.HostCashOutFailedDisableKey,
                SystemDisablePriority.Immediate,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HardCashLockMessage));
        }

        /// <inheritdoc />
        public void RemoveLockupPresentation()
        {
            Locked = false;
            _systemDisableManager.Enable(ApplicationConstants.HostCashOutFailedDisableKey);
        }

        /// <inheritdoc />
        public void Set()
        {
            lock (_lock)
            {
                if (!WaitingForKeyOff)
                {
                    return;
                }

                Logger.Debug("Set was called, releasing transaction and not starting a transfer.");
                _aftIncoming = true;
                _keyedOff = false;
                _transactionCoordinator.ReleaseTransaction(_transactionId);
                _transactionId = Guid.Empty;
                _autoResetEvent.Set();
            }
        }

        /// <inheritdoc />
        public void OnKeyedOff()
        {
            lock (_lock)
            {
                _eventBus.Publish(new HardCashKeyOffEvent());
                _keyedOff = true;
                _autoResetEvent.Set();
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _autoResetEvent.Set();
                _autoResetEvent.Close();
            }
        }

        private bool DisablesPresentThatStopCashout()
        {
            return _systemDisableManager.CurrentDisableKeys.Except(ExcludeDisables).Any();
        }
    }
}