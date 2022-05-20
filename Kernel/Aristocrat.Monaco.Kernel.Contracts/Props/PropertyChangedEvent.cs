namespace Aristocrat.Monaco.Kernel
{
    using System;

    /// <summary>
    ///     This event signals that a property has been added to the Properties Manager or a new property value has been set.
    /// </summary>
    [Serializable]
    public class PropertyChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertyChangedEvent" /> class.
        /// </summary>
        /// <remarks>
        ///     The no argument constructor is necessary to work with the EventSerializer and KeyConverter
        /// </remarks>
        public PropertyChangedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertyChangedEvent" /> class.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed</param>
        public PropertyChangedEvent(string propertyName)
        {
            PropertyName = propertyName;
        }

        /// <summary>
        ///     Gets or sets the name of the property that has changed
        /// </summary>
        public string PropertyName { get; set; }
    }
}