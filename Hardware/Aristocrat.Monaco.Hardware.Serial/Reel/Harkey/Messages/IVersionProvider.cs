namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    /// <summary>
    ///     An interface for messages that require a version
    /// </summary>
    public interface IVersionProvider
    {
        /// <summary>
        ///     Gets the major version
        /// </summary>
        int MajorVersion { get; }

        /// <summary>
        ///     Gets the minor version
        /// </summary>
        int MinorVersion { get; }

        /// <summary>
        ///     Gets the mini version
        /// </summary>
        int MiniVersion { get; }
    }
}