namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    /// <summary>
    ///     Defines different asp parameter event reporting access types.
    /// </summary>
    public enum EventAccessType
    {
        /// <summary>
        ///     Event is always reported for the parameter.
        /// </summary>
        Always,

        /// <summary>
        ///     Event is only reported if requested for the parameter.
        /// </summary>
        OnRequest,

        /// <summary>
        ///     Event is never reported for the parameter.
        /// </summary>
        Never
    }
}