namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using System.Linq;
    using Attribute;
    using Helpers;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.AttributesChanged"/> to a <see cref="T:Aristocrat.Mgam.Client.Messaging.AttributesChanged"/>.
    /// </summary>
    public class AttributesChangedTranslator : MessageTranslator<Protocol.AttributesChanged>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.AttributesChanged message)
        {
            return new AttributesChanged
            {
                Attributes = (message.Attributes.elem?.Select(
                                         x => new AttributeItem
                                         {
                                             Scope = x.Scope.Value.ToAttributeScope(),
                                             Name = x.Name.Value,
                                             Value = x.Value.Value
                                         })
                                     ?? Enumerable.Empty<AttributeItem>()).ToList()
            };
        }
    }
}
