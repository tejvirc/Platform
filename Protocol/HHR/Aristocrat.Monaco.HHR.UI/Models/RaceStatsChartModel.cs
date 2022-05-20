namespace Aristocrat.Monaco.Hhr.UI.Models
{
    using System.Collections.Generic;
    public class RaceStatsChartModel
    {
        /// <summary>
        /// constructor
        /// </summary>
        public RaceStatsChartModel(IList<WinningOddsModel> winningOddsList, string chartInfo)
        {
            WinningOddsList = winningOddsList;
            ChartInfo = chartInfo;
        }
        /// <summary>
        /// List of all winning odd for horses
        /// </summary>
        public IList<WinningOddsModel> WinningOddsList { get; set; }

        /// <summary>
        /// Information about the Stat Chart like "JOCKEY EARNING",TRAINER THIRD PLACE(s) etc displayed on the footer of the chart.
        /// </summary>
        public string ChartInfo { get; set; }
    }
}
