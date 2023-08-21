namespace Aristocrat.Monaco.Hardware.Audio
{
    /// <summary>
    ///     The EDataFlow enumeration defines constants that indicate the direction in which audio data flows between an audio
    ///     endpoint device and an application.
    /// </summary>
    public enum EDataFlow
    {
        /// <summary>
        ///     Audio rendering stream. Audio data flows from the application to the audio endpoint device, which renders the
        ///     stream.
        /// </summary>
        ERender,

        /// <summary>
        ///     Audio capture stream. Audio data flows from the audio endpoint device that captures the stream, to the application.
        /// </summary>
        ECapture,

        /// <summary>
        ///     Audio rendering or capture stream. Audio data can flow either from the application to the audio endpoint device, or
        ///     from the audio endpoint device to the application.
        /// </summary>
        EAll,

        /// <summary>
        ///     The number of members in the EDataFlow enumeration (not counting the EDataFlow_enum_count member).
        /// </summary>
        EDataFlowEnumCount
    }

    public enum ERole
    {
        /// <summary>
        ///     Games, system notification sounds, and voice commands.
        /// </summary>
        EConsole,

        /// <summary>
        ///     Music, movies, narration, and live music recording.
        /// </summary>
        EMultimedia,

        /// <summary>
        ///     Voice communications (talking to another person).
        /// </summary>
        ECommunications,

        /// <summary>
        ///     The number of members in the ERole enumeration (not counting the ERole_enum_count member).
        /// </summary>
        ERoleEnumCount
    }
}