namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Kernel;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Localization.Properties;
    using ProtoBuf;

    /// <summary>Definition of the ExcessiveDocumentRejectedEvent class. </summary>
    /// <remarks>
    ///     This event is posted when a document had been rejected too many times.
    /// </remarks>
    [ProtoContract]
    public class ExcessiveDocumentRejectedEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExcessiveDocumentRejectMessage);
        }
    }
}