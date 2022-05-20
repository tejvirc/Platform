namespace Aristocrat.Monaco.G2S.Data.Model
{
    /// <summary>
    ///     Apply conditions for change request.
    /// </summary>
    public enum ApplyCondition
    {
        /// <summary>
        ///     Apply immediately.
        /// </summary>
        Immediate = 0,

        /// <summary>
        ///     The disableApply on EGM disable.
        /// </summary>
        Disable = 1,

        /// <summary>
        ///     Apply on action on egm.
        /// </summary>
        EgmAction = 2,

        /// <summary>
        ///     Must not apply.
        /// </summary>
        Cancel = 3
    }
}