namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using Kernel;

    /// <summary>
    ///     Provides convenience methods for localization services.
    /// </summary>
    public static class Localizer
    {
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
    }
}
