namespace Aristocrat.G2S.Client
{
    using System;

    /// <summary>
    ///     Provides a mechanism to control a host.
    /// </summary>
    public interface IHostControl : IHost
    {
        /// <summary>
        ///     Gets the host command queue.
        /// </summary>
        IHostQueue Queue { get; }

        /// <summary>
        ///     Starts the host.
        /// </summary>
        void Start();

        /// <summary>
        ///     Stops the host.
        /// </summary>
        void Stop();

        /// <summary>
        ///     Sets the address of the host
        /// </summary>
        /// <param name="address">The address of the host</param>
        void SetAddress(Uri address);

        /// <summary>
        ///     Sets if the host is required for play
        /// </summary>
        /// <param name="requiredForPlay">Is the host required for play</param>
        void SetRequiredForPlay(bool requiredForPlay);
    }
}