namespace Aristocrat.G2S.Client
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Defines a contract to add and remove a host
    /// </summary>
    public interface IHostConnector
    {
        /// <summary>
        ///     Gets a collection of registered hosts
        /// </summary>
        IEnumerable<IHostControl> Hosts { get; }

        /// <summary>
        ///     Gets a host by it's unique identifier
        /// </summary>
        /// <param name="hostId">The host identifier</param>
        /// <returns>The host or null if it doesn't exist</returns>
        IHostControl GetHostById(int hostId);

        /// <summary>
        ///     Registers a host and starts the communication process
        /// </summary>
        /// <param name="hostId">The unique host id</param>
        /// <param name="hostUri">The Uri of the host</param>
        /// <param name="requiredForPlay">Is the host required for play</param>
        /// <param name="index">The host index</param>
        /// <param name="isProgressiveHost">Is the host the default progressive host</param>
        /// <param name="offlineTimerInterval">The interval at which the host will be detected offline if not reset</param>
        /// <returns>Returns an instance of IHostControl, if successful</returns>
        IHostControl RegisterHost(int hostId, Uri hostUri, bool requiredForPlay, int index, bool isProgressiveHost = false, TimeSpan offlineTimerInterval = new TimeSpan());

        /// <summary>
        ///     Removes a host and ends all communications. Any owned devices are assigned to the EGM
        /// </summary>
        /// <param name="hostId">The unique host id</param>
        /// <returns>IHost</returns>
        /// <param name="egmStateManager">An instance of IEgmStateManager.</param>
        /// <returns>IHost</returns>
        IHost UnregisterHost(int hostId, IEgmStateManager egmStateManager = null);
    }
}