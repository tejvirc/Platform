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
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Localization.Properties;

    /// <summary>
    ///     An <see cref="IPlayerBank" /> implementation.
    /// </summary>
    public class CoamPlayerBankDecorator : PlayerBankBaseDecorator
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
        public CoamPlayerBankDecorator(
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
            ) : base(bank, transactionCoordinator, transferOut, persistentStorage, meters, players, bus, properties, history, cashOutAmountCalculator, systemDisableManager)
        {
            _bus = bus;
            _cashOutAmountCalculator = cashOutAmountCalculator;
            _systemDisableManager = systemDisableManager;

            _handCountPayoutLimit = properties.GetValue<long>(AccountingConstants.HandCountPayoutLimit, 0);
        }

        private async Task CheckLargePayoutAsync(long amount)
        {
            Logger.Debug($"Check Payout Limit: {amount}");
            if (amount > _handCountPayoutLimit)
            {
                var keyOff = Initiate();
                await keyOff.Task;

                _systemDisableManager.Enable(ApplicationConstants.LargePayoutDisableKey);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            base.Dispose(disposing);

            if (disposing)
            {
                _bus.UnsubscribeAll(this);
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
                () => Localizer.For(CultureFor.Player).GetString(ResourceKeys.LargePayoutReached),
                true,
                () => Localizer.For(CultureFor.Player).GetString(ResourceKeys.LargePayoutReachedHelpMessage));

            return keyOff;
        }

        /// <inheritdoc />
        public override bool CashOut()
        {
            var amountCashable = _cashOutAmountCalculator.GetCashableAmount(PlayerBank.Balance);
            if (amountCashable > 0)
            {
                CheckLargePayoutAsync(amountCashable).Wait();

                _bus.Publish(new CashOutStartedEvent(false, true));

                var success = PlayerBank.CashOut(amountCashable);

                if (!success)
                {
                    _bus.Publish(new CashOutAbortedEvent());
                }

                return success;
            }

            return true;
        }
    }
}