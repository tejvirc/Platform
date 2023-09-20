namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig
{
    using System;
    using System.Linq;
    using Data.Model;
    using Data.OptionConfig;
    using Monaco.Common.Scheduler;
    using Monaco.Common.Storage;
    using Services;

    /// <summary>
    ///     Task used to check the status of the communication changes
    /// </summary>
    public class CheckValidityTask : ITaskSchedulerJob
    {
        private readonly IOptionChangeLogRepository _changeLogRepository;
        private readonly IConfigurationService _configuration;
        private readonly IMonacoContextFactory _contextFactory;
        private readonly ITaskScheduler _taskScheduler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CheckValidityTask" /> class.
        /// </summary>
        /// <param name="changeLogRepository">The change log repository.</param>
        /// <param name="contextFactory">The context factory.</param>
        /// <param name="configuration">An <see cref="IConfigurationService" /> instance.</param>
        /// <param name="taskScheduler">An <see cref="ITaskScheduler" /> instance.</param>
        public CheckValidityTask(
            IOptionChangeLogRepository changeLogRepository,
            IMonacoContextFactory contextFactory,
            IConfigurationService configuration,
            ITaskScheduler taskScheduler)
        {
            _changeLogRepository = changeLogRepository;
            _contextFactory = contextFactory;
            _configuration = configuration;
            _taskScheduler = taskScheduler;
        }

        /// <summary>
        ///     Gets or sets the transaction identifier for the task
        /// </summary>
        public long TransactionId { get; set; }

        /// <inheritdoc />
        public void Execute(TaskSchedulerContext context)
        {
            using (var dbContext = _contextFactory.CreateDbContext())
            {
                var log = _changeLogRepository.GetPendingByTransactionId(dbContext, TransactionId);
                if (log == null)
                {
                    return;
                }

                if (log.EndDateTime < DateTime.UtcNow)
                {
                    _configuration.Abort(TransactionId, ChangeExceptionErrorCode.Expired);
                    return;
                }

                var pending = log.AuthorizeItems.Where(
                    a => a.AuthorizeStatus == AuthorizationState.Pending && a.TimeoutDate < DateTime.UtcNow);

                foreach (var item in pending)
                {
                    if (item.TimeoutAction == TimeoutActionType.Abort)
                    {
                        _configuration.Abort(TransactionId, ChangeExceptionErrorCode.Timeout);
                        return;
                    }

                    _configuration.Authorize(TransactionId, item.HostId, true);
                }

                ScheduleTimeout(log);
            }
        }

        /// <inheritdoc />
        public string SerializeJobData()
        {
            return $"{TransactionId}";
        }

        /// <inheritdoc />
        public void DeserializeJobData(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                var parts = data.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                TransactionId = long.Parse(parts[0]);
            }
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="CheckValidityTask" /> class.
        /// </summary>
        /// <param name="transactionId">The transaction id.</param>
        /// <returns>Returns instance of <c>CheckCommChangeStatus</c>.</returns>
        public static CheckValidityTask Create(long transactionId)
        {
            return new CheckValidityTask(null, null, null, null)
            {
                TransactionId = transactionId
            };
        }

        private void ScheduleTimeout(ConfigChangeLog log)
        {
            var timeout = log.EndDateTime;

            if (log.AuthorizeItems != null && log.AuthorizeItems.Count > 0)
            {
                var firstTimeout = log.AuthorizeItems.Where(a => a.AuthorizeStatus == AuthorizationState.Pending)
                    .Min(i => i.TimeoutDate);

                if (firstTimeout != null && log.EndDateTime != null)
                {
                    timeout = firstTimeout.Value < log.EndDateTime.Value ? firstTimeout.Value : log.EndDateTime.Value;
                }
                else if (firstTimeout != null)
                {
                    timeout = firstTimeout.Value;
                }
            }

            if (timeout.HasValue)
            {
                _taskScheduler.ScheduleTask(
                    Create(log.TransactionId),
                    "CheckCommChangeExpiration",
                    timeout.Value.UtcDateTime);
            }
        }
    }
}