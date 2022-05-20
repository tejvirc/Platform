namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    /// <summary>
    ///     Converter for <see cref="HeartBeatResponse" />
    /// </summary>
    public class HeartBeatResponseConverter : IResponseConverter<HeartBeatResponse>
    {
        /// <inheritdoc />
        public HeartBeatResponse Convert(byte[] data)
        {
            return new HeartBeatResponse();
        }
    }
}