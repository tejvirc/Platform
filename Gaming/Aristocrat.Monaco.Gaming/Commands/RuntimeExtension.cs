namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Linq;
    using Contracts;
    using Contracts.Progressives;
    using Kernel;
    using log4net;
    using Progressives;
    using Runtime;

    public static class RuntimeExtension
    {
        /// <summary>
        /// Calls the runtime to ask for the Jackpot values if the bet/wager amount has changed
        /// </summary>
        /// <param name="runtime">The runtime process</param>
        /// <param name="propertiesManager">The properties manager service</param>
        /// <param name="logger">Logger Service</param>
        /// <param name="progressiveGameProvider">ProgressiveGameProvider Service</param>
        /// <param name="betOrWager">The Bet/wager in cents</param>
        /// <param name="gameId">Game/variation Id</param>
        /// <param name="denomValue">Denom Value in cents</param>
        public static void CallJackpotNotificationIfBetAmountChanged(
            this IRuntime runtime,
            IPropertiesManager propertiesManager,
            ILog logger,
            IProgressiveGameProvider progressiveGameProvider,
            long betOrWager,
            int gameId,
            long denomValue)
        {
            var betCreditsSaved = propertiesManager.GetValue(GamingConstants.SelectedBetCredits, (long)0);

            // bet/wager comes in Amount
            if (betOrWager/denomValue == betCreditsSaved)
            {
                return;
            }

            propertiesManager.SetProperty(GamingConstants.SelectedBetCredits, betOrWager / denomValue);

            var progressivePoolCreationType = (ProgressivePoolCreation)propertiesManager.GetProperty(
                GamingConstants.ProgressivePoolCreationType,
                ProgressivePoolCreation.Default);

            if (progressivePoolCreationType != ProgressivePoolCreation.WagerBased)
            {
                return;
            }

            var creationType = progressiveGameProvider?.GetActiveProgressiveLevels()?.FirstOrDefault()?.CreationType ??
                               LevelCreationType.Default;

            if (creationType == LevelCreationType.Default)
            {
                return;
            }

            if (!runtime.Connected)
            {
                return;
            }

            runtime.JackpotNotification();

            logger.Debug(
                $"Jackpot notification invoked for game Id {gameId}, denom value {denomValue}, bet/wagered credits = {betOrWager / denomValue}");
        }
    }
}
