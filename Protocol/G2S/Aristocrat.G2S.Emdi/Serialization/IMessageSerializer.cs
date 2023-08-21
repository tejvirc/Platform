namespace Aristocrat.G2S.Emdi.Serialization
{
    using Protocol.v21ext1b1;

    /// <summary>
    /// Interfaced used to serialize/deserialize an <see cref="mdMsg"/>
    /// </summary>
    public interface IMessageSerializer
    {
        /// <summary>
        /// Serializes the <see cref="mdMsg"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="System.Exception"></exception>
        /// <returns>Serialized message</returns>
        string Serialize(mdMsg message);

        /// <summary>
        /// Tries to deserialize the XML string to a <see cref="mdMsg"/>
        /// </summary>
        /// <param name="xmlMessage"></param>
        /// <param name="message"></param>
        /// <returns>True if deserialization succeeded, otherwise false.</returns>
        bool TryDeserialize(string xmlMessage, out mdMsg message);

        /// <summary>
        /// Deserialize the XML string to a <see cref="mdMsg"/>.
        /// </summary>
        /// <param name="xml"></param>
        /// <exception cref="System.Exception"></exception>
        /// <returns></returns>
        mdMsg Deserialize(string xml);

        /// <summary>
        /// Tries to serializes the <see cref="mdMsg"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="xml"></param>
        /// <returns>True serialization succeeded, otherwise false.</returns>
        bool TrySerialize(mdMsg message, out string xml);
    }
}