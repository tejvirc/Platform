namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Application.Contracts.OperatorMenu;
    using Contracts.Barkeeper;

    public class OperatorMenuEnteredConsumer : Consumes<OperatorMenuEnteredEvent>
    {
        private readonly IBarkeeperHandler _barkeeper;

        public OperatorMenuEnteredConsumer(IBarkeeperHandler barkeeper)
        {
            _barkeeper = barkeeper ?? throw new ArgumentNullException(nameof(barkeeper));
        }

        public override void Consume(OperatorMenuEnteredEvent theEvent)
        {
            _barkeeper.OnAuditEntered();
        }
    }
}