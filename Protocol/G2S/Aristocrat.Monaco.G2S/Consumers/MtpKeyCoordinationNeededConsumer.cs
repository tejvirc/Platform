namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.G2S.Client.Communications;
    using Handlers;

    public class MtpKeyCoordinationNeededConsumer : Consumes<MtpKeyCoordinationNeededEvent>
    {
        private readonly ICommandBuilder<ICommunicationsDevice, getMcastKeyUpdate> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        public MtpKeyCoordinationNeededConsumer(
            IG2SEgm egm,
            ICommandBuilder<ICommunicationsDevice, getMcastKeyUpdate> commandBuilder,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public override void Consume(MtpKeyCoordinationNeededEvent theEvent)
        {
            var device = _egm.GetDevice<ICommunicationsDevice>();
            if (device == null)
            {
                return;
            }

            var keyUpdateRequest = new getMcastKeyUpdate();
            _commandBuilder.Build(device, keyUpdateRequest);
            _eventLift.Report(device, EventCode.G2S_CCE103, device.DeviceList(keyUpdateRequest));
        }
    }
}