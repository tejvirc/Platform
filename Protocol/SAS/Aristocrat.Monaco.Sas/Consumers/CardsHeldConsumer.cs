namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Exceptions;
    using Gaming.Contracts;

    /// <summary>
    ///     Handles the <see cref="CardsHeldEvent" />.
    ///     This event is sent from PrimaryEventHandler with card information
    /// </summary>
    public class CardsHeldConsumer : Consumes<CardsHeldEvent>
    {
        private readonly IRteStatusProvider _rteProvider;
        private readonly ISasExceptionHandler _exceptionHandler;

        private const byte SasClient1 = 0;
        private const byte SasClient2 = 1;
        private const int MaxCardsToReport = 5;

        /// <summary>
        ///     Handles the GameRoundPokerTriggeredEvent
        /// </summary>
        /// <param name="rteProvider">The RealTimeEventReportingEnabled provider</param>
        /// <param name="exceptionHandler">The Sas exception handler</param>
        public CardsHeldConsumer(
            IRteStatusProvider rteProvider,
            ISasExceptionHandler exceptionHandler)
        {
            _rteProvider = rteProvider ?? throw new ArgumentNullException(nameof(rteProvider));
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(CardsHeldEvent theEvent)
        {
            if (_rteProvider.Client1RteEnabled)
            {
                ReportCardsHeldState(theEvent.CardsHeld, SasClient1);
            }

            if (_rteProvider.Client2RteEnabled)
            {
                ReportCardsHeldState(theEvent.CardsHeld, SasClient2);
            }
        }

        private void ReportCardsHeldState(IEnumerable<HoldStatus> heldCards, byte clientNumber)
        {
            // report held status for each card in the hand, up to a maximum of 5 cards
            var i = 0;
            foreach (var card in heldCards)
            {
                _exceptionHandler.ReportException(new CardHeldExceptionBuilder(card, i++), clientNumber);
                if (i >= MaxCardsToReport)
                {
                    break;
                }
            }
        }
    }
}
