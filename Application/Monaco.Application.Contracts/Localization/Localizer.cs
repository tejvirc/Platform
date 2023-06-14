namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using Kernel;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     Provides convenience methods for localization services.
    /// </summary>
    public static class Localizer
    {
        private static ILocalizerFactory _factory;
        private static IOperatorMenuLauncher _operatorMenu;
        private static IPropertiesManager _properties;
        private static string _lockupCulture;

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
        ///     Gets the localizer provider to dynamically update the string based on whether the audit menu is open.
        ///     This is used for strings that can be displayed both in the audit menu and in the game/lobby
        ///     (primarily lockups) because these two places can have different language settings.     
        /// </summary>
        /// <returns>Operator culture provider when in operator menu; Player culture provider when in game or lobby</returns>
        public static ILocalizer DynamicCulture()
        {
            _factory ??= ServiceManager.GetInstance().GetService<ILocalizerFactory>();
            _operatorMenu ??= ServiceManager.GetInstance().GetService<IOperatorMenuLauncher>();

            return For(
                _operatorMenu?.IsShowing ?? false
                    ? CultureFor.Operator
                    : CultureFor.Player);
        }

        /// <summary>
        ///     Gets the localizer provider specified in the configuration for lockups
        /// </summary>
        /// <returns>The provider specified for lockups</returns>
        public static ILocalizer ForLockup()
        {
            _properties ??= ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _lockupCulture ??= (string)_properties.GetProperty(ApplicationConstants.LockupCulture, CultureFor.Operator);

            return For(_lockupCulture);
        }
    }
}
