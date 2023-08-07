namespace Aristocrat.Monaco.G2S.Common.GAT.CommandHandlers
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts.Authentication;
    using Kernel.Contracts.Components;
    using log4net;
    using Monaco.Common.Storage;
    using Storage;

    /// <summary>
    ///     Verify Component
    /// </summary>
    public class VerifyComponentHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IAuthenticationService _componentHashService;
        private readonly IComponentRegistry _components;
        private readonly GatComponentVerification _componentVerification;
        private readonly IGatComponentVerificationRepository _componentVerificationRepository;
        private readonly IMonacoContextFactory _contextFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="VerifyComponentHandler" /> class.
        /// </summary>
        /// <param name="componentVerification">The component verification.</param>
        /// <param name="componentHashService">The component hash service.</param>
        /// <param name="components">The component repository.</param>
        /// <param name="componentVerificationRepository">The component verification repository.</param>
        /// <param name="contextFactory">The context factory.</param>
        public VerifyComponentHandler(
            GatComponentVerification componentVerification,
            IAuthenticationService componentHashService,
            IComponentRegistry components,
            IGatComponentVerificationRepository componentVerificationRepository,
            IMonacoContextFactory contextFactory)
        {
            _componentVerification = componentVerification;
            _componentHashService = componentHashService;
            _components = components;
            _componentVerificationRepository = componentVerificationRepository;
            _contextFactory = contextFactory;
        }

        /// <summary>
        ///     Verifies the component.
        /// </summary>
        public void VerifyComponent()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var component =
                    _components.Components.SingleOrDefault(x => x.ComponentId == _componentVerification.ComponentId);

                if (component == null)
                {
                    return;
                }

                Verify(context, component);
            }
        }

        private void Verify(DbContext context, Component component)
        {
            _componentVerification.State = ComponentVerificationState.InProcess;
            _componentVerificationRepository.Update(context, _componentVerification);

            Logger.Debug($"Verifying component {component?.ComponentId} - {component?.Description}");

            try
            {
                _componentHashService.CalculateHash(component, _componentVerification);
                _componentVerification.State = ComponentVerificationState.Complete;

                Logger.Debug(
                    $"Component {component?.ComponentId} hash completed with result: {_componentVerification?.Result} for algorithm {_componentVerification?.AlgorithmType}");
            }
            catch (Exception ex)
            {
                Logger.Error("CalculateHash failed", ex);

                _componentVerification.State = ComponentVerificationState.Error;
            }

            _componentVerificationRepository.Update(context, _componentVerification);
        }
    }
}