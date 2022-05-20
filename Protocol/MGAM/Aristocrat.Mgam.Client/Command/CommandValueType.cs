namespace Aristocrat.Mgam.Client.Command
{
    using System.ComponentModel;

    /// <summary>
    ///     Attribute ItemValue types.
    /// </summary>
    public enum CommandValueType
    {
        /// <summary>Integer value type.</summary>
        [Description("int")]
        Integer,

        /// <summary>Decimal value type (double precision).</summary>
        [Description("double")]
        Decimal,

        /// <summary>String value type.</summary>
        [Description("string")]
        String,

        /// <summary>Boolean value type.</summary>
        [Description("bool")]
        Boolean,

        /// <summary>Void value type.</summary>
        [Description("void")]
        Void
    }
}
