namespace Aristocrat.Monaco.G2S.Data.Model
{
    /// <summary>
    ///     Disable conditions for EGM to apply the change request.
    /// </summary>
    public enum DisableCondition
    {
        /// <summary>
        ///     No conditions.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Disable immediately.
        /// </summary>
        Immediate = 1,

        /// <summary>
        ///     Disable when zero credits.
        /// </summary>
        ZeroCredits = 2,

        /// <summary>
        ///     Disable on idle.
        /// </summary>
        Idle = 3
    }
}