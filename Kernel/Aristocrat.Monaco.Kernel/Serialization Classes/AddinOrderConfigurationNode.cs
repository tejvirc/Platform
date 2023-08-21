namespace Aristocrat.Monaco.Kernel
{
    using System.Xml.Serialization;

    /// <summary>
    ///     Definition of the AddinOrderNode class.
    /// </summary>
    [XmlRoot("AddinOrderConfiguration")]
    public class AddinOrderConfigurationNode
    {
        /// <summary>Gets or sets the ordered addin collection</summary>
        [XmlElement("AddinOrdering")]
        public AddinOrderingNode[] AddinOrderNodes { get; set; }
    }
}
