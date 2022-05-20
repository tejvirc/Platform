namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Application.Contracts.Localization;
    using Contracts;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     Command handler for the <see cref="GameFatalError" /> command.
    /// </summary>
    public class GameFatalErrorHandler : ICommandHandler<GameFatalError>
    {
        private readonly IEventBus _eventBus;
        private readonly IGameHistory _gameHistory;
        private readonly ISystemDisableManager _systemDisable;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameFatalErrorHandler" /> class.
        /// </summary>
        /// <param name="systemDisable">An <see cref="ISystemDisableManager" /> instance.</param>
        /// <param name="eventBus">An <see cref="IEventBus" /> instance.</param>
        /// <param name="gameHistory">An <see cref="IGameHistory" /> instance.</param>
        public GameFatalErrorHandler(
            ISystemDisableManager systemDisable,
            IEventBus eventBus,
            IGameHistory gameHistory)
        {
            _systemDisable = systemDisable ?? throw new ArgumentNullException(nameof(systemDisable));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
        }

        /// <inheritdoc />
        public void Handle(GameFatalError command)
        {
            Func<string> messageCallback;

            switch (command.ErrorCode)
            {
                case GameErrorCode.LiabilityLimit:
                    messageCallback = () => Localizer.For(CultureFor.Player).GetString(ResourceKeys.LiabilityCheckFailed);
                    break;
                case GameErrorCode.LegitimacyLimit:
                    messageCallback = () => Localizer.For(CultureFor.Player).GetString(ResourceKeys.LegitimacyCheckFailed);
                    break;
                default:
                    // This should never happen, but we're just going to bail if it does
                    return;
            }

            _eventBus.Publish(new GameFatalErrorEvent());
            _gameHistory.LogFatalError(command.ErrorCode);
            _systemDisable.Disable(GamingConstants.FatalGameErrorGuid, SystemDisablePriority.Immediate, messageCallback);
        }
    }
}