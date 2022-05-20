namespace Aristocrat.G2S.Client
{
    using Communications;

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
    }
}