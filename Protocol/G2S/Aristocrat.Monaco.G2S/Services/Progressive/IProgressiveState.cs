namespace Aristocrat.Monaco.G2S.Services.Progressive
{
    using Aristocrat.G2S.Client.Devices;
    using System;

    public interface IProgressiveState
    {
        /// <summary>
        ///     Set the state of a progressive device
        /// </summary>
        /// <param name="state">
        ///     The online or offline state
        /// </param>
        /// <param name="device">
        ///     The progressive device
        /// </param>
        /// <param name="hostReason">
        ///     The string reason sent by the host
        /// </param>
        void SetProgressiveDeviceState(bool state, IProgressiveDevice device, string hostReason = null);
    }
}
