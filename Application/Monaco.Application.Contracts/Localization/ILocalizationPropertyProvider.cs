namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using System;
    using Kernel;

    /// <summary>
    ///     Extends <see cref="IPropertyProvider"/> interface to allow properties to be added.
    /// </summary>
    public interface ILocalizationPropertyProvider : IPropertyProvider
    {
        /// <summary>
        ///     Add a new property to the properties collection.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="callback">The property set callback function.</param>
        void AddProperty<T>(string propertyName, T propertyValue, Action<object> callback = null);
    }
}
