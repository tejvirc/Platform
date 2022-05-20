namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.PackageManager;
    using Common.PackageManager.Storage;
    using Data.Model;
    using ExpressMapper;
    using Kernel;

    /// <summary>
    ///     Handles the v21.setScript G2S message
    /// </summary>
    [ProhibitWhenDisabled]
    public class SetScript : ICommandHandler<download, setScript>
    {
        private readonly ICommandBuilder<IDownloadDevice, scriptStatus> _command;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IIdProvider _idProvider;
        private readonly IPropertiesManager _properties;
        private readonly ICommandBuilder<IDownloadDevice, scriptLog> _logCommand;
        private readonly IPackageManager _packageManager;
        private readonly IScriptManager _scriptManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetScript" /> class.
        /// </summary>
        public SetScript(
            IG2SEgm egm,
            IPackageManager packageManager,
            IScriptManager scriptManager,
            IEventLift eventLift,
            IIdProvider idProvider,
            IPropertiesManager properties,
            ICommandBuilder<IDownloadDevice, scriptStatus> command,
            ICommandBuilder<IDownloadDevice, scriptLog> logCommand)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
            _scriptManager = scriptManager ?? throw new ArgumentNullException(nameof(scriptManager));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _logCommand = logCommand ?? throw new ArgumentNullException(nameof(logCommand));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, setScript> command)
        {
            var error = await Sanction.OnlyOwner<IDownloadDevice>(_egm, command);
            if (error != null)
            {
                return error;
            }

            var device = _egm.GetDevice<IDownloadDevice>(command.IClass.deviceId);
            if (!device.DownloadEnabled && command.Command.commandList.Items.Any(c => c is packageCmd || c is moduleCmd))
            {
                return new Error(ErrorCode.G2S_APX008);
            }

            return null;
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<download, setScript> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var dl = _egm.GetDevice<IDownloadDevice>(command.IClass.deviceId);
                if (dl == null)
                {
                    return;
                }

                // TODO: should this be a configuration error
                if (!dl.ScriptingEnabled)
                {
                    command.Error.Code = ErrorCode.G2S_APX008;
                    return;
                }

                var se = _packageManager.GetScript(command.Command.scriptId);

                if (se != null)
                {
                    command.Error.Code = ErrorCode.G2S_DLX006;
                    return;
                }

                if (command.Command.disableCondition == "G2S_none" && command.Command.applyCondition == "G2S_disable")
                {
                    command.Error.Code = ErrorCode.G2S_APX009 /*ErrorCode."G2S_DLX020"*/;
                    return;
                }

                // TODO: G2S_DLX014 script is too complex.
                // TODO: • G2S_DLX015 invalid commandString in command
                if (command.Command.commandList.Items == null || command.Command.commandList.Items.Length == 0)
                {
                    command.Error.Code = /*ErrorCode.*/ "G2S_DLX021";
                    return;
                }

                //// TODO: G2S_DLX017 Invalid cmdSequence Values In The setScript Command
                //// TODO: G2S_DLX018 Script Contains An Invalid timeoutDate
                //// TODO: G2S_DLX020 Invalid Apply Condition
                //// TODO: G2S_DLX021 Invalid Change Request

                foreach (var cmd in command.Command.commandList.Items)
                {
                    if (cmd is packageCmd pkgCmd)
                    {
                        if (pkgCmd.operation == t_operPackage.G2S_install &&
                            _packageManager.HasModule(pkgCmd.pkgId + ".iso"))
                        {
                            command.Error.Code = /*ErrorCode.*/ "G2S_DLX021";
                            return;
                        }

                        var pe = _packageManager.GetPackageEntity(pkgCmd.pkgId);

                        if (pe == null ||
                            pe.State == PackageState.Deleted && pkgCmd.operation == t_operPackage.G2S_install)
                        {
                            command.Error.Code = ErrorCode.G2S_DLX001;
                            return;
                        }

                        if(pe.State != PackageState.Available)
                        {
                            command.Error.Code = /*ErrorCode.*/ "G2S_DLX021";
                            return;
                        }

                        if (pkgCmd.operation == t_operPackage.G2S_uninstall)
                        {
                            var found = _packageManager.ModuleEntityList.Any(
                                module => module.PackageId == pkgCmd.pkgId);

                            if (!found)
                            {
                                command.Error.Code = ErrorCode.G2S_DLX001;
                                return;
                            }
                        }

                        continue;
                    }

                    if (cmd is moduleCmd modCmd)
                    {
                        if (modCmd.operation == t_operModule.G2S_install &&
                            _packageManager.HasModule(modCmd.pkgId + ".iso"))
                        {
                            command.Error.Code = /*ErrorCode.*/ "G2S_DLX021";
                            return;
                        }

                        if (!_packageManager.HasModule(modCmd.modId))
                        {
                            command.Error.Code = ErrorCode.G2S_DLX009;
                            return;
                        }
                    }
                }

                var authorizeStatusList = GetAuthorizedStatusList(
                    command.Command.authorizeList,
                    command.IClass.dateTime);
                var commandStatusList = GetCommandStatusList(command.Command.commandList);

                se = new Script
                {
                    TransactionId = _idProvider.GetNextTransactionId(),
                    ApplyCondition = command.Command.applyCondition.ApplyConditionFromG2SString(),
                    DisableCondition = command.Command.disableCondition.DisableConditionFromG2SString(),
                    ReasonCode = command.Command.reasonCode,
                    ScriptId = command.Command.scriptId,
                    State = ScriptState.PendingDisable,
                    DeviceId = dl.Id,
                    AuthorizeItems = authorizeStatusList,
                    CommandData = _packageManager.ToXml(commandStatusList)
                };

                if (command.Command.endDateTimeSpecified)
                {
                    se.EndDateTime = command.Command.endDateTime.ToUniversalTime();
                }

                if (command.Command.startDateTimeSpecified)
                {
                    se.StartDateTime = command.Command.startDateTime.ToUniversalTime();
                }

                if (command.Command.endDateTimeSpecified &&
                    DateTime.UtcNow >= command.Command.endDateTime.ToUniversalTime())
                {
                    se.State = ScriptState.Error;
                    se.ScriptException = 2;
                }
                else if (command.Command.startDateTimeSpecified &&
                         DateTime.UtcNow < command.Command.startDateTime.ToUniversalTime())
                {
                    se.State = ScriptState.PendingDateTime;
                }
                else if (authorizeStatusList != null)
                {
                    se.State = ScriptState.PendingAuthorization;
                }
                else
                {
                    if (se.DisableCondition != DisableCondition.None)
                    {
                        se.State = ScriptState.PendingDisable;
                    }
                    else
                    {
                        switch (command.Command.applyCondition)
                        {
                            case "G2S_cancel":
                                se.State = ScriptState.Canceled;
                                break;
                            case "G2S_egmAction":
                                se.State = ScriptState.PendingOperatorAction;
                                break;
                            case "G2S_disable":
                                se.State = ScriptState.PendingDisable;
                                break;
                        }
                    }
                }

                _packageManager.UpdateScript(se);

                PostEvent(dl, EventCode.G2S_DLE201, se.ScriptId);

                if (se.State != ScriptState.Error)
                {
                    if (authorizeStatusList?.Count > 0)
                    {
                        se.State = ScriptState.PendingAuthorization;
                        _packageManager.UpdateScript(se);
                    }

                    _scriptManager.AddScript(se);
                }

                var response = command.GenerateResponse<scriptStatus>();
                response.Command.scriptId = se.ScriptId;
                await _command.Build(dl, response.Command);
            }

            await Task.CompletedTask;
        }

        private void PostEvent(IDownloadDevice dl, string code, int scriptId)
        {
            var scriptLog = new scriptLog { scriptId = scriptId };
            _logCommand.Build(dl, scriptLog);
            var info = new transactionInfo
            {
                deviceId = dl.Id,
                deviceClass = dl.PrefixedDeviceClass(),
                Item = scriptLog
            };

            var transList = new transactionList { transactionInfo = new[] { info } };

            _eventLift.Report(
                dl,
                code,
                scriptLog.transactionId,
                transList);
        }

        private commandStatusList GetCommandStatusList(commandList commandList)
        {
            var result = new commandStatusList();
            var items = new SortedList<int, object>();

            foreach (var command in commandList.Items)
            {
                if (command is packageCmd pkgCmd)
                {
                    var pkgCmdStatus = new packageCmdStatus
                    {
                        cmdSequence = pkgCmd.cmdSequence,
                        commandString = pkgCmd.commandString,
                        deletePkgAfter = pkgCmd.deletePkgAfter
                            ? pkgCmd.deletePkgAfter
                            : _properties.GetValue(ApplicationConstants.DeletePackageAfterInstall, false),
                        // TODO: exception
                        operation = pkgCmd.operation,
                        pkgId = pkgCmd.pkgId,
                        status = t_operCmdStates.G2S_pending
                    };
                    items[pkgCmdStatus.cmdSequence] = pkgCmdStatus;
                }
                else if (command is moduleCmd modCmd)
                {
                    var modCmdStatus = new moduleCmdStatus
                    {
                        modId = modCmd.modId,
                        cmdSequence = modCmd.cmdSequence,
                        operation = modCmd.operation,
                        pkgId = modCmd.pkgId,
                        pkgIdSpecified = modCmd.pkgIdSpecified
                    };
                    items[modCmdStatus.cmdSequence] = modCmdStatus;
                }
                else if (command is systemCmd sysCmd)
                {
                    var sysCmdStatus = new systemCmdStatus
                    {
                        cmdSequence = sysCmd.cmdSequence,
                        operation = sysCmd.operation,
                        status = t_operCmdStates.G2S_pending
                    };
                    items[sysCmdStatus.cmdSequence] = sysCmdStatus;
                }
            }

            result.Items = items.Values.ToArray();

            return result;
        }

        private ICollection<ConfigChangeAuthorizeItem> GetAuthorizedStatusList(
            authorizeList authorizeList,
            DateTime dateTime)
        {
            if (authorizeList?.authorizeItem != null)
            {
                var authorizeItems =
                    Mapper.Map<authorizeItem[], ConfigChangeAuthorizeItem[]>(authorizeList.authorizeItem).ToList();

                // The timeout MUST be within 24 hours of the dateTime specified within the class level element
                authorizeItems.ForEach(a => a.TimeoutDate = a.TimeoutDate ?? dateTime);

                return authorizeItems;
            }

            return null;
        }
    }
}