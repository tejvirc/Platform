namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig
{
    using System;
    using System.Linq;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using ExpressMapper;
    using Monaco.Common.Scheduler;
    using Monaco.Common.Storage;
    using Services;

    /// <summary>
    ///     Implementation of task that checks execution status of <c>SetOptionChange</c> command handler.
    /// </summary>
    public class CheckOptionChangeStatus : ITaskSchedulerJob
    {
        private readonly IOptionChangeLogRepository _changeLogRepository;
        private readonly IConfigurationService _configuration;
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CheckOptionChangeStatus" /> class.
        /// </summary>
        /// <param name="changeLogRepository">The change log repository.</param>
        /// <param name="eventLift">The event lift.</param>
        /// <param name="egm">The egm.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="contextFactory">The context factory.</param>
        public CheckOptionChangeStatus(
            IOptionChangeLogRepository changeLogRepository,
            IEventLift eventLift,
            IG2SEgm egm,
            IConfigurationService configuration,
            IMonacoContextFactory contextFactory)
        {
            _changeLogRepository = changeLogRepository;
            _eventLift = eventLift;
            _egm = egm;
            _configuration = configuration;
            _contextFactory = contextFactory;
        }

        /// <summary>
        ///     Gets or sets the unique transaction identifier.
        /// </summary>
        public long TransactionId { get; set; }

        /// <summary>
        ///     Gets or sets current authorize entity unique identifier.
        /// </summary>
        public long AuthorizeItemId { get; set; }

        /// <inheritdoc />
        public void Execute(TaskSchedulerContext context)
        {
            CheckAndAbortChangeLogsIfRequired();
        }

        /// <inheritdoc />
        public string SerializeJobData()
        {
            return $"{TransactionId};{AuthorizeItemId}";
        }

        /// <inheritdoc />
        public void DeserializeJobData(string data)
        {
            if (string.IsNullOrEmpty(data) == false)
            {
                var parts = data.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                TransactionId = long.Parse(parts[0]);
                AuthorizeItemId = long.Parse(parts[1]);
            }
        }

        /// <summary>
        ///     Create a new instance of the <see cref="CheckOptionChangeStatus" /> class.
        /// </summary>
        /// <param name="transactionId">Transaction Id.</param>
        /// <param name="authorizeItemId">Authorize id.</param>
        /// <returns>Returns instance of <c>CheckOptionChangeStatus</c>.</returns>
        public static CheckOptionChangeStatus Create(long transactionId, long authorizeItemId)
        {
            return new CheckOptionChangeStatus(null, null, null, null, null)
            {
                TransactionId = transactionId,
                AuthorizeItemId = authorizeItemId
            };
        }

        private void CheckAndAbortChangeLogsIfRequired()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var currentTime = DateTime.UtcNow;
                var pendingChangeLog = _changeLogRepository.GetByTransactionId(context, TransactionId);

                if (pendingChangeLog == null)
                {
                    return;
                }

                var authorizeItem = pendingChangeLog.AuthorizeItems.FirstOrDefault(x => x.Id == AuthorizeItemId);

                if (authorizeItem?.TimeoutDate != null && currentTime.CompareTo(authorizeItem.TimeoutDate.Value) > 0 &&
                    pendingChangeLog.ChangeStatus != ChangeStatus.Authorized)
                {
                    _configuration.Abort(pendingChangeLog.TransactionId, ChangeExceptionErrorCode.Timeout);

                    var device = _egm.GetDevice<IOptionConfigDevice>(pendingChangeLog.DeviceId);

                    var changeLog = Mapper.Map<OptionChangeLog, optionChangeLog>(pendingChangeLog);

                    var authorizeItems = pendingChangeLog.AuthorizeItems?.Select(
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

                    if (authorizeItems.Length > 0)
                    {
                        changeLog.authorizeStatusList = new authorizeStatusList
                        {
                            authorizeStatus = authorizeItems.ToArray()
                        };
                    }

                    var info = new transactionInfo
                    {
                        deviceId = device.Id,
                        deviceClass = device.PrefixedDeviceClass(),
                        Item = changeLog
                    };

                    var transactionList = new transactionList { transactionInfo = new[] { info } };

                    _eventLift.Report(device, EventCode.G2S_OCE107, pendingChangeLog.TransactionId, transactionList);
                }
            }
        }
    }
}