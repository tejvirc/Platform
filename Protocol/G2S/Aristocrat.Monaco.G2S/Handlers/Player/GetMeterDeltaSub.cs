namespace Aristocrat.Monaco.G2S.Handlers.Player
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class GetMeterDeltaSub : ICommandHandler<player, getMeterDeltaSub>
    {
        private readonly IG2SEgm _egm;

        public GetMeterDeltaSub(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        public async Task<Error> Verify(ClassCommand<player, getMeterDeltaSub> command)
        {
            return await Sanction.OwnerAndGuests<IPlayerDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<player, getMeterDeltaSub> command)
        {
            var response = command.GenerateResponse<meterDeltaSubList>();

            var playerDevice = _egm.GetDevice<IPlayerDevice>();

            if (!playerDevice.MeterDeltaSupported || !playerDevice.SubscribedMeters.Any())
            {
                response.Command.meterDeltaSubscription = new meterDeltaSubscription[0];
            }
            else
            {
                response.Command.meterDeltaSubscription =
                    playerDevice.SubscribedMeters
                        .Expand(_egm.Devices)
                        .Filter(command.Command.meterSelect)
                        .ToArray();
            }

            await Task.CompletedTask;
        }
    }
}
