namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using Hardware.Contracts.IdReader;

    public class SetValidationEventConsumer : Consumes<SetValidationEvent>
    {
        private readonly ICommandBuilder<IIdReaderDevice, idReaderStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetValidationEventConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{TDevice,TCommand}" /> instance.</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance.</param>
        public SetValidationEventConsumer(
            IG2SEgm egm,
            ICommandBuilder<IIdReaderDevice, idReaderStatus> commandBuilder,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public override void Consume(SetValidationEvent setValidationEvent)
        {
            var idReaderId = setValidationEvent.IdReaderId;

            var idDevice = _egm.GetDevice<IIdReaderDevice>(idReaderId);

            var status = new idReaderStatus();

            _commandBuilder.Build(idDevice, status);

            var eventCode = GetEventCode(setValidationEvent);

            _eventLift.Report(idDevice, eventCode, idDevice.DeviceList(status), setValidationEvent);
        }

        private static string GetEventCode(SetValidationEvent setValidationEvent)
        {
            if (setValidationEvent.Identity == null)
            {
                return EventCode.G2S_IDE103; //if null, it means card was removed and we need to send 'G2S_IDE103'
            }

            return setValidationEvent.Identity.Type == IdTypes.None ||
                   setValidationEvent.Identity.Type == IdTypes.Invalid
                ? EventCode.G2S_IDE102
                : EventCode.G2S_IDE101;
        }
    }
}
