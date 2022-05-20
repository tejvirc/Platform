namespace Aristocrat.Monaco.Asp.Client.Devices
{
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using Contracts;

    public class DeviceType : INamedId
    {
        [XmlElement(ElementName = "Parameter")]
        public List<ParameterPrototype> InternalParameters = new List<ParameterPrototype>();

        public IReadOnlyList<IParameterPrototype> Parameters => InternalParameters;

        public IParameterPrototype this[int paramId]
        {
            get { return InternalParameters.Find(x => x.Id == paramId); }
        }

        [XmlAttribute(AttributeName = "ID")]
        public int Id { get; set; }

        [XmlAttribute]
        public string Name { get; set; }
    }
}