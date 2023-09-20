namespace Aristocrat.G2S.Client.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Provides a mechanism to interact with the Analytics device
    /// </summary>
    public interface IAnalyticsDevice : IDevice, ISingleDevice, IRestartStatus
    {
        /// <summary>
        ///     Gets the time-to-live value for requests originated by the device.
        /// </summary>
        int TimeToLive { get; }


        /// <summary>
        ///     Maximum amount of time the analytics device will wait
        ///     for a response from its owner host before deeming the
        ///     host to be offline and disabling the analytics device
        ///     by setting its hostEnabled attribute to false. A 0 (zero)
        ///     value indicates that the timer is disabled.
        /// </summary>
        int NoResponseTimer { get; }

        /// <summary>
        ///     Maximum amount of time the analytics device will wait
        ///     for a message from its owner host before deeming the
        ///     host to be offline and disabling the analytics device
        ///     by setting its hostEnabled attribute to false. A 0 (zero)
        ///     value indicates that the timer is disabled.
        /// </summary>
        int NoMessageTimer { get; }


        /// <summary>
        ///     Text message to display if analytics host communications
        ///     are lost.
        /// </summary>
        string NoHostText { get; }

        /// <summary>
        ///     Sends the track command and responds with trackAck
        /// </summary>
        /// <param name="command">The track command</param>
        /// <returns>The trackAck command</returns>
        trackAck SendTrack(track command);

        /// <summary>
        ///     Sets the allowed tracking interval for the specified action/category combination. 
        /// </summary>
        /// <param name="action">The type of action to track</param>
        /// <param name="category">Category to be applied to the tracked action</param>
        /// <param name="interval">The new interval to set.<br />
        ///     0 means no limit on tracking; any other positive value
        ///     prevents a new track from being sent for the specified time</param>
        void SetTrackInterval(string action, string category, TimeSpan interval);
    }
}
