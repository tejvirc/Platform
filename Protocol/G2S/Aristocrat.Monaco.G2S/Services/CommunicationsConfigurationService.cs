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
    using Data.CommConfig;
    using Data.Model;
    using Data.Profile;
    using ExpressMapper;
    using Handlers;
    using Handlers.CommConfig;
    using Kernel;
    using Monaco.Common.Scheduler;
    using Monaco.Common.Storage;
    using Newtonsoft.Json;
    using Constants = G2S.Constants;
    using Host = G2S.Host;

    /// <summary>
    ///     Service for applying changes for commConfig.
    /// </summary>
    public class CommunicationsConfigurationService : BaseConfigurationService
    {
        private readonly ICommChangeLogRepository _changeLogRepository;
        private readonly IHostFactory _hostFactory;
        private readonly IProfileService _profiles;
        private readonly IPropertiesManager _properties;
        private readonly ICommandBuilder<ICommConfigDevice, commChangeStatus> _statusBuilder;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommunicationsConfigurationService" /> class.
        /// </summary>
        /// <param name="egm">The egm.</param>
        /// <param name="configurationMode">The configuration mode.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="profiles">The profiles.</param>
        /// <param name="bus">The bus.</param>
        /// <param name="hostFactory">The host factory.</param>
        /// <param name="statusBuilder">The status builder.</param>
        /// <param name="contextFactory">The context factory.</param>
        /// <param name="changeLogRepository">The change log repository.</param>
        /// <param name="taskScheduler">The task scheduler.</param>
        /// <param name="eventLift">The event lift.</param>
        public CommunicationsConfigurationService(
            IG2SEgm egm,
            IDisableConditionSaga configurationMode,
            IPropertiesManager properties,
            IProfileService profiles,
            IEventBus bus,
            IHostFactory hostFactory,
            ICommandBuilder<ICommConfigDevice, commChangeStatus> statusBuilder,
            IMonacoContextFactory contextFactory,
            ICommChangeLogRepository changeLogRepository,
            ITaskScheduler taskScheduler,
            IEventLift eventLift)
            : base(egm, configurationMode, bus, contextFactory, taskScheduler, eventLift)
        {
            _changeLogRepository = changeLogRepository ?? throw new ArgumentNullException(nameof(changeLogRepository));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
            _hostFactory = hostFactory ?? throw new ArgumentNullException(nameof(hostFactory));
            _statusBuilder = statusBuilder ?? throw new ArgumentNullException(nameof(statusBuilder));
        }

        /// <inheritdoc />
        protected override string ConfigurationAuthorizedByHostEventCode => EventCode.G2S_CCE104;

        /// <inheritdoc />
        protected override string EgmLockedByDeviceEventCode => EventCode.G2S_CCE007;

        /// <inheritdoc />
        protected override string EgmNotLockedByDeviceEventCode => EventCode.G2S_CCE008;

        /// <inheritdoc />
        protected override string ConfigurationChangesAppliedEventCode => EventCode.G2S_CCE106;

        /// <inheritdoc />
        protected override string DeviceConfigErrorEventCode => EventCode.G2S_CCE108;

        /// <inheritdoc />
        protected override string ConfigConfigurationCancelledEventCode => EventCode.G2S_CCE105;

        /// <inheritdoc />
        protected override string ConfigConfigurationAbortedEventCode => EventCode.G2S_CCE107;

        /// <inheritdoc />
        protected override string ConfigurationAuthorizedByHostTimeOutEventCode => EventCode.G2S_CCE110;

        /// <inheritdoc />
        protected override ConfigChangeLog GetChangeLog(DbContext context, long transactionId)
        {
            return _changeLogRepository.GetPendingByTransactionId(context, transactionId);
        }

        /// <inheritdoc />
        protected override void UpdateChangeLog(DbContext context, ConfigChangeLog changeLog)
        {
            _changeLogRepository.Update(context, changeLog as CommChangeLog);
        }

        /// <inheritdoc />
        protected override void SendStatus(ConfigChangeLog log)
        {
            var device = Egm.GetDevice<ICommConfigDevice>();

            var status = new commChangeStatus
            {
                configurationId = log.ConfigurationId, transactionId = log.TransactionId
            };

            _statusBuilder.Build(device, status);

            device.CommChangeStatus(status);
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
            Logger.Debug($"Applying communication configuration changes for transaction {changeLog.TransactionId}");

            try
            {
                var request = (ChangeCommConfigRequest)JsonConvert.DeserializeObject(
                    changeLog.ChangeData,
                    typeof(ChangeCommConfigRequest));

                var current = _properties.GetValues<IHost>(Constants.RegisteredHosts).ToList();

                var hosts = new List<IHost>();

                var contexts = current.Select(h => new StartupContext { HostId = h.Id }).ToList();

                hosts.AddRange(RegisterHosts(changeLog, request.SetHostItems, contexts));

                // Any host that was not included in the setHostItems needs to be added to the collection
                hosts.AddRange(current.Where(h => hosts.All(a => a.Index != h.Index)).ToList());

                // Remove any hosts that were explicitly unregistered
                var removed = new List<IHost>();

                foreach (var hostItem in
                    request.SetHostItems.Where(i => !i.HostRegistered && i.HostIndex != Aristocrat.G2S.Client.Constants.EgmHostIndex))
                {
                    var host = hosts.FirstOrDefault(h => h.Index == hostItem.HostIndex);

                    if (host == null)
                    {
                        host = current.FirstOrDefault(h => h.Index == hostItem.HostIndex);

                        if (host != null)
                        {
                            if (host.Registered && current.Count - hosts.Count >= 1)
                            {
                                hosts.Remove(host);
                                current.Remove(host);

                                _hostFactory.Delete(host);
                                removed.Add(host);
                            }
                        }

                        continue;
                    }

                    if (hostItem.HostRegistered || !host.Registered)
                    {
                        continue;
                    }

                    _hostFactory.Delete(host);
                    removed.Add(host);
                }

                hosts.RemoveAll(h => removed.Any(r => r.Id == h.Id));

                _properties.SetProperty(Constants.RegisteredHosts, hosts);

                SetDeviceState(request.SetHostItems, contexts);

                base.ApplyCommand(context, changeLog);

                if (!changeLog.RestartAfter)
                {
                    Logger.Debug(
                        $"Communication configuration changes for transaction {changeLog.TransactionId} applied: Restarting comms");

                    // This is soft reset of the comms only as opposed to a restart of the EGM
                    Egm.Restart(contexts.Where(c => c.DeviceChanged));

                    // Restarting comms will call NotifyConfigurationChanged() within the communications device,
                    //  but if we didn't restart comms we need to close out this process with the communications device event notification(s)
                    foreach (var ctx in contexts)
                    {
                        var communications = Egm.GetDevice<ICommunicationsDevice>(ctx.HostId);
                        communications?.NotifyConfigurationChanged();
                    }
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
            return Egm.GetDevice<ICommConfigDevice>();
        }

        /// <inheritdoc />
        protected override long GetTransactionLog(ConfigChangeLog log, out transactionList transactionList)
        {
            var device = Egm.GetDevice<ICommConfigDevice>();

            var commChangeLog = log as CommChangeLog;
            var changeLog = Mapper.Map<CommChangeLog, commChangeLog>(commChangeLog);

            var authorizeItems = commChangeLog?.AuthorizeItems?.Select(
                a => new authorizeStatus
                {
                    hostId = a.HostId,
                    authorizationState =
                        (t_authorizationStates)Enum.Parse(
                            typeof(t_authorizationStates),
                            $"G2S_{a.AuthorizeStatus.ToString()}",
                            true),
                    timeoutDateSpecified = a.TimeoutDate.HasValue,
                    timeoutDate = (a.TimeoutDate ?? DateTime.MinValue).UtcDateTime
                }).ToArray();

            if (authorizeItems != null && authorizeItems.Length > 0)
            {
                changeLog.authorizeStatusList = new authorizeStatusList { authorizeStatus = authorizeItems.ToArray() };
            }

            var info = new transactionInfo
            {
                deviceId = device.Id, deviceClass = device.PrefixedDeviceClass(), Item = changeLog
            };

            transactionList = new transactionList { transactionInfo = new[] { info } };
            return log.TransactionId;
        }

        private IEnumerable<IHost> RegisterHosts(
            ConfigChangeLog changeLog,
            IEnumerable<SetHostItem> request,
            ICollection<StartupContext> contexts)
        {
            var registered = new List<IHost>();

            var hosts = _properties.GetValues<IHost>(Constants.RegisteredHosts).ToList();

            var setHostItems = request as IList<SetHostItem> ?? request.ToList();

            var newRegistration = false;

            foreach (var hostItem in setHostItems.Where(
                i => i.HostRegistered && i.HostIndex != Aristocrat.G2S.Client.Constants.EgmHostIndex))
            {
                var host = hosts.FirstOrDefault(h => h.Index == hostItem.HostIndex);

                if (host == null)
                {
                    host = _hostFactory.Create(
                        new Host
                        {
                            Index = hostItem.HostIndex,
                            Id = hostItem.HostId,
                            Address = new Uri(hostItem.HostLocation),
                            Registered = true
                        });

                    contexts.Add(
                        new StartupContext
                        {
                            HostId = host.Id,
                            DeviceChanged = true,
                            DeviceStateChanged = true,
                            DeviceAccessChanged = true,
                            SubscriptionLost = true,
                            MetersReset = true,
                            DeviceReset = true
                        });

                    newRegistration = true;
                }
                else
                {
                    host = _hostFactory.Update(
                        new Host { Index = hostItem.HostIndex, Address = new Uri(hostItem.HostLocation) });
                }

                registered.Add(host);

                var communications = Egm.GetDevice<ICommunicationsDevice>(host.Id);
                communications.Configure(
                    changeLog.ConfigurationId,
                    hostItem.UseDefaultConfig,
                    hostItem.RequiredForPlay,
                    hostItem.TimeToLive,
                    hostItem.NoResponseTimer,
                    hostItem.DisplayCommFault);
                _profiles.Save(communications);
            }

            if (newRegistration)
            {
                contexts.ToList().ForEach(
                    c =>
                    {
                        c.DeviceChanged = true;
                        c.DeviceStateChanged = true;
                    });
            }

            foreach (var hostItem in setHostItems.Where(
                i => i.HostRegistered && i.HostIndex != Aristocrat.G2S.Client.Constants.EgmHostIndex))
            {
                var host = registered.FirstOrDefault(h => h.Index == hostItem.HostIndex);
                if (host != null)
                {
                    ApplyPermissions(host, hostItem, contexts.Single(c => c.HostId == host.Id));
                }
            }

            return registered;
        }

        private void ApplyPermissions(IHost host, SetHostItem setHostItem, StartupContext context)
        {
            if (ApplyOwnership(host, setHostItem))
            {
                context.DeviceChanged = true;
                context.DeviceAccessChanged = true;
            }

            if (ApplyConfigurator(host, setHostItem))
            {
                context.DeviceChanged = true;
                context.DeviceAccessChanged = true;
            }

            if (ApplyGuest(host, setHostItem))
            {
                context.DeviceChanged = true;
                context.DeviceAccessChanged = true;
            }

            Logger.Debug($"Applied permissions to Host {host.Id}");
        }

        private void SetDeviceState(
            IEnumerable<SetHostItem> request,
            ICollection<StartupContext> contexts)
        {
            foreach (var setHostItem in request)
            {
                if (setHostItem.OwnedDevices == null)
                {
                    continue;
                }

                var statusChanged = Egm.Devices.Where(
                    d => setHostItem.OwnedDevices.Any(
                        o => o.DeviceId == d.Id && o.DeviceClass == d.PrefixedDeviceClass() &&
                             o.DeviceActive != d.Active));

                foreach (var device in statusChanged)
                {
                    device.SetStatus(false);
                    _profiles.Save(device);

                    foreach (var context in contexts)
                    {
                        context.DeviceChanged = true;
                        context.DeviceStateChanged = true;
                    }
                }
            }
        }

        private bool ApplyOwnership(IHost host, SetHostItem setHostItem)
        {
            var changed = false;

            var egm = Egm.GetHostById(Aristocrat.G2S.Client.Constants.EgmHostId);

            var revoked = Egm.Devices.Where(
                d => d.IsOwner(host.Id) && (setHostItem.OwnedDevices == null ||
                                            !setHostItem.OwnedDevices.Any(
                                                o => o.DeviceId == d.Id && o.DeviceClass == d.PrefixedDeviceClass())
                    ));

            foreach (var device in revoked)
            {
                device.Owner(egm, Egm);
                _profiles.Save(device);
                changed = true;
            }

            if (setHostItem.OwnedDevices != null)
            {
                var granted = Egm.Devices.Where(
                    d => !d.IsOwner(host.Id) && setHostItem.OwnedDevices.Any(
                        o => o.DeviceId == d.Id && o.DeviceClass == d.PrefixedDeviceClass()));

                foreach (var device in granted)
                {
                    device.Owner(host, Egm);
                    _profiles.Save(device);
                    changed = true;
                }
            }

            return changed;
        }

        private bool ApplyConfigurator(IHost host, SetHostItem setHostItem)
        {
            var changed = false;

            var egm = Egm.GetHostById(Aristocrat.G2S.Client.Constants.EgmHostId);

            var revoked = Egm.Devices.Where(
                d => d.IsConfigurator(host.Id) && (setHostItem.ConfigDevices == null ||
                                                   !setHostItem.ConfigDevices.Any(
                                                       c =>
                                                           c.DeviceId == d.Id &&
                                                           c.DeviceClass == d.PrefixedDeviceClass())));

            foreach (var device in revoked)
            {
                device.Configurator(egm);
                _profiles.Save(device);
                changed = true;
            }

            if (setHostItem.ConfigDevices != null)
            {
                var granted = Egm.Devices.Where(
                    d => !d.IsConfigurator(host.Id) &&
                         setHostItem.ConfigDevices.Any(
                             c => d.Id == c.DeviceId && c.DeviceClass == d.PrefixedDeviceClass()));

                foreach (var device in granted)
                {
                    device.Configurator(host);
                    _profiles.Save(device);
                    changed = true;
                }
            }

            return changed;
        }

        private bool ApplyGuest(IHost host, SetHostItem setHostItem)
        {
            var changed = false;

            var revoked = Egm.Devices.Where(
                d => d.IsGuest(host.Id) && (setHostItem.GuestDevices == null ||
                                            !setHostItem.GuestDevices.Any(
                                                g => g.DeviceId == d.Id && g.DeviceClass == d.PrefixedDeviceClass())
                    ));

            foreach (var device in revoked)
            {
                device.Uninvited(host);
                _profiles.Save(device);
                changed = true;
            }

            if (setHostItem.GuestDevices != null)
            {
                var granted = Egm.Devices.Where(
                    d => !d.IsGuest(host.Id) &&
                         setHostItem.GuestDevices.Any(
                             g => g.DeviceId == d.Id && g.DeviceClass == d.PrefixedDeviceClass()));

                foreach (var device in granted)
                {
                    device.Invited(host);
                    _profiles.Save(device);
                    changed = true;
                }
            }

            return changed;
        }
    }
}