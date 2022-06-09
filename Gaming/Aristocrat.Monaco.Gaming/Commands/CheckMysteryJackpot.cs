namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;

    /// <summary>
    ///     Trigger jackpot command
    /// </summary>
    public class CheckMysteryJackpot
    {
        /// <summary>
        ///     Gets or sets the result of the command
        /// </summary>
        public Dictionary<uint, bool> Results { get; set; }
    }
}