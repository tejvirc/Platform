namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    using System;

    /// <summary>
    ///     Applies display limits to a bonus award
    /// </summary>
    public interface IAwardLimit
    {
        /// <summary>
        ///     Gets the display limit for the bonus.  If the amount exceeds the limit the bonus will fail.  A value of 0 disables
        ///     the check
        /// </summary>
        long DisplayLimit { get; }

        /// <summary>
        ///     Gets the display limit text for the bonus that will be displayed based if the display limit validation fails
        /// </summary>
        string DisplayLimitText { get; }

        /// <summary>
        ///     Gets the display limit text for the bonus that will be displayed based if the display limit validation fails
        /// </summary>
        TimeSpan DisplayLimitTextDuration { get; }
    }
}