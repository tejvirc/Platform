namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Reflection;
    using Contracts;
    using Kernel;
    using log4net;

    /// <summary>
    ///     An implementation of <see cref="IGameRecovery" />
    ///     Notes about recovery:
    ///     =====================
    ///     We use the following recovery condition: if a game has a valid start time but no end time,
    ///     then we conclude it never completed and needs to be recovered.
    ///     The end time is set after presentation is completed.  So if a game crashes during
    ///     presentation, we recover it.  Technically, we know the results even before presentation
    ///     starts, but this makes more sense, as the results have not been presented to the player yet.
    ///     In some rare case where we cannot recover, but know the result, we have to go into a
    ///     special handpay type situation.
    ///     If the lobby is shown at some point during the recovery process, it needs to hide
    ///     the bank amount.  This is because it could be temporarily out of sync before
    ///     launching game recovery.
    ///     Lobby will need to present something like "Recovering" while a game is
    ///     being loaded for recovery.
    ///     Some recovery cases:
    ///     1. Game crashes and returns to lobby.
    ///     2. Platform crashes and brings game down with it.
    ///     3. Replay started during game round; this kills the current game so it would
    ///     need to be recovered.
    /// </summary>
    public class GameRecovery : IGameRecovery, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IGamePlayState _gamePlayState;

        private bool _disposed;

        private IEventBus _eventBus;

        private bool _wasRecovering;
        private bool _isRecovering;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameRecovery" /> class.
        /// </summary>
        /// <param name="eventBus">The IEventBus</param>
        /// <param name="gamePlayState">The IGamePlayState</param>
        public GameRecovery(IEventBus eventBus, IGamePlayState gamePlayState)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));

            _wasRecovering = IsRecovering;
        }

        /// <summary>
        ///     Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public bool IsRecovering
        {
            get => _isRecovering;
            private set
            {
                _isRecovering = value;
                Logger.Debug($"IsRecovering={value}");
            }
        }

        /// <inheritdoc />
        public int GameId { get; private set; }

        /// <inheritdoc />
        public bool TryStartRecovery(int gameId, bool verifyState)
        {
            Logger.Debug($"TryStartRecovery: gameId={gameId}, verifyState={verifyState}, CurrentState={_gamePlayState.CurrentState}");
            _gamePlayState.Faulted();
            if (verifyState && _gamePlayState.Idle)
            {
                return false;
            }

            IsRecovering = true;
            _wasRecovering = true;

            GameId = gameId;

            return true;
        }

        /// <inheritdoc />
        public void EndRecovery()
        {
            Logger.Debug("EndRecovery");
            IsRecovering = _wasRecovering;

            if (IsRecovering)
            {
                IsRecovering = false;
                _wasRecovering = false;
                GameId = 0;
            }
        }

        /// <inheritdoc />
        public void AbortRecovery()
        {
            Logger.Debug("AbortRecovery");

            _wasRecovering = IsRecovering;
            if (IsRecovering)
            {
                IsRecovering = false;
            }
            else
            {
                Logger.Error("AbortRecovery() called without having started recovery.");
            }
        }

        /// <summary>
        ///     Dispose.
        /// </summary>
        /// <param name="disposing">True if we are disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);
            }

            _eventBus = null;

            _disposed = true;
        }
    }
}