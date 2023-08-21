namespace Aristocrat.Monaco.Application.Contracts.NoteAcceptorMonitor
{
    using Kernel;
    using Localization;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     An event when a Note Acceptor Document Check tilt is cleared
    /// </summary>
    public class NoteAcceptorDocumentCheckClearedEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoteAcceptorDocumentCheckClearedEvent);
        }
    }
}