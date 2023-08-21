namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Common.Events;
    using Data.Model;
    using Data.OptionConfig;
    using ExpressMapper;
    using Handlers;
    using Handlers.OptionConfig;
    using Kernel;
    using Monaco.Common.Scheduler;
    using Monaco.Common.Storage;
    using Constants = Aristocrat.G2S.Client.Constants;

    /// <summary>
    ///     Service for applying changes for optionConfig.
    /// </summary>
    public class OptionConfigurationService : BaseConfigurationService
    {
        private readonly IApplyOptionConfigService _applyOptionConfigService;
        private readonly IEnumerable<IDeviceOptionsBuilder> _builders;
        private readonly IOptionChangeLogRepository _changeLogRepository;
        private readonly ICommandBuilder<IOptionConfigDevice, optionChangeStatus> _statusBuilder;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionConfigurationService" /> class.
        /// </summary>
        /// <param name="egm">The egm.</param>
        /// <param name="configurationMode">The configuration mode.</param>
        /// <param name="bus">The bus.</param>
        /// <param name="statusBuilder">The status builder.</param>
        /// <param name="applyOptionConfigService">The apply option configuration service.</param>
        /// <param name="contextFactory">The context factory.</param>
        /// <param name="changeLogRepository">The change log repository.</param>
        /// <param name="taskScheduler">The task scheduler.</param>
        /// <param name="builders">A list of option config builders</param>
        /// <param name="eventLift">The event lift.</param>
        public OptionConfigurationService(
            IG2SEgm egm,
            IDisableConditionSaga configurationMode,
            IEventBus bus,
            ICommandBuilder<IOptionConfigDevice, optionChangeStatus> statusBuilder,
            IApplyOptionConfigService applyOptionConfigService,
            IMonacoContextFactory contextFactory,
            IOptionChangeLogRepository changeLogRepository,
            ITaskScheduler taskScheduler,
            IEnumerable<IDeviceOptionsBuilder> builders,
            IEventLift eventLift)
            : base(egm, configurationMode, bus, contextFactory, taskScheduler, eventLift)
        {
            _changeLogRepository = changeLogRepository ?? throw new ArgumentNullException(nameof(changeLogRepository));
            _statusBuilder = statusBuilder ?? throw new ArgumentNullException(nameof(statusBuilder));
            _applyOptionConfigService = applyOptionConfigService ??
                                        throw new ArgumentNullException(nameof(applyOptionConfigService));
            _builders = builders ?? throw new ArgumentNullException(nameof(builders));
        }

        /// <inheritdoc />
        protected override string ConfigurationAuthorizedByHostEventCode => EventCode.G2S_OCE104;

        /// <inheritdoc />
        protected override string EgmLockedByDeviceEventCode => EventCode.G2S_OCE007;

        /// <inheritdoc />
        protected override string EgmNotLockedByDeviceEventCode => EventCode.G2S_OCE008;

        /// <inheritdoc />
        protected override string ConfigurationChangesAppliedEventCode => EventCode.G2S_OCE106;

        /// <inheritdoc />
        protected override string DeviceConfigErrorEventCode => EventCode.G2S_OCE108;

        /// <inheritdoc />
        protected override string ConfigConfigurationCancelledEventCode => EventCode.G2S_OCE105;

        /// <inheritdoc />
        protected override string ConfigConfigurationAbortedEventCode => EventCode.G2S_OCE107;

        /// <inheritdoc />
        protected override string ConfigurationAuthorizedByHostTimeOutEventCode => EventCode.G2S_OCE110;

        /// <inheritdoc />
        protected override ConfigChangeLog GetChangeLog(DbContext context, long transactionId)
        {
            return _changeLogRepository.GetPendingByTransactionId(context, transactionId);
        }

        /// <inheritdoc />
        protected override void UpdateChangeLog(DbContext context, ConfigChangeLog changeLog)
        {
            _changeLogRepository.Update(context, changeLog as OptionChangeLog);
        }

        /// <inheritdoc />
        protected override void SendStatus(ConfigChangeLog log)
        {
            var device = Egm.GetDevice<IOptionConfigDevice>(log.DeviceId);

            var status = new optionChangeStatus
            {
                configurationId = log.ConfigurationId,
                transactionId = log.TransactionId
            };

            _statusBuilder.Build(device, status);

            device.OptionChangeStatus(status);
        }

        /// <inheritdoc />
        protected override ITaskSchedulerJob GetCheckValidityTask(long transactionId)
        {
            return CheckValidityTask.Create(transactionId);
        }

        /// <inheritdoc />
        protected override void SendStatusAsync(ConfigChangeLog log)
        {
            Task.Run(() => SendStatus(log));
        }

        /// <inheritdoc />
        protected override void ApplyCommand(DbContext context, ConfigChangeLog changeLog)
        {
            Logger.Debug($"Applying options configuration changes for transaction {changeLog.TransactionId}");

            try
            {
                var changeRequest = ((OptionChangeLog)changeLog).GetChangeRequest();

                _applyOptionConfigService.UpdateDeviceProfiles(changeRequest);

                var options = changeRequest.Options.ToList();

                var devices = options
                    .Select(option => Egm.GetDevice(option.DeviceClass.TrimmedDeviceClass(), option.DeviceId))
                    .Where(device => device != null)
                    .Distinct();

                foreach (var device in devices)
                {
                    Bus.Publish(new DeviceConfigurationChangedEvent(device));
                }

                var affectedDevices = options
                    .Select(
                        option => new
                        {
                            Device = Egm.GetDevice(option.DeviceClass.TrimmedDeviceClass(), option.DeviceId),
                            option.OptionGroupId,
                            option.OptionId
                        })
                    .Where(u => u.Device != null);

                var optionConfigDevice = Egm.GetDevice<IOptionConfigDevice>(changeLog.DeviceId);

                optionConfigDevice.OptionsChanged(
                    affectedDevices.Select(d => DeviceOptions(d.Device, d.OptionGroupId, d.OptionId)).ToArray());

                base.ApplyCommand(context, changeLog);

                var pendingChangeLog = _changeLogRepository.GetPendingChangeLogs(context).FirstOrDefault();

                if (pendingChangeLog != null)
                {
                    // Use the scheduler to run this in the background
                    // NOTE: Per the spec the start/end dates are only applicable when the apply condition is set to disable
                    TaskScheduler.ScheduleTask(
                        ApplyOptionConfigurationTask.Create(pendingChangeLog.Id, pendingChangeLog.TransactionId),
                        "SetOptionChange",
                        pendingChangeLog.ApplyCondition == ApplyCondition.Disable
                            ? pendingChangeLog.StartDateTime ?? DateTime.UtcNow
                            : DateTime.UtcNow);
                }
            }
            catch (Exception ex)
            {
                ProcessApplyError(context, changeLog, ex);
            }
        }

        /// <inheritdoc />
        protected override IDevice GetDevice(int deviceId)
        {
            return Egm.GetDevice<IOptionConfigDevice>(deviceId);
        }

        /// <inheritdoc />
        protected override long GetTransactionLog(ConfigChangeLog log, out transactionList transactionList)
        {
            var device = Egm.GetDevice<IOptionConfigDevice>(log.DeviceId);

            var optionChangeLog = log as OptionChangeLog;
            var changeLog = Mapper.Map<OptionChangeLog, optionChangeLog>(optionChangeLog);

            var authorizeItems = optionChangeLog?.AuthorizeItems?.Select(
                a => new authorizeStatus
                {
                    hostId = a.HostId,
                    authorizationState =
                        (t_authorizationStates)Enum.Parse(
                            typeof(t_authorizationStates),
                            $"{Constants.DefaultPrefix}{a.AuthorizeStatus.ToString()}",
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

            var info = new transactionInfo
            {
                deviceId = device.Id,
                deviceClass = device.PrefixedDeviceClass(),
                Item = changeLog
            };

            transactionList = new transactionList { transactionInfo = new[] { info } };
            return log.TransactionId;
        }

        private deviceOptions DeviceOptions(IDevice device, string optionGroupId, string optionId)
        {
            var builder =
                _builders.FirstOrDefault(b => b.Matches(device.PrefixedDeviceClass().DeviceClassFromG2SString()));
            if (builder == null)
            {
                // throw new InvalidOperationException($"Missing builder for device class {device.PrefixedDeviceClass()}");
                return new deviceOptions
                {
                    deviceClass = device.PrefixedDeviceClass(),
                    deviceId = device.Id
                };
            }

            var options = builder.Build(
                device,
                new OptionListCommandBuilderParameters
                {
                    DeviceClass = device.DeviceClass,
                    DeviceId = device.Id,
                    OptionGroupId = optionGroupId,
                    OptionId = optionId
                });

            return options;
        }
    }
}