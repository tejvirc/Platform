namespace Aristocrat.Mgam.Client.Attribute
{
    /// <summary>
    ///     Attribute event args.
    /// </summary>
    public class AttributeEventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AttributeEventArgs"/> class.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The attribute value.</param>
        public AttributeEventArgs(string name, object value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        ///     Gets the attribute name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the attribute value.
        /// </summary>
        public object Value { get; }
    }
}
