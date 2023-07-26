namespace Aristocrat.Monaco.Hardware.Contracts.Gds.CoinAcceptor
{
    using System;

    /// <summary>(Serializable) a device reset.</summary>
    [Serializable]
    public class DeviceReset : GdsSerializableMessage
    {
        /// <summary>
        /// 
        /// </summary>
        public DeviceReset() : base(GdsConstants.ReportId.DeviceReset) { }
    }
}