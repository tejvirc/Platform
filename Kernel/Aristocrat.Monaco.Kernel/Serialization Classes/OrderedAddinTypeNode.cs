namespace Aristocrat.Monaco.Kernel
{
    using System.Xml.Serialization;

    /// <summary>
    ///     Definition of the OrderedAddinEntry class.
    /// </summary>
    public class OrderedAddinTypeNode
    {
        /// <summary>Gets or sets the type of the addin being ordered</summary>
        [XmlAttribute("type")]
        public string TypeString { get; set; }
    }
}
