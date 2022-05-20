namespace Aristocrat.Mgam.Client.Attribute
{
    /// <summary>
    ///     Defines a attribute returned from the site controller.
    /// </summary>
    public struct AttributeItem
    {
        /// <summary>
        ///     Gets or sets the scope.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public AttributeScope Scope { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        public string Value { get; set; }
    }
}
