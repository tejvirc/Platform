namespace Aristocrat.Monaco.Kernel
{
    /// <summary>
    ///     Indicates the priority of a system disable.
    /// </summary>
    public enum SystemDisablePriority
    {
        /// <summary>
        ///     The system should disable, though tasks in progress may complete first.
        /// </summary>
        Normal,

        /// <summary>
        ///     The system should interrupt any tasks in progress and disable now.
        /// </summary>
        Immediate
    }
}