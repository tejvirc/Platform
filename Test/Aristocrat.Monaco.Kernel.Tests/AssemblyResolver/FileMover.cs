namespace Aristocrat.Monaco.Kernel.Tests.AssemblyResolver
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Mono.Addins;

    /// <summary>
    ///     This class makes sure that if any files are moved from a temporary name
    ///     to a particular name during testing that they get reverted no matter what
    ///     at the end of the test.
    /// </summary>
    public class FileMover : IDisposable
    {
        /// <summary>
        ///     The file mappings used to rename files for tests and then rename them back
        /// </summary>
        private readonly Dictionary<string, string> fileMapping;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FileMover" /> class.
        /// </summary>
        /// <param name="fileMapping">
        ///     The file mapping to use for reverting filenames. The key should be the name
        ///     of the file as it exists at deployment. The value should be set to the name
        ///     of the file as it may exist during the test.
        /// </param>
        /// <param name="initialize">
        ///     If true, the FileMover will move all the files to their new filenames and
        ///     will call update on the Addin Registry
        /// </param>
        public FileMover(Dictionary<string, string> fileMapping, bool initialize)
        {
            this.fileMapping = fileMapping;

            if (initialize)
            {
                RenameFiles();

                AddinManager.Registry.Update(null);
            }
        }

        /// <summary>
        ///     Disposes of the FileMover class and makes sure that all files are reverted
        ///     to their previous filename.
        /// </summary>
        public void Dispose()
        {
            foreach (var fileNamePair in fileMapping)
            {
                if (File.Exists(fileNamePair.Value))
                {
                    File.Move(fileNamePair.Value, fileNamePair.Key);
                }
            }
        }

        /// <summary>
        ///     A utility function to rename all files from the value stored as the "key"
        ///     to the name stored as the "value". Reverting will be handled by disposable and
        ///     is not done explicitly by the caller.
        /// </summary>
        public void RenameFiles()
        {
            foreach (var fileNamePair in fileMapping)
            {
                if (File.Exists(fileNamePair.Key))
                {
                    File.Move(fileNamePair.Key, fileNamePair.Value);
                }
            }
        }
    }
}