namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     Event emitted when one of the meters' sub pages request to change the displayed periodic meter
    ///     clear DateTime on the Meters Main page.
    /// </summary>
    /// <remarks>
    ///     This event is only required when the periodic clear date for a set of meters can potentially differ from 
    ///     the periodic clear date provided by IMeterManager.
    /// </remarks>
    [Serializable]
    public class PeriodMetersDateTimeChangeRequestEvent : BaseEvent
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pageName">Name of the requesting page</param>
        /// <param name="periodicClearDateTime"></param>
        public PeriodMetersDateTimeChangeRequestEvent(string pageName, DateTime periodicClearDateTime)
        {
            PageName = pageName;
            PeriodicClearDateTime = periodicClearDateTime;
        }

        /// <summary>PageName</summary>
        public string PageName { get; }

        /// <summary>PeriodicClearDateTime</summary>
        public DateTime PeriodicClearDateTime { get; }
    }
}