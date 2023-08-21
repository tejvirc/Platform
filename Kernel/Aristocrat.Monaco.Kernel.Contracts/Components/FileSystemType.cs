namespace Aristocrat.Monaco.Kernel.Contracts.Components
{
    /// <summary>
    ///     File system type
    /// </summary>
    public enum FileSystemType
    {
        /// <summary>
        ///     None
        /// </summary>
        None,

        /// <summary>
        ///     A single file
        /// </summary>
        File,

        /// <summary>
        ///     A directory or path
        /// </summary>
        Directory,

        /// <summary>
        ///     Not used.  Kept to maintain the order/value
        /// </summary>
        NotUsed,

        /// <summary>
        ///     A stream
        /// </summary>
        Stream
    }
}
