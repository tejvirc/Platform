namespace Aristocrat.Monaco.Kernel
{
    #region Usings

    using System;
    using System.Xml.Serialization;
    using Mono.Addins;

    #endregion

    /// <summary>
    ///     Definition of the NodeSpecificationNode class.
    /// </summary>
    [CLSCompliant(false)]
    [ExtensionNode("NodeSpecification")]
    public class NodeSpecificationNode : ExtensionNode
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NodeSpecificationNode" /> class.
        /// </summary>
        public NodeSpecificationNode()
            : this(null, int.MaxValue, null, null)
        {
            // int.MaxValue ensures that nodes with a specified order come before those without
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NodeSpecificationNode" /> class.
        /// </summary>
        /// <param name="addinId">The addin that the specified node belongs to</param>
        /// <param name="order">The ordered index of this node</param>
        /// <param name="typeName">The full name of this specification's object type</param>
        /// <param name="filterId">The the extension's unique identifier</param>
        public NodeSpecificationNode(string addinId, int order, string typeName, string filterId)
        {
            AddinId = addinId;
            Order = order;
            TypeName = typeName;
            FilterId = filterId;
        }

        /// <summary>
        ///     Gets or sets the addin that the specified node belongs to
        /// </summary>
        [NodeAttribute("addinId")]
        [XmlAttribute("addinId")]
        public string AddinId { get; set; }

        /// <summary>
        ///     Gets or sets the ordered index of this node,
        ///     if an order was not specified, int.MaxValue is returned
        /// </summary>
        [NodeAttribute("order")]
        [XmlAttribute("order")]
        public int Order { get; set; }

        /// <summary>
        ///     Gets or sets the full name of this specification's object type.
        /// </summary>
        [NodeAttribute("typeName")]
        [XmlAttribute("typeName")]
        public string TypeName { get; set; }

        /// <summary>
        ///     Gets or sets the extension's identifier, which is unique for the extension point.
        /// </summary>
        [NodeAttribute("filterId")]
        [XmlAttribute("filterId")]
        public string FilterId { get; set; }
    }
}
