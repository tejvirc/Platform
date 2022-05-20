namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    /// <summary>
    ///     Converter for <see cref="PlayerIdRequest" />
    /// </summary>
    public class PlayerIdRequestConverter : IRequestConverter<PlayerIdRequest>
    {
        /// <inheritdoc />
        public byte[] Convert(PlayerIdRequest message)
        {
            // Unfortunately .NET marshals empty structs as a 1 byte array. Since there's nothing to translate here and we
            // need a 0 byte array, we can just return it directly.
            return new byte[0];
        }
    }
}