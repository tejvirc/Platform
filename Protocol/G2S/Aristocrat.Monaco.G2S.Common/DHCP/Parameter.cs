namespace Aristocrat.Monaco.G2S.Common.DHCP
{
    /// <summary>
    ///     Contains DHCP vendor specific parameter information.
    /// </summary>
    public class Parameter
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Parameter" /> class.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="value">Parameter value.</param>
        public Parameter(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        ///     Gets the DHCP vendor specific information parameter name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the DHCP vendor specific information parameter value.
        /// </summary>
        public string Value { get; }
    }
}