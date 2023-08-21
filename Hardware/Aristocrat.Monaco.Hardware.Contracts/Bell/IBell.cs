namespace Aristocrat.Monaco.Hardware.Contracts.Bell
{
    using System;
    using System.Threading.Tasks;
    using SharedDevice;

    /// <summary>
    ///     The bell device used for ringing
    /// </summary>
    public interface IBell : IDeviceService
    {
        /// <summary>
        ///     Gets whether or not the bell is currently ringing
        /// </summary>
        bool IsRinging { get; }

        /// <summary>
        ///     Rings the bell for the requested amount of time
        /// </summary>
        /// <param name="duration">The duration to ring the bell for</param>
        /// <returns>The task for ringing and turning off the bell</returns>
        Task<bool> RingBell(TimeSpan duration);

        /// <summary>
        ///     Rings the bell constantly
        /// </summary>
        bool RingBell();

        /// <summary>
        ///     Stops the bell from ringing
        /// </summary>
        bool StopBell();
    }
}
