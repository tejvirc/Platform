namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Gaming.Contracts;

    /// <summary>
    ///     Handles the <see cref="GameInstalledEvent" /> event.
    /// </summary>
    public class GameInstalledConsumer : Consumes<GameInstalledEvent>
    {
        private readonly IGatComponentFactory _componentFactory;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameInstalledConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance</param>
        /// <param name="componentFactory">An <see cref="IGatComponentFactory" /> instance</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance</param>
        public GameInstalledConsumer(
            IG2SEgm egm,
            IGatComponentFactory componentFactory,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _componentFactory = componentFactory ?? throw new ArgumentNullException(nameof(componentFactory));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public override void Consume(GameInstalledEvent theEvent)
        {
            _componentFactory.RegisterGame(theEvent.PackageId, theEvent.GamePackage);

            var device = _egm.GetDevice<IDownloadDevice>();
            if (device != null)
            {
                _eventLift.Report(device, EventCode.G2S_DLE301);
                _eventLift.Report(device, EventCode.G2S_DLE303);
            }
        }
    }
}