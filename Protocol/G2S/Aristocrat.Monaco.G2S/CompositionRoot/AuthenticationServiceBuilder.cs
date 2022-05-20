namespace Aristocrat.Monaco.G2S.CompositionRoot
{
    using System;
    using Common.GAT.Storage;
    using SimpleInjector;

    /// <summary>
    ///     Handles configuring the Authentication Service.
    /// </summary>
    internal static class AuthenticationServiceBuilder
    {
        /// <summary>
        ///     Registers the authentication service with the container.
        /// </summary>
        /// <param name="this">The container.</param>
        /// <param name="connectionString">The connection string.</param>
        internal static void RegisterAuthenticationService(this Container @this, string connectionString)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            @this.Register<IGatVerificationRequestRepository, GatVerificationRequestRepository>(Lifestyle.Singleton);
            @this.Register<IGatComponentVerificationRepository, GatComponentVerificationRepository>(
                Lifestyle.Singleton);
            @this.Register<IGatSpecialFunctionRepository, GatSpecialFunctionRepository>(Lifestyle.Singleton);

            @this.Register<IGatService, GatService>(Lifestyle.Singleton);
        }
    }
}
