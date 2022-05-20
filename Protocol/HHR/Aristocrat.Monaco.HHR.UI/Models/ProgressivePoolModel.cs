namespace Aristocrat.Monaco.Hhr.UI.Models
{
    using System.Collections.Generic;

    public class ProgressivePoolModel
    {
        /// <summary>
        /// Bet corresponding to progressive pool
        /// </summary>
        public int Bet { get; set; }

        /// <summary>
        /// list of current amounts , corresponding to active levels
        /// </summary>
        public IList<double> CurrentAmount { get; set; }
    }
}