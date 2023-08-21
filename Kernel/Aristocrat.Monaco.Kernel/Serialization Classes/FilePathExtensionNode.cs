namespace Aristocrat.Monaco.Kernel
{
    using System;
    using Mono.Addins;

    /// <summary>
    ///     Definition of the FilePathExtensionNode class.
    /// </summary>
    [CLSCompliant(false)]
    [ExtensionNode("FilePath")]
    public class FilePathExtensionNode : FilterableExtensionNode
    {
        /// <summary>
        ///     Gets or sets the file path.
        /// </summary>
        [NodeAttribute]
        public string FilePath { get; set; }

        /// <summary>
        ///     Gets or sets the friendly/display name.
        /// </summary>
        [NodeAttribute(Required = false)]
        public string Name { get; set; }
    }
}
