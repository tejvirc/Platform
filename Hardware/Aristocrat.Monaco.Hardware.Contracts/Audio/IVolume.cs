namespace Aristocrat.Monaco.Hardware.Contracts.Audio
{
    /// <summary>
    ///     Provides a mechanism to control the volume
    /// </summary>
    public interface IVolume
    {
        /// <summary>
        ///     Get the application volume
        /// </summary>
        /// <returns>returns the current application volume (0-100.0)</returns>
        float GetVolume();

        /// <summary>
        ///     Set the application volume
        /// </summary>
        /// <param name="volume"> float indicating the volume (0-100.0) </param>
        void SetVolume(float volume);

        /// <summary>
        ///     Set the mute state
        /// </summary>
        /// <param name="muted">true to mute the system, otherwise false</param>
        void SetMuted(bool muted);
    }
}