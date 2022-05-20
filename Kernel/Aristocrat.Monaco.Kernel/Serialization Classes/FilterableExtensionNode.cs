namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Xml.Serialization;
    using Mono.Addins;

    /// <summary>
    ///     Definition of the FilterableExtensionNode class.
    /// </summary>
    [CLSCompliant(false)]
    [ExtensionNode("FilterableExtensionNode")]
    public class FilterableExtensionNode : ExtensionNode
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FilterableExtensionNode" /> class.
        /// </summary>
        public FilterableExtensionNode()
            : this(null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FilterableExtensionNode" /> class.
        /// </summary>
        /// <param name="filterId">The the extension's unique identifier</param>
        public FilterableExtensionNode(string filterId)
        {
            FilterId = filterId;
        }

        /// <summary>
        ///     Gets or sets the extension's identifier, which is unique for the extension point.
        /// </summary>
        [NodeAttribute("filterId")]
        [XmlAttribute("filterId")]
        public string FilterId { get; set; }
    }
}
