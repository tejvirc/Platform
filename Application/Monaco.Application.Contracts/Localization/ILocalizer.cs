namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using System;
    using System.Globalization;

    /// <summary>
    ///     Retrieves localized resources.
    /// </summary>
    public interface ILocalizer
    {
        /// <summary>
        ///     Gets the current culture.
        /// </summary>
        CultureInfo CurrentCulture { get; }

        /// <summary>
        ///     Gets the localized resource for a given key based on the current language in scope.
        /// </summary>
        /// <returns>Returns a new <see cref="CultureScope"/> instance.</returns>
        CultureScope NewScope();

        /// <summary>
        ///     Gets the localized resource for a given key based on the current language in scope.
        /// </summary>
        /// <typeparam name="TResource">The type of the return value.</typeparam>
        /// <param name="key">The key used to lookup the resource.</param>
        /// <returns>Returns the localized resource for the specified key.</returns>
        TResource GetObject<TResource>(string key);

        /// <summary>
        ///     Gets the localized resource for a given key based on the current language in scope.
        /// </summary>
        /// <typeparam name="TResource">The type of the return value.</typeparam>
        /// <param name="key">The key used to lookup the resource.</param>
        /// <param name="exceptionHandler">Optional handler for exceptions. If this value is null, an exception will be thrown.</param>
        /// <returns>Returns the localized resource for the specified key.</returns>
        TResource GetObject<TResource>(string key, Action<Exception> exceptionHandler);

        /// <summary>
        ///     Gets the localized resource for a given key based on the current language in scope.
        /// </summary>
        /// <typeparam name="TResource">The type of the return value.</typeparam>
        /// <param name="culture">The target culture.</param>
        /// <param name="key">The key used to lookup the resource.</param>
        /// <returns>Returns the localized resource for the specified key.</returns>
        TResource GetObject<TResource>(CultureInfo culture, string key);

        /// <summary>
        ///     Gets the localized resource for a given key based on the current language in scope.
        /// </summary>
        /// <typeparam name="TResource">The type of the return value.</typeparam>
        /// <param name="culture">The target culture.</param>
        /// <param name="key">The key used to lookup the resource.</param>
        /// <param name="exceptionHandler">Optional handler for exceptions. If this value is null, an exception will be thrown.</param>
        /// <returns>Returns the localized resource for the specified key.</returns>
        TResource GetObject<TResource>(CultureInfo culture, string key, Action<Exception> exceptionHandler);

        /// <summary>
        ///     Gets the localized string value for a given key based on the current language in scope.
        /// </summary>
        /// <param name="key">The key to lookup the text value.</param>
        /// <returns>Returns the localized string for the specified key.</returns>
        string GetString(string key);

        /// <summary>
        ///     Gets the localized string value for a given key based on the current language in scope.
        /// </summary>
        /// <param name="key">The key to lookup the text value.</param>
        /// <param name="exceptionHandler">Optional handler for exceptions. If this value is null, an exception will be thrown.</param>
        /// <returns>Returns the localized string for the specified key.</returns>
        string GetString(string key, Action<Exception> exceptionHandler);

        /// <summary>
        ///     Gets the localized string value for a given key based on the current language in scope.
        /// </summary>
        /// <param name="culture">The target culture.</param>
        /// <param name="key">The key to lookup the text value.</param>
        /// <returns>Returns the localized string for the specified key.</returns>
        string GetString(CultureInfo culture, string key);

        /// <summary>
        ///     Gets the localized string value for a given key based on the current language in scope.
        /// </summary>
        /// <param name="culture">The target culture.</param>
        /// <param name="key">The key to lookup the text value.</param>
        /// <param name="exceptionHandler">Optional handler for exceptions. If this value is null, an exception will be thrown.</param>
        /// <returns>Returns the localized string for the specified key.</returns>
        string GetString(CultureInfo culture, string key, Action<Exception> exceptionHandler);

        /// <summary>
        ///     Gets the localized string value for a given key based on the current language in scope. Use this
        ///     method when the string returned needs to be formatted.
        /// </summary>
        /// <param name="key">The key used to lookup the resource.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>Returns the localized string for the specified key.</returns>
        string FormatString(string key, params object[] args);

        /// <summary>
        ///     Gets the localized string value for a given key based on the current language in scope. Use this
        ///     method when the string returned needs to be formatted.
        /// </summary>
        /// <param name="key">The key used to lookup the resource.</param>
        /// <param name="culture">The target culture.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>Returns the localized string for the specified key.</returns>
        string FormatString(CultureInfo culture, string key, params object[] args);
    }
}
