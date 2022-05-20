namespace Aristocrat.Monaco.Hhr.Services
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using Accounting.Contracts;
    using Client.Messages;
    using Client.WorkFlow;
    using Gaming.Contracts;
    using Kernel;
    using log4net;
    using Timer = System.Timers.Timer;

    /// <summary>
    ///     Implementation of <see cref="IPlayerSessionService"/>
    /// </summary>
    public class PlayerSessionService : IPlayerSessionService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // A timer to keep track of the timeout when the credit is played down to zero before we ask for a new player ID.
        private readonly Timer _creditZeroTimer;

        // A lock we can use to make sure that we have stopped the timer and it is not currently firing, when we dispose.
        private readonly SemaphoreSlim _timerLock = new SemaphoreSlim(1, 1);

        // A lock we can use to make sure that no two threads can be trying to get a new player ID at any given time.
        private readonly SemaphoreSlim _messageLock = new SemaphoreSlim(1, 1);

        // The rest of these fields are internal and should be self-explanatory in their usage.
        private readonly ICentralManager _centralManager;
        private readonly IEventBus _eventBus;
        private readonly IPlayerBank _playerBank;

        // The current value of the player ID, which starts as empty but can usually be restored from persistence.
        private string _currentPlayerId;

        // Indicates that we have cleaned up this object and we should not attempt to do any further work.
        private bool _disposed;

        /// <summary>
        ///     The timeout for zero credit before we get anew player ID. The default value for this parameter is 30
        ///     seconds, as per the HHR spec.
        /// </summary>
        public double InactivityIntervalMillis
        {
            get => _creditZeroTimer.Interval;
            set => _creditZeroTimer.Interval = value;
        }

        /// <summary>
        ///     Implement the <see cref="IPlayerSessionService"/> interface, so we may fetch a player ID from the central
        ///     server as required.
        /// </summary>
        /// <param name="centralManager">The HHR central manager, used for sending messages to the server</param>
        /// <param name="eventBus">The event bus, where we can listen for the events that cause us to renew the ID</param>
        /// <param name="playerBank">The player bank which tells us how much credit is currently on the meter</param>
        public PlayerSessionService(
            ICentralManager centralManager,
            IEventBus eventBus,
            IPlayerBank playerBank)
        {
            _centralManager = centralManager;
            _eventBus = eventBus;
            _playerBank = playerBank;

            // Subscribe to the events that are going to cause us to update the player ID
            _eventBus.Subscribe<TransferOutCompletedEvent>(this, HandleTransferOutCompleted);
            _eventBus.Subscribe<BankBalanceChangedEvent>(this, HandleBalanceChanged);

            _creditZeroTimer = new Timer(30000.0) { AutoReset = false };
            _creditZeroTimer.Elapsed += CreditZeroTimeout;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public async Task<string> GetCurrentPlayerId()
        {
            // Indicate that we're already getting an ID, in case some other thread tries to get one at the same time.
            await _messageLock.WaitAsync();

            try
            {
                if (string.IsNullOrEmpty(_currentPlayerId))
                {
                    await GetNewPlayerId();
                    if (string.IsNullOrEmpty(_currentPlayerId))
                    {
                        Logger.Error($"PlayerId Empty");

                        throw new InvalidOperationException();
                    }
                }
            }
            finally
            {
                _messageLock.Release();
            }

            return _currentPlayerId;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _timerLock.Wait();
                _creditZeroTimer.Stop();
                _timerLock.Release();
                _timerLock.Dispose();
                _messageLock.Dispose();
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void HandleTransferOutCompleted(TransferOutCompletedEvent obj)
        {
            // If the player has cashed out of the machine completely, we need to get a new player ID. We can also stop
            // the timer if we had one running, because we won't need that any more.
            if (_playerBank.Balance == 0)
            {
                _currentPlayerId = string.Empty;
                _creditZeroTimer.Stop();
            }
        }

        private void HandleBalanceChanged(BankBalanceChangedEvent obj)
        {
            // If the machine's credit is zero, then we must have either played down to zero, or the credit has been
            // transferred out. If we played down to zero then we start the countdown so that we can get a new player ID
            // if no more credit is inserted. If it turns out we're at zero because of a cashout, we'll immediately
            // invalidate the ID and stop the timer. If we get the cashout first, we'll stop this timer in the event that
            // someone causes us to fetch a new ID.
            if (obj.NewBalance == 0)
            {
                _creditZeroTimer.Start();
            }

            // If the machine's credit is non-zero, then we must have inserted some credit (or won some somehow), so we
            // can stop the timer if it was running.
            if (obj.NewBalance > 0)
            {
                _creditZeroTimer.Stop();
            }
        }

        private async void CreditZeroTimeout(object sender, ElapsedEventArgs e)
        {
            await _timerLock.WaitAsync();

            // Check again that the balance is zero, just in case something went wrong or we missed something.
            if (_playerBank.Balance == 0)
            {
                _currentPlayerId = string.Empty;
            }

            _timerLock.Release();
        }

        private async Task GetNewPlayerId()
        {
            // Protect against any event trying to fire this after we are disposed. Our timer code should already protect us.
            if (_disposed)
            {
                return;
            }

            // Set up a request for a new player ID. We do not retry, unless the server responds telling us to do so.
            var request = new PlayerIdRequest();

            try
            {
                // Ask the central server for a new player ID.
                var response = await _centralManager.Send<PlayerIdRequest, PlayerIdResponse>(request);
                _currentPlayerId = response.PlayerId;

                Logger.Debug($"Got PlayerId : {_currentPlayerId}");
            }
            catch (UnexpectedResponseException ex)
            {
                // If the response isn't a new player ID, we'll indicate that we have no ID, so that we fetch a new one
                // next time someone asks.
                _currentPlayerId = string.Empty;
                Logger.Warn(
                    $"Got incorrect response to player ID request of type {ex.Response.GetType()} with status {ex.Response.MessageStatus}",
                    ex);
            }
        }
    }
}