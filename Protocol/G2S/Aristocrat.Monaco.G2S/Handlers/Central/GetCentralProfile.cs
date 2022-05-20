namespace Aristocrat.Monaco.G2S.Handlers.Central
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;

    public class GetCentralProfile : ICommandHandler<central, getCentralProfile>
    {
        private readonly IG2SEgm _egm;
        private readonly IGameProvider _games;
        private readonly ITransactionHistory _transactions;

        public GetCentralProfile(IG2SEgm egm, IGameProvider games, ITransactionHistory transactions)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _games = games ?? throw new ArgumentNullException(nameof(games));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
        }

        public async Task<Error> Verify(ClassCommand<central, getCentralProfile> command)
        {
            return await Sanction.OwnerAndGuests<ICentralDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<central, getCentralProfile> command)
        {
            var device = _egm.GetDevice<ICentralDevice>(command.IClass.deviceId);

            var response = command.GenerateResponse<centralProfile>();

            response.Command.configurationId = device.ConfigurationId;
            response.Command.restartStatus = device.RestartStatus;
            response.Command.requiredForPlay = device.RequiredForPlay;
            response.Command.minLogEntries = _transactions.GetMaxTransactions<CentralTransaction>();
            response.Command.timeToLive = device.TimeToLive;
            response.Command.noResponseTimer = (int)device.NoResponseTimer.TotalMilliseconds;

            response.Command.configComplete = device.ConfigComplete;
            response.Command.configDateTime = device.ConfigDateTime;

            var games = _games.GetAllGames()
                .Where(g => g.CentralAllowed)
                .SelectMany(
                    g => g.Denominations,
                    (detail, denomination) => new { Detail = detail, Denomination = denomination })
                .SelectMany(
                    g => g.Detail.WagerCategories,
                    (detail, wagerCategory) =>
                        new { detail.Detail, detail.Denomination, WagerCategory = wagerCategory });

            response.Command.Items = games.Select(
                g => new centralGamePlay
                {
                    gamePlayId = g.Detail.Id,
                    themeId = g.Detail.ThemeId,
                    paytableId = g.Detail.PaytableId,
                    denomId = g.Denomination.Id,
                    wagerCategory = g.WagerCategory.Id
                }).Cast<object>().ToArray();

            await Task.CompletedTask;
        }
    }
}