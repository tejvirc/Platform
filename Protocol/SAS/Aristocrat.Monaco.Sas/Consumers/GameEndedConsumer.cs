namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Contracts.SASProperties;
    using Exceptions;
    using Gaming.Contracts;
    using Kernel;
    using Progressive;
    using Storage.Models;

    /// <summary>
    ///     Handles the <see cref="GameEndedEvent" /> event.
    /// </summary>
    public class GameEndedConsumer : Consumes<GameEndedEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly IPropertiesManager _properties;
        private readonly IProgressiveWinDetailsProvider _progressiveWinDetailsProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameEndedConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        /// <param name="properties">An instance of <see cref="IPropertiesManager"/></param>
        /// <param name="progressiveWinDetailsProvider">An instance of <see cref="IProgressiveWinDetailsProvider"/></param>
        public GameEndedConsumer(ISasExceptionHandler exceptionHandler, IPropertiesManager properties, IProgressiveWinDetailsProvider progressiveWinDetailsProvider)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _progressiveWinDetailsProvider = progressiveWinDetailsProvider ?? throw new ArgumentNullException(nameof(progressiveWinDetailsProvider));
        }

        /// <inheritdoc />
        public override void Consume(GameEndedEvent theEvent)
        {
            var winAmount = theEvent.Log.TotalWon / theEvent.Denomination.MillicentsToCents();
            var denoms = _properties.GetValue(SasProperties.SasHosts, Enumerable.Empty<Host>())
               .Select(x => x.AccountingDenom).ToList();
            _exceptionHandler.ReportException(
                client => GetGameEndedException(winAmount, denoms, client),
                GeneralExceptionCode.GameHasEnded);

            if (theEvent.Log.Jackpots.Any())
            {
                var creditChanged = theEvent.Log.EndCredits - theEvent.Log.StartCredits > 0;
                var cashout = theEvent.Log.CashOutInfo.Any(c => !c.Handpay);
                _progressiveWinDetailsProvider.SetLastProgressiveWin(theEvent.Log);

                if (creditChanged || cashout)
                {
                    _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.ProgressiveWin));
                }
            }
        }

        private static GameEndedExceptionBuilder GetGameEndedException(
            long winAmount,
            IReadOnlyList<long> accountingDenoms,
            byte clientNumber)
        {
            return accountingDenoms.Count <= clientNumber
                ? null
                : new GameEndedExceptionBuilder(winAmount, accountingDenoms[clientNumber]);
        }
    }
}
