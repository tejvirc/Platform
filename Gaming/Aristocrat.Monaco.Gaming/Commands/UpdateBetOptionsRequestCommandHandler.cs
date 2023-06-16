namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts.Extensions;
    using Contracts;
    using Kernel;
    using log4net;
    using Progressives;
    using Runtime;

    /// <summary>
    ///     Command handler for the <see cref="UpdateBetOptions" /> command.
    /// </summary>
    public class UpdateBetOptionsRequestCommandHandler : ICommandHandler<UpdateBetOptions>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IPropertiesManager _properties;
        private readonly IRuntime _runtime;
        private readonly IProgressiveGameProvider _progressiveGameProvider;

        public UpdateBetOptionsRequestCommandHandler(
            IPropertiesManager properties,
            IRuntime runtime,
            IProgressiveGameProvider progressiveGameProvider)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _progressiveGameProvider = progressiveGameProvider ??
                                       throw new ArgumentNullException(nameof(progressiveGameProvider));
        }

        /// <inheritdoc />
        public void Handle(UpdateBetOptions command)
        {
            var (currentGame, currentDenom) = _properties.GetActiveGame();
            Logger.Debug($"UpdateBetOptions for GameId {currentGame?.Id.ToString() ?? "null"} and Denom {currentDenom?.Value.ToString() ?? "null"}");
            if (currentGame == null || currentDenom == null)
            {
                return;
            }

            // TODO: Update this to compare GameId with UniqueGameId when we have it in GameDetail, as HHR doesn't always use 0 for main game
            var mainGame = command.BetDetails.Single(x => x.GameId == 0);

            _properties.SetProperty(GamingConstants.SelectedBetMultiplier, mainGame.BetMultiplier);
            _properties.SetProperty(GamingConstants.SelectedLineCost, mainGame.BetPerLine);
            _properties.SetProperty(GamingConstants.SelectedBetDetails, mainGame);
            _properties.SetProperty(GamingConstants.SelectedMultiGameBetDetails, command.BetDetails);

            var betCredits = mainGame.Wager;

            _runtime?.CallJackpotNotificationIfBetAmountChanged(
                _properties,
                Logger,
                _progressiveGameProvider,
                betCredits,
                currentGame.Id,
                currentDenom.Value.MillicentsToCents());
        }
    }
}