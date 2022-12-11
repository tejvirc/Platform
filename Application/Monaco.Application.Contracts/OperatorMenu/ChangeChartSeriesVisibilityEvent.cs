namespace Aristocrat.Monaco.Application.Contracts.OperatorMenu
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     An event to change chart series visibility.
    /// </summary>
    [ProtoContract]
    public class ChangeChartSeriesVisibilityEvent : BaseEvent
    {

        /// <summary>
        /// Empty constructor for deserialization
        /// </summary>
        public ChangeChartSeriesVisibilityEvent()
        {
        }

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
