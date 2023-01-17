﻿namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using System;
    using Kernel;
    using Kernel.Contracts.MessageDisplay;

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

        /// <summary>
        ///     Get the localized message from the resource file according to the key and
        /// the culture provider name, if culture provider name is left empty, CultureFor.Operator
        /// will be the default value.
        /// </summary>
        public static string GetString(string key, CultureProviderType providerType) => For(providerType.ToString()).GetString(key);

        /// <summary>
        ///     Get the localized message from the resource file according to the key and
        /// the culture provider name, if culture provider name is left empty, CultureFor.Operator
        /// will be the default value.
        /// </summary>
        public static string GetString(string key, CultureProviderType providerType, Action<Exception> exceptionHandler) => For(providerType.ToString()).GetString(key, exceptionHandler);
    }
}
