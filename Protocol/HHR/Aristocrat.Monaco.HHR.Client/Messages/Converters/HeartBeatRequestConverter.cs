namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    /// <summary>
    ///     Converter for <see cref="HeartBeatRequest" />
    /// </summary>
    public class HeartBeatRequestConverter : IRequestConverter<HeartBeatRequest>
    {
        /// <inheritdoc />
        public byte[] Convert(HeartBeatRequest message)
        {
            // Unfortunately .NET marshals empty structs as a 1 byte array. Since there's nothing to translate here and we
            // need a 0 byte array, we can just return it directly.
            return new byte[0];
        }
    }
}