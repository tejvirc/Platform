namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    /// <summary>
    ///     Converts bytes received on the network into a response message structure.
    /// </summary>
    public interface IResponseConverter<out TMessage> where TMessage : Response
    {
        /// <summary>
        ///     Converts an array of bytes received on the network into a message.
        /// </summary>
        TMessage Convert(byte[] data);
    }
}