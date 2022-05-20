namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Application.Contracts.OperatorMenu;
    using Contracts.Barkeeper;

    public class OperatorMenuExitedConsumer : Consumes<OperatorMenuExitedEvent>
    {
        private readonly IBarkeeperHandler _barkeeper;

        public OperatorMenuExitedConsumer(IBarkeeperHandler barkeeper)
        {
            _barkeeper = barkeeper ?? throw new ArgumentNullException(nameof(barkeeper));
        }

        public override void Consume(OperatorMenuExitedEvent theEvent)
        {
            _barkeeper.OnAuditExited();
        }
    }
}