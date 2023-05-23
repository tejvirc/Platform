namespace Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData
{
    /// <summary>
    ///     The data associated with a reel controller animation file
    /// </summary>
    public class AnimationData
    {
        /// <summary>
        /// The path to the animation file
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The friendly name of the light show file
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The hash of the file
        /// </summary>
        public byte[] Hash { get; set; }

        /// <summary>
        /// The id of the file
        /// </summary>
        public int AnimationId { get; set; }

        /// <summary>
        /// The type of the animation file
        /// </summary>
        public AnimationType AnimationType { get; set; }

        /// <summary>
        /// Create an AnimationFile
        /// </summary>
        /// <param name="path"></param>
        /// <param name="animationType"></param>
        public AnimationData(string path, AnimationType animationType)
        {
            Path = path;
            AnimationType = animationType;
        }
    }
}
