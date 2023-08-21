namespace Aristocrat.Monaco.Bootstrap
{
    using Mono.Addins;

    /// <summary>
    ///     Definition of the ArgumentValueNode class.
    /// </summary>
    [ExtensionNode("ArgumentValue")]
    public class ArgumentValueNode : ExtensionNode
    {
        /// <summary>
        ///     Gets or sets the valid value for an argument type. The valid value is arbitrary string text and
        ///     can be used to represent anything like ranges for example, 1-99.
        /// </summary>
        [NodeAttribute]
        public string ValidValue { get; set; }

        /// <summary>
        ///     Gets or sets the description of the valid value
        /// </summary>
        [NodeAttribute]
        public string Description { get; set; }
    }
}
