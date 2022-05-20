namespace Aristocrat.Monaco.Gaming.Runtime.Server
{
    using GDKRuntime.Contract;

    /// <summary>
    ///     A runtime service extensions.
    /// </summary>
    public static class WcfServiceExtensions
    {
        /// <summary>
        ///     A <see cref="GameRoundPlayMode" /> extension method that query if '@this' is replay or recovery.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>True if replay or recovery, false if not.</returns>
        public static bool IsReplayOrRecovery(this GameRoundPlayMode @this)
        {
            return @this == GameRoundPlayMode.Recovery || @this == GameRoundPlayMode.Replay;
        }
    }
}