namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Application.Contracts.OperatorMenu;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;

    /// <summary>
    ///     Handles the OperatorMenuEnteredEvent, which sets the cabinet's state.
    /// </summary>
    public class OperatorMenuExitedConsumer : Consumes<OperatorMenuExitedEvent>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorMenuExitedConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        public OperatorMenuExitedConsumer(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public override void Consume(OperatorMenuExitedEvent theEvent)
        {
            var device = _egm.GetDevice<ICabinetDevice>();

            device?.RemoveCondition(device, EgmState.OperatorMode);
            device?.RemoveCondition(device, EgmState.AuditMode);
        }
    }
}