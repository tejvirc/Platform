namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    using System.Collections.Generic;

    /// <summary>
    ///     Holds the values of denominations to be configured
    /// </summary>
    public class LongPollBillDenominationsData : LongPollData
    {
        public IList<ulong> Denominations { get;  } = new List<ulong>();
        public bool DisableAfterAccept { get; set; }
    }
}