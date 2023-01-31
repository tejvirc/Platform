namespace Aristocrat.Monaco.Hardware.Contracts.EdgeLighting
{
    /// <summary>
    ///     Specifies the platform target to expect for a certain game context.
    /// </summary>
    public enum PlatformTarget
    {
        /// <summary>
        ///     A PlatformTarget of "None" (default).
        /// </summary>
        None,

        /// <summary>
        ///     A PlatformTarget of "Legacy".
        /// </summary>
        Legacy,

        /// <summary>
        ///     A PlatformTarget of "Native".
        /// </summary>
        Native
    }
}