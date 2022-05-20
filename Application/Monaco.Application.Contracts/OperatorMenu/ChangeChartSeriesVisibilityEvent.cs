namespace Aristocrat.Monaco.Application.Contracts.OperatorMenu
{
    using System;
    using Kernel;

    /// <summary>
    ///     An event to change chart series visibility.
    /// </summary>
    [Serializable]
    public class ChangeChartSeriesVisibilityEvent : BaseEvent
    {

        /// <summary>
        ///     An event to change chart series visibility.
        /// </summary>
        public ChangeChartSeriesVisibilityEvent(int seriesIndex)
        {
            SeriesIndex = seriesIndex;
        }

        /// <summary>
        ///     Indicate that calibration text to be hidden or not, when IsHidden = true, otherwise when IsHidden = flase
        /// </summary>
        public int SeriesIndex;
    }
}
