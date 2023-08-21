namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.OptionConfig;
    using ExpressMapper;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Implementation of 'getOptionChangeLog' command of 'OptionConfig' G2S class.
    /// </summary>
    public class GetOptionChangeLog : ICommandHandler<optionConfig, getOptionChangeLog>
    {
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IG2SEgm _egm;

        private readonly IOptionChangeLogRepository _repository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetOptionChangeLog" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="repository">Repository</param>
        /// <param name="contextFactory">Context factory.</param>
        public GetOptionChangeLog(
            IG2SEgm egm,
            IOptionChangeLogRepository repository,
            IMonacoContextFactory contextFactory)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<optionConfig, getOptionChangeLog> command)
        {
            return await Sanction.OwnerAndGuests<IOptionConfigDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<optionConfig, getOptionChangeLog> command)
        {
            var response = command.GenerateResponse<optionChangeLogList>();

            using (var context = _contextFactory.CreateDbContext())
            {
                var logEntries = _repository.GetAll(context);

                response.Command.optionChangeLog = logEntries
                    .AsEnumerable()
                    .TakeLogs(command.Command.lastSequence, command.Command.totalEntries)
                    .Select(
                        log =>
                        {
                            var changeLog = Mapper.Map<OptionChangeLog, optionChangeLog>(log);

                            var authorizeItems = log.AuthorizeItems?.Select(
                                a => new authorizeStatus
                                {
                                    hostId = a.HostId,
                                    authorizationState =
                                        (t_authorizationStates)Enum.Parse(
                                            typeof(t_authorizationStates),
                                            $"G2S_{a.AuthorizeStatus.ToString()}",
                                            true),
                                    timeoutDateSpecified = a.TimeoutDate.HasValue,
                                    timeoutDate = a.TimeoutDate ?? DateTime.MinValue
                                }).ToArray();

                            if (authorizeItems != null && authorizeItems.Length > 0)
                            {
                                changeLog.authorizeStatusList = new authorizeStatusList
                                {
                                    authorizeStatus = authorizeItems.ToArray()
                                };
                            }

                            return changeLog;
                        }).ToArray();
            }

            await Task.CompletedTask;
        }
    }
}