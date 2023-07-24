namespace Aristocrat.Monaco.Hardware.Contracts.PWM
{
    using Gds;

    /// <summary>
    /// 
    /// </summary>
    public class DeviceReset : GdsSerializableMessage
    {
        /// <summary>
        /// 
        /// </summary>
        public DeviceReset() : base(GdsConstants.ReportId.DeviceReset) { }
    }
}