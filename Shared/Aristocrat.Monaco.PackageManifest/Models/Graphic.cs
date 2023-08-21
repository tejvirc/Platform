namespace Aristocrat.Monaco.PackageManifest.Models
{
    using System.Collections.Generic;

    /// <summary>
    ///     Graphic.
    /// </summary>
    public class Graphic
    {
        /// <summary>
        ///     Gets or sets the type.
        /// </summary>
        public GraphicType GraphicType { get; set; }

        /// <summary>
        ///     Gets or sets the encoding.
        /// </summary>
        public ImageEncodingType Encoding { get; set; }

        /// <summary>
        ///     Gets or sets the filename of the file.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        ///     Gets or sets the tags associated with the file.
        /// </summary>
        public HashSet<string> Tags { get; set; }
    }
}