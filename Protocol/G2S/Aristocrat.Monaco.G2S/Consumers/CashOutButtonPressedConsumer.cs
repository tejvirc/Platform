namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Gaming.Contracts;
    using Kernel;

    /// <summary>
    ///     Handles the <see cref="CashOutButtonPressedEvent" /> event.
    /// </summary>
    public class CashOutButtonPressedConsumer : Consumes<CashOutButtonPressedEvent>
    {
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CashOutButtonPressedConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance.</param>
        /// <param name="properties">An <see cref="IPropertiesManager" /> instance</param>
        public CashOutButtonPressedConsumer(IG2SEgm egm, IEventLift eventLift, IPropertiesManager properties)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <inheritdoc />
        public override void Consume(CashOutButtonPressedEvent theEvent)
        {
            var device = _egm.GetDevice<ICabinetDevice>();
            if (device == null)
            {
                return;
            }
            
            _eventLift.Report(device, EventCode.G2S_CBE316, theEvent);
        }
    }
}