namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using System.Threading.Tasks;
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
            var device = _egm.GetDevice<ICommunicationsDevice>(theEvent.HostId);
            if (device == null)
            {
                return;
            }

            var keyUpdateRequest = new getMcastKeyUpdate();
            _commandBuilder.Build(device, keyUpdateRequest);
            keyUpdateRequest.multicastId = theEvent.MulticastId;
            var session = device.SendKeyUpdateRequest(keyUpdateRequest);
            session.WaitForCompletion();
            if (session.SessionState == SessionStatus.Success && session.Responses.Count > 0)
            {
                if (session.Responses[0].IClass.Item is mcastKeyUpdate keyUpdate)
                {
                    using (MessageBuilder messageBuilder = new MessageBuilder())
                    {
                        messageBuilder.LoadSecurityNamespace(SchemaVersion.m105, null);
                        var sp = messageBuilder.DecodeSecurityParams(keyUpdate.securityParams);
                        device.UpdateSecurityParameters(keyUpdate.multicastId,
                                                         EndpointUtilities.EncryptorKeyStringToArray(sp.currentKey),
                                                         sp.currentMsgId,
                                                         EndpointUtilities.EncryptorKeyStringToArray(sp.newKey),
                                                         sp.newKeyMsgId,
                                                         sp.currentKeyLastMsgId);
                    }
                }
            }
        }
    }
}