namespace Aristocrat.Monaco.G2S.Handlers.CommConfig
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.CommConfig;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Implementation of 'getCommChangeLog' command of 'CommConfig' G2S class.
    /// </summary>
    public class GetCommChangeLog : ICommandHandler<commConfig, getCommChangeLog>
    {
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IG2SEgm _egm;
        private readonly ICommChangeLogRepository _repository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetCommChangeLog" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="repository">Repository</param>
        /// <param name="contextFactory">Context factory.</param>
        public GetCommChangeLog(
            IG2SEgm egm,
            ICommChangeLogRepository repository,
            IMonacoContextFactory contextFactory)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<commConfig, getCommChangeLog> command)
        {
            return await Sanction.OwnerAndGuests<ICommConfigDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<commConfig, getCommChangeLog> command)
        {
            var response = command.GenerateResponse<commChangeLogList>();

            using (var context = _contextFactory.Create())
            {
                var logEntries = _repository.GetAll(context);

                response.Command.commChangeLog = logEntries
                    .AsEnumerable()
                    .TakeLogs(command.Command.lastSequence, command.Command.totalEntries)
                    .Select(
                        log =>
                        {
                            var changeLog = new commChangeLog
                            {
                                logSequence = log.Id,
                                configurationId = log.ConfigurationId,
                                transactionId = log.TransactionId,
                                deviceId = log.DeviceId,
                                applyCondition = log.ApplyCondition.ToG2SString(),
                                disableCondition = log.DisableCondition.ToG2SString(),
                                changeDateTime = log.ChangeDateTime,
                                changeException = (int)log.ChangeException,
                                restartAfter = log.RestartAfter,
                                changeStatus =
                                    (t_changeStatus)Enum.Parse(typeof(t_changeStatus), $"G2S_{log.ChangeStatus}", true),
                                startDateTimeSpecified = log.StartDateTime.HasValue
                            };

                            if (log.StartDateTime.HasValue)
                            {
                                changeLog.startDateTime = log.StartDateTime.Value;
                            }

                            changeLog.endDateTimeSpecified = log.EndDateTime.HasValue;
                            if (log.EndDateTime.HasValue)
                            {
                                changeLog.endDateTime = log.EndDateTime.Value;
                            }

                            var authorizeItems = log.AuthorizeItems?.Select(
                                a => new authorizeStatus
                                {
                                    hostId = a.HostId,
                                    authorizationState = (t_authorizationStates)Enum.Parse(
                                        typeof(t_authorizationStates),
                                        $"G2S_{a.AuthorizeStatus.ToString()}",
                                        true),
                                    timeoutDateSpecified = a.TimeoutDate.HasValue,
                                    timeoutDate = a.TimeoutDate ?? DateTime.MinValue
                                }).ToArray();

                            if (authorizeItems != null && authorizeItems.Length > 0)
                            {
                                changeLog.authorizeStatusList =
                                    new authorizeStatusList { authorizeStatus = authorizeItems.ToArray() };
                            }

                            return changeLog;
                        }).ToArray();
            }

            await Task.CompletedTask;
        }
    }
}