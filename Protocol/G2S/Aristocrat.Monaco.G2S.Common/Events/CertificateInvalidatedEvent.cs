namespace Aristocrat.Monaco.G2S.Common.Events
{
    using Kernel;

    /// <summary>
    ///     This event is posted when the currently bound certificate is deemed expired or revoked.
    /// </summary>
    /// <seealso cref="Aristocrat.Monaco.Kernel.BaseEvent" />
    public class CertificateInvalidatedEvent : BaseEvent
    {
    }
}