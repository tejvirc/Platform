namespace Aristocrat.Monaco.Kernel
{
    using System.IO;

    /// <summary>
    ///     Provides an interface to get DirectoryInfo for the location of a file using platform specified path
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The platform path is a slash delineated path. Platform paths must take the unix style notation (i.e. they begin
    ///         with a forward slash). e.g. '/Packages'
    ///         This is translated into a directory info path like 'D:\Aristocrat\Platform\packages'
    ///     </para>
    /// </remarks>
    public interface IPathMapper
    {
        /// <summary>
        ///     Gets a DirectoryInfo object for a file system location that is mapped to the specified platform path
        /// </summary>
        /// <param name="platformPath">The platform path to get a DirectoryInfo for</param>
        /// <returns>A DirectoryInfo object representing the location of the platform path</returns>
        DirectoryInfo GetDirectory(string platformPath);
    }
}