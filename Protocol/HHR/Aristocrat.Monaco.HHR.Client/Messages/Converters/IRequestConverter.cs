namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    /// <summary>
    ///     Converts a request message structure into bytes for network transmission.
    /// </summary>
    public interface IRequestConverter<in TMessage> where TMessage : Request
    {
        /// <summary>
        ///     Converts the given message into an array of bytes for network transmission.
        /// </summary>
        byte[] Convert(TMessage message);
    }
}