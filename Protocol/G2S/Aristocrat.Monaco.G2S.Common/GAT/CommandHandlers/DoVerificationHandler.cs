namespace Aristocrat.Monaco.G2S.Common.GAT.CommandHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Authentication;
    using Application.Contracts.Localization;
    using Exceptions;
    using Kernel.Contracts.Components;
    using Localization.Properties;
    using Models;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Storage;
    using Storage;

    /// <summary>
    ///     Do verification handler
    /// </summary>
    public class DoVerificationHandler : IParametersFuncHandler<DoVerificationArgs, VerificationStatus>
    {
        private readonly IAuthenticationService _componentHashService;
        private readonly IComponentRegistry _components;
        private readonly IGatComponentVerificationRepository _componentVerificationRepository;
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IIdProvider _idProvider;
        private readonly IGatVerificationRequestRepository _verificationRequestRepository;
        private DoVerificationArgs _parameter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DoVerificationHandler" /> class.
        /// </summary>
        /// <param name="verificationRequestRepository">The verification request repository.</param>
        /// <param name="componentVerificationRepository">The component verification repository.</param>
        /// <param name="components">The component repository.</param>
        /// <param name="componentHashService">The component hash service.</param>
        /// <param name="contextFactory">The context factory.</param>
        /// <param name="idProvider">Id Provider</param>
        public DoVerificationHandler(
            IGatVerificationRequestRepository verificationRequestRepository,
            IGatComponentVerificationRepository componentVerificationRepository,
            IComponentRegistry components,
            IAuthenticationService componentHashService,
            IMonacoContextFactory contextFactory,
            IIdProvider idProvider)
        {
            _verificationRequestRepository = verificationRequestRepository ??
                                             throw new ArgumentNullException(nameof(verificationRequestRepository));
            _componentVerificationRepository = componentVerificationRepository ??
                                               throw new ArgumentNullException(nameof(componentVerificationRepository));
            _components = components ?? throw new ArgumentNullException(nameof(components));
            _componentHashService =
                componentHashService ?? throw new ArgumentNullException(nameof(componentHashService));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
        }

        /// <summary>
        ///     Executes the specified do verification arguments.
        /// </summary>
        /// <param name="parameter">The do verification arguments.</param>
        /// <returns>Verification status</returns>
        public VerificationStatus Execute(DoVerificationArgs parameter)
        {
            _parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));

            using (var context = _contextFactory.Create())
            {
                if (_parameter.VerificationId != 0)
                {
                    var verificationRequest = _verificationRequestRepository.Get(
                        context,
                        x => x.VerificationId == _parameter.VerificationId &&
                             x.DeviceId == _parameter.DeviceId)
                        .Include(v => v.ComponentVerifications).SingleOrDefault();

                    if (verificationRequest != null)
                    {
                        return new VerificationStatus(
                            verificationRequest.VerificationId,
                            verificationRequest.TransactionId,
                            verificationRequest.ComponentVerifications.Select(
                                x => new ComponentStatus(x.ComponentId, x.State)));
                    }

                    verificationRequest =
                        _verificationRequestRepository.GetByVerificationId(context, _parameter.VerificationId);

                    if (verificationRequest != null && verificationRequest.DeviceId != _parameter.DeviceId)
                    {
                        throw new DeviceIdNotCorrespondingVerificationIdException(
                            Localizer.For(CultureFor.Operator).FormatString(ResourceKeys.DeviceIdNotCorrespondingVerificationIdErrorMessageTemplate,
                                _parameter.DeviceId,
                                _parameter.VerificationId));
                    }
                }

                var componentVerificationRequest = SaveVerificationRequest(context, _parameter);

                long transactionId = 0;
                if (componentVerificationRequest != null)
                {
                    transactionId = componentVerificationRequest.TransactionId;
                }

                _parameter.QueueVerifyCallback?.Invoke(transactionId);

                if (componentVerificationRequest != null)
                {
                    foreach (var verifyComponent in componentVerificationRequest.ComponentVerifications)
                    {
                        verifyComponent.State = ComponentVerificationState.InProcess;
                        _componentVerificationRepository.Update(context, verifyComponent);
                    }

                    VerifyNewComponents(context, _parameter.VerificationId, transactionId);
                }

                return new VerificationStatus(
                    _parameter.VerificationId,
                    transactionId,
                    _parameter.VerifyComponents.Select(
                        x => new ComponentStatus(x.ComponentId, ComponentVerificationState.Queued)));
            }
        }

        private GatVerificationRequest SaveVerificationRequest(DbContext context, DoVerificationArgs doVerificationArgs)
        {
            var components = new List<GatComponentVerification>();

            foreach (var verifyComponent in doVerificationArgs.VerifyComponents)
            {
                if (CheckExistComponent(verifyComponent.ComponentId))
                {
                    var componentVerification = new GatComponentVerification
                    {
                        ComponentId = verifyComponent.ComponentId,
                        AlgorithmType = verifyComponent.AlgorithmType,
                        Seed = verifyComponent.Seed.Length > 0 ? BitConverter.GetBytes(int.Parse(verifyComponent.Seed)) : new byte[0],
                        Salt = Convert.FromBase64String(verifyComponent.Salt),
                        StartOffset = verifyComponent.StartOffset,
                        EndOffset = verifyComponent.EndOffset,
                        State = ComponentVerificationState.Queued,
                        Result = new byte[0],
                        GatExec = string.Empty
                    };

                    components.Add(componentVerification);
                }
                else
                {
                    throw new UnknownComponentException(
                        Localizer.For(CultureFor.Operator).FormatString(
                            ResourceKeys.UnknownComponentErrorMessageTemplate,
                            verifyComponent.ComponentId));
                }
            }

            if (components.Count <= 0)
            {
                return null;
            }

            var request = new GatVerificationRequest
            {
                VerificationId = doVerificationArgs.VerificationId,
                TransactionId = _idProvider.GetNextTransactionId(),
                Completed = false,
                Date = DateTime.UtcNow,
                DeviceId = doVerificationArgs.DeviceId,
                EmployeeId = doVerificationArgs.EmployeeId,
                FunctionType = FunctionType.DoVerification,
                ComponentVerifications = components
            };

            _verificationRequestRepository.Add(context, request);

            return request;
        }

        private void VerifyNewComponents(DbContext context, long verificationId, long transactionId)
        {
            _parameter.StartVerifyCallback?.Invoke(transactionId);

            var request = _verificationRequestRepository.GetByVerificationId(context, verificationId);

            var handlers = new List<VerifyComponentHandler>();
            var requestId = 0L;

            var results = _componentVerificationRepository.Get(context, x => x.RequestId == request.Id);
            foreach (var result in results)
            {
                var handler = new VerifyComponentHandler(
                    result,
                    _componentHashService,
                    _components,
                    _componentVerificationRepository,
                    _contextFactory);
                handlers.Add(handler);

                requestId = result.RequestId;
            }

            Task.Run(
                () =>
                {
                    foreach (var handler in handlers)
                    {
                        handler.VerifyComponent();
                    }

                    VerificationComplete(requestId);
                });
        }

        private void VerificationComplete(long requestId)
        {
            using (var context = _contextFactory.Create())
            {
                var gatVerificationRequest = _verificationRequestRepository.Get(context, requestId);
                if (gatVerificationRequest != null)
                {
                    gatVerificationRequest.Completed = true;
                    _verificationRequestRepository.Update(context, gatVerificationRequest);

                    _parameter.TransactionId = gatVerificationRequest.TransactionId;
                    _parameter.VerificationCallback?.Invoke(_parameter);
                }
            }
        }

        private bool CheckExistComponent(string componentId)
        {
            return _components.Components.Any(x => x.ComponentId == componentId);
        }
    }
}
