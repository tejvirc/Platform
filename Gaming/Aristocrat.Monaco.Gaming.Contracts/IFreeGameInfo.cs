namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;

    /// <summary>
    ///     Defines an instance of free game
    /// </summary>
    public interface IFreeGameInfo
    {
        /// <summary>
        ///     Gets the start date time of the free game
        /// </summary>
        DateTime StartDateTime { get; }

        /// <summary>
        ///     Gets the start date time of the free game
        /// </summary>
        DateTime EndDateTime { get; }

        /// <summary>
        ///     Gets the starting credits
        /// </summary>
        long StartCredits { get; }

        /// <summary>
        ///     Gets the ending credits
        /// </summary>
        long EndCredits { get; }

        /// <summary>
        ///     Gets the final amount won of the free game
        /// </summary>
        long FinalWin { get; }

        /// <summary>
        ///     Gets the result of the free game
        /// </summary>
        GameResult Result { get; }

        /// <summary>
        ///     Gets the amount out attributed to the free game (in millicents)
        /// </summary>
        long AmountOut { get; }
    }
}
