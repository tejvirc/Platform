namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using Data.OptionConfig.ChangeOptionConfig;
    using ExpressMapper;
    using Monaco.Common.Scheduler;
    using Monaco.Common.Storage;
    using Newtonsoft.Json;

    /// <summary>
    ///     Implementation of 'setOptionChange' command of 'OptionConfig' G2S class.
    /// </summary>
    public class SetOptionChange : ICommandHandler<optionConfig, setOptionChange>
    {
        private readonly IOptionChangeLogRepository _changeLogRepository;
        private readonly ICommandBuilder<IOptionConfigDevice, optionChangeStatus> _commandBuilder;
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IIdProvider _idProvider;
        private readonly ITaskScheduler _taskScheduler;
        private readonly ISetOptionChangeValidateService _validationService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetOptionChange" /> class.
        ///     Creates a new instance of the SetOptionChange handler
        /// </summary>
        /// <param name="egm">A G2S egm.</param>
        /// <param name="commandBuilder">The command builder.</param>
        /// <param name="changeLogRepository">The change log repository.</param>
        /// <param name="taskScheduler">Task scheduler instance.</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance.</param>
        /// <param name="contextFactory">The context factory.</param>
        /// <param name="validationService">Validator instance.</param>
        /// <param name="idProvider">An <see cref="IIdProvider" /> instance.</param>
        public SetOptionChange(
            IG2SEgm egm,
            ICommandBuilder<IOptionConfigDevice, optionChangeStatus> commandBuilder,
            IOptionChangeLogRepository changeLogRepository,
            ITaskScheduler taskScheduler,
            IEventLift eventLift,
            IMonacoContextFactory contextFactory,
            ISetOptionChangeValidateService validationService,
            IIdProvider idProvider)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _changeLogRepository = changeLogRepository ?? throw new ArgumentNullException(nameof(changeLogRepository));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _taskScheduler = taskScheduler ?? throw new ArgumentNullException(nameof(taskScheduler));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<optionConfig, setOptionChange> command)
        {
            // Only active devices may be configured using the setOptionChange command.
            if (!CheckActiveDevices(command.Command))
            {
                return new Error(ErrorCode.G2S_OCX017);
            }

            // If a host has not been assigned as the configurator of a device, the host must not attempt to make option
            // changes to the device.
            // If a setOptionChange command
            // contains a device where the host is not the configurator of the device, the EGM MUST abort the command
            // and return an appropriate error code in its response, such as error code G2S_OCX018 Command Restricted to
            // Configurator.
            if (!CheckIfConfiguratorOfDevices(command.Command, command.HostId))
            {
                return new Error(ErrorCode.G2S_OCX018);
            }

            var result = _validationService.Verify(command);
            if (!result.IsValid)
            {
                // TODO: log validation errors
                // return new Error(ErrorCode.G2S_OCX017);
            }

            if (command.Command.applyCondition == ApplyCondition.Disable.ToG2SString() &&
                command.Command.disableCondition == DisableCondition.None.ToG2SString())
            {
                return new Error(ErrorCode.G2S_OCX009);
            }

            return await Sanction.OnlyOwner<IOptionConfigDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<optionConfig, setOptionChange> command)
        {
            if (command.Class.sessionType != t_sessionTypes.G2S_request)
            {
                return;
            }

            var device = _egm.GetDevice<IOptionConfigDevice>(command.IClass.deviceId);

            var optionChangeLog = AddNewChangeLog(device.Id, command.Command);

            var response = command.GenerateResponse<optionChangeStatus>();
            response.Command.configurationId = optionChangeLog.ConfigurationId;
            response.Command.transactionId = optionChangeLog.TransactionId;
            await _commandBuilder.Build(device, response.Command);

            FireEventReport(device, optionChangeLog);

            if (optionChangeLog.ApplyCondition != ApplyCondition.Cancel)
            {
                // Use the scheduler to run this in the background
                // NOTE: Per the spec the start/end dates are only applicable when the apply condition is set to disable
                _taskScheduler.ScheduleTask(
                    ApplyOptionConfigurationTask.Create(optionChangeLog.Id, optionChangeLog.TransactionId),
                    "SetOptionChange",
                    optionChangeLog.ApplyCondition == ApplyCondition.Disable
                        ? optionChangeLog.StartDateTime ?? DateTime.UtcNow
                        : DateTime.UtcNow);
            }
        }

        private void FireEventReport(IOptionConfigDevice device, OptionChangeLog optionChangeLog)
        {
            var changeLog = Mapper.Map<OptionChangeLog, optionChangeLog>(optionChangeLog);

            var authorizeItems = optionChangeLog.AuthorizeItems?.Select(
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

            var transList = new transactionList
            {
                transactionInfo = new[]
                {
                    new transactionInfo
                    {
                        deviceId = device.Id,
                        deviceClass = device.PrefixedDeviceClass(),
                        Item = changeLog
                    }
                }
            };

            _eventLift.Report(device, EventCode.G2S_OCE103, optionChangeLog.TransactionId, transList);

            if (optionChangeLog.ApplyCondition == ApplyCondition.Cancel)
            {
                _eventLift.Report(device, EventCode.G2S_OCE105, optionChangeLog.TransactionId, transList);
                return;
            }

            if (authorizeItems != null && authorizeItems.Length > 0)
            {
                _eventLift.Report(device, EventCode.G2S_OCE109, optionChangeLog.TransactionId, transList);
            }
        }

        private OptionChangeLog AddNewChangeLog(int deviceId, setOptionChange command)
        {
            using (var context = _contextFactory.Create())
            {
                var optionChangeLog = Mapper.Map<setOptionChange, OptionChangeLog>(command);
                optionChangeLog.DeviceId = deviceId;
                optionChangeLog.TransactionId = _idProvider.GetNextTransactionId();
                optionChangeLog.ConfigurationId = command.configurationId;
                optionChangeLog.ChangeStatus = ChangeStatus.Pending;
                optionChangeLog.ChangeException = ChangeExceptionErrorCode.Successful;
                optionChangeLog.ChangeDateTime = DateTime.UtcNow;

                var changeOptionConfigRequest = Mapper.Map<setOptionChange, ChangeOptionConfigRequest>(command);
                optionChangeLog.ChangeData = JsonConvert.SerializeObject(changeOptionConfigRequest);

                if (command.authorizeList?.authorizeItem != null)
                {
                    var authorizeItems =
                        Mapper.Map<authorizeItem[], ConfigChangeAuthorizeItem[]>(
                                command.authorizeList.authorizeItem)
                            .ToList();
                    optionChangeLog.AuthorizeItems = authorizeItems;
                }

                _changeLogRepository.Add(context, optionChangeLog);

                return optionChangeLog;
            }
        }

        private bool CheckActiveDevices(c_optionChangeList command)
        {
            return command.option
                .Select(option => _egm.GetDevice(option.deviceClass.TrimmedDeviceClass(), option.deviceId))
                .All(device => device != null && device.Active);
        }

        private bool CheckIfConfiguratorOfDevices(c_optionChangeList command, int hostId)
        {
            return command.option
                .Select(option => _egm.GetDevice(option.deviceClass.TrimmedDeviceClass(), option.deviceId))
                .All(device => device != null && (device.IsConfigurator(hostId) || device.IsOwner(hostId)));
        }

        private IEnumerable<KeyValuePair<string, bool>> GetParameterIdWithCanModRemoteAttributeFromCommonType(
            OptionConfigParameter optionConfigParameter)
        {
            var parametersIdWithCanModRemoteAttribute = new List<KeyValuePair<string, bool>>();

            if (optionConfigParameter.ParameterType != OptionConfigParameterType.Complex)
            {
                parametersIdWithCanModRemoteAttribute.Add(
                    new KeyValuePair<string, bool>(
                        optionConfigParameter.ParameterId,
                        optionConfigParameter.CanModLocal));
            }
            else if (optionConfigParameter.ParameterType == OptionConfigParameterType.Complex)
            {
                parametersIdWithCanModRemoteAttribute.AddRange(
                    GetParameterIdWithCanModRemoteAttributeFromComplexType(optionConfigParameter));
            }

            return parametersIdWithCanModRemoteAttribute;
        }

        private IEnumerable<KeyValuePair<string, bool>> GetParameterIdWithCanModRemoteAttributeFromComplexType(
            OptionConfigParameter optionConfigParameter)
        {
            var parametersIdWithCanModRemoteAttribute = new List<KeyValuePair<string, bool>>();

            if (optionConfigParameter is OptionConfigComplexParameter complexParameter)
            {
                foreach (var item in complexParameter.Items)
                {
                    parametersIdWithCanModRemoteAttribute.AddRange(
                        GetParameterIdWithCanModRemoteAttributeFromCommonType(item));
                }
            }

            return parametersIdWithCanModRemoteAttribute;
        }

        private IEnumerable<string> GetParameterIdFromG2SType(object item)
        {
            var parameterIds = new List<string>();
            switch (item)
            {
                case c_integerValue integerValue:
                    parameterIds.Add(integerValue.paramId);
                    break;
                case c_decimalValue decimalValue:
                    parameterIds.Add(decimalValue.paramId);
                    break;
                case c_stringValue stringValue:
                    parameterIds.Add(stringValue.paramId);
                    break;
                case c_booleanValue booleanValue:
                    parameterIds.Add(booleanValue.paramId);
                    break;
                case c_complexValue complexValue:
                    parameterIds.AddRange(GetParameterIdsFromComplexType(complexValue.Items));
                    break;
            }

            return parameterIds;
        }

        private IEnumerable<string> GetParameterIdsFromComplexType(object[] items)
        {
            var parameterIds = new List<string>();
            foreach (var item in items)
            {
                parameterIds.AddRange(GetParameterIdFromG2SType(item));
            }

            return parameterIds;
        }
    }
}