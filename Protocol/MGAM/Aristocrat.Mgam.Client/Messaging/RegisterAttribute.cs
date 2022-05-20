namespace Aristocrat.Mgam.Client.Messaging
{
    using Attribute;

    /// <summary>
    ///     The RegisterAttribute message is used to register an attribute with the site controller.
    /// </summary>
    public class RegisterAttribute : Request, IInstanceId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }

        /// <summary>
        ///     Gets and sets the attribute scope.
        /// </summary>
        public AttributeScope Scope { get; set; }

        /// <summary>
        ///     Gets or sets the attribute item name.
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        ///     Gets or sets the attribute item value.
        /// </summary>
        public string ItemValue { get; set; }

        /// <summary>
        ///     Gets or sets the attribute minimum value.
        /// </summary>
        public string Minimum { get; set; }

        /// <summary>
        ///     Gets or sets the attribute maximum value.
        /// </summary>
        public string Maximum { get; set; }

        /// <summary>
        ///     Gets the attribute allowed values.
        /// </summary>
        public string[] AllowedValues { get; set; }

        /// <summary>
        ///     Gets or sets the attribute value type.
        /// </summary>
        public AttributeValueType Type { get; set; }

        /// <summary>
        ///     Gets or sets the attribute control type.
        /// </summary>
        public AttributeControlType ControlType { get; set; }

        /// <summary>
        ///     Gets or sets the attribute access type.
        /// </summary>
        public AttributeAccessType AccessType { get; set; }
    }
}
