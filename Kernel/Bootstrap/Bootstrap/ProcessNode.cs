namespace Aristocrat.Monaco.Bootstrap
{
    using Mono.Addins;

    /// <summary>
    ///     Defines a process node that can be used to terminate a dependent given process at startup
    /// </summary>
    [ExtensionNode("Process")]
    public class ProcessNode : ExtensionNode
    {
        /// <summary>
        ///     Gets or sets the process name
        /// </summary>
        [NodeAttribute]
        public string Name { get; set; }
    }
}