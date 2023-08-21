namespace Aristocrat.Mgam.Client.Protocol
{
    using System;

    /// <summary>
    ///     Serializes and deserializes xml messages.
    /// </summary>
    public interface IXmlMessageSerializer
    {
        /// <summary>
        ///     Serializes the <see cref="XmlMessage"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns>Serialized message</returns>
        string Serialize(XmlMessage message);

        /// <summary>
        ///     Tries to serializes the <see cref="XmlMessage"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="xml"></param>
        /// <returns>True serialization succeeded, otherwise false.</returns>
        bool TrySerialize(XmlMessage message, out string xml);

        /// <summary>
        ///     Deserialize the XML string to a <see cref="XmlMessage"/>.
        /// </summary>
        /// <param name="xml"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        XmlMessage Deserialize(string xml);

        /// <summary>
        ///     Tries to deserialize the XML string to a <see cref="XmlMessage"/>
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="message"></param>
        /// <returns>True if deserialization succeeded, otherwise false.</returns>
        bool TryDeserialize(string xml, out XmlMessage message);
    }
}
