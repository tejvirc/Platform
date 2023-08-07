namespace Aristocrat.Monaco.Hhr.UI.Consumers
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Gaming.Contracts;
    using Gaming.Contracts.Bonus;
    using Gaming.Contracts.Progressives;
    using Kernel;
    using log4net;
    using Services;
    using Menu;

    /// <summary>
    ///     Handler for displaying current progressives page.
    /// </summary>
    public class CurrentProgressivesLauncher : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IEventBus _eventBus;
        private readonly IMenuAccessService _menu;
        private readonly ISystemDisableManager _disableManager;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

        private Task CurrentProgressiveDisplayTask { get; set; }
        private bool _disposed;
        private bool _balanceChangedFromZero;
        private Guid _currentTransactionId;

        public CurrentProgressivesLauncher(
            IEventBus eventBus,
            IBank bank,
            IGameHistory gameHistory,
            ISystemDisableManager disableManager,
            IMenuAccessService menuAccessService,
            IPropertiesManager propertiesManager,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _menu = menuAccessService ?? throw new ArgumentNullException(nameof(menuAccessService));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ??
                                                throw new ArgumentNullException(
                                                    nameof(protocolLinkedProgressiveAdapter));

            if (gameHistory == null)
            {
                throw new ArgumentNullException(nameof(gameHistory));
            }
            if (bank == null)
            {
                throw new ArgumentNullException(nameof(bank));
            }

            Setup(bank.QueryBalance(), gameHistory.IsRecoveryNeeded);

            _currentTransactionId = Guid.Empty;
        }

        private void Setup(long balance, bool inRecovery)
        {
            _eventBus.Subscribe<CurrencyInCompletedEvent>(
                this,
                async (evt, ct) =>
                {
                    _currentTransactionId = Guid.Empty;
                    if (_balanceChangedFromZero && evt.Amount != 0) await ShowCurrentProgressives();
                });
            _eventBus.Subscribe<VoucherRedeemedEvent>(
                this,
                async (evt, ct) =>
                {
                    _currentTransactionId = Guid.Empty;
                    if (_balanceChangedFromZero && evt.Transaction.Amount != 0) await ShowCurrentProgressives();
                });
            _eventBus.Subscribe<WatOnCompleteEvent>(
                this,
                async (evt, ct) =>
                {
                    _currentTransactionId = Guid.Empty;
                    if (_balanceChangedFromZero && evt.Transaction.AuthorizedAmount != 0) await ShowCurrentProgressives();
                });
            _eventBus.Subscribe<BonusAwardedEvent>(
                this,
                async (evt, ct) =>
                {
                    _currentTransactionId = Guid.Empty;
                    if (_balanceChangedFromZero && evt.Transaction.TotalAmount != 0) await ShowCurrentProgressives();
                });
            _eventBus.Subscribe<BankBalanceChangedEvent>(
                this,
                evt =>
                {
                    Logger.Debug($"Current TransactionId:{_currentTransactionId},Event's TransactionId is {evt?.TransactionId},oldBalance:{evt?.OldBalance},newBalance:{evt?.NewBalance}");
                    if (_currentTransactionId == evt?.TransactionId)
                    {
                        Logger.Debug("Current Transaction and new event's transactions are equal");
                        return;
                    }

                    if (evt != null)
                    {
                        _currentTransactionId = evt.TransactionId;

                        _balanceChangedFromZero = (evt.OldBalance == 0 && evt.NewBalance > 0);
                    }

                    Logger.Debug($"_balanceChangedFromZero is {_balanceChangedFromZero}");
                });

            // If there is no credit or we are in recovery, we don't setup to display current progressives screen on boot up
            if (balance <= 0 || inRecovery)
            {
                return;
            }

            SubscribeBootUpEvents();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
                _lock.Dispose();
            }

            _disposed = true;
        }

        private void SubscribeBootUpEvents()
        {
            _eventBus.Subscribe<SystemEnabledEvent>(this, Handle);
            _eventBus.Subscribe<ProgressivesActivatedEvent>(this, Handle);

        }

        private bool CanShowProgressivePage()
        {
            var progressivePoolCreationType = (ProgressivePoolCreation)_propertiesManager.GetProperty(
                GamingConstants.ProgressivePoolCreationType,
                ProgressivePoolCreation.Default);

            if (progressivePoolCreationType != ProgressivePoolCreation.WagerBased)
            {
                return false;
            }

            var creationType = _protocolLinkedProgressiveAdapter?.GetActiveProgressiveLevels()?.FirstOrDefault()?.CreationType ??
                               LevelCreationType.Default;

            return creationType != LevelCreationType.Default;
        }

        private async Task Handle(ProgressivesActivatedEvent evt, CancellationToken token)
        {
            await _lock.WaitAsync(token);
            try
            {
                CurrentProgressiveDisplayTask = new Task<Task>(ShowCurrentProgressives);

                if (_disableManager.IsDisabled)
                {
                    return;
                }

                await Show();
            }
            catch (Exception ex)
            {
                Logger.Error("Progressives activated : Failed to display progressive screen", ex);
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task Handle(SystemEnabledEvent evt, CancellationToken token)
        {
            await _lock.WaitAsync(token);
            try
            {
                if (CurrentProgressiveDisplayTask == null || CurrentProgressiveDisplayTask.IsCompleted)
                {
                    return;
                }

                await Show();
            }
            catch (Exception ex)
            {
                Logger.Error("SystemEnabled : Failed to display progressive screen", ex);
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task ShowCurrentProgressives()
        {
            _balanceChangedFromZero = false;
            if (CanShowProgressivePage())
            {
                Logger.Debug("Displaying current progressives");

                await _menu.Show(Command.CurrentProgressiveMoneyIn);
            }
        }

        private async Task Show()
        {
            CurrentProgressiveDisplayTask.Start();
            await CurrentProgressiveDisplayTask;

            _eventBus.Unsubscribe<ProgressivesActivatedEvent>(this);
            _eventBus.Unsubscribe<SystemEnabledEvent>(this);
        }
    }
}