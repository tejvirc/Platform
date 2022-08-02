namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
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
            Logger.Debug($"UpdateBetOptions for GameId {currentGame} and Denom {currentDenom}");

            if (currentGame == null || currentDenom == null)
            {
                return;
            }

            _properties.SetProperty(GamingConstants.SelectedBetMultiplier, command.BetMultiplier);
            _properties.SetProperty(GamingConstants.SelectedLineCost, command.LineCost);

            var details = new BetDetails(
                command.BetLinePresetId,
                (int)((long)command.LineCost).MillicentsToCents(),
                command.NumberLines,
                command.Ante,
                command.Stake);
            _properties.SetProperty(GamingConstants.SelectedBetDetails, details);

            var betCredits = command.Wager;

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