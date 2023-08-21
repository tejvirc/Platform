namespace Aristocrat.Monaco.Kernel
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    ///     Definition of the OrderedAddinNode class.
    /// </summary>
    public class AddinOrderingNode
    {
        /// <summary>The list of ordered addins</summary>
        private readonly List<OrderedAddinTypeNode> _orderedAddinList = new List<OrderedAddinTypeNode>();

        /// <summary>Gets or sets the extension path being ordered</summary>
        [XmlAttribute("extensionPath")]
        public string ExtensionPath { get; set; }

        /// <summary>Gets or sets the default behavior for addins that are found and not part of the list</summary>
        [XmlAttribute("defaultBehavior")]
        public DefaultOrderedAddinBehavior DefaultOrderedAddinBehavior { get; set; }

        /// <summary>Gets or sets the addin elements</summary>
        [XmlElement("OrderedAddinType")]
        public OrderedAddinTypeNode[] OrderedAddinsTypes
        {
            get
            {
                var items = new OrderedAddinTypeNode[_orderedAddinList.Count];
                _orderedAddinList.CopyTo(items);
                return items;
            }

            set
            {
                _orderedAddinList.Clear();

                if (value != null)
                {
                    _orderedAddinList.InsertRange(0, value);
                }
            }
        }
    }
}
