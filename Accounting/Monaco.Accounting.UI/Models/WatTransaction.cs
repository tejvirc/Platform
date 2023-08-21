namespace Aristocrat.Monaco.Accounting.UI.Models
{
    using Contracts.Wat;
    using System;

    public class WatTransaction
    {
        public long LogSequence { get; set; }
        public int DeviceId { get; set; }
        public string RequestId { get; set; }
        public string CashableAmount { get; set; }
        public string PromoAmount { get; set; }
        public string NonCashAmount { get; set; }
        public DateTime TransactionDateTime { get; set; }
        public WatStatus Status { get; set; }
    }
}
