namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.Checksum"/> to a <see cref="T:Aristocrat.Mgam.Client.Protocol.Checksum"/>.
    /// </summary>
    public class ChecksumTranslator : MessageTranslator<Messaging.Checksum>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.Checksum message)
        {
            return new Checksum
            {
                InstanceID = new ChecksumInstanceID
                {
                    Value = message.InstanceId
                },
                ChecksumValue = new ChecksumChecksumValue()
                {
                    Value = message.ChecksumValue
                }
            };
        }
    }
}
