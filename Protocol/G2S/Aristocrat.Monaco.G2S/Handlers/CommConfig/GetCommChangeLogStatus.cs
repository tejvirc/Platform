namespace Aristocrat.Monaco.G2S.Handlers.CommConfig
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.CommConfig;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Implementation of 'getCommChangeLogStatus' command of 'CommConfig' G2S class.
    /// </summary>
    public class GetCommChangeLogStatus : ICommandHandler<commConfig, getCommChangeLogStatus>
    {
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IG2SEgm _egm;
        private readonly ICommChangeLogRepository _repository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetCommChangeLogStatus" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="repository">Repository</param>
        /// <param name="contextFactory">Context factory.</param>
        public GetCommChangeLogStatus(
            IG2SEgm egm,
            ICommChangeLogRepository repository,
            IMonacoContextFactory contextFactory)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<commConfig, getCommChangeLogStatus> command)
        {
            return await Sanction.OwnerAndGuests<ICommConfigDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<commConfig, getCommChangeLogStatus> command)
        {
            var response = command.GenerateResponse<commChangeLogStatus>();

            using (var context = _contextFactory.CreateDbContext())
            {
                response.Command.totalEntries = _repository.Count(context);
                response.Command.lastSequence = _repository.GetMaxLastSequence<CommChangeLog>(context);
            }

            await Task.CompletedTask;
        }
    }
}