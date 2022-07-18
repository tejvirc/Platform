namespace Aristocrat.Sas.Client.Eft
{
    /// <summary>
    ///     (From section 8.9 EFT of the SAS v5.02 document)  -
    ///     https://confy.aristocrat.com/pages/viewpage.action?pageId=159599156
    /// </summary>
    public class EftSendLastCashoutData : LongPollData
    {
        /// <summary>Acknowledgement flag.</summary>
        public bool Acknowledgement { get; set; }
    }
}