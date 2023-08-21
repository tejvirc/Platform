namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Application.Contracts;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;

    /// <summary>
    ///     Handles the <see cref="SystemEnabledByOperatorEvent" /> event.
    /// </summary>
    public class SystemEnabledByOperatorConsumer : Consumes<SystemEnabledByOperatorEvent>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemEnabledByOperatorConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        public SystemEnabledByOperatorConsumer(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public override void Consume(SystemEnabledByOperatorEvent theEvent)
        {
            var device = _egm.GetDevice<ICabinetDevice>();

            device?.RemoveCondition(device, EgmState.OperatorLocked);
        }
    }
}