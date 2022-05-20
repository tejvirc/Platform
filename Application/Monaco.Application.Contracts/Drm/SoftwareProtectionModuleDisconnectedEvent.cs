namespace Aristocrat.Monaco.Application.Contracts.Drm
{
    using Kernel;
    using static System.FormattableString;

    /// <summary>
    ///     The <see cref="SoftwareProtectionModuleDisconnectedEvent" /> is emitted whenever the software protection module
    ///     (such as the Smart Card) is disconnected/removed
    /// </summary>
    public class SoftwareProtectionModuleDisconnectedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SoftwareProtectionModuleDisconnectedEvent"/> class.
        /// </summary>
        /// <param name="reason">Reason for the event.</param>
        public SoftwareProtectionModuleDisconnectedEvent(string reason)
        {
            Reason = reason;
        }

        /// <summary>Gets the reason for the event.</summary>
        public string Reason { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} {Reason}");
        }
    }
}