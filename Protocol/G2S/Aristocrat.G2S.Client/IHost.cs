namespace Aristocrat.G2S.Client
{
    using Communications;
    using System;

    /// <summary>
    ///     Definition of host
    /// </summary>
    public interface IHost : IEndpoint
    {
        /// <summary>
        ///     Gets the host index.
        /// </summary>
        /// <remarks>
        ///     This is not to be confused with the host Id.  This is used in commsConfig to set/remove hosts and should remain
        ///     static once it's assigned.
        /// </remarks>
        int Index { get; }

        /// <summary>
        ///     Gets the host Identifier.
        /// </summary>
        int Id { get; }

        /// <summary>
        ///     Gets a value indicating whether the host is registered.
        /// </summary>
        bool Registered { get; }

        /// <summary>
        ///     Gets a value indicating whether indicates the device MUST be functioning and enabled before the EGM can be
        ///     played.
        ///     (true = enabled, false = disabled)
        /// </summary>
        bool RequiredForPlay { get; }

        /// <summary>
        ///     Determines whether this host will default to the host of a given progressiveDevice.
        ///     (Currently only used/modifiable for G2S vertex progressives.)
        ///     (true = Will be the default progressive host, false = Will not be the default progressive host)
        /// </summary>
        bool IsProgressiveHost { get; }

        /// <summary>
        ///     Gets the interval at which the progressive host offline timer will trigger
        ///     (Currently only used if the progressive host is selectable)
        /// </summary>
        TimeSpan OfflineTimerInterval { get; }
    }
}