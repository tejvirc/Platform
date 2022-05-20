namespace Aristocrat.Monaco.G2S.Handlers.CommConfig
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.CommConfig;
    using Monaco.Common.Storage;

    /// <summary>
    ///     An implementation of ICommandBuilder&lt;ICabinet, cabinetStatus&gt;.
    /// </summary>
    public class CommChangeStatusCommandBuilder : ICommandBuilder<ICommConfigDevice, commChangeStatus>
    {
        private readonly ICommChangeLogRepository _changeLogRepository;

        private readonly IMonacoContextFactory _contextFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommChangeStatusCommandBuilder" /> class.
        /// </summary>
        /// <param name="changeLogRepository">The change log repository.</param>
        /// <param name="contextFactory">The context factory.</param>
        public CommChangeStatusCommandBuilder(
            ICommChangeLogRepository changeLogRepository,
            IMonacoContextFactory contextFactory)
        {
            _changeLogRepository = changeLogRepository ?? throw new ArgumentNullException(nameof(changeLogRepository));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <inheritdoc />
        public async Task Build(ICommConfigDevice device, commChangeStatus command)
        {
            using (var context = _contextFactory.Create())
            {
                var log = _changeLogRepository.GetByTransactionId(context, command.transactionId);

                command.configurationId = log.ConfigurationId;
                command.transactionId = log.TransactionId;
                command.applyCondition = log.ApplyCondition.ToG2SString();
                command.disableCondition = log.DisableCondition.ToG2SString();
                command.startDateTimeSpecified = log.StartDateTime.HasValue;
                if (log.StartDateTime.HasValue)
                {
                    command.startDateTime = log.StartDateTime.Value;
                }

                command.endDateTimeSpecified = log.EndDateTime.HasValue;
                if (log.EndDateTime.HasValue)
                {
                    command.endDateTime = log.EndDateTime.Value;
                }

                command.restartAfter = log.RestartAfter;
                command.changeStatus =
                    (t_changeStatus)Enum.Parse(typeof(t_changeStatus), $"G2S_{log.ChangeStatus}", true);

                command.changeDateTime = log.ChangeDateTime;
                command.changeException = (int)log.ChangeException;
                command.listStateDateTimeSpecified = false;

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
                    command.authorizeStatusList =
                        new authorizeStatusList { authorizeStatus = authorizeItems.ToArray() };
                }
            }

            await Task.CompletedTask;
        }
    }
}