namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    /// <summary>
    ///     This enumeration describes the importance level of information kept in persistent storage.
    /// </summary>
    public enum PersistenceLevel
    {
        /// <summary>
        ///     Information at this level will only be cleared in serious cases, like changes of hardware configuration.
        /// </summary>
        Static,

        /// <summary>
        ///     Information at this level will be cleared in some cases, like when the software version is updated.
        /// </summary>
        Critical,

        /// <summary>
        ///     Information at this level will be cleared as a first matter of course when attempting to fix or upgrade an EGM.
        /// </summary>
        Transient
    }
}