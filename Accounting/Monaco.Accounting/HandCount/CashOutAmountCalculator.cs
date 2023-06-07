namespace Aristocrat.Monaco.Accounting.HandCount
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Contracts;
    using Contracts.HandCount;
    using Hardware.Contracts.Button;
    using Kernel;
    using Localization.Properties;
    using log4net;

    /// <summary>
    ///     An <see cref="ICashOutAmountCalculator" /> that restricts the amount that can be cashed out based
    ///     on the amount of hand count that the player has.
    /// </summary>
    public class CashOutAmountCalculator : ICashOutAmountCalculator, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IHandCountService _handCountService;
        private readonly ISystemDisableManager _disableManager;
        private readonly IEventBus _eventBus;
        private readonly long _cashOutAmountPerHand;
        private readonly IPropertiesManager _properties;

        private bool _disposed;

        /// <summary>
        /// The cashout is issued because locked up
        /// </summary>
        private bool _lockupCashout = false;

        /// <summary>
        ///     Constructs the calculator by retrieving all necessary services from the service manager. This
        ///     constructor is necessary because this is a service in the accounting layer where DI is not used.
        /// </summary>
        public CashOutAmountCalculator()
            : this(
                ServiceManager.GetInstance().GetService<IHandCountService>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        /// <summary>
        ///     Constructs the provider taking all required services as parameters. For unit testing.
        /// </summary>
        public CashOutAmountCalculator(
            IHandCountService handCountService,
            IPropertiesManager properties,
            ISystemDisableManager disableManager,
            IEventBus eventBus)
        {
            _handCountService = handCountService
                ?? throw new ArgumentNullException(nameof(handCountService));
            _disableManager = disableManager
                ?? throw new ArgumentNullException(nameof(disableManager));
            _eventBus = eventBus
                ?? throw new ArgumentNullException(nameof(eventBus));
            _cashOutAmountPerHand = properties.GetValue(AccountingConstants.CashoutAmountPerHandCount, 0L);
            _properties = properties
                 ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <inheritdoc />
        public string Name => typeof(CashOutAmountCalculator).FullName;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(ICashOutAmountCalculator) };

        /// <inheritdoc />
        public void Initialize()
        {
            // Nothing to do for this service.
        }

        /// <inheritdoc />
        public long GetCashableAmount(long amount)
        {
            if (_disableManager.IsDisabled)
            {
                _lockupCashout = true;
                return ((long)amount.MillicentsToDollars()).DollarsToMillicents();
            }

            var amountCashable = Math.Min(amount, _handCountService.HandCount * _cashOutAmountPerHand);

            amountCashable = ((long)amountCashable.MillicentsToDollars()).DollarsToMillicents();

            CheckLargePayoutAsync(amountCashable).Wait();
            return amountCashable;
        }

        /// <inheritdoc />
        public void PostProcessTransaction(long amount)
        {
            var handCountUsed = 0;
            if (_lockupCashout)
            {
                _lockupCashout = false;
                handCountUsed = _handCountService.HandCount;
            }
            else
            {
                handCountUsed = (int)Math.Ceiling(amount / (decimal)_cashOutAmountPerHand);
            }

            _handCountService.DecreaseHandCount(handCountUsed);
        }

        /// <inheritdoc />
        ~CashOutAmountCalculator() => Dispose(disposing: false);

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private async Task CheckLargePayoutAsync(long amount)
        {
            Logger.Debug($"Check Payout Limit: {amount}");
            _eventBus.Publish(new PayoutAmountUpdatedEvent(amount));
            var handCountPayoutLimit = _properties.GetValue<long>(AccountingConstants.HandCountPayoutLimit, 0);
            if (amount > handCountPayoutLimit)
            {
                var keyOff = Initiate();
                await keyOff.Task;

                _disableManager.Enable(ApplicationConstants.LargePayoutDisableKey);
            }
        }

        private TaskCompletionSource<object> Initiate()
        {
            var keyOff = new TaskCompletionSource<object>();

            _eventBus.Subscribe<DownEvent>(
                this,
                _ =>
                {
                    keyOff.TrySetResult(null);
                },
                evt => evt.LogicalId == (int)ButtonLogicalId.Button30);

            _disableManager.Disable(
                ApplicationConstants.LargePayoutDisableKey,
                SystemDisablePriority.Immediate,
                () => Localizer.For(CultureFor.Player).GetString(ResourceKeys.LargePayoutReached),
                true,
                () => Localizer.For(CultureFor.Player).GetString(ResourceKeys.LargePayoutReachedHelpMessage));

            return keyOff;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _eventBus.UnsubscribeAll(this);
                }

                _disposed = true;
            }
        }
    }
}