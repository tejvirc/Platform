namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Application.Contracts.OperatorMenu;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;

    /// <summary>
    ///     Handles the OperatorMenuEnteredEvent, which sets the cabinet's state.
    /// </summary>
    public class OperatorMenuEnteredConsumer : Consumes<OperatorMenuEnteredEvent>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorMenuEnteredConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        public OperatorMenuEnteredConsumer(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public override void Consume(OperatorMenuEnteredEvent theEvent)
        {
            var device = _egm.GetDevice<ICabinetDevice>();
            device?.AddCondition(device, theEvent.IsTechnicianRole ? EgmState.OperatorMode : EgmState.AuditMode);
        }
    }
}
