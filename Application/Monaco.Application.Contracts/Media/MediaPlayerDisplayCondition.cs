namespace Aristocrat.Monaco.Application.Contracts.Media
{
    /// <summary>
    ///     The media player display condition
    /// </summary>
    public enum MediaPlayerDisplayCondition
    {
        /// <summary>
        ///     Wait until game has entered idle state to display overlay window (Default)
        /// </summary>
        GameIdle,

        /// <summary>
        ///     Wait until game has ended to display overlay window
        /// </summary>
        GameEnded
    }
}