namespace Aristocrat.Monaco.G2S
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts;
    using Application.Contracts.Authentication;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.Events;
    using Common.GAT.CommandHandlers;
    using Common.GAT.Models;
    using Common.GAT.Storage;
    using Handlers.Gat;
    using Kernel;
    using Kernel.Contracts.Components;
    using log4net;
    using Monaco.Common.Models;
    using Monaco.Common.Storage;
    using IGatDevice = Aristocrat.G2S.Client.Devices.IGatDevice;

    /// <summary>
    ///     Implementation of GAT Service
    /// </summary>
    public class GatService : IGatService, IDisposable
    {
        private readonly IAuthenticationService _componentHashService;
        private readonly IComponentRegistry _componentRegistry;
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IG2SEgm _egm;
        private readonly IEventBus _eventBus;
        private readonly IEventLift _eventLift;
        private readonly IGatComponentVerificationRepository _gatComponentVerificationRepository;
        private readonly IGatSpecialFunctionRepository _gatSpecialFunctionRepository;
        private readonly IGatVerificationRequestRepository _gatVerificationRequestRepository;
        private readonly IIdProvider _idProvider;
        private bool _disposed;

        private bool _started;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Initializes a new instance of the <see cref="GatService" /> class.
        /// </summary>
        /// <param name="componentRegistry">Component repository</param>
        /// <param name="gatVerificationRequestRepository">Verification request repository</param>
        /// <param name="gatComponentVerificationRepository">Component verification repository</param>
        /// <param name="gatSpecialFunctionRepository">Special function repository</param>
        /// <param name="componentHashService">Component hash service</param>
        /// <param name="contextFactory">An <see cref="IMonacoContextFactory" /> instance.</param>
        /// <param name="idProvider">An <see cref="IIdProvider" /> instance.</param>
        /// <param name="eventBus">An <see cref="IEventBus" /> </param>
        /// <param name="egm">EGM.</param>
        /// <param name="eventLift">An <see cref="IEventLift" /></param>
        public GatService(
            IComponentRegistry componentRegistry,
            IGatVerificationRequestRepository gatVerificationRequestRepository,
            IGatComponentVerificationRepository gatComponentVerificationRepository,
            IGatSpecialFunctionRepository gatSpecialFunctionRepository,
            IAuthenticationService componentHashService,
            IMonacoContextFactory contextFactory,
            IIdProvider idProvider,
            IEventBus eventBus,
            IG2SEgm egm,
            IEventLift eventLift)
        {
            _componentRegistry = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));
            _gatVerificationRequestRepository = gatVerificationRequestRepository ??
                                                throw new ArgumentNullException(
                                                    nameof(gatVerificationRequestRepository));
            _gatComponentVerificationRepository = gatComponentVerificationRepository ??
                                                  throw new ArgumentNullException(
                                                      nameof(gatComponentVerificationRepository));
            _gatSpecialFunctionRepository = gatSpecialFunctionRepository ??
                                            throw new ArgumentNullException(nameof(gatSpecialFunctionRepository));
            _componentHashService =
                componentHashService ?? throw new ArgumentNullException(nameof(componentHashService));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));

            _started = false;

            eventBus.Subscribe<CommunicationsStateChangedEvent>(this, HandleEvent);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public bool HasVerificationId(long verificationId)
        {
            using (var context = _contextFactory.Create())
            {
                return _gatVerificationRequestRepository.GetByVerificationId(context, verificationId) != null;
            }
        }

        /// <inheritdoc />
        public bool HasTransactionId(long transactionId)
        {
            using (var context = _contextFactory.Create())
            {
                return _gatVerificationRequestRepository.GetByTransactionId(context, transactionId) != null;
            }
        }

        /// <inheritdoc />
        public GatVerificationRequest GetVerificationRequestById(long verificationId)
        {
            using (var context = _contextFactory.Create())
            {
                return _gatVerificationRequestRepository.GetByVerificationId(context, verificationId);
            }
        }

        /// <inheritdoc />
        public IEnumerable<Component> GetComponentList()
        {
            var handler = new GetComponentsHandler(_componentRegistry);
            return handler.Execute();
        }

        /// <inheritdoc />
        public Component GetComponent(string componentId)
        {
            return _componentRegistry.Components.FirstOrDefault(c => c.ComponentId == componentId);
        }

        /// <inheritdoc />
        public VerificationStatus DoVerification(DoVerificationArgs initVerificationArgs)
        {
            var handler = new DoVerificationHandler(
                _gatVerificationRequestRepository,
                _gatComponentVerificationRepository,
                _componentRegistry,
                _componentHashService,
                _contextFactory,
                _idProvider);

            return handler.Execute(initVerificationArgs);
        }

        /// <inheritdoc />
        public VerificationStatusResult GetVerificationStatus(
            GetVerificationStatusByTransactionArgs getVerificationStatusByTransactionArgs)
        {
            var handler = new GetVerificationStatusByTransactionHandler(
                _gatVerificationRequestRepository,
                _gatComponentVerificationRepository,
                _contextFactory);

            return handler.Execute(getVerificationStatusByTransactionArgs);
        }

        /// <inheritdoc />
        public VerificationStatusResult GetVerificationStatus(
            GetVerificationStatusByDeviceArgs getVerificationStatusByDeviceArgs)
        {
            var handler = new GetVerificationStatusByDeviceHandler(
                _gatVerificationRequestRepository,
                _gatComponentVerificationRepository,
                _contextFactory);
            return handler.Execute(getVerificationStatusByDeviceArgs);
        }

        /// <inheritdoc />
        public void SaveVerificationAck(SaveVerificationAckArgs saveVerificationAckArgs)
        {
            var handler = new SaveVerificationAckHandler(
                _gatVerificationRequestRepository,
                _gatComponentVerificationRepository,
                _contextFactory);
            handler.Execute(saveVerificationAckArgs);
        }

        /// <inheritdoc />
        public SaveEntityResult SaveComponent(Component componentEntity)
        {
            var handler = new SaveComponentHandler(_componentRegistry);
            return handler.Execute(componentEntity);
        }

        /// <inheritdoc />
        public IEnumerable<GatSpecialFunction> GetSpecialFunctions()
        {
            var handler = new GetSpecialFunctionsHandler(_gatSpecialFunctionRepository, _contextFactory);
            return handler.Execute();
        }

        /// <inheritdoc />
        public SaveEntityResult SaveSpecialFunction(GatSpecialFunction gatSpecialFunctionEntity)
        {
            var handler = new SaveSpecialFunctionHandler(_gatSpecialFunctionRepository, _contextFactory);
            return handler.Execute(gatSpecialFunctionEntity);
        }

        /// <inheritdoc />
        public GatVerificationRequest GetLogForTransactionId(long transactionId)
        {
            using (var context = _contextFactory.Create())
            {
                return _gatVerificationRequestRepository.GetByTransactionId(context, transactionId);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IAlgorithm> GetSupportedAlgorithms(ComponentType type)
        {
            var handler = new GetSupportedAlgorithmsHandler(type);
            return handler.Execute();
        }

        /// <inheritdoc />
        public GetLogStatusResult GetLogStatus()
        {
            var handler = new GetLogStatusHandler(_gatVerificationRequestRepository, _contextFactory);
            return handler.Execute();
        }

        /// <inheritdoc />
        public void DeleteComponent(string componentId, ComponentType type)
        {
            _componentRegistry.UnRegister(componentId);
        }

        /// <inheritdoc />
        public GatComponentVerification GetGatComponentVerificationEntity(string componentId, long verificationId)
        {
            using (var context = _contextFactory.Create())
            {
                var request = _gatVerificationRequestRepository.GetByVerificationId(context, verificationId);

                return _gatComponentVerificationRepository.GetByComponentIdAndVerificationId(
                    context,
                    componentId,
                    request.Id);
            }
        }

        /// <inheritdoc />
        public void UpdateGatComponentVerificationEntity(GatComponentVerification entity)
        {
            using (var context = _contextFactory.Create())
            {
                _gatComponentVerificationRepository.Update(context, entity);
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

        private void HandleEvent(CommunicationsStateChangedEvent evt)
        {
            if (!evt.Online || _started)
            {
                return;
            }

            _started = true;

            var devices = _egm?.GetDevices<IGatDevice>();
            var device = devices?.FirstOrDefault(a => a.Id == evt.HostId);

            if (device == null)
            {
                return;
            }

            List<GatVerificationRequest> results;

            try
            {
                using (var context = _contextFactory.Create())
                {
                    results = _gatVerificationRequestRepository.GetAll(context).Include(a => a.ComponentVerifications).ToList();
                }
            }
            catch (Exception e)
            {
                // the original exception here was caught under VLT-6868. This fixes that immediate issue, but we added the logging to post the
                // exception so we can document why the exception is occuring.
                // The setting result to null sends the process back to the HostConfig page and user can continue to edit/delete hosts
                var ex = e.InnerException;
                Log.Error("GatService.HandleEvent() - " + ex?.InnerException);
                results = null;
            }

            if(results == null)
            {
                return;
            }

            foreach (var result in results)
            {
                if (!result.Completed)
                {
                    result.Completed = true;
                    foreach (var req in result.ComponentVerifications)
                    {
                        req.State = ComponentVerificationState.Error;

                        UpdateGatComponentVerificationEntity(req);
                    }

                    using (var context = _contextFactory.Create())
                    {
                        _gatVerificationRequestRepository.Update(context, result);
                    }

                    var transactionList = GetTransactionList(result.TransactionId, device);

                    _eventLift.Report(
                    device,
                    EventCode.G2S_GAE104,
                    result.TransactionId,
                    transactionList);
                }
            }
        }

        private transactionList GetTransactionList(
            long transactionId,
            IGatDevice device)
        {
            GatVerificationRequest log;

            using (var context = _contextFactory.Create())
            {
                log = _gatVerificationRequestRepository.GetByTransactionId(context, transactionId);
            }

            if (log == null)
            {
                return null;
            }

            var gatLog = GatEnumExtensions.GetGatLog(log);

            var info = new transactionInfo
            {
                Item = gatLog,
                deviceId = device.Id,
                deviceClass = device.PrefixedDeviceClass()
            };

            var transList = new transactionList
            {
                transactionInfo = new[] { info }
            };

            return transList;
        }
    }
}
