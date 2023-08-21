namespace Aristocrat.Mgam.Client.Attribute
{
    using System;

    /// <summary>
    ///     Stores server attributes on the VLT.
    /// </summary>
    public interface IAttributeCache
    {
        /// <summary>
        ///     Gets or sets the attribute for a given name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        object this[string name] { get; set; }

        /// <summary>
        ///     Add an attribute to the cache.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        bool TryAddAttribute(string name, object value);

        /// <summary>
        ///     Check if an attribute is in the cache.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <returns>True if the attribute with the given name has been added.</returns>
        bool ContainsAttribute(string name);

        /// <summary>
        ///     Gets the attribute value for the give name.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <param name="value">The value stored in the cache.</param>
        /// <returns>True if the value was store. Otherwise, False.</returns>
        bool TryGetAttribute(string name, out object value);

        /// <summary>
        ///     Subscribes to attribute changes for attribute with the given name.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <param name="observer"><see cref="IObserver{TValue}"/></param>
        /// <returns>A subscription instance that should be unsubscribed.</returns>
        IDisposable Subscribe<TValue>(string name, IObserver<TValue> observer);
    }
}
