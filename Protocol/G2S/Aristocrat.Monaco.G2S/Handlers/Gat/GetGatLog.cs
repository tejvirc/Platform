namespace Aristocrat.Monaco.G2S.Handlers.Gat
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.GAT.Storage;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Defines a new instance of an ICommandHandler.
    /// </summary>
    public class GetGatLog : ICommandHandler<gat, getGatLog>
    {
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IG2SEgm _egm;
        private readonly IGatVerificationRequestRepository _repository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetGatLog" /> class.
        ///     Constructs a new instance using an egm and the GAT service.
        /// </summary>
        /// <param name="egm">An instance of an IG2SEgm.</param>
        /// <param name="contextFactory">DB context factory</param>
        /// <param name="repository">Package log repository</param>
        public GetGatLog(
            IG2SEgm egm,
            IMonacoContextFactory contextFactory,
            IGatVerificationRequestRepository repository)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<gat, getGatLog> command)
        {
            return await Sanction.OwnerAndGuests<IGatDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<gat, getGatLog> command)
        {
            var response = command.GenerateResponse<gatLogList>();

            using (var context = _contextFactory.CreateDbContext())
            {
                var logEntries = _repository.GetAll(context);

                response.Command.gatLog = logEntries
                    .AsEnumerable()
                    .TakeLogs(command.Command.lastSequence, command.Command.totalEntries).Select(
                        GatEnumExtensions.GetGatLog).ToArray();
            }

            await Task.CompletedTask;
        }
    }
}