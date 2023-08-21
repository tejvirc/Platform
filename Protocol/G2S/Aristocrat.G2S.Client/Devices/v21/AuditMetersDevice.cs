namespace Aristocrat.G2S.Client.Devices.v21
{
    using Protocol.v21;

    /// <summary>
    ///     The auditMeters class is used to identify whether an EGM supports an audit meter subscription and, if an audit
    ///     meter subscription is supported, which host can set the audit meter subscription.
    /// </summary>
    public class AuditMetersDevice : ClientDeviceBase<auditMeters>, IAuditMetersDevice
    {
        /// <inheritdoc />
        public AuditMetersDevice(IDeviceObserver deviceStateObserver)
            : base(1, deviceStateObserver)
        {
        }

        /// <inheritdoc />
        public override void Open(IStartupContext context)
        {
        }

        /// <inheritdoc />
        public override void Close()
        {
        }
    }
}