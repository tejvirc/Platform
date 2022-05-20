namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    /// <summary>
    ///     Asp Constants
    /// </summary>
    public static class AspConstants
    {
        /// <summary>
        ///     machine accounting denomination value in cents
        /// </summary>
        public const long AccountingDenomination = 1;

        /// <summary>
        ///     machine denomination status resulted from games enabled
        /// </summary>
        public enum DenomAttribute
        {
            SingleDenom = 0,

            MultiDenom = 1
        }

        public const string AuditUpdateStatusField = "AuditUpdate.Status";
        public const string AuditUpdateTimeStampField = "AuditUpdate.TimeStamp";
    }

}
