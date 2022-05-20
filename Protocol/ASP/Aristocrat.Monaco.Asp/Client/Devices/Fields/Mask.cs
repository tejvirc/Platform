namespace Aristocrat.Monaco.Asp.Client.Devices.Fields
{
    using System.Xml.Serialization;
    using Contracts;
    using MaskOperationMap =
        System.Collections.Generic.Dictionary<Contracts.MaskOperation, (System.Func<int, int, int> Set,
            System.Func<int, int, int> UnSet)>;

    public class Mask : IMask
    {
        [XmlAttribute(AttributeName = "Operation")]
        public MaskOperation MaskOperation { get; set; }

        [XmlAttribute(AttributeName = "Value")]
        public int Value { get; set; }

        [XmlAttribute(AttributeName = "TrueText")]
        public string TrueText { get; set; }

        [XmlAttribute(AttributeName = "FalseText")]
        public string FalseText { get; set; }

        [XmlAttribute(AttributeName = "DataMemberName")]
        public string DataMemberName { get; set; }
    }
}