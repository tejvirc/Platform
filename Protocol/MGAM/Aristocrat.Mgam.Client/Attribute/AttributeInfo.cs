namespace Aristocrat.Mgam.Client.Attribute
{
    /// <summary>
    ///     Defines an attribute.
    /// </summary>
    public struct AttributeInfo
    {
        /// <summary>
        ///     Gets or sets the Scope of the attribute.
        /// </summary>
        public AttributeScope Scope { get; set; }

        /// <summary>
        ///     Gets of sets the name of the attribute.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets are sets the types of the item name.
        /// </summary>
        public AttributeValueType Type { get; set; }

        /// <summary>
        ///     Gets or sets the minimum the attribute item value.
        /// </summary>
        /// <remarks>
        ///     Optional minimum for numeric attributes.
        /// </remarks>
        public object Minimum { get; set; }

        /// <summary>
        ///     Gets or sets the maximum the attribute item value.
        /// </summary>
        /// <remarks>
        ///     Optional maximum for numeric attributes.
        /// </remarks>
        public object Maximum { get; set; }

        /// <summary>
        ///     Gets or sets the GUI control type for editing or displaying this attribute.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public AttributeControlType ControlType { get; set; }

        /// <summary>
        ///     Gets are sets the allowed types for ItemValue.
        /// </summary>
        public AttributeAccessType AccessType { get; set; }

        /// <summary>
        ///     Gets or sets the default value of the attribute.
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        ///     Gets are sets the allowed values.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string[] AllowedValues { get; set; }
    }
}
