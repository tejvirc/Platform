namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;

    /// <summary>
    ///     Provides a mechanism to set the current game delay
    /// </summary>
    public interface IGameDelay
    {
        /// <summary>
        ///     Gets the remaining game delay period
        /// </summary>
        TimeSpan GameEndDelay { get; }

        /// <summary>
        ///     Gets the delay duration
        /// </summary>
        TimeSpan DelayDuration { get; }

        /// <summary>
        ///     Gets the remaining number of games for which the game delay is active
        /// </summary>
        int DelayedGames { get; }

        /// <summary>
        ///     Gets a value indicating whether both <see cref="DelayDuration" /> and <see cref="DelayedGames" /> are evaluated to
        ///     determine if game delay is in effect
        /// </summary>
        bool EvaluateBoth { get; }

        /// <summary>
        ///     Sets the game end delay
        /// </summary>
        /// <param name="delay">The delay period</param>
        void SetGameEndDelay(TimeSpan delay);

        /// <summary>
        ///     Sets the game delay period
        /// </summary>
        /// <param name="delay">The delay period</param>
        /// <param name="duration">The duration of the delay</param>
        /// <param name="numberOfGames">The number of games to be delayed</param>
        /// <param name="useBoth">
        ///     Gets a value indicating whether both duration and numberOfGames are evaluated to determine if
        ///     game delay is in effect
        /// </param>
        void SetGameEndDelay(TimeSpan delay, TimeSpan duration, int numberOfGames, bool useBoth);

        /// <summary>
        ///     Skips the current game end delay
        /// </summary>
        void SkipGameEndDelay();
    }
}