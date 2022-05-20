namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Hardware.Contracts;

    /// <summary>
    ///     Handles the <see cref="FirmwareInstalledEvent" /> event.
    /// </summary>
    public class FirmwareInstalledConsumer : Consumes<FirmwareInstalledEvent>
    {
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FirmwareInstalledConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance</param>
        public FirmwareInstalledConsumer(IG2SEgm egm, IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public override void Consume(FirmwareInstalledEvent theEvent)
        {
            var device = _egm.GetDevice<IDownloadDevice>();
            if (device != null)
            {
                _eventLift.Report(device, EventCode.G2S_DLE301);
                _eventLift.Report(device, EventCode.G2S_DLE303);
            }
        }
    }
}