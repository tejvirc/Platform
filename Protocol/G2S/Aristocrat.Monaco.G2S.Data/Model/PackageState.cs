namespace Aristocrat.Monaco.G2S.Data.Model
{
    /// <summary>
    ///     Enumeration of package states.
    /// </summary>
    public enum PackageState
    {
        /// <summary>
        ///     The pending
        /// </summary>
        Pending,

        /// <summary>
        ///     The available
        /// </summary>
        Available,

        /// <summary>
        ///     The in use
        /// </summary>
        InUse,

        /// <summary>
        ///     The delete pending
        /// </summary>
        DeletePending,

        /// <summary>
        ///     The deleted
        /// </summary>
        Deleted,

        /// <summary>
        ///     The error
        /// </summary>
        Error
    }
}