namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Gaming.Contracts;
    using Handlers.GamePlay;

    public class SecondaryGameChoiceConsumer : Consumes<SecondaryGameChoiceEvent>
    {
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SecondaryGameChoiceConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance</param>
        public SecondaryGameChoiceConsumer(IG2SEgm egm, IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public override void Consume(SecondaryGameChoiceEvent theEvent)
        {
            var device = _egm.GetDevice<IGamePlayDevice>(theEvent.GameId);
            if (device == null)
            {
                return;
            }

            _eventLift.Report(
                device,
                EventCode.G2S_GPE106,
                theEvent.Log.TransactionId,
                device.TransactionList(theEvent.Log.ToRecallLog()),
                theEvent);
        }
    }
}