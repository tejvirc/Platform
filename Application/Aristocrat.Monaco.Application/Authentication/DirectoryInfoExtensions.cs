namespace Aristocrat.Monaco.Application.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    ///     DirectoryInfo extension methods.
    /// </summary>
    public static class DirectoryInfoExtensions
    {
        /// <summary>
        ///     A DirectoryInfo extension method that gets files by a collection of patterns.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="directoryInfo"> The directoryInfo to act on.</param>
        /// <param name="searchPatterns">The search patterns.  There may be multiple patters separated by a '|'.</param>
        /// <param name="searchOption">  The search option.</param>
        /// <returns>
        ///     The files by patterns.
        /// </returns>
        public static FileInfo[] GetFilesByPattern(
            this DirectoryInfo directoryInfo,
            string searchPatterns,
            SearchOption searchOption)
        {
            if (directoryInfo == null)
            {
                throw new ArgumentNullException(nameof(directoryInfo));
            }

            if (searchPatterns == null)
            {
                throw new ArgumentNullException(nameof(searchPatterns));
            }

            var patterns = searchPatterns.Split('|');

            var files = new List<FileInfo>();

            foreach (var pattern in patterns)
            {
                files.AddRange(directoryInfo.GetFiles(pattern, searchOption));
            }

            return files.ToArray();
        }
    }
}
