namespace Aristocrat.Monaco.G2S.Handlers.Chooser
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;

    public class GetGameCombo : ICommandHandler<chooser, getGameCombo>
    {
        private readonly IG2SEgm _egm;
        private readonly IGameProvider _gameProvider;

        public GetGameCombo(IG2SEgm egm, IGameProvider gameProvider)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        public async Task<Error> Verify(ClassCommand<chooser, getGameCombo> command)
        {
            return await Sanction.OwnerAndGuests<IChooserDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<chooser, getGameCombo> command)
        {
            var response = command.GenerateResponse<gameComboList>();

            response.Command.gameComboInfo = _gameProvider.GetGames().Select(
                game => new gameComboInfo
                {
                    gamePlayId = game.Id,
                    gamePlayCombo = game.SupportedDenominations.Select(
                        d =>
                            new gamePlayCombo
                            {
                                themeId = game.ThemeId,
                                paytableId = game.PaytableId,
                                denomId = d
                            }).ToArray()
                }).ToArray();

            await Task.CompletedTask;
        }
    }
}
