namespace Aristocrat.Mgam.Client.Messaging
{
    using System.Collections.Generic;
    using Attribute;

    /// <summary>
    ///     This message will be received by the VLT when the site controller detects that an attribute
    ///     registered with the current InstanceID has changed. The names and values of all changed
    ///     attributes are listed. The VLT may validate the contents of the changed attributes against names
    ///     and values that are expected, and if any are not valid, the VLT may send a
    ///     LOCKED_SOFTWARE_ERROR notification to the site controller.
    /// </summary>
    public class AttributesChanged : Request
    {
        /// <summary>
        ///     Gets or sets a list of attributes that have changed.
        /// </summary>
        public IReadOnlyList<AttributeItem> Attributes { get; internal set; }
    }
}
