namespace Aristocrat.Monaco.Application.Contracts.Drm
{
    using Kernel;
    using static System.FormattableString;

    /// <summary>
    ///     The <see cref="LicenseErrorEvent" /> is published when there is an error reading or validating the license file
    /// </summary>
    public class LicenseErrorEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LicenseErrorEvent"/> class.
        /// </summary>
        /// <param name="reason">Reasons for the event.</param>
        public LicenseErrorEvent(string reason)
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