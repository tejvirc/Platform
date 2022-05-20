namespace Aristocrat.Monaco.Sas.Progressive
{
    using System.Collections.Generic;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;

    /// <summary>
    ///     The provider for formatting and retrieving the last progressive win amount for a given game
    /// </summary>
    public interface IProgressiveWinDetailsProvider
    {
        /// <summary>
        ///     Sets the last progressive win amount for the provided game history log
        /// </summary>
        /// <param name="log">The game history log to set the last progressive win amount for</param>
        void SetLastProgressiveWin(IGameHistoryLog log);

        /// <summary>
        ///     Gets the last progressive win amount
        /// </summary>
        /// <returns>The progressive win details for the lat progressive win amount</returns>
        ProgressiveWinDetails GetLastProgressiveWin();

        /// <summary>
        ///     Gets the progressive win details for the provided game history log
        /// </summary>
        /// <param name="log">The game history log to get the progressive win details for</param>
        /// <returns>The progressive win details for the provided game history log</returns>
        ProgressiveWinDetails GetProgressiveWinDetails(IGameHistoryLog log);

        /// <summary>
        ///     Adds a non SAS progressive level.
        /// </summary>
        /// <param name="level">The level.</param>
        void AddNonSasProgressiveLevelWin(IViewableProgressiveLevel level);

        /// <summary>
        ///     Gets the Non Sas Progressive Win Data and marks them as sent
        /// </summary>
        /// <param name="clientNumber">The clientNumber.</param>
        /// <returns></returns>
        IEnumerable<NonSasProgressiveWinData> GetNonSasProgressiveWinData(byte clientNumber);

        /// <summary>
        ///   Handle Non Sas Progressive Win Data Acknowledged   
        /// </summary>
        /// <param name="clientNumber">The clientNumber.</param>
        void HandleNonSasProgressiveWinDataAcknowledged(byte clientNumber);

        /// <summary>
        ///     Checks for Non Sas Progressive Win Data
        /// </summary>
        /// <param name="clientNumber">The clientNumber.</param>
        bool HasNonSasProgressiveWinData(byte clientNumber);

        /// <summary>
        ///     Update Settings
        /// </summary>
        void UpdateSettings();
    }
}