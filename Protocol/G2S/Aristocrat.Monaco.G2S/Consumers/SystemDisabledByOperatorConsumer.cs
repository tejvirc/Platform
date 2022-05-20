namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Application.Contracts;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;

    /// <summary>
    ///     Handles the <see cref="SystemDisabledByOperatorEvent" /> event.
    /// </summary>
    public class SystemDisabledByOperatorConsumer : Consumes<SystemDisabledByOperatorEvent>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemDisabledByOperatorConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        public SystemDisabledByOperatorConsumer(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public override void Consume(SystemDisabledByOperatorEvent theEvent)
        {
            var device = _egm.GetDevice<ICabinetDevice>();

            device?.AddCondition(device, EgmState.OperatorLocked);
        }
    }
}