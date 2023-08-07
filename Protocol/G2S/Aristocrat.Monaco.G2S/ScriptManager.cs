namespace Aristocrat.Monaco.G2S
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts.Localization;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.Events;
    using Common.PackageManager;
    using Common.PackageManager.CommandHandlers;
    using Common.PackageManager.Storage;
    using Data.Model;
    using Gaming.Contracts;
    using Handlers;
    using Handlers.Downloads;
    using Kernel;
    using Kernel.Contracts;
    using Kernel.Contracts.Components;
    using Localization.Properties;
    using log4net;
    using Monaco.Common.Exceptions;
    using Services;

    /// <summary>
    ///     Script manager implementation used to control and track G2S script commands and states.
    /// </summary>
    public class ScriptManager : IScriptManager, IDisposable
    {
        private const int TimeOutOffsetDays = 100;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IDisableConditionSaga _disableSaga;
        private readonly IG2SEgm _egm;
        private readonly IEgmStateManager _egmStateManager;
        private readonly IEventBus _eventBus;
        private readonly IEventLift _eventLift;
        private readonly IGatService _gatService;
        private readonly IPackageManager _packageManager;
        private readonly List<int> _pendingAuthorizationScripts;
        private readonly Queue<int> _pendingScripts;
        private readonly IPropertiesManager _properties;
        private readonly object _scriptLock = new object();
        private readonly ICommandBuilder<IDownloadDevice, scriptLog> _scriptLogBuilder;
        private readonly Guid _scriptManagerGuid;
        private readonly ICommandBuilder<IDownloadDevice, scriptStatus> _scriptStatusBuilder;
        private int _activeScript;
        private bool _disposed;
        private IDownloadDevice _downloadDevice;
        private bool _onLine;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptManager" /> class.
        ///     Creates Instance of the Script Manager.
        /// </summary>
        /// <param name="packageManager">Package Manager.</param>
        /// <param name="egm">G2S Egm.</param>
        /// <param name="gatService">Gat Service.</param>
        /// <param name="eventBus">Event bus.</param>
        /// <param name="egmStateManager">Egm state manager.</param>
        /// <param name="eventLift">Event lift.</param>
        /// <param name="properties">Properties manager</param>
        /// <param name="scriptStatusBuilder">Script Status command builder.</param>
        /// <param name="scriptLogBuilder">Script log command builder.</param>
        /// <param name="disableSaga">System disable saga</param>
        public ScriptManager(
            IPackageManager packageManager,
            IG2SEgm egm,
            IGatService gatService,
            IEventBus eventBus,
            IEgmStateManager egmStateManager,
            IEventLift eventLift,
            IPropertiesManager properties,
            ICommandBuilder<IDownloadDevice, scriptStatus> scriptStatusBuilder,
            ICommandBuilder<IDownloadDevice, scriptLog> scriptLogBuilder,
            IDisableConditionSaga disableSaga)
        {
            _packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
            _pendingAuthorizationScripts = new List<int>();
            _pendingScripts = new Queue<int>();
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _gatService = gatService ?? throw new ArgumentNullException(nameof(gatService));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _egmStateManager = egmStateManager ?? throw new ArgumentNullException(nameof(egmStateManager));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _scriptStatusBuilder = scriptStatusBuilder ?? throw new ArgumentNullException(nameof(scriptStatusBuilder));
            _scriptLogBuilder = scriptLogBuilder ?? throw new ArgumentNullException(nameof(scriptLogBuilder));
            _disableSaga = disableSaga ?? throw new ArgumentNullException(nameof(disableSaga));
            _scriptManagerGuid = Guid.NewGuid();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void Start()
        {
            _onLine = false;

            _eventBus.Subscribe<CommunicationsStateChangedEvent>(this, HandleEvent);
        }

        /// <inheritdoc />
        public void AddScript(Script scriptEntity)
        {
            Logger.Info("AddScript script=" + scriptEntity);

            ProcessScript(scriptEntity);

            if (scriptEntity.AuthorizeItems != null && scriptEntity.AuthorizeItems.Count > 0)
            {
                foreach (var authHost in scriptEntity.AuthorizeItems)
                {
                    if (authHost.AuthorizeStatus == AuthorizationState.Pending)
                    {
                        PostWaitingOnAuthEvent(scriptEntity.ScriptId, authHost);
                    }
                }
            }
        }

        /// <inheritdoc />
        public void CancelScript(Script script)
        {
            CancelScript(script, false);
        }

        /// <inheritdoc />
        public void AuthorizeScript(Script scriptEntity)
        {
            if (!_pendingAuthorizationScripts.Contains(scriptEntity.ScriptId))
            {
                return;
            }

            if (!GetAuthorize(scriptEntity))
            {
                return;
            }

            _pendingAuthorizationScripts.Remove(scriptEntity.ScriptId);

            ProcessScript(scriptEntity);
        }

        /// <inheritdoc />
        public void UpdateScript(Script scriptEntity, bool sendStatus = true)
        {
            _packageManager.UpdateScript(scriptEntity);

            if (sendStatus)
            {
                switch (scriptEntity.State)
                {
                    case ScriptState.Completed:
                    case ScriptState.Error:
                    case ScriptState.Canceled:
                        var status = new scriptStatus { scriptId = scriptEntity.ScriptId };

                        _scriptStatusBuilder.Build(_downloadDevice, status);

                        _downloadDevice.SendStatus(
                            status,
                            ()
                                =>
                            {
                                scriptEntity.ScriptCompleteHostAcknowledged = true;
                                _packageManager.UpdateScript(scriptEntity);
                            });

                        break;
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void ExitDisable()
        {
            _disableSaga.Exit(
                _downloadDevice,
                DisableCondition.Immediate,
                TimeSpan.MaxValue,
                result =>
                {
                    _downloadDevice.Locked = false;
                    _eventLift.Report(_downloadDevice, EventCode.G2S_DLE008);
                });
        }

        private static bool GetAuthorize(Script script)
        {
            return script.AuthorizeItems == null || script.AuthorizeItems.Count == 0 ||
                   script.AuthorizeItems.All(
                       authStatus => authStatus.AuthorizeStatus == AuthorizationState.Authorized);
        }

        private void PostEvent(string code, int scriptId)
        {
            Logger.Info("PostEvent code=" + code + " scriptId=" + scriptId);
            var log = new scriptLog { scriptId = scriptId };
            _scriptLogBuilder.Build(_downloadDevice, log);

            var info = new transactionInfo
            {
                deviceId = _downloadDevice.Id,
                deviceClass = _downloadDevice.PrefixedDeviceClass(),
                Item = log
            };

            var transList = new transactionList { transactionInfo = new[] { info } };

            _eventLift.Report(_downloadDevice, code, log.transactionId, transList);
        }

        private void PostWaitingOnAuthEvent(int scriptId, ConfigChangeAuthorizeItem item)
        {
            Task.Run(
                () =>
                {
                    var retries = 0;
                    var waiting = true;
                    var dl = _downloadDevice;
                    while (waiting && ++retries <= dl.AuthenticationWaitRetries)
                    {
                        PostEvent(EventCode.G2S_DLE207, scriptId);
                        Thread.Sleep(new TimeSpan(0, 0, 0, 0, _downloadDevice.AuthenticationWaitTimeOut));
                        waiting = _pendingAuthorizationScripts.Contains(scriptId);
                    }

                    if (waiting)
                    {
                        var script = _packageManager.GetScript(scriptId);

                        if (script == null)
                        {
                            return;
                        }

                        TimeSpan wait = script.AuthorizeItems?.FirstOrDefault()?.TimeoutDate - DateTime.UtcNow ?? TimeSpan.Zero;
                        if (wait < TimeSpan.Zero)
                        {
                            wait = TimeSpan.Zero;
                        }
                        Task.Delay(wait).ContinueWith(
                            a =>
                            {
                                script = _packageManager.GetScript(scriptId);

                                if (script == null || script.State != ScriptState.PendingAuthorization)
                                {
                                    return;
                                }
                                script.State = ScriptState.Error;
                                script.ScriptException = 5;
                                UpdateScript(script);

                                PostEvent(EventCode.G2S_DLE212, script.ScriptId);
                                _pendingAuthorizationScripts.Remove(scriptId);
                            });
                    }
                });
        }

        private void PendingAuthorization(Script script)
        {
            if (!_pendingAuthorizationScripts.Contains(script.ScriptId))
            {
                script.State = ScriptState.PendingAuthorization;
                UpdateScript(script);
                _pendingAuthorizationScripts.Add(script.ScriptId);

                CheckAuthorizeScript(script.ScriptId);
            }
        }

        private void CheckAuthorizeScript(int scriptId)
        {
            var script = _packageManager.GetScript(scriptId);

            if (script == null)
            {
                return;
            }

            if (_pendingAuthorizationScripts.Contains(scriptId) && ScriptExpired(script))
            {
                _pendingAuthorizationScripts.Remove(scriptId);
                return;
            }

            if (script.State != ScriptState.PendingAuthorization)
            {
                return;
            }

            var pending = script.AuthorizeItems.Where(
                a => a.AuthorizeStatus == AuthorizationState.Pending && a.TimeoutDate < DateTime.UtcNow);

            foreach (var item in pending)
            {
                if (item.TimeoutAction == TimeoutActionType.Abort)
                {
                    item.AuthorizeStatus = AuthorizationState.Timeout;
                    script.State = ScriptState.Error;
                    script.ScriptException = 4;
                    UpdateScript(script);

                    PostEvent(EventCode.G2S_DLE212, script.ScriptId);
                    _pendingAuthorizationScripts.Remove(scriptId);
                    return;
                }

                item.AuthorizeStatus = AuthorizationState.Authorized;
                _packageManager.UpdateScript(script);

                PostEvent(EventCode.G2S_DLE202, script.ScriptId);
                AuthorizeScript(script);
            }

            ScheduleTimeout(script);
        }

        private void ScheduleTimeout(Script script)
        {
            var timeout = script.EndDateTime;

            if (script.AuthorizeItems != null && script.AuthorizeItems.Count > 0)
            {
                var firstTimeout = script.AuthorizeItems.Where(a => a.AuthorizeStatus == AuthorizationState.Pending)
                    .Min(i => i.TimeoutDate);

                if (firstTimeout != null && script.EndDateTime != null)
                {
                    timeout = firstTimeout.Value < script.EndDateTime.Value
                        ? firstTimeout.Value
                        : script.EndDateTime.Value;
                }
                else if (firstTimeout != null)
                {
                    timeout = firstTimeout.Value;
                }
            }

            if (!timeout.HasValue)
            {
                timeout = DateTime.UtcNow.AddSeconds(30);
            }

            Task.Delay(timeout.Value - DateTime.UtcNow).ContinueWith(task => CheckAuthorizeScript(script.ScriptId));
        }

        private void ProcessScript(Script script)
        {
            if (ScriptExpired(script))
            {
                return;
            }

            if (!script.StartDateTime.HasValue || script.StartDateTime.Value <= DateTime.UtcNow)
            {
                if (script.StartDateTime.HasValue)
                {
                    PostEvent(EventCode.G2S_DLE204, script.ScriptId);
                }

                if (script.AuthorizeItems == null || script.AuthorizeItems.Count == 0 || GetAuthorize(script))
                {
                    RunScript(script);
                }
                else
                {
                    PendingAuthorization(script);
                }
            }
            else
            {
                script.State = ScriptState.PendingDateTime;
                UpdateScript(script);

                Task.Delay(script.StartDateTime.Value - DateTime.UtcNow)
                    .ContinueWith(task => CheckScript(script.ScriptId));
            }
        }

        private bool ScriptExpired(Script script)
        {
            if (script.EndDateTime.HasValue && script.EndDateTime.Value != DateTime.MinValue &&
                DateTime.UtcNow >= script.EndDateTime.Value)
            {
                if (script.ApplyCondition == ApplyCondition.Immediate && script.StartDateTime == DateTime.MinValue &&
                    script.EndDateTime == DateTime.MinValue)
                {
                    return false;
                }

                _eventLift.Report(_downloadDevice, EventCode.G2S_DLE205);

                script.State = ScriptState.Error;
                script.ScriptException = 4;
                UpdateScript(script);
                return true;
            }

            return false;
        }

        private void CheckScript(int scriptId)
        {
            var script = _packageManager.GetScript(scriptId);

            if (script == null)
            {
                return;
            }

            if (script.State == ScriptState.Canceled)
            {
                return;
            }

            if (script.StartDateTime.HasValue)
            {
                PostEvent(EventCode.G2S_DLE204, scriptId);
            }

            if (ScriptExpired(script))
            {
                return;
            }

            if (!_pendingAuthorizationScripts.Contains(script.ScriptId))
            {
                RunScript(script);
            }
        }

        private void RunScript(Script script)
        {
            lock (_scriptLock)
            {
                if (_activeScript == 0 && _onLine)
                {
                    BeginScript(script);
                }
                else
                {
                    if (!_pendingScripts.Contains(script.ScriptId))
                    {
                        _pendingScripts.Enqueue(script.ScriptId);
                    }
                }
            }
        }

        private void BeginScript(Script script)
        {
            _activeScript = script.ScriptId;
            HandleExceptions(Task.Run(() => StartScript(script)), script);
        }

        private void ApplyPendingScripts()
        {
            lock (_scriptLock)
            {
                if (_pendingScripts.Count > 0)
                {
                    var script = _packageManager.GetScript(_pendingScripts.Dequeue());

                    if (!ScriptExpired(script) && (script.State != ScriptState.Canceled ||
                                                   script.State != ScriptState.Completed ||
                                                   script.State != ScriptState.Error))
                    {
                        BeginScript(script);
                    }
                    else
                    {
                        ApplyPendingScripts();
                    }
                }
                else
                {
                    _activeScript = 0;
                }
            }
        }

        private void HandleExceptions(Task task, Script script)
        {
            task.ContinueWith(
                t =>
                {
                    if (t.Exception != null)
                    {
                        var aggException = t.Exception.Flatten();
                        foreach (var exception in aggException.InnerExceptions)
                        {
                            Logger.Error("Script exception: " + exception + "\n" + script);
                        }
                    }

                    script.State = ScriptState.Error;
                    script.ScriptException = 7;
                    UpdateScript(script);

                    if (_activeScript == script.ScriptId)
                    {
                        ApplyPendingScripts();
                    }
                },
                TaskContinuationOptions.OnlyOnFaulted);
        }

        private void StartScript(Script script)
        {
            Logger.Info($"StartScript script={script}");

            var disableCondition = script.DisableCondition;

            // Don't promote an apply condition of cancel
            if (script.ApplyCondition != ApplyCondition.Cancel)
            {
                var disableStrategyOverride = _properties.GetValue(
                    GamingConstants.StateChangeOverride,
                    DisableStrategy.None);
                if (disableStrategyOverride != DisableStrategy.None &&
                    (DisableCondition)disableStrategyOverride > disableCondition)
                {
                    disableCondition = (DisableCondition)disableStrategyOverride;

                    Logger.Debug($"Overriding script disable condition to {disableCondition}");
                }
            }

            if (disableCondition == DisableCondition.None)
            {
                ScriptApplyCondition(script);
            }
            else
            {
                HandleDisableCondition(script, disableCondition);
            }
        }

        private void HandleDisableCondition(Script script, DisableCondition disableCondition)
        {
            TimeSpan timeToLive;

            // this sets it to max timeout minus offset days
            if (!script.EndDateTime.HasValue)
            {
                timeToLive = DateTime.UtcNow.AddDays(TimeOutOffsetDays) - DateTime.UtcNow;
            }
            else
            {
                timeToLive = script.EndDateTime.Value - DateTime.UtcNow;
            }

            if (IsScriptCancelled(script.ScriptId))
            {
                return;
            }

            script.State = ScriptState.PendingDisable;
            UpdateScript(script);

            _disableSaga.Enter(
                _downloadDevice,
                disableCondition,
                timeToLive,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ApplyingScripts),
                result =>
                {
                    if (result)
                    {
                        if (IsScriptCancelled(script.ScriptId))
                        {
                            ExitDisable();
                            return;
                        }

                        _eventLift.Report(_downloadDevice, EventCode.G2S_DLE007);
                        _downloadDevice.Locked = true;
                        ScriptApplyCondition(script);
                    }
                    else
                    {
                        script.State = ScriptState.Error;
                        script.ScriptException = 7;
                        UpdateScript(script);
                        PostEvent(EventCode.G2S_DLE212, script.ScriptId);
                        ApplyPendingScripts();
                    }
                });
        }

        private bool IsScriptCancelled(int scriptId)
        {
            var se = _packageManager.GetScript(scriptId);
            if (se.State == ScriptState.Canceled)
            {
                return true;
            }

            return false;
        }

        private void ScriptApplyCondition(Script script)
        {
            if (IsScriptCancelled(script.ScriptId))
            {
                return;
            }

            switch (script.ApplyCondition)
            {
                case ApplyCondition.Cancel:
                    CancelScript(script, true);

                    PostEvent(EventCode.G2S_DLE206, script.ScriptId);
                    break;
                case ApplyCondition.Immediate:
                    ApplyScript(script);
                    break;
                case ApplyCondition.EgmAction:
                    script.State = ScriptState.PendingOperatorAction;
                    UpdateScript(script);

                    PostEvent(EventCode.G2S_DLE208, script.ScriptId);

                    // TODO: block/wait on operator (signal)
                    // TODO:  There is currently no service for this, trigger signal and wait for operator
                    ApplyScript(script);
                    break;
                case ApplyCondition.Disable:
                    ApplyScript(script);
                    break;
                default:
                    script.State = ScriptState.Error;
                    script.ScriptException = 1;
                    UpdateScript(script);

                    PostEvent(EventCode.G2S_DLE212, script.ScriptId);
                    ApplyPendingScripts();
                    break;
            }
        }

        private void CancelScript(Script script, bool sendStatus)
        {
            Logger.Info("CancelScript script=" + script);
            script.State = ScriptState.Canceled;
            var commandList = _packageManager.ParseXml<commandStatusList>(script.CommandData);
            var commandItems = commandList.Items;

            foreach (var command in commandItems)
            {
                switch (command)
                {
                    case packageCmdStatus pkgCmd:
                        pkgCmd.status = t_operCmdStates.G2S_cancelled;
                        break;
                    case moduleCmdStatus modCmd:
                        modCmd.status = t_operCmdStates.G2S_cancelled;
                        break;
                    case systemCmdStatus sysCmd:
                        sysCmd.status = t_operCmdStates.G2S_cancelled;
                        break;
                }
            }

            script.CommandData = _packageManager.ToXml(commandList);

            UpdateScript(script, sendStatus);

            PostEvent(EventCode.G2S_DLE213, script.ScriptId);

            if (_activeScript == script.ScriptId)
            {
                ExitDisable();

                ApplyPendingScripts();
            }
        }

        private void ApplyScript(Script script)
        {
            Logger.Info("ApplyScript script=" + script);
            script.State = ScriptState.InProgress;
            var commandList = _packageManager.ParseXml<commandStatusList>(script.CommandData);
            var commandItems = commandList.Items;

            UpdateScript(script);

            PostEvent(EventCode.G2S_DLE209, script.ScriptId);
            PostEvent(EventCode.G2S_DLE210, script.ScriptId);

            var deviceChange = false;
            var removedModule = false;
            ExitAction? exitAction = null;
            var scriptComplete = false;
            StartupContext context = null;

            try
            {
                foreach (var command in commandItems)
                {
                    if (CommandCompleted(command))
                    {
                        continue;
                    }

                    UpdateCommandItems(script, commandList, command, false);

                    try
                    {
                        var results = ProcessCommand(script, command);

                        deviceChange |= results.deviceChanged;
                        removedModule |= results.removedModule;
                        if (results.exitAction.HasValue)
                        {
                            exitAction = results.exitAction;
                        }

                        if (script.State == ScriptState.Error)
                        {
                            throw new CommandException("Error processing command=" + command);
                        }

                        UpdateCommandItems(script, commandList, command, true);

                        scriptComplete = commandItems[commandItems.Length - 1] == command;

                        if (exitAction.HasValue)
                        {
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Exception while processing script=" + script + " " + e);
                        script.State = ScriptState.Error;
                        UpdateCommandItems(script, commandList, command, false);
                        throw new CommandException(e.ToString());
                    }
                }

                if (exitAction.HasValue)
                {
                    context = new StartupContext { DeviceChanged = deviceChange };

                    if (removedModule)
                    {
                        context.DeviceReset = true;
                        context.SubscriptionLost = true;
                        context.MetersReset = true;
                    }
                }

                if (!scriptComplete && context != null)
                {
                    Restart(context, exitAction.Value);
                    return;
                }

                // TODO:  This needs to be modified but we have to wait for the install/uninstall and add/remove events to be processed
                //  There are a couple of ways to handle this, but none of them are a quick fix.  This is low risk and functional (at least for now)
                Task.Delay(TimeSpan.FromSeconds(1))
                    .ContinueWith(_ => FinalizeScript(script, deviceChange, context, exitAction));
            }
            catch (CommandException e)
            {
                Logger.Error("Exception while processing script=" + script + " " + e);
                PostEvent(EventCode.G2S_DLE212, script.ScriptId);

                ExitDisable();

                if (_activeScript == script.ScriptId)
                {
                    ApplyPendingScripts();
                }
            }
        }

        private void FinalizeScript(Script script, bool deviceChange, StartupContext context, ExitAction? exitAction)
        {
            var runNextScript = true;
            script.State = ScriptState.Completed;
            script.CompletedDateTime = DateTime.UtcNow;
            UpdateScript(script, context == null);

            if (context == null)
            {
                PostEvent(EventCode.G2S_DLE211, script.ScriptId);
            }

            ExitDisable();

            if (context != null)
            {
                runNextScript = false;
                Restart(context, exitAction);
            }
            else if (deviceChange)
            {
                runNextScript = false;
                _eventBus.Publish(new DeviceChangedEvent());
            }

            if (runNextScript)
            {
                ApplyPendingScripts();
            }
            else
            {
                _activeScript = 0;
            }
        }

        private void Restart(StartupContext context, ExitAction? exitAction)
        {
            _egm.Stop();

            _properties.SetProperty(Constants.StartupContext, context);

            if (exitAction.HasValue)
            {
                _eventBus.Publish(new ExitRequestedEvent(exitAction.Value));
            }
        }

        private static bool CommandCompleted(object command)
        {
            switch (command)
            {
                case packageCmdStatus pkgCmd:
                    return pkgCmd.status == t_operCmdStates.G2S_complete;
                case moduleCmdStatus modCmd:
                    return modCmd.status == t_operCmdStates.G2S_complete;
                case systemCmdStatus sysCmd:
                    return sysCmd.status == t_operCmdStates.G2S_complete;
            }

            return false;
        }

        private void UpdateCommandItems(Script script, commandStatusList commandList, object command, bool complete)
        {
            switch (command)
            {
                case packageCmdStatus pkgCmd:
                    pkgCmd.status = GetCommandItemStatus(script, complete);
                    break;
                case moduleCmdStatus modCmd:
                    modCmd.status = GetCommandItemStatus(script, complete);
                    break;
                case systemCmdStatus sysCmd:
                    sysCmd.status = GetCommandItemStatus(script, complete);
                    break;
            }

            script.CommandData = _packageManager.ToXml(commandList);
            UpdateScript(script);
        }

        private static t_operCmdStates GetCommandItemStatus(Script script, bool complete)
        {
            if (script.State == ScriptState.Error)
            {
                return t_operCmdStates.G2S_error;
            }

            return complete ? t_operCmdStates.G2S_complete : t_operCmdStates.G2S_inProgress;
        }

        private (bool deviceChanged, bool removedModule, ExitAction? exitAction) ProcessCommand(Script scriptEntity, object command)
        {
            Logger.Info("ProcessCommand script=" + scriptEntity + " command=" + command);
            var deviceChanged = false;
            var removedModule = false;
            ExitAction? exitAction = null;

            if (command is packageCmdStatus pkgCmd)
            {
                var pe = _packageManager.GetPackageEntity(pkgCmd.pkgId);
                if (pkgCmd.operation == t_operPackage.G2S_install)
                {
                    if (pe != null)
                    {
                        _packageManager.InstallPackage(
                            pe,
                            a =>
                            {
                                if (a.PackageEntity.State == PackageState.Error)
                                {
                                    scriptEntity.State = ScriptState.Error;
                                    scriptEntity.ScriptException = 7;

                                    pkgCmd.exception = 7;
                                    pkgCmd.status = t_operCmdStates.G2S_error;
                                }
                                else
                                {
                                    deviceChanged = a.DeviceChanged;
                                    exitAction = a.ExitAction;
                                }

                                PackageUpdate(scriptEntity, a);
                            },
                            pkgCmd.deletePkgAfter);
                    }
                }
                else if (pkgCmd.operation == t_operPackage.G2S_uninstall)
                {
                    if (pe != null)
                    {
                        _packageManager.UninstallPackage(
                            pe,
                            a => { deviceChanged = UninstallPackageUpdate(scriptEntity, a); },
                            pkgCmd.deletePkgAfter);
                        removedModule = true;
                    }
                }
            }
            else if (command is moduleCmdStatus modCmd)
            {
                if (modCmd.operation == t_operModule.G2S_uninstall)
                {
                    var me = _packageManager.GetModuleEntity(modCmd.modId);
                    if (me != null)
                    {
                        _packageManager.UninstallModule(
                            me,
                            args =>
                            {
                                RemoveModule(me.ModuleId);

                                deviceChanged = args.DeviceChanged;
                            });
                        removedModule = true;
                    }
                }
            }
            else if (command is systemCmdStatus sysCmd)
            {
                switch (sysCmd.operation)
                {
                    case t_operSystem.G2S_disableEgm:
                        _eventLift.Report(_downloadDevice, EventCode.G2S_DLE007);
                        _downloadDevice.Locked = true;
                        _egmStateManager.Disable(
                            _scriptManagerGuid,
                            _downloadDevice,
                            EgmState.EgmLocked,
                            false,
                            () => Localizer.ForLockup().GetString(ResourceKeys.ApplyingScripts));

                        PostEvent(EventCode.G2S_DLE206, scriptEntity.ScriptId);
                        break;
                    case t_operSystem.G2S_enableEgm:
                        if (_downloadDevice.Locked)
                        {
                            _eventLift.Report(_downloadDevice, EventCode.G2S_DLE008);
                            _downloadDevice.Locked = false;
                            _egmStateManager.Enable(_scriptManagerGuid, _downloadDevice, EgmState.EgmLocked);
                        }

                        break;
                    case t_operSystem.G2S_resetEgm:
                        exitAction = ExitAction.Restart;
                        break;
                }
            }

            return (deviceChanged, removedModule, exitAction);
        }

        private bool UninstallPackageUpdate(Script scriptEntity, InstallPackageArgs args)
        {
            Logger.Info("UninstallPackageUpdate script=" + scriptEntity);
            PackageUpdate(scriptEntity, args);

            RemoveModule(args);

            return args.DeviceChanged;
        }

        private void RemoveModule(InstallPackageArgs args)
        {
            var moduleId = args.PackageEntity.PackageId + ".iso";

            RemoveModule(moduleId);
        }

        private void RemoveModule(string moduleId)
        {
            Logger.Info("RemoveModule module=" + moduleId);
            _eventLift.Report(_downloadDevice, EventCode.G2S_DLE304);

            _gatService.DeleteComponent(moduleId, ComponentType.Module);

            _eventLift.Report(_downloadDevice, EventCode.G2S_DLE302);
        }

        private void PostEvent(string code, string packageId)
        {
            Logger.Info("PostEvent packageId=" + packageId + " code=" + code);

            var log = _packageManager.GetPackageLogEntity(packageId);
            var packageLog = log.ToPackageLog();

            var info = new transactionInfo
            {
                deviceId = _downloadDevice.Id,
                deviceClass = _downloadDevice.PrefixedDeviceClass(),
                Item = packageLog
            };

            var transList = new transactionList { transactionInfo = new[] { info } };

            _eventLift.Report(_downloadDevice, code, packageLog.transactionId, transList);
        }

        private void PackageUpdate(Script script, InstallPackageArgs args)
        {
            Logger.Info("PackageUpdate.. " + script);

            UpdateScript(script);

            if (args.DeleteAfter)
            {
                _packageManager.DeletePackage(
                    new DeletePackageArgs(
                        args.PackageEntity.PackageId,
                        a =>
                        {
                            PostEvent(EventCode.G2S_DLE140, args.PackageEntity.PackageId);
                            _gatService.DeleteComponent(args.PackageEntity.PackageId, ComponentType.Package);
                        }));
            }
        }

        private void HandleEvent(CommunicationsStateChangedEvent evt)
        {
            if (_onLine && evt.Online && _activeScript == 0)
            {
                ApplyPendingScripts();
            }

            if (!evt.Online || _onLine)
            {
                return;
            }

            var downloadDevice = _egm?.GetDevice<IDownloadDevice>();
            if (downloadDevice?.Owner == evt.HostId)
            {
                _onLine = true;

                Logger.Info("checking for unfinished scripts...");

                _downloadDevice = _egm.GetDevice<IDownloadDevice>();
                foreach (var script in _packageManager.ScriptEntityList)
                {
                    if (script.State != ScriptState.Canceled && script.State != ScriptState.Completed &&
                        script.State != ScriptState.Error)
                    {
                        if (!_pendingScripts.Contains(script.ScriptId))
                        {
                            ProcessScript(script);

                            if (script.AuthorizeItems != null && script.AuthorizeItems.Count > 0)
                            {
                                foreach (var authHost in script.AuthorizeItems)
                                {
                                    if (authHost.AuthorizeStatus == AuthorizationState.Pending)
                                    {
                                        PostWaitingOnAuthEvent(script.ScriptId, authHost);
                                    }
                                }
                            }
                        }
                    }
                    else if (script.State == ScriptState.Completed && !script.ScriptCompleteHostAcknowledged)
                    {
                        var status = new scriptStatus { scriptId = script.ScriptId };

                        _scriptStatusBuilder.Build(_downloadDevice, status);

                        _downloadDevice.SendStatus(
                            status,
                            ()
                                =>
                            {
                                script.ScriptCompleteHostAcknowledged = true;
                                _packageManager.UpdateScript(script);
                                PostEvent(EventCode.G2S_DLE211, script.ScriptId);
                            });
                    }
                }

                if (_activeScript == 0)
                {
                    ApplyPendingScripts();
                }
            }
        }
    }
}
