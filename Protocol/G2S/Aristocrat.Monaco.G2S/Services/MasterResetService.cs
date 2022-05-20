namespace Aristocrat.Monaco.G2S.Services
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.Events;
    using Data.Model;
    using ExpressMapper;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts.Localization;
    using Localization.Properties;

    /// <summary>
    ///     An <see cref="IMasterResetService" /> implementation
    /// </summary>
    public class MasterResetService : IMasterResetService, IService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IEventBus _bus;
        private readonly IDisableConditionSaga _disableConditionSaga;
        private readonly IPersistentStorageManager _persistentStorage;

        private bool _disposed;
        private CancellationTokenSource _timeoutCancellationTokenSource;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MasterResetService" /> class.
        /// </summary>
        /// <param name="egm">Egm</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance.</param>
        /// <param name="bus">An <see cref="IEventBus" /> instance.</param>
        /// <param name="disableConditionSaga">Disable condition saga.</param>
        /// <param name="persistentStorage">An <see cref="IPersistentStorageManager" /> instance.</param>
        public MasterResetService(
            IG2SEgm egm,
            IEventLift eventLift,
            IEventBus bus,
            IDisableConditionSaga disableConditionSaga,
            IPersistentStorageManager persistentStorage)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _disableConditionSaga =
                disableConditionSaga ?? throw new ArgumentNullException(nameof(disableConditionSaga));

            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public long RequestId { get; private set; }

        /// <inheritdoc />
        public MasterResetStatus Status { get; set; }

        /// <inheritdoc />
        public ICollection<ConfigChangeAuthorizeItem> AuthorizeItems { get; private set; }

        /// <inheritdoc />
        public void Start()
        {
            Logger.Info("Initializing the G2S master reset service.");

            var device = _egm.GetDevice<ICabinetDevice>();
            if (device == null)
            {
                Logger.Error("Cabinet device is not available");
                return;
            }

            _bus.Subscribe<MasterResetCancelEvent>(
                this,
                a =>
                {
                    SetStatus(MasterResetStatus.Canceled);
                    _timeoutCancellationTokenSource?.Cancel(false);
                    _timeoutCancellationTokenSource = null;
                });

            SetAuthorizationTimeout();

            CheckPending();
        }

        public bool HasMasterReset()
        {
            if (RequestId == 0)
            {
                return false;
            }

            return !(Status == MasterResetStatus.Aborted || Status == MasterResetStatus.Canceled);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<cabinet, masterReset> command)
        {
            var cabinet = _egm.GetDevice<ICabinetDevice>();

            var authorizationRequired = false;

            if (command.Command.authorizeList?.authorizeItem != null &&
                command.Command.authorizeList?.authorizeItem?.Length > 0)
            {
                foreach (var item in command.Command.authorizeList.authorizeItem)
                {
                    if (!item.timeoutDateSpecified || item.timeoutDate > DateTime.UtcNow + TimeSpan.FromHours(24))
                    {
                        command.Error.SetErrorCode("GTK_CBX004");
                        return;
                    }
                }

                authorizationRequired = true;
            }

            AddNewMasterResetLog(command.Command, command.IClass.dateTime);

            var response = command.GenerateResponse<masterResetStatus>();
            await BuildStatus(cabinet, response.Command);

            _eventLift.Report(cabinet, EventCode.GTK_CBE005);

            if (authorizationRequired)
            {
                SetAuthorizationTimeout();

                _eventLift.Report(cabinet, EventCode.GTK_CBE010);
            }
            else
            {
                Authorize();
            }
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<cabinet, authorizeMasterReset> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var cabinet = _egm.GetDevice<ICabinetDevice>();

                var authorized = true;

                if (AuthorizeItems != null)
                {
                    foreach (var item in AuthorizeItems)
                    {
                        if (item.HostId == command.HostId && item.AuthorizeStatus != AuthorizationState.Timeout)
                        {
                            if (item.AuthorizeStatus != AuthorizationState.Authorized && command.Command.authorized)
                            {
                                _eventLift.Report(cabinet, EventCode.GTK_CBE011);
                            }

                            item.AuthorizeStatus =
                                command.Command.authorized ? AuthorizationState.Authorized : AuthorizationState.Pending;
                        }

                        authorized &= item.AuthorizeStatus == AuthorizationState.Authorized;
                    }

                    if (authorized)
                    {
                        SetStatus(MasterResetStatus.Authorized);

                        DisableAndStart(cabinet);
                    }
                }

                var response = command.GenerateResponse<masterResetStatus>();

                await BuildStatus(cabinet, response.Command);
            }
        }

        /// <inheritdoc />
        public async Task BuildStatus(ICabinetDevice device, masterResetStatus command)
        {
            BuildStatusCommand(command);

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IMasterResetService) };

        /// <inheritdoc />
        public void Initialize()
        {
            Logger.Info("Initializing the G2S MasterResetService.");
        }

        private void Authorize()
        {
            Task.Delay(100).ContinueWith(
                task =>
                {
                    SetStatus(MasterResetStatus.Authorized);
                    DisableAndStart(_egm.GetDevice<ICabinetDevice>());
                });
        }

        private void BuildStatusCommand(masterResetStatus command)
        {
            if (RequestId != 0)
            {
                command.requestId = RequestId;
                command.masterResetStatus = GetStatus();

                var authorizeItems = AuthorizeItems?.Select(
                    a => new authorizeStatus2
                    {
                        hostId = a.HostId,
                        authorizeStatus = GetAuthorizeStatus(a),
                        timeoutAction = GetTimeoutAction(a),
                        timeoutDateSpecified = a.TimeoutDate.HasValue,
                        timeoutDate = a.TimeoutDate ?? DateTime.MinValue
                    }).ToArray();

                if (authorizeItems != null && authorizeItems.Length > 0)
                {
                    command.authorizeStatusList =
                        new authorizeStatusList2 { authorizeStatus = authorizeItems.ToArray() };
                }
            }
        }

        private t_authorizationStates GetAuthorizeStatus(ConfigChangeAuthorizeItem item)
        {
            return item.AuthorizeStatus == AuthorizationState.Pending
                ? t_authorizationStates.G2S_pending
                : item.AuthorizeStatus == AuthorizationState.Authorized
                    ? t_authorizationStates.G2S_authorized
                    : t_authorizationStates.G2S_timeout;
        }

        private t_timeoutActionTypes GetTimeoutAction(ConfigChangeAuthorizeItem item)
        {
            return item.TimeoutAction == TimeoutActionType.Abort
                ? t_timeoutActionTypes.G2S_abort
                : t_timeoutActionTypes.G2S_ignore;
        }

        private string GetStatus()
        {
            switch (Status)
            {
                case MasterResetStatus.Pending:
                    return @"GTK_pending";
                case MasterResetStatus.Authorized:
                    return @"GTK_authorized";
                case MasterResetStatus.Aborted:
                    return @"GTK_aborted";
                case MasterResetStatus.Canceled:
                    return @"GTK_cancelled";
                case MasterResetStatus.InProcess:
                    return @"GTK_inProcess";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _bus.UnsubscribeAll(this);

                if (_timeoutCancellationTokenSource != null)
                {
                    _timeoutCancellationTokenSource.Cancel(false);
                    _timeoutCancellationTokenSource.Dispose();
                    _timeoutCancellationTokenSource = null;
                }
            }

            _disposed = true;
        }

        private void AddNewMasterResetLog(masterReset command, DateTime dateTime)
        {
            RequestId = command.requestId;

            if (command.authorizeList?.authorizeItem != null)
            {
                var authorizeItems =
                    Mapper.Map<authorizeItem2[], ConfigChangeAuthorizeItem[]>(command.authorizeList.authorizeItem)
                        .ToList();

                // The timeout MUST be within 24 hours of the dateTime specified within the class level element
                authorizeItems.ForEach(a => a.TimeoutDate = GetDateTimeUTC(a.TimeoutDate) ?? dateTime);
                authorizeItems.ForEach(a => a.AuthorizeStatus = AuthorizationState.Pending);

                AuthorizeItems = authorizeItems;
                SetStatus(MasterResetStatus.Pending);
            }
            else
            {
                SetStatus(MasterResetStatus.Authorized);
            }
        }

        private DateTime? GetDateTimeUTC(DateTime? dateTime)
        {
            return dateTime?.ToUniversalTime();
        }

        private void StartReset()
        {
            var cabinet = _egm.GetDevice<ICabinetDevice>();

            if (RequestId != 0 && (Status == MasterResetStatus.Authorized || Status == MasterResetStatus.Pending))
            {
                SetStatus(MasterResetStatus.InProcess);

                _persistentStorage.Clear(PersistenceLevel.Critical);
            }
            else
            {
                _disableConditionSaga.Exit(cabinet, DisableCondition.Immediate, TimeSpan.Zero, a => { });
            }
        }

        private void SetAuthorizationTimeout()
        {
            _timeoutCancellationTokenSource?.Cancel(false);
            _timeoutCancellationTokenSource = null;

            if (RequestId != 0 && Status == MasterResetStatus.Pending && AuthorizeItems != null)
            {
                foreach (var item in AuthorizeItems)
                {
                    if (!item.TimeoutDate.HasValue)
                    {
                        continue;
                    }

                    if (item.TimeoutDate.Value <= DateTime.UtcNow)
                    {
                        TimeOut(item.HostId);
                        break;
                    }

                    if (_timeoutCancellationTokenSource == null)
                    {
                        _timeoutCancellationTokenSource = new CancellationTokenSource();
                    }

                    try
                    {
                        Task.Delay(
                            item.TimeoutDate.Value - DateTime.UtcNow,
                            _timeoutCancellationTokenSource.Token).ContinueWith(
                            a => { TimeOut(item.HostId); },
                            _timeoutCancellationTokenSource.Token);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("SetAuthorizationTimeout failed", ex);
                    }
                }
            }
        }

        private void CheckPending()
        {
            if (RequestId != 0 && (Status == MasterResetStatus.Pending || Status == MasterResetStatus.Authorized) &&
                (AuthorizeItems == null || AuthorizeItems.Count == 0))
            {
                var cabinet = _egm.GetDevice<ICabinetDevice>();

                DisableAndStart(cabinet);
            }
        }

        private void TimeOut(int hostId)
        {
            var cabinet = _egm.GetDevice<ICabinetDevice>();
            if (RequestId != 0 && Status == MasterResetStatus.Pending && AuthorizeItems != null)
            {
                foreach (var item in AuthorizeItems)
                {
                    if (item.TimeoutDate.HasValue)
                    {
                        if (item.HostId == hostId)
                        {
                            item.AuthorizeStatus = AuthorizationState.Timeout;
                            _eventLift.Report(cabinet, EventCode.GTK_CBE012);

                            if (item.TimeoutAction != TimeoutActionType.Ignore)
                            {
                                SetStatus(MasterResetStatus.Aborted);
                            }
                            else
                            {
                                _timeoutCancellationTokenSource?.Cancel(false);
                                _timeoutCancellationTokenSource = null;
                                _eventLift.Report(cabinet, EventCode.GTK_CBE006);
                                DisableAndStart(cabinet);
                            }

                            break;
                        }
                    }
                }
            }
        }

        private void SetStatus(MasterResetStatus status)
        {
            if (Status == status)
            {
                return;
            }

            Status = status;

            Task.Run(() =>
            {
                var cabinet = _egm.GetDevice<ICabinetDevice>();

                var masterResetStatus = new masterResetStatus();
                BuildStatusCommand(masterResetStatus);

                var session = cabinet.SendMasterResetStatus(masterResetStatus);
                session.WaitForCompletion();

                // TODO:  Retry command if it fails

                switch (status)
                {
                    case MasterResetStatus.Authorized:
                        _eventLift.Report(cabinet, EventCode.GTK_CBE006);
                        break;
                    case MasterResetStatus.Aborted:
                        _eventLift.Report(cabinet, EventCode.GTK_CBE008);
                        RequestId = 0;
                        break;
                    case MasterResetStatus.Canceled:
                        _eventLift.Report(cabinet, EventCode.GTK_CBE009);
                        RequestId = 0;
                        break;
                    case MasterResetStatus.InProcess:
                        _eventLift.Report(cabinet, EventCode.GTK_CBE007);
                        break;
                }
            });
        }

        private void DisableAndStart(ICabinetDevice cabinet)
        {
            Task.Run(
                () =>
                {
                    _disableConditionSaga.Enter(
                        cabinet,
                        DisableCondition.ZeroCredits,
                        cabinet.Queue.SessionTimeout,
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MasterResetNotification),
                        success =>
                        {
                            if (success)
                            {
                                StartReset();
                            }
                            else
                            {
                                SetStatus(MasterResetStatus.Aborted);
                            }
                        });
                });
        }
    }

    /// <summary>
    ///     Master reset status
    /// </summary>
    public enum MasterResetStatus
    {
        /// <summary>
        ///     Waiting for authorization.
        /// </summary>
        Pending,

        /// <summary>
        ///     Authorized
        /// </summary>
        Authorized,

        /// <summary>
        ///     Aborted
        /// </summary>
        Aborted,

        /// <summary>
        ///     Cancelled
        /// </summary>
        Canceled,

        /// <summary>
        ///     In process
        /// </summary>
        InProcess
    }
}
