namespace Aristocrat.Monaco.Mgam.Common.Events
{
    using Kernel;

    /// <summary>
    ///     Published when an attribute value has changed.
    /// </summary>
    public class AttributeChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AttributeChangedEvent"/> class.
        /// </summary>
        /// <param name="attributeName">Name of attribute that was updated</param>
        /// <param name="updatedFromServer">Whether this update originated from the server</param>
        public AttributeChangedEvent(string attributeName, bool updatedFromServer = false)
        {
            AttributeName = attributeName;
            UpdatedFromServer = updatedFromServer;
        }

        /// <summary>
        ///     Gets the name of the attribute that changed.
        /// </summary>
        public string AttributeName { get; }

        /// <summary>
        ///     Gets whether the updated originated from the server
        /// </summary>
        public bool UpdatedFromServer { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{GetType().FullName} (Name: {AttributeName})]";
        }
    }
}