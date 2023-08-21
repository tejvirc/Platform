namespace Aristocrat.Mgam.Client.Messaging
{
    using System.Collections.Generic;
    using Attribute;

    /// <summary>
    ///     This message is sent in response to a <see cref="AttributesChanged"/> message.
    /// </summary>
    public class AttributesChangedResponse : Response
    {
        /// <summary>
        ///     Gets or sets a list of attributes that have changed.
        /// </summary>
        public IReadOnlyList<AttributeItem> Attributes { get; internal set; }
    }
}
