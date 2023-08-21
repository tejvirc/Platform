namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using Aristocrat.Monaco.Application.Contracts;
    /// <summary>
    /// Application Meters mapped for ASP (TXM-2300)
    /// </summary>
    public static class AspApplicationMeters
    {
        public static readonly string MainDoorOpenTotalCount = ApplicationMeters.MainDoorOpenTotalCount;
        public static readonly string TopMainOpenTotalCount = ApplicationMeters.TopBoxDoorOpenTotalCount;
        public static readonly string BellyDoorOpenTotalCount = ApplicationMeters.BellyDoorOpenTotalCount;
        public static readonly string CashBoxOpenTotalCount = ApplicationMeters.DropDoorOpenTotalCount;
        public static readonly string BillStackerOpenTotalCount = ApplicationMeters.CashDoorOpenTotalCount;
    }
}
