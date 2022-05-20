namespace Aristocrat.Monaco.Gaming.Contracts.Meters
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Game Added event args
    /// </summary>
    public class GameAddedEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameAddedEventArgs" /> class.
        /// </summary>
        /// <param name="games">The added games</param>
        public GameAddedEventArgs(IReadOnlyCollection<IGame> games)
        {
            Games = games;  
        }

        /// <summary>
        ///     Gets the games being added
        /// </summary>
        public IReadOnlyCollection<IGame> Games { get; }
    }
}