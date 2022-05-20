namespace Aristocrat.Monaco.Bootstrap
{
    using Mono.Addins;

    /// <summary>
    ///     Extension node used for extension point where components
    ///     can specify the arguments, valid values and descriptions
    ///     of the command line arguments they use
    /// </summary>
    [ExtensionNode("CommandLineArgument")]
    [ExtensionNodeChild(typeof(ArgumentValueNode))]
    public class CommandLineArgumentExtensionNode : ExtensionNode
    {
        /// <summary>
        ///     Gets or sets the name of the argument
        /// </summary>
        [NodeAttribute]
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the collection of valid values for this command line argument
        /// </summary>
        [NodeAttribute]
        public ArgumentValueNode[] ValidValues { get; set; }
    }
}
