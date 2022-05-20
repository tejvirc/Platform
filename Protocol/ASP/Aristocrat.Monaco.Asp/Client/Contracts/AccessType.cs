namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    /// <summary>
    ///     Defines different asp parameter access types.
    /// </summary>
    public enum AccessType
    {
        /// <summary>
        ///     Parameter can be read or written.
        /// </summary>
        ReadWrite,

        /// <summary>
        ///     Parameter can be read but not written.
        /// </summary>
        ReadOnly
    }
}