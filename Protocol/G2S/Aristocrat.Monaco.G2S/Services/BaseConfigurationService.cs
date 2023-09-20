namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts.Localization;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Kernel;
    using Kernel.Contracts;
    using log4net;
    using Localization.Properties;
    using Monaco.Common.Scheduler;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Base implementation of ICommunicationsConfiguration
    /// </summary>
    /// <seealso cref="IConfigurationService" />
    public abstract class BaseConfigurationService : IConfigurationService
    {
        /// <summary>
        ///     The logger
        /// </summary>
        protected static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IDisableConditionSaga _configurationMode;
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IEventLift _eventLift;
        private readonly ITaskScheduler _taskScheduler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseConfigurationService" /> class.
        /// </summary>
        /// <param name="egm">The egm.</param>
        /// <param name="configurationMode">The configuration mode.</param>
        /// <param name="bus">The bus.</param>
        /// <param name="contextFactory">The context factory.</param>
        /// <param name="taskScheduler">The task scheduler.</param>
        /// <param name="eventLift">The event lift.</param>
        protected BaseConfigurationService(
            IG2SEgm egm,
            IDisableConditionSaga configurationMode,
            IEventBus bus,
            IMonacoContextFactory contextFactory,
            ITaskScheduler taskScheduler,
            IEventLift eventLift)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _taskScheduler = taskScheduler ?? throw new ArgumentNullException(nameof(taskScheduler));
            Bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _configurationMode = configurationMode ?? throw new ArgumentNullException(nameof(configurationMode));
            Egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <summary>
        ///     Gets the egm.
        /// </summary>
        /// <value>
        ///     The egm.
        /// </value>
        protected IG2SEgm Egm { get; }

        /// <summary>
        ///     Gets the event bus.
        /// </summary>
        protected IEventBus Bus { get; }

        /// <summary>
        ///     Gets the <see cref="ITaskScheduler"/>.
        /// </summary>
        protected ITaskScheduler TaskScheduler => _taskScheduler;

        /// <summary>
        ///     Gets the configuration authorized by host event code.
        /// </summary>
        /// <value>
        ///     The configuration authorized by host event code.
        /// </value>
        protected abstract string ConfigurationAuthorizedByHostEventCode { get; }

        /// <summary>
        ///     Gets the egm locked by device event code.
        /// </summary>
        /// <value>
        ///     The egm locked by device event code.
        /// </value>
        protected abstract string EgmLockedByDeviceEventCode { get; }

        /// <summary>
        ///     Gets the egm not locked by device event code.
        /// </summary>
        /// <value>
        ///     The egm not locked by device event code.
        /// </value>
        protected abstract string EgmNotLockedByDeviceEventCode { get; }

        /// <summary>
        ///     Gets the configuration changes applied event code.
        /// </summary>
        /// <value>
        ///     The configuration changes applied event code.
        /// </value>
        protected abstract string ConfigurationChangesAppliedEventCode { get; }

        /// <summary>
        ///     Gets the configuration authorized by host time out event code.
        /// </summary>
        /// <value>
        ///     The configuration authorized by host time out event code.
        /// </value>
        protected abstract string ConfigurationAuthorizedByHostTimeOutEventCode { get; }

        /// <summary>
        ///     Gets the device configuration error event code.
        /// </summary>
        /// <value>
        ///     The device configuration error event code.
        /// </value>
        protected abstract string DeviceConfigErrorEventCode { get; }

        /// <summary>
        ///     Gets the configuration configuration cancelled event code.
        /// </summary>
        /// <value>
        ///     The configuration configuration cancelled event code.
        /// </value>
        protected abstract string ConfigConfigurationCancelledEventCode { get; }

        /// <summary>
        ///     Gets the configuration configuration aborted event code.
        /// </summary>
        /// <value>
        ///     The configuration configuration aborted event code.
        /// </value>
        protected abstract string ConfigConfigurationAbortedEventCode { get; }

        /// <inheritdoc />
        public void Abort(long transactionId, ChangeExceptionErrorCode exception)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var log = GetChangeLog(context, transactionId);
                if (log == null)
                {
                    return;
                }

                Abort(context, log, exception);
            }
        }

        /// <inheritdoc />
        public void Apply(long transactionId)
        {
            Logger.Debug($"Preparing to apply configuration changes for transaction {transactionId}");

            using (var context = _contextFactory.CreateDbContext())
            {
                var log = GetChangeLog(context, transactionId);

                if (log == null)
                {
                    return;
                }

                if (log.ApplyCondition == ApplyCondition.Cancel)
                {
                    Cancel(context, log);
                    return;
                }

                if (log.EndDateTime < DateTime.UtcNow)
                {
                    Logger.Debug($"Aborting configuration changes for transaction {transactionId} - Expired");

                    Abort(context, log, ChangeExceptionErrorCode.Expired);
                    return;
                }

                if (log.ApplyCondition == ApplyCondition.Immediate && IsAuthorized(log))
                {
                    ApplyCommand(context, log);
                }
                else if (log.ApplyCondition == ApplyCondition.Disable)
                {
                    Disable(log);
                }

                ScheduleTimeout(log);
            }
        }

        /// <inheritdoc />
        public void Cancel(long transactionId)
        {
            Logger.Debug($"Canceling configuration changes for transaction {transactionId}");

            using (var context = _contextFactory.CreateDbContext())
            {
                var log = GetChangeLog(context, transactionId);
                if (log == null)
                {
                    return;
                }

                Cancel(context, log);
            }
        }

        /// <inheritdoc />
        public void Authorize(long transactionId, int hostId, bool timeout = false)
        {
            Logger.Debug($"Authorizing configuration changes for transaction {transactionId}");

            using (var context = _contextFactory.CreateDbContext())
            {
                var log = GetChangeLog(context, transactionId);

                var authorizeItem =
                    log?.AuthorizeItems.SingleOrDefault(
                        x => x.HostId == hostId);

                if (authorizeItem?.AuthorizeStatus != AuthorizationState.Pending)
                {
                    return;
                }

                if (timeout)
                {
                    log.ChangeDateTime = DateTime.UtcNow;
                    authorizeItem.AuthorizeStatus = AuthorizationState.Timeout;
                    UpdateChangeLog(context, log);
                }
                else
                {
                    log.ChangeDateTime = DateTime.UtcNow;
                    authorizeItem.AuthorizeStatus = AuthorizationState.Authorized;
                    UpdateChangeLog(context, log);
                }

                if (log.AuthorizeItems.All(
                        a =>
                            a.AuthorizeStatus == AuthorizationState.Authorized ||
                            a.AuthorizeStatus == AuthorizationState.Timeout) &&
                    log.ChangeStatus != ChangeStatus.Authorized)
                {
                    log.ChangeDateTime = DateTime.UtcNow;
                    log.ChangeStatus = ChangeStatus.Authorized;
                    UpdateChangeLog(context, log);
                }

                if (authorizeItem.AuthorizeStatus == AuthorizationState.Timeout)
                {
                    SendTimeout(log);
                }
                else
                {
                    SendAuthorized(log);
                }

                if (log.ChangeStatus == ChangeStatus.Authorized)
                {
                    Authorize(context, log);
                }
            }
        }

        /// <summary>
        ///     Gets the change log.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <returns>
        ///     Config change log.
        /// </returns>
        protected abstract ConfigChangeLog GetChangeLog(DbContext context, long transactionId);

        /// <summary>
        ///     Updates the change log.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="changeLog">The change log.</param>
        protected abstract void UpdateChangeLog(DbContext context, ConfigChangeLog changeLog);

        /// <summary>
        ///     Sends the status.
        /// </summary>
        /// <param name="log">The log.</param>
        protected abstract void SendStatus(ConfigChangeLog log);

        /// <summary>
        ///     Sends the status asynchronously.
        /// </summary>
        /// <param name="log">The log.</param>
        protected abstract void SendStatusAsync(ConfigChangeLog log);

        /// <summary>
        ///     Gets Check Validity Task job.
        /// </summary>
        /// <param name="transactionId">Transaction Id.</param>
        /// <returns>ITaskSchedulerJob</returns>
        protected abstract ITaskSchedulerJob GetCheckValidityTask(long transactionId);

        /// <summary>
        ///     Generates the event.
        /// </summary>
        /// <param name="device">The device associated to the event.</param>
        /// <param name="eventCode">The event code.</param>
        /// <param name="transactionId">Transaction Id.</param>
        /// <param name="transactionList">Transaction List.</param>
        protected void GenerateEvent(
            IDevice device,
            string eventCode,
            long transactionId = 0,
            transactionList transactionList = null)
        {
            _eventLift.Report(device, eventCode, transactionId, transactionList);
        }

        /// <summary>
        ///     Applies the command.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="changeLog">The change log.</param>
        protected virtual void ApplyCommand(DbContext context, ConfigChangeLog changeLog)
        {
            changeLog.ChangeStatus = ChangeStatus.Applied;
            changeLog.ChangeDateTime = DateTime.UtcNow;
            UpdateChangeLog(context, changeLog);

            Finalize(changeLog, ConfigurationChangesAppliedEventCode);

            Logger.Debug($"Applied configuration changes for transaction {changeLog.TransactionId}");

            if (changeLog.RestartAfter)
            {
                Logger.Debug($"Configuration changes for transaction {changeLog.TransactionId} applied: Restarting");

                Egm.Stop();

                Bus.Publish(new ExitRequestedEvent(ExitAction.Restart));
            }
        }

        /// <summary>
        ///     Processes the apply error.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="log">The log.</param>
        /// <param name="ex">The ex.</param>
        protected void ProcessApplyError(DbContext context, ConfigChangeLog log, Exception ex)
        {
            Logger.Error("Failed to apply communication changes", ex);

            log.ChangeStatus = ChangeStatus.Error;
            log.ChangeException = ChangeExceptionErrorCode.ErrorApplyingChanges;
            log.ChangeDateTime = DateTime.UtcNow;
            UpdateChangeLog(context, log);

            Finalize(log, DeviceConfigErrorEventCode);
        }

        /// <summary>
        ///     Finalizes the specified log.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="eventCode">The event code.</param>
        protected void Finalize(ConfigChangeLog log, string eventCode)
        {
            var transactionId = GetTransactionLog(log, out var transList);

            GenerateEvent(GetDevice(log.DeviceId), eventCode, transactionId, transList);

            SendStatus(log);

            Enable(log);
        }

        /// <summary>
        ///     Gets the device used for configuration change
        /// </summary>
        /// <param name="deviceId">The device identifier</param>
        /// <returns>The device</returns>
        protected abstract IDevice GetDevice(int deviceId);

        /// <summary>
        ///     Gets transaction list for event report.
        /// </summary>
        /// <param name="log">Change log.</param>
        /// <param name="transactionList">Transaction List.</param>
        /// <returns>Transaction Id</returns>
        protected virtual long GetTransactionLog(ConfigChangeLog log, out transactionList transactionList)
        {
            transactionList = null;

            return 0;
        }

        private static bool IsAuthorized(ConfigChangeLog log)
        {
            if (log.ChangeStatus == ChangeStatus.Authorized || log.AuthorizeItems.Count == 0)
            {
                return true;
            }

            return log.AuthorizeItems.All(a => a.AuthorizeStatus == AuthorizationState.Authorized);
        }

        private void ScheduleTimeout(ConfigChangeLog log)
        {
            var timeout = log.EndDateTime;

            if (log.AuthorizeItems != null && log.AuthorizeItems.Count > 0)
            {
                var firstTimeout =
                    log.AuthorizeItems.Where(a => a.AuthorizeStatus == AuthorizationState.Pending)
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
                Logger.Debug($"Scheduling timeout for transaction {log.TransactionId}");

                _taskScheduler.ScheduleTask(
                    GetCheckValidityTask(log.TransactionId),
                    "ConfigurationChangeExpiration",
                    timeout.Value.UtcDateTime);
            }
        }

        private void Disable(ConfigChangeLog log)
        {
            if (_configurationMode.Enabled(GetDevice(log.DeviceId)))
            {
                ApplyCommand(log.TransactionId);
                return;
            }

            Logger.Debug($"Disabling EGM for transaction {log.TransactionId}");

            var device = GetDevice(log.DeviceId);

            _configurationMode.Enter(
                device,
                log.DisableCondition,
                log.EndDateTime == null ? TimeSpan.MaxValue : log.EndDateTime.Value - DateTime.UtcNow,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HostLocked),
                result =>
                {
                    if (result && IsAuthorized(log.TransactionId))
                    {
                        GenerateEvent(device, EgmLockedByDeviceEventCode);

                        ApplyCommand(log.TransactionId);
                    }
                    else if (!result)
                    {
                        Abort(log.TransactionId, ChangeExceptionErrorCode.Expired);
                    }
                });
        }

        private bool IsAuthorized(long transactionId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var log = GetChangeLog(context, transactionId);

                return log != null && IsAuthorized(log);
            }
        }

        private void ApplyCommand(long transactionId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var log = GetChangeLog(context, transactionId);
                if (log == null)
                {
                    return;
                }

                ApplyCommand(context, log);
            }
        }

        private void Cancel(DbContext context, ConfigChangeLog log)
        {
            log.ChangeStatus = ChangeStatus.Cancelled;
            log.ChangeDateTime = DateTime.UtcNow;

            UpdateChangeLog(context, log);

            var transactionId = GetTransactionLog(log, out var transList);

            GenerateEvent(GetDevice(log.DeviceId), ConfigConfigurationCancelledEventCode, transactionId, transList);
            Enable(log);

            Logger.Info($"Canceled configuration changes for transaction {log.TransactionId}");
        }

        private void Abort(DbContext context, ConfigChangeLog log, ChangeExceptionErrorCode errorCode)
        {
            log.ChangeStatus = ChangeStatus.Aborted;
            log.ChangeException = errorCode;
            log.ChangeDateTime = DateTime.UtcNow;

            UpdateChangeLog(context, log);

            Finalize(log, ConfigConfigurationAbortedEventCode);

            if (errorCode == ChangeExceptionErrorCode.Timeout)
            {
                SendTimeout(log);
            }

            Logger.Info($"Aborted configuration changes for transaction {log.TransactionId} - {errorCode}");
        }

        private void SendTimeout(ConfigChangeLog log)
        {
            var transactionId = GetTransactionLog(log, out var transList);

            GenerateEvent(
                GetDevice(log.DeviceId),
                ConfigurationAuthorizedByHostTimeOutEventCode,
                transactionId,
                transList);
        }

        private void SendAuthorized(ConfigChangeLog log)
        {
            var device = GetDevice(log.DeviceId);
            if (device == null)
            {
                return;
            }

            var transactionId = GetTransactionLog(log, out var transList);

            GenerateEvent(device, ConfigurationAuthorizedByHostEventCode, transactionId, transList);
        }

        private void Enable(ConfigChangeLog log)
        {
            if (log.DisableCondition == DisableCondition.None)
            {
                return;
            }

            Logger.Debug($"Enabling EGM for transaction {log.TransactionId}");

            var device = GetDevice(log.DeviceId);

            var enabled = _configurationMode.Enabled(device);

            _configurationMode.Exit(
                device,
                log.DisableCondition,
                TimeSpan.MaxValue,
                status =>
                {
                    if (status && enabled)
                    {
                        GenerateEvent(device, EgmNotLockedByDeviceEventCode);
                    }
                });
        }

        private void Authorize(DbContext context, ConfigChangeLog log)
        {
            var device = GetDevice(log.DeviceId);

            SendStatusAsync(log);

            Logger.Info($"Authorized configuration changes for transaction {log.TransactionId}");

            if (log.ApplyCondition == ApplyCondition.Immediate ||
                log.ApplyCondition == ApplyCondition.Disable && _configurationMode.Enabled(device))
            {
                ApplyCommand(context, log);
            }
        }
    }
}