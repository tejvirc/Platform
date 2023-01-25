namespace Aristocrat.Monaco.Application.Contracts.Handlers
{
    using System;
    using Cabinet.Contracts;

    /// <summary>
    /// 
    /// </summary>
    public interface IDeviceStatusHandler
    {
        /// <summary>
        /// 
        /// </summary>
        DeviceStatus Status { get; }

        /// <summary>
        /// 
        /// </summary>
        string Meter { get; set; }

        /// <summary>
        /// 
        /// </summary>
        IDevice Device { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Action<IDeviceStatusHandler> ConnectAction { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Action<IDeviceStatusHandler> DisconnectAction { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public void Refresh();
    }
}
