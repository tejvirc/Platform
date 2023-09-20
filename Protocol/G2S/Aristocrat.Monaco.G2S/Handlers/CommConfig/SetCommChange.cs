namespace Aristocrat.Monaco.G2S.Handlers.CommConfig
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.CommConfig;
    using Data.Model;
    using ExpressMapper;
    using Monaco.Common.Scheduler;
    using Monaco.Common.Storage;
    using Newtonsoft.Json;
    using Services;
    using Constants = Aristocrat.G2S.Client.Constants;

    /// <summary>
    ///     Implementation of 'setCommChange' command of 'CommConfig' G2S class.
    /// </summary>
    public class SetCommChange : ICommandHandler<commConfig, setCommChange>
    {
        private readonly ICommChangeLogRepository _changeLogRepository;
        private readonly ICommandBuilder<ICommConfigDevice, commChangeStatus> _commandBuilder;
        private readonly IConfigurationService _configuration;
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        private readonly string[] _hostOrientedDevices = { "G2S_communications", "G2S_eventHandler","G2S_meters", "G2S_commConfig", "G2S_commConfig" };
        private readonly IIdProvider _idProvider;
        private readonly string[] _requiredDevices = { "G2S_communications", "G2S_eventHandler" };
        private readonly ITaskScheduler _taskScheduler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetCommChange" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="commandBuilder">The command builder.</param>
        /// <param name="contextFactory">DB context factory.</param>
        /// <param name="changeLogRepository">ChangeLog repository</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance.</param>
        /// <param name="taskScheduler">Task scheduler instance.</param>
        /// <param name="configuration">An <see cref="IConfigurationService" /> instance.</param>
        /// <param name="idProvider">An <see cref="IIdProvider" /> instance.</param>
        public SetCommChange(
            IG2SEgm egm,
            ICommandBuilder<ICommConfigDevice, commChangeStatus> commandBuilder,
            IMonacoContextFactory contextFactory,
            ICommChangeLogRepository changeLogRepository,
            IEventLift eventLift,
            ITaskScheduler taskScheduler,
            IConfigurationService configuration,
            IIdProvider idProvider)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _changeLogRepository = changeLogRepository ?? throw new ArgumentNullException(nameof(changeLogRepository));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _taskScheduler = taskScheduler ?? throw new ArgumentNullException(nameof(taskScheduler));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<commConfig, setCommChange> command)
        {
            var error = await Sanction.OnlyOwner<ICommConfigDevice>(_egm, command);
            if (error != null)
            {
                return error;
            }

            if (command.Command.applyCondition.Equals(ApplyCondition.Disable.ToG2SString()) &&
                command.Command.disableCondition.Equals(DisableCondition.None.ToG2SString()))
            {
                return new Error(ErrorCode.G2S_CCX009);
            }

            using (var context = _contextFactory.CreateDbContext())
            {
                if (IsDuplicateCommand(context, command.Command))
                {
                    return new Error(ErrorCode.G2S_CCX018);
                }
            }

            if (!HasRegisteredHost(command.Command))
            {
                return new Error(ErrorCode.G2S_CCX018);
            }

            if (IsChangingRegisteredHostId(command.Command))
            {
                return new Error(ErrorCode.G2S_CCX012);
            }

            if (!HasRequiredClasses(command.Command))
            {
                return new Error(ErrorCode.G2S_CCX018);
            }

            if (!HasValidDeviceIdentifiers(command.Command))
            {
                return new Error(ErrorCode.G2S_CCX018);
            }

            if (IsRemovingAllDevices(command.Command))
            {
                return new Error(ErrorCode.G2S_CCX018);
            }

            if (!HasValidHostIndex(command.Command))
            {
                return new Error(ErrorCode.G2S_CCX016);
            }

            return await Task.FromResult<Error>(null);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<commConfig, setCommChange> command)
        {
            if (command.Class.sessionType != t_sessionTypes.G2S_request)
            {
                return;
            }

            var device = _egm.GetDevice<ICommConfigDevice>(command.IClass.deviceId);

            var commChangeLog = AddNewChangeLog(device.Id, command.Command, command.IClass.dateTime);

            var response = command.GenerateResponse<commChangeStatus>();
            response.Command.configurationId = commChangeLog.ConfigurationId;
            response.Command.transactionId = commChangeLog.TransactionId;
            await _commandBuilder.Build(device, response.Command);

            FireEventReport(device, commChangeLog);

            if (commChangeLog.ApplyCondition != ApplyCondition.Cancel)
            {
                // Use the scheduler to run this in the background
                // NOTE: Per the spec the start/end dates are only applicable when the apply condition is set to disable
                _taskScheduler.ScheduleTask(
                    ApplyCommConfigurationTask.Create(commChangeLog.Id, commChangeLog.TransactionId),
                    "SetCommChange",
                    commChangeLog.ApplyCondition == ApplyCondition.Disable
                        ? (commChangeLog.StartDateTime ?? DateTimeOffset.UtcNow).UtcDateTime
                        : DateTime.UtcNow);
            }
        }

        private void FireEventReport(ICommConfigDevice device, CommChangeLog commChangeLog)
        {
            var changeLog = Mapper.Map<CommChangeLog, commChangeLog>(commChangeLog);

            var authorizeItems = commChangeLog.AuthorizeItems?.Select(
                a => new authorizeStatus
                {
                    hostId = a.HostId,
                    authorizationState = (t_authorizationStates)Enum.Parse(typeof(t_authorizationStates),$"G2S_{a.AuthorizeStatus.ToString()}",true),
                    timeoutDateSpecified = a.TimeoutDate.HasValue,
                    timeoutDate = (a.TimeoutDate ?? DateTime.MinValue).UtcDateTime
                }).ToArray();

            if (authorizeItems is { Length: > 0 })
            {
                changeLog.authorizeStatusList = new authorizeStatusList
                {
                    authorizeStatus = authorizeItems.ToArray()
                };
            }

            var transList = new transactionList { transactionInfo = new[] {  new transactionInfo
            {
                deviceId = device.Id,
                deviceClass = device.PrefixedDeviceClass(),
                Item = changeLog} }
            };

            if (commChangeLog.ApplyCondition == ApplyCondition.Cancel)
            {
                _eventLift.Report(device, EventCode.G2S_CCE105, commChangeLog.TransactionId, transList);
                return;
            }

            if (authorizeItems != null && authorizeItems.Length > 0)
            {
                _eventLift.Report(device, EventCode.G2S_CCE109, commChangeLog.TransactionId, transList);
            }

            _eventLift.Report(device, EventCode.G2S_CCE103, commChangeLog.TransactionId, transList);
        }

        private static bool HasRegisteredHost(c_commHostConfigList command)
        {
            // The EGM MUST always have at least one registered host.
            // The EGM itself is considered a registered host; therefore,
            // to support commConfig, the EGM must support at least 2 (two) registered hosts
            return command.setHostItem?.Count(h => h.hostRegistered) >= 2;
        }

        private bool HasRequiredClasses(c_commHostConfigList command)
        {
            foreach (var item in command.setHostItem.Where(h => h.hostRegistered && h.hostIndex != Constants.EgmHostIndex))
            {
                if (!item.ownedDevice1?.Any(x => _requiredDevices.Contains(x.deviceClass)) ?? false)
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasValidDeviceIdentifiers(c_commHostConfigList command)
        {
            // The device identifiers for devices within the communications, eventHandler, meters, optionConfig,
            // and gat classes MUST be equal to the host identifier of the host that owns the device
            // If the deviceId of a host-oriented device is not set equal to the hostId of the host being registered,
            // the EGM MUST NOT execute the command and MUST include error code G2S_CCX018 Invalid
            // Change Request in its response.
            foreach (var item in command.setHostItem.Where(h => h.hostRegistered && h.hostIndex != Constants.EgmHostIndex))
            {
                if (item.ownedDevice1?.Any(x => _hostOrientedDevices.Contains(x.deviceClass) && x.deviceId != item.hostId) ?? false)
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasValidHostIndex(c_commHostConfigList command)
        {
            if (command.authorizeList != null)
            {
                return true;
            }

            // The EGM will only permit setCommChange commands that manipulate the settings of hosts reported by the
            // EGM in the commHostList command.If a host attempts to create or manipulate a new host index(by
            // specifying a hostIndex that was not reported by the EGM), then an error code G2S_CCX016 Invalid
            // hostIndex Specified must be returned by the EGM.
            return command.setHostItem.All(item => item.hostIndex < G2S.Constants.MaxHosts);
        }

        private bool IsDuplicateCommand(DbContext context, setCommChange command)
        {
            // The EGM MUST NOT consider a setCommChange command logically equivalent to any previous
            // setCommChange command. Each setCommChange command MUST be processed as if it were a unique command.
            // Two or more setCommChange commands containing the same configurationId may not be logically equivalent
            var changeLog = _changeLogRepository.Get(context, c => c.ConfigurationId == command.configurationId)
                .Include(c => c.AuthorizeItems)
                .OrderByDescending(c => c.TransactionId)
                .FirstOrDefault();
            if (changeLog == null)
            {
                return false;
            }

            var incomingRequest = Mapper.Map<setCommChange, ChangeCommConfigRequest>(command);

            return incomingRequest.Equals(changeLog.GetChangeRequest());
        }

        private bool IsRemovingAllDevices(c_commHostConfigList command)
        {
            // If a setCommChange command indicates that all remaining devices within the class are to be removed,
            // the EGM MUST NOT execute the command and MUST include error code G2S_CCX018
            // Invalid Change Request in its response.

            // TODO: The verbiage in the spec is unclear
            return false;
        }

        private bool IsChangingRegisteredHostId(c_commHostConfigList command)
        {
            // If the host attempts to change the hostId of a commHostItem to a hostId that is already
            // assigned to a registered host, then the EGM SHOULD respond with G2S_CCX012 Invalid Host Identifier.
            foreach (var setHostItem in command.setHostItem)
            {
                var host = _egm.Hosts.FirstOrDefault(x => x.Index == setHostItem.hostIndex);
                if (host != null && host.Id != setHostItem.hostId && host.Registered && setHostItem.hostRegistered)
                {
                    return true;
                }
            }

            return false;
        }

        private CommChangeLog AddNewChangeLog(int deviceId, setCommChange command, DateTime dateTime)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                AbortExistingPendingChangeLogs(context);

                var changeLog = Mapper.Map<setCommChange, CommChangeLog>(command);

                changeLog.DeviceId = deviceId;
                changeLog.TransactionId = _idProvider.GetNextTransactionId();
                changeLog.ChangeDateTime = DateTime.UtcNow;
                changeLog.ChangeStatus = changeLog.ApplyCondition == ApplyCondition.Cancel ? ChangeStatus.Cancelled : ChangeStatus.Pending;
                changeLog.ChangeException = ChangeExceptionErrorCode.Successful;

                var changeCommConfigRequest = Mapper.Map<setCommChange, ChangeCommConfigRequest>(command);
                changeLog.ChangeData = JsonConvert.SerializeObject(changeCommConfigRequest);

                if (command.authorizeList?.authorizeItem != null)
                {
                    var authorizeItems = Mapper.Map<authorizeItem[], ConfigChangeAuthorizeItem[]>(command.authorizeList.authorizeItem).ToList();

                    // The timeout MUST be within 24 hours of the dateTime specified within the class level element
                    authorizeItems.ForEach(a => a.TimeoutDate = a.TimeoutDate ?? dateTime);

                    changeLog.AuthorizeItems = authorizeItems;
                }

                _changeLogRepository.Add(context, changeLog);

                return changeLog;
            }
        }

        private void AbortExistingPendingChangeLogs(DbContext context)
        {
            var pendingChangeLogs = _changeLogRepository.GetPending(context);

            foreach (var log in pendingChangeLogs)
            {
                _configuration.Abort(log.TransactionId, ChangeExceptionErrorCode.Expired);
            }
        }
    }
}