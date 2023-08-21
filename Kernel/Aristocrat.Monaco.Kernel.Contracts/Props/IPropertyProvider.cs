namespace Aristocrat.Monaco.Kernel
{
    using System.Collections.Generic;

    /// <summary>
    ///     Definition of the IPropertyProvider interface, which provides access to stored properties used throughout the
    ///     system.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Objects that implement this interface are known as Property Providers and are contained by the Properties
    ///         Manager.
    ///         Typically,
    ///         Property Providers are not used outside the context of the Properties Manager. They service requests to store
    ///         and
    ///         retrieve
    ///         properties, which are objects paired with unique names. Often, but not always, a Property Provider component
    ///         uses a
    ///         block of
    ///         persistent storage to retain its property values when the system is re-booted.
    ///     </para>
    ///     <para>
    ///         In the bootstrap code for each layer in XSpin, a Mono.Addins extension point is defined and components that
    ///         implement the
    ///         IPropertyProvider interface are discovered and added to the Properties Manager.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     Shown below are 2 snippets of XML contained in Mono.Addins configuration files (*.addin.xml). The first defines an
    ///     extension point,
    ///     and the second defines a type of object to be loaded at the extension point. The path attributes must match, and
    ///     the
    ///     type of the
    ///     extension must derive from the object type specified for the extension point.
    ///     <code>
    ///  {ExtensionPoint path = "/Application/PropertyProviders"}
    ///      {ExtensionNode objectType="Vgt.Client12.Kernel.IPropertyProvider"/}
    ///  {/ExtensionPoint}
    /// 
    /// 
    /// 
    ///  {Extension path="/Application/PropertyProviders"}
    ///      {Type type="Vgt.Client12.Application.SystemPropertiesProvider" /}
    ///  {/Extension}
    ///  </code>
    /// </example>
    /// <seealso cref="IPropertiesManager" />
    public interface IPropertyProvider
    {
        /// <summary>
        ///     Gets a reference to a collection of properties stored by the property provider
        /// </summary>
        /// <returns>A read-only reference to a collection of key/value pairs. The keys are property names.</returns>
        ICollection<KeyValuePair<string, object>> GetCollection { get; }

        /// <summary>
        ///     Get an existing property value.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>an object which contains the property.</returns>
        /// <exception cref="UnknownPropertyException">
        ///     Thrown when the propertyName is not recognized, and the provider has
        ///     no valid path of execution.
        /// </exception>
        /// <remarks>
        ///     Prior to calling this method, GetCollection should be checked to see if it contains the property name. If not,
        ///     GetProperty will throw an
        ///     exception and should not be called.
        /// </remarks>
        object GetProperty(string propertyName);

        /// <summary>
        ///     Set a property to a value.
        /// </summary>
        /// <param name="propertyName">The property to set.</param>
        /// <param name="propertyValue">The new value for the property.</param>
        /// <exception cref="UnknownPropertyException">
        ///     Thrown when the propertyName is not recognized, and the provider has
        ///     no valid path of execution.
        /// </exception>
        /// <remarks>
        ///     Some property providers allow new properties to be added, while others don't. Typically, those that are Mono.Addins
        ///     extensions and those that
        ///     keep their properties in persistent storage are in the latter category. If the user of this class is intending to
        ///     update a pre-existing property,
        ///     GetCollection should be checked to see if it contains the property name.
        /// </remarks>
        void SetProperty(string propertyName, object propertyValue);
    }
}