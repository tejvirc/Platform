namespace Aristocrat.Monaco.PackageManifest.Models
{
    /// <summary>
    ///     Describes an animation file in the manifest
    /// </summary>
    public class AnimationFile
    {
        /// <summary>
        ///     Gets or sets the file path
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        ///     Gets or sets the file identifier (friendly name)
        /// </summary>
        public string FileIdentifier { get; set; }
    }
}