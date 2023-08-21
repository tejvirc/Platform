namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Gaming.Contracts;
    using Kernel;

    /// <summary>
    ///     The handler for LP 56 Send Enabled Game Numbers
    /// </summary>
    public class LP56SendEnabledGameNumbersHandler : ISasLongPollMultiDenomAwareHandler<SendEnabledGameNumbersResponse,
        LongPollMultiDenomAwareData>
    {
        private readonly IGameProvider _gameProvider;
        private readonly IPropertiesManager _propertiesManager;

        /// <summary>
        ///     Creates an Instance of the LP56SendEnabledGameNumbersHandler
        /// </summary>
        /// <param name="gameProvider">The game provider</param>
        /// <param name="propertiesManager">The properties manager</param>
        public LP56SendEnabledGameNumbersHandler(IGameProvider gameProvider, IPropertiesManager propertiesManager)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(IGameProvider));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(IPropertiesManager));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll> { LongPoll.SendEnabledGameNumbers };

        /// <inheritdoc />
        public SendEnabledGameNumbersResponse Handle(LongPollMultiDenomAwareData data)
        {
            if (data.TargetDenomination == 0)
            {
                var activeDenom = _propertiesManager.GetValue(GamingConstants.SelectedDenom, 0L);
                return new SendEnabledGameNumbersResponse(
                    _gameProvider.GetEnabledGames().Where(x => x.ActiveDenominations.Contains(activeDenom))
                    .SelectMany(game => game.Denominations.Where(denom => denom.Value == activeDenom))
                        .Select(denom => denom.Id).ToList());
            }

            var denomValue = data.TargetDenomination.CentsToMillicents();
            if (!_gameProvider.GetAllGames().Any(g => g.SupportedDenominations.Contains(denomValue)))
            {
                return new SendEnabledGameNumbersResponse(new List<long>())
                {
                    ErrorCode = MultiDenomAwareErrorCode.NotValidPlayerDenom
                };
            }

            return new SendEnabledGameNumbersResponse(
                _gameProvider.GetEnabledGames().Where(x => x.ActiveDenominations.Contains(denomValue))
                    .SelectMany(game => game.Denominations.Where(denom => denom.Value == denomValue))
                    .Select(denom => denom.Id).ToList());
        }
    }
}