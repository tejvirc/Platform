namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
    /// <summary>
    ///     The progress model used to report progress when loading animation files
    /// </summary>
    public class LoadingAnimationFileModel
    {
        /// <summary>
        ///   Gets or sets the current count of the animation files being loaded
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        ///   Gets or sets the total number of animation files being loaded
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        ///   Gets or sets the file name of the current animation file being loaded
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        ///   Gets or sets the state of the current animation file
        /// </summary>
        public LoadingAnimationState State { get; set;  }
    }
}
