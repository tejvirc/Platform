namespace Aristocrat.Monaco.G2S.Handlers.Chooser
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class GetChooserProfile : ICommandHandler<chooser, getChooserProfile>
    {
        private readonly IG2SEgm _egm;
        private readonly IGameProvider _gameProvider;
        private readonly IGameOrderSettings _gameOrderSettings;

        public GetChooserProfile(IG2SEgm egm, IGameProvider gameProvider, IGameOrderSettings gameOrderService)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _gameOrderSettings = gameOrderService ?? throw new ArgumentNullException(nameof(gameOrderService));
        }

        public async Task<Error> Verify(ClassCommand<chooser, getChooserProfile> command)
        {
            return await Sanction.OwnerAndGuests<IChooserDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<chooser, getChooserProfile> command)
        {
            var device = _egm.GetDevice<IChooserDevice>(command.IClass.deviceId);

            var response = command.GenerateResponse<chooserProfile>();

            response.Command.configurationId = device.ConfigurationId;
            response.Command.configDateTime = device.ConfigDateTime;
            response.Command.configComplete = device.ConfigComplete;
            response.Command.useDefaultConfig = device.UseDefaultConfig;
            
            response.Command.gameComboData = _gameProvider.GetGames().SelectMany(
                game => game.SupportedDenominations,
                (g, d) => new gameComboData
                {
                    gamePlayId = g.Id,
                    themeId = g.ThemeId,
                    paytableId = g.PaytableId,
                    denomId = d,
                    gamePosPriority = _gameOrderSettings.GetIconPositionPriority(g.ThemeId),
                    gameComboTag = GetGameComboTag(g)
                }).ToArray();

            await Task.CompletedTask;
        }

        private static gameComboTag[] GetGameComboTag(IGameProfile game)
        {
            try
            {
                return game.GameTags.Select(t => new gameComboTag
                {
                    gameTag = t.ToG2SGameTagString()
                }).ToArray();
            }
            catch (Exception)
            {
                return new gameComboTag[] { };
            }
        }


    }
}
