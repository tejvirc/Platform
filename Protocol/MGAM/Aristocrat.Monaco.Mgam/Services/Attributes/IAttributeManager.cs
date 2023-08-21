namespace Aristocrat.Monaco.Mgam.Services.Attributes
{
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client.Attribute;

    public interface IAttributeManager
    {
        /// <summary>
        ///     Add a vendor defined attribute.
        /// </summary>
        /// <param name="attribute"><see cref="AttributeInfo"/>.</param>
        void Add(AttributeInfo attribute);

        /// <summary>
        ///     Check for a vendor defined attribute.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <returns>True if the attribute with the given name has been added.</returns>
        bool Has(string name);

        /// <summary>
        ///     Sets the attribute value.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <param name="value">The new attribute value.</param>
        /// <param name="behavior">Determines if the nae value should be set on the server as well.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">Thrown if the attribute with the given name has not been added.</exception>
        void Set(string name, object value, AttributeSyncBehavior behavior = AttributeSyncBehavior.LocalOnly);

        /// <summary>
        ///     Gets the attribute value.
        /// </summary>
        /// <typeparam name="TValue">The type of the attribute value.</typeparam>
        /// <param name="name">Attribute name.</param>
        /// <param name="defaultValue">The value to return if the value for the attribute is not set.</param>
        /// <returns>The current value for the attribute with the given name.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">Thrown if the attribute with the given name has not been added.</exception>
        TValue Get<TValue>(string name, TValue defaultValue = default);

        /// <summary>
        ///     Syncs attribute values with the server.
        /// </summary>
        /// <returns><see cref="Task"/>.</returns>
        Task Update();
    }
}