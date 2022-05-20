namespace Aristocrat.Monaco.G2S.Data.Model
{
    /// <summary>
    ///     Enumerates available change statuses for CommConfig or OptionConfig G2S class
    /// </summary>
    public enum ChangeStatus
    {
        /// <summary>
        ///     The pending
        /// </summary>
        Pending = 0,

        /// <summary>
        ///     The authorized
        /// </summary>
        Authorized = 1,

        /// <summary>
        ///     The canceled
        /// </summary>
        Cancelled = 2,

        /// <summary>
        ///     The applied
        /// </summary>
        Applied = 3,

        /// <summary>
        ///     The aborted
        /// </summary>
        Aborted = 4,

        /// <summary>
        ///     The error
        /// </summary>
        Error = 5
    }
}