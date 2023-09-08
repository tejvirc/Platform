namespace Aristocrat.Monaco.Hardware.Contracts.Audio
{
    using System;
    using System.ComponentModel;

    /// <summary>Volume Levels</summary>
    public enum VolumeLevel : byte
    {
        /// <summary>Extra Low</summary>
        [Description("Extra-Low")]
        ExtraLow = 1,

        /// <summary>Low</summary>
        [Description("Low")]
        Low = 2,

        /// <summary>Medium-Low</summary>
        [Description("Medium-Low")]
        MediumLow = 3,

        /// <summary>Medium</summary>
        [Description("Medium")]
        Medium = 4,

        /// <summary>Medium-High</summary>
        [Description("Medium-High")]
        MediumHigh = 5,

        /// <summary>High</summary>
        [Description("High")]
        High = 6,

        /// <summary>Extra High</summary>
        [Description("Extra-High")]
        ExtraHigh = 7
    }

    /// <summary>Volume Scalars</summary>
    public enum VolumeScalar : byte
    {
        /// <summary>Low</summary>
        [Description("20%")]
        Scale20 = 1,

        /// <summary>Medium-Low</summary>
        [Description("40%")]
        Scale40 = 2,

        /// <summary>Medium</summary>
        [Description("60%")]
        Scale60 = 3,

        /// <summary>Medium-High</summary>
        [Description("80%")]
        Scale80 = 4,

        /// <summary>High</summary>
        [Description("100%")]
        Scale100 = 5
    }

    /// <summary>
    ///     Provides a mechanism for audio playback.
    /// </summary>
    public interface IAudio
    {
        /// <summary>
        ///     This event is fired when play has ended
        /// </summary>
        event EventHandler PlayEnded;

        /// <summary>
        ///     Gets a value indicating whether or not the audio service is available and ready
        /// </summary>
        bool IsAvailable { get; }
        
        /// <summary>
        ///     Plays the specified file.
        /// </summary>
        /// <param name="soundName">The audio name to play.</param>
        /// <param name="volume">volume of play channel</param>
        /// <param name="speakers">Speaker mix of play channel</param>
        /// <param name="callback">Callback when audio event finishes (will not be called if sound is interrupted)</param>
        void Play(SoundName soundName, float? volume, SpeakerMix speakers = SpeakerMix.All, Action callback = null);

        /// <summary>
        ///     Plays the specified file.
        /// </summary>
        /// <param name="soundName">The audio name to play.</param>
        /// <param name="loopCount">Repeat the playback for the specified count. Set to -1 to loop forever.</param>
        /// <param name="volume">volume of play channel</param>
        /// <param name="speakers">Speaker mix of play channel</param>
        /// <param name="callback">Callback when audio event finishes (will not be called if sound is interrupted)</param>
        void Play(SoundName soundName, int loopCount, float? volume, SpeakerMix speakers = SpeakerMix.All, Action callback = null);

        /// <summary>
        ///     Stops any audio playback.
        /// </summary>
        void Stop();

        /// <summary>
        ///     Stops any audio playback.
        /// </summary>
        /// <param name="soundName">The sound name</param>
        void Stop(SoundName soundName);

        /// <summary>
        ///     Returns true if audio playback is active.
        /// </summary>
        /// <returns>true if playback is active.</returns>
        bool IsPlaying();

        /// <summary>
        ///     Returns true if audio playback is active.
        /// </summary>
        /// <param name="soundName">The sound name</param>
        /// <returns>true if playback is active.</returns>
        bool IsPlaying(SoundName soundName);

        /// <summary>
        ///     Set the mute state for the system.
        /// </summary>
        /// <param name="muted">true to mute the system, otherwise false</param>
        void SetSystemMuted(bool muted);

        /// <summary>
        ///     Get the mute state for the system.
        /// </summary>
        /// <returns>True if the system is muted.</returns>
        bool GetSystemMuted();

        /// <summary>
        ///     Gets the default volume configuration of
        /// </summary>
        /// <returns>returns the volume (0-100.0) </returns>
        float GetDefaultVolume();

        /// <summary>
        ///     Get volume by preset
        /// </summary>
        /// <param name="level">the volume level</param>
        /// <returns>returns the volume (0-100.0)</returns>
        float GetVolume(VolumeLevel level);

        /// <summary>
        ///     Gets an <see cref="IVolume"/> instance that can be used to control the volume for a process
        /// </summary>
        /// <param name="processId">The process to control</param>
        /// <returns>An <see cref="IVolume"/> instance</returns>
        IVolume GetVolumeControl(int processId);

        /// <summary>
        ///     Get volume scalar corresponding to enum
        /// </summary>
        /// <param name="scalarLevel">the VolumeScalar enum</param>
        /// <returns>returns the scalar (0.0-1.0)</returns>
        float GetVolumeScalar(VolumeScalar scalarLevel);

        /// <summary>
        ///     Gets the length of the sound.
        /// </summary>
        /// <param name="soundName">The sound name</param>
        /// <returns>Returns the length of the sound or <c>TimeSpan.Zero</c> if length could not be determined</returns>
        TimeSpan GetLength(SoundName soundName);

        /// <summary>
        ///     Sets the speaker mix
        /// </summary>
        /// <param name="speakers">Speaker mix of play channel</param>
        /// <remarks>If sound is not currently playing, this call has no effect.</remarks>
        void SetSpeakerMix(SpeakerMix speakers);
    }
}