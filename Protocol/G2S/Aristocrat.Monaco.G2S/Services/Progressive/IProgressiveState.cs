﻿namespace Aristocrat.Monaco.G2S.Services.Progressive
{
    using Aristocrat.G2S.Client.Devices;
    using System;

    public interface IProgressiveState
    {
        /// <summary>
        ///     Last update SetProgressive Value Received Time
        /// </summary>
        DateTime LastProgressiveUpdateTime { get; set; }

        /// <summary>
        ///     This method is called whenever the ProgressiveHostOfflineTimer should be reset.
        ///     Currently this will happen any time that SetProgressiveValue is called, though it may be moved if a more suitable location is found.
        ///     If there is no progressive host with the offline check enabled then this returns out.
        /// </summary>
        void ReceiveProgressiveValueUpdate();

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
