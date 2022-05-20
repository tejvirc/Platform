namespace Aristocrat.Monaco.Test.Common
{
    using System;
    using System.IO;
    using System.Threading;

    public static class DirectoryHelpers
    {
        private const int WaitTime = 0;
        private const int MaxRetries = 10;

        /// <summary>
        /// Deletes a directory and waits until the deletion is completed
        /// before returning.
        /// </summary>
        /// <param name="directory">the directory to delete</param>
        public static void VerifiedDirectoryDelete(string directory)
        {
            var retries = 0;
            Console.WriteLine($@"deleting directory: {directory}");
            while (Directory.Exists(directory) && retries < MaxRetries)
            {
                try
                {
                    Directory.Delete(directory, true);
                }
                catch (IOException)
                {
                    // something has a handle to an open file in the directory we are trying to delete.
                    // wait a little bit for the file to be released and then try again
                    Thread.Sleep(WaitTime);
                }
                catch (UnauthorizedAccessException)
                {
                    // something has a locked file in the directory we are trying to delete.
                    // wait a little bit for the file to be released and then try again
                    Thread.Sleep(WaitTime);
                }

                retries++;
            }

            // if we reached the max retries, try to delete one more time and
            // let any exceptions get passed to the caller
            if (retries >= MaxRetries)
            {
                Directory.Delete(directory, true);
            }

            Console.WriteLine($@"directory wsa deleted. Retries was {retries}");
        }
    }
}
