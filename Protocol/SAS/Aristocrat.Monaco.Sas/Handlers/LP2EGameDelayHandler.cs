namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Gaming.Contracts.Bonus;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Kernel;

    /// <summary>
    ///     The handler for LP 2E game Delay
    /// </summary>
    public class LP2EGameDelayHandler : ISasLongPollHandler<LongPollReadSingleValueResponse<bool>, LongPollSingleValueData<uint>>
    {
        private readonly IBonusHandler _bonusHandler;
        private readonly IPropertiesManager _propertiesManager;

        /// <summary>
        ///     Creates an instance of the LP 2E handler class
        /// <param name="bonusHandler">The bonus handler from the game. Used to set delay time.</param>
        /// <param name="propertiesManager">The properties manager</param>
        /// </summary>
        public LP2EGameDelayHandler(
            IBonusHandler bonusHandler,
            IPropertiesManager propertiesManager)
        {
            _bonusHandler = bonusHandler ?? throw new ArgumentNullException(nameof(bonusHandler));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll> { LongPoll.DelayGame };

        /// <inheritdoc />
        public LongPollReadSingleValueResponse<bool> Handle(LongPollSingleValueData<uint> data)
        {
            var result = new LongPollReadSingleValueResponse<bool>(false);
            if (IsLegacyBonusingEnabled())
            {
                const int milliseconds100ToMilliseconds = 100;

                // Value passed in is in 100 ms intervals. Zero time will stop the delay.
                var delay = TimeSpan.FromMilliseconds(data.Value * milliseconds100ToMilliseconds);

                // 2E exceptions are to delay the current and all future played games, so have it delay indefinitely.
                // - Jeff Eastman, (12/11/19)
                _bonusHandler.SetGameEndDelay(delay, TimeSpan.MaxValue, 0, true);
                result.Data = true;
            }

            return result;
        }

        private bool IsLegacyBonusingEnabled() =>
            _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).LegacyBonusAllowed;
    }
}