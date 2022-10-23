namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.OptionConfig;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Implementation of 'getOptionChangeLogStatus' command of 'OptionConfig' G2S class.
    /// </summary>
    public class GetOptionChangeLogStatus : ICommandHandler<optionConfig, getOptionChangeLogStatus>
    {
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IG2SEgm _egm;
        private readonly IOptionChangeLogRepository _repository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetOptionChangeLogStatus" /> class using an egm and a optionConfig
        ///     status command builder.
        /// </summary>
        /// <param name="egm">An instance of an IG2SEgm.</param>
        /// <param name="repository">Repository</param>
        /// <param name="contextFactory">Context factory.</param>
        public GetOptionChangeLogStatus(
            IG2SEgm egm,
            IOptionChangeLogRepository repository,
            IMonacoContextFactory contextFactory)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<optionConfig, getOptionChangeLogStatus> command)
        {
            var response = command.GenerateResponse<optionChangeLogStatus>();

            using (var context = _contextFactory.CreateDbContext())
            {
                response.Command.totalEntries = _repository.Count(context);
                response.Command.lastSequence = response.Command.totalEntries > 0
                    ? _repository.GetMaxLastSequence<OptionChangeLog>(context)
                    : 0;
            }

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<optionConfig, getOptionChangeLogStatus> command)
        {
            return await Sanction.OwnerAndGuests<IOptionConfigDevice>(_egm, command);
        }
    }
}