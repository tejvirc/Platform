namespace Aristocrat.Monaco.Gaming.Commands
{
    public class UpdateVolume
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UpdateVolume" /> class.
        /// </summary>
        /// <param name="volume">The volume.</param>
        public UpdateVolume(float volume)
        {
            Volume = volume;
        }

        /// <summary>
        ///     Gets the volume.
        /// </summary>
        public float Volume { get; }
    }
}
