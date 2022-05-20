namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Kernel;

    /// <summary>
    ///     Handles the SystemEnabledEvent
    /// </summary>
    public class SystemEnabledEventConsumer : Consumes<SystemEnabledEvent>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemEnabledEventConsumer" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        public SystemEnabledEventConsumer(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public override void Consume(SystemEnabledEvent theEvent)
        {
            var device = _egm.GetDevice<ICabinetDevice>();

            device?.RemoveAllConditions();
        }
    }
}