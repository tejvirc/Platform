namespace Aristocrat.Monaco.PackageManifest
{
    using System;
    using System.IO;

    /// <summary>
    ///     Interface for a manifest reader.
    /// </summary>
    /// <typeparam name="TManifest">The type of manifest</typeparam>
    public interface IManifest<out TManifest>
    {
        /// <summary>
        ///     Reads a manifest.
        /// </summary>
        /// <param name="file">The manifest file to parse/read.</param>
        /// <returns>TManifest</returns>
        /// The manifest.
        TManifest Read(string file);

        /// <summary>
        ///     Reads and creates a TManifest from stream
        /// </summary>
        /// <param name="streamProvider">Callback to get the stream</param>
        /// <returns>Deserialized TManifest instance.</returns>
        TManifest Read(Func<Stream> streamProvider);
    }
}
