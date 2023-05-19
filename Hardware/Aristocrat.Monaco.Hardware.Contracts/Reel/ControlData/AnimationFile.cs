namespace Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData
{
    /// <summary>
    ///     The data associated with a reel controller animation file
    /// </summary>
    public class AnimationFile
    {
        /// <summary>
        /// The path to the animation file
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Create an AnimationFile
        /// </summary>
        /// <param name="path"></param>
        public AnimationFile(string path)
        {
            Path = path;
        }
    }
}
