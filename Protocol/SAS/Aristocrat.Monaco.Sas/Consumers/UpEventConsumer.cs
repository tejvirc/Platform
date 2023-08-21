namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using AftTransferProvider;
    using Contracts.Client;
    using Hardware.Contracts.Button;

    /// <summary>
    ///     Handles the <see cref="UpEvent" /> event.
    /// </summary>
    public class UpEventConsumer : Consumes<UpEvent>
    {
        private readonly IHardCashOutLock _hardCashOutLock;
        private readonly IAftOffTransferProvider _aftOffTransferProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UpEventConsumer" /> class.
        /// </summary>
        /// <param name="hardCashOutLock">The hard cash out lock</param>
        /// <param name="aftOffTransferProvider">The aft off transfer provider</param>
        public UpEventConsumer(
            IHardCashOutLock hardCashOutLock,
            IAftOffTransferProvider aftOffTransferProvider)
            : base((evt) => evt.LogicalId == (int)ButtonLogicalId.Button30)
        {
            _hardCashOutLock = hardCashOutLock ?? throw new ArgumentNullException(nameof(hardCashOutLock));
            _aftOffTransferProvider = aftOffTransferProvider ?? throw new ArgumentNullException(nameof(aftOffTransferProvider));
        }

        /// <inheritdoc />
        public override void Consume(UpEvent theEvent)
        {
            if (_hardCashOutLock.WaitingForKeyOff)
            {
                _hardCashOutLock.OnKeyedOff();
            }

            if (_aftOffTransferProvider.WaitingForKeyOff)
            {
                _aftOffTransferProvider.OnKeyedOff();
            }
        }
    }
}
