namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;

    /// <summary>
    ///     This command returns a list of levels of Mystery progressives that have been won, to GDK.
    /// </summary>
    public class CheckMysteryJackpot
    {
        /// <summary>
        ///     Gets or sets the result of the command
        /// </summary>
        public IEnumerable<uint> Results { get; set; }
    }
}