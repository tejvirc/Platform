namespace Aristocrat.Monaco.Protocol.Common.Installer
{
    /// <summary>
    ///     Supported archive formats for package.
    /// </summary>
    public enum ArchiveFormat
    {
        /// <summary>
        ///     None
        /// </summary>
        None,

        /// <summary>
        ///     Standard Windows Zip format
        /// </summary>
        Zip = 1,

        /// <summary>
        ///     Tar format
        /// </summary>
        Tar = 2
    }
}