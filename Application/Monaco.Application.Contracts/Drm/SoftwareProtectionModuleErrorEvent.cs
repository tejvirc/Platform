namespace Aristocrat.Monaco.Application.Contracts.Drm
{
    using Kernel;
    using static System.FormattableString;

    /// <summary>
    ///     The <see cref="SoftwareProtectionModuleErrorEvent" /> is emitted whenever an error is detected with the software
    ///     protection module (such as the Smart Card)
    /// </summary>
    public class SoftwareProtectionModuleErrorEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SoftwareProtectionModuleErrorEvent"/> class.
        /// </summary>
        /// <param name="reason">Reasons for the event.</param>
        public SoftwareProtectionModuleErrorEvent(string reason)
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