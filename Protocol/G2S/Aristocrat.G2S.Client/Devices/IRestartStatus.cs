namespace Aristocrat.G2S.Client.Devices
{
    /// <summary>
    ///     Provides a mechanism for controlling the host enabled attribute when the EGM restarts.
    /// </summary>
    public interface IRestartStatus
    {
        /// <summary>
        ///     Gets a value indicating whether the HostEnabled attribute is reset or restored.
        /// </summary>
        bool RestartStatus { get; }
    }
}