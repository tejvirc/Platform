namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using Kernel;

    /// <summary>
    ///     Provides convenience methods for localization services.
    /// </summary>
    public static class Localizer
    {
        /// <summary>
        /// The type of culture provider
        /// </summary>
        public enum CultureProviderType
        {
            /// <summary>
            ///     Used to get current culture for operator.
            /// </summary>
            Operator = 0,
            /// <summary>
            ///     Used to target the operator culture provider.
            /// </summary>
            OperatorTicket = 1,
            /// <summary>
            ///     Used to target the player culture provider.
            /// </summary>
            Player = 2,

            /// <summary>
            ///     Used to target the player ticket culture provider.
            /// </summary>
            PlayerTicket = 3,

            /// <summary>
            ///     Used to target the currency culture provider.
            /// </summary>
            Currency = 4
    }

        private static ILocalizerFactory _factory;

        /// <summary>
        ///     Gets the provider.
        /// </summary>
        /// <param name="name">The provider name.</param>
        /// <returns>The provider with the specified name.</returns>
        public static ILocalizer For(string name)
        {
            _factory ??= ServiceManager.GetInstance().GetService<ILocalizerFactory>();

            return _factory.For(name);
        }

        /// <summary>
        ///     Get the localized message from the resource file according to the key and
        /// the culture provider name, if culture provider name is left empty, CultureFor.Operator
        /// will be the default value.
        /// </summary>
        public static string GetString(string key, CultureProviderType providerType=CultureProviderType.Operator) => For(providerType.ToString()).GetString(key);
    }
}
