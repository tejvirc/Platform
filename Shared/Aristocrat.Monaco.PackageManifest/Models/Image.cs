namespace Aristocrat.Monaco.PackageManifest.Models
{
    /// <summary>
    ///     Defines the structure of an image manifest
    /// </summary>
    public class Image
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Image" /> class.
        /// </summary>
        /// <param name="name">The image name</param>
        /// <param name="type">The image type</param>
        /// <param name="file">The associated file</param>
        /// <param name="size">The file size</param>
        /// <param name="version">The file version</param>
        /// <param name="jurisdiction">The image jurisdiction</param>
        /// <param name="assemblyHash">The hash of all assemblies in the image</param>
        /// <param name="fileHash">The hash of the referenced file</param>
        /// <param name="manifestHash">The hash of the manifest</param>
        /// <param name="signature">The signature of the manifest hash</param>
        /// <param name="contents">The raw contents of the file (all lines)</param>
        public Image(
            string name,
            string type,
            string file,
            long size,
            string version,
            string jurisdiction,
            string assemblyHash,
            string fileHash,
            string manifestHash,
            string signature,
            string[] contents)
        {
            Name = name;
            Type = type;
            File = file;
            Size = size;
            Version = version;
            Jurisdiction = jurisdiction;
            AssemblyHash = assemblyHash;
            FileHash = fileHash;
            ManifestHash = manifestHash;
            Signature = signature;
            Contents = contents;
        }

        /// <summary>
        ///     Gets or sets the image name
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets or sets the image type
        /// </summary>
        public string Type { get; }

        /// <summary>
        ///     Gets or sets the image file
        /// </summary>
        public string File { get; }

        /// <summary>
        ///     Gets or sets the image size
        /// </summary>
        public long Size { get; }

        /// <summary>
        ///     Gets or sets the version
        /// </summary>
        public string Version { get; }

        /// <summary>
        ///     Gets or sets the jurisdiction
        /// </summary>
        public string Jurisdiction { get; }

        /// <summary>
        ///     Get the hash of all assemblies in the image
        /// </summary>
        public string AssemblyHash { get; }

        /// <summary>
        ///     Gets or sets the image hash
        /// </summary>
        public string FileHash { get; }

        /// <summary>
        ///     Gets or sets the manifest hash
        /// </summary>
        public string ManifestHash { get; }

        /// <summary>
        ///     Gets or sets the signature
        /// </summary>
        public string Signature { get; }

        /// <summary>
        ///     Gets or set the raw contents
        /// </summary>
        public string[] Contents { get; }
    }
}
