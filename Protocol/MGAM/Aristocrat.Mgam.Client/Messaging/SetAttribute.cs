namespace Aristocrat.Mgam.Client.Messaging
{
    using Attribute;

    /// <summary>
    ///     When the VLT needs to change an attribute, it uses this message to inform the site controller of
    ///     the new value.
    /// </summary>
    public class SetAttribute : Request, IInstanceId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }

        /// <summary>
        ///     Gets the attribute scope.
        /// </summary>
        public AttributeScope Scope { get; set; }

        /// <summary>
        ///     Gets the attribute name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets the attribute value.
        /// </summary>
        public string Value { get; set; }
    }
}
