namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    ///     Interface for message factory responsible for Serialization and Deserialization of messages received.
    ///     This depends on converters to serialize/deserialize.
    /// </summary>
    public interface IMessageFactory
    {
        /// <summary>
        ///     Serializes Request message into bytes.
        /// </summary>
        /// <param name="message">Request message which needs to be Serialized.</param>
        /// <returns>Serialized bytes[] for the given message.</returns>
        byte[] Serialize(Request message);

        /// <summary>
        ///     DeSerializes bytes into Response.
        /// </summary>
        /// <param name="data">Bytes which needs to be deserialized.</param>
        /// <returns>Deserialized Response</returns>
        Response Deserialize(byte[] data);
    }
}