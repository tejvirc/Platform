namespace Aristocrat.Mgam.Client.Messaging
{
    using System.Collections.Generic;
    using Attribute;

    /// <summary>
    ///     This message is sent in response to a <see cref="GetAttributes"/> message.
    /// </summary>
    public class GetAttributesResponse : Response
    {
        /// <summary>
        ///     Gets or sets a list of attributes that have changed.
        /// </summary>
        public IReadOnlyList<AttributeItem> Attributes { get; internal set; }
    }
}
