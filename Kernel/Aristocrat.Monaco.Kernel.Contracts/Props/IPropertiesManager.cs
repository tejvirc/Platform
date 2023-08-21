namespace Aristocrat.Monaco.Kernel
{
    /// <summary>
    ///     An interface through which a property value can be accessed using a string as a unique property name.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The Properties Manager service is one of the primary data sharing mechanisms in XSpin. Its function is pairing
    ///         objects with
    ///         unique string names. This allows any component with a knowledge of property names to get access to stored
    ///         objects
    ///         or properties.
    ///         The Kernel layer bootstrap uses Mono-Addins and associated configuration files to locate the components
    ///         implementing this interface
    ///         and add them to the Service Manager. Components that need to use the Properties Manager service to get or set
    ///         properties, or to add
    ///         themselves as property providers, must obtain the Properties Manager object from the Service Manager and access
    ///         it
    ///         through this interface.
    ///     </para>
    ///     <para>
    ///         The Properties Manager manages <see cref="IPropertyProvider" /> objects and facilitates the access to their
    ///         stored
    ///         properties. Components
    ///         in the system make the Properties Manager aware of properties either by using
    ///         <see cref="AddPropertyProvider" /> to
    ///         add a property provider
    ///         or by setting a property that does not currently exist in any of the property providers. In the first case,
    ///         when a
    ///         property provider gets
    ///         added, properties in the provider become visible to the Properties Manager. However, properties subsequently
    ///         added
    ///         to the provider will NOT
    ///         be visible to the Properties Manager. In the second case, if a component sets a property that is unknown to the
    ///         Properties Manager, it gets
    ///         added to the DefaultPropertyProvider, which can be thought of as an in-memory property provider. The
    ///         DefaultPropertyProvider retains its
    ///         properties for the duration of the current system boot, but values will not be restored if the system is
    ///         re-booted.
    ///         Once a property is added
    ///         to the Properties Manager there is no mechanism for removal.
    ///     </para>
    ///     <para>
    ///         The Properties Manager will post a <see cref="PropertyChangedEvent" /> to the EventBus when new properties are
    ///         added or a new non-null property value is
    ///         different than the previous. Components can subscribe to this event, determine the name of the property that
    ///         was
    ///         has changed, and respond
    ///         accordingly.
    ///     </para>
    ///     <para>
    ///         Creating a component that will be a property provider makes the most sense when the component has a need to
    ///         store
    ///         its properties in persistent
    ///         storage. However, there is not a requirement for property providers to store their properties in persistent
    ///         storage.
    ///     </para>
    /// </remarks>
    public interface IPropertiesManager : IService
    {
        /// <summary>
        ///     Add a property provider.
        /// </summary>
        /// <param name="provider">
        ///     An object that implements the IPropertyProperty interfaces and provides properties to be added
        ///     to the Properties Manager.
        /// </param>
        /// <remarks>
        ///     The Properties Manager will obtain the properties in the given Property Provider and make them accessible via its
        ///     interface.
        ///     Properties subsequently added to the provider will NOT be visible to the Properties Manager.
        /// </remarks>
        /// <example>
        ///     Shown below is code that adds a Property Provider to the Properties Manager. This code is in an object that passes
        ///     a
        ///     reference to itself.
        ///     Therefore, it must implement the IPropertyProvider interface.
        ///     <code>
        /// IPropertiesManager propertiesManager = ServiceManager.GetInstance().GetService{IPropertiesManager}();
        /// propertiesManager.AddPropertyProvider(this);
        /// </code>
        /// </example>
        void AddPropertyProvider(IPropertyProvider provider);

        /// <summary>
        ///     Gets a property. If the property is not found, the default value will be returned.
        /// </summary>
        /// <param name="propertyName">The name of the property to get</param>
        /// <param name="defaultValue">
        ///     The default value for the property if it is not
        ///     currently defined.
        /// </param>
        /// <returns>The object associated with the property name</returns>
        /// <remarks>
        ///     Typically, a cast is applied to the object returned. Therefore, users must know the type of the property in
        ///     addition
        ///     to its unique name.
        /// </remarks>
        /// <example>
        ///     The code below shows how to get two properties from the Properties Manager. In both cases, the names of the
        ///     properties were previously
        ///     assigned to string constants. The first property returned is either an object previously stored in the Properties
        ///     Manager or an empty
        ///     string if the property is not found. It is cast as a string and assigned to machineId. The second property returned
        ///     is either an object
        ///     previously stored or zero if the property is not found. It is cast as an int and assigned to serialNumber. In both
        ///     cases, if the object
        ///     returned cannot be cast to the expected type, an InvalidCastException gets thrown. Typically, the default values
        ///     will
        ///     be outside of the
        ///     range of expected values and will indicate that a property was not found.
        ///     <code>
        /// IPropertiesManager propManager = ServiceManager.GetInstance().GetService{IPropertiesManager}();
        /// string machineId = (string)propManager.GetProperty(MachineIdKey, string.Empty);
        /// int serialNumber = (int)propManager.GetProperty(SerialNumberKey, 0);
        /// </code>
        /// </example>
        /// <seealso cref="IPropertyProvider" />
        object GetProperty(string propertyName, object defaultValue);

        /// <summary>
        ///     Sets a property value.
        /// </summary>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="propertyValue">The value to set the property to.</param>
        /// <remarks>
        ///     To set a property, a component must supply a unique string identifier for the property and a value. The Properties
        ///     Manager will either set the
        ///     property or create a new entry in the DefaultPropertyProvider. If a new property is created or if there is a change
        ///     in the value of an existing
        ///     property, the Properties Manager will post a <see cref="PropertyChangedEvent" />.
        /// </remarks>
        /// <example>
        ///     The code below shows how to set two properties in the Properties Manager. In both cases, the names of the
        ///     properties
        ///     were previously
        ///     assigned to string constants. The first property is a string with a value of "123". It will be paired with the name
        ///     assigned to MachineIdKey
        ///     and stored as an object. Code that uses <see cref="GetProperty" /> to retrieve the property will need to apply a
        ///     string cast. The second
        ///     property is an integer value of 123. It will be paired with the name assigned to SerialNumberKey and stored as an
        ///     object. When GetProperty
        ///     is used to retrieve the property, a cast that can be applied to a numeric value must be used.
        ///     <code>
        /// IPropertiesManager propManager = ServiceManager.GetInstance().GetService{IPropertiesManager}();
        /// propManager.SetProperty(MachineIdKey, "123");
        /// propManager.SetProperty(SerialNumberKey, 123);
        /// </code>
        /// </example>
        void SetProperty(string propertyName, object propertyValue);

        /// <summary>
        ///     Sets a property value.
        /// </summary>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="propertyValue">The value to set the property to.</param>
        /// <param name="isConfig">Indicates whether or not this is a property used for configuration.</param>
        /// <remarks>
        ///     To set a property, a component must supply a unique string identifier for the property and a value. The Properties
        ///     Manager will either set the
        ///     property or create a new entry in the DefaultPropertyProvider. If a new property is created or if there is a change
        ///     in the value of an existing
        ///     property, the Properties Manager will post a <see cref="PropertyChangedEvent" />.
        /// </remarks>
        /// <example>
        ///     The code below shows how to set two properties in the Properties Manager. In both cases, the names of the
        ///     properties
        ///     were previously
        ///     assigned to string constants. The first property is a string with a value of "123". It will be paired with the name
        ///     assigned to MachineIdKey
        ///     and stored as an object. Code that uses <see cref="GetProperty" /> to retrieve the property will need to apply a
        ///     string cast. The second
        ///     property is an integer value of 123. It will be paired with the name assigned to SerialNumberKey and stored as an
        ///     object. When GetProperty
        ///     is used to retrieve the property, a cast that can be applied to a numeric value must be used.
        ///     <code>
        /// IPropertiesManager propManager = ServiceManager.GetInstance().GetService{IPropertiesManager}();
        /// propManager.SetProperty(MachineIdKey, "123");
        /// propManager.SetProperty(SerialNumberKey, 123);
        /// </code>
        /// </example>
        void SetProperty(string propertyName, object propertyValue, bool isConfig);
    }
}