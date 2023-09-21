namespace Aristocrat.Monaco.G2S.Handlers.Analytics
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class TrackAck : ICommandHandler<analytics, trackAck>
    {
        private readonly IG2SEgm _egm;

        public TrackAck(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        public async Task<Error> Verify(ClassCommand<analytics, trackAck> command)
        {
            return await Sanction.OnlyOwner<IProgressiveDevice>(_egm, command);
        }

        public Task Handle(ClassCommand<analytics, trackAck> command)
        {
            return Task.CompletedTask;
        }
    }
}