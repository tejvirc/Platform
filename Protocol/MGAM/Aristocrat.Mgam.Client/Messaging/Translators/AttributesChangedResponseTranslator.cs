namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Returns null because there is no response for <see cref="T:Aristocrat.Mgam.Client.Protocol.AttributesChanged"/> message.
    /// </summary>
    public class AttributesChangedResponseTranslator : MessageTranslator<AttributesChangedResponse>
    {
        /// <inheritdoc />
        public override object Translate(AttributesChangedResponse message)
        {
            return null;
        }
    }
}
