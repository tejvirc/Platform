namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using System.Linq;
    using Attribute;
    using Helpers;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.GetAttributesResponse"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Messaging.GetAttributesResponse"/> instance.
    /// </summary>
    public class GetAttributesResponseTranslator : MessageTranslator<Protocol.GetAttributesResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.GetAttributesResponse message)
        {
            return new GetAttributesResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value,
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
