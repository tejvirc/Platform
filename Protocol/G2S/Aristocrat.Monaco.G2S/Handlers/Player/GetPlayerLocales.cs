namespace Aristocrat.Monaco.G2S.Handlers.Player
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class GetPlayerLocales : ICommandHandler<player, getPlayerLocales>
    {
        private readonly IG2SEgm _egm;

        public GetPlayerLocales(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        public async Task<Error> Verify(ClassCommand<player, getPlayerLocales> command)
        {
            return await Sanction.OwnerAndGuests<IPlayerDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<player, getPlayerLocales> command)
        {
            var response = command.GenerateResponse<playerLocaleList>();

            response.Command.playerLocale = new playerLocale[0];

            await Task.CompletedTask;
        }
    }
}