namespace Aristocrat.Sas.Client.Eft
{
    /// <summary>
    ///     Interface to provide all business logic for EFT player initiated cashout
    ///     (From section 8.9 EFT of the SAS v5.02 document)  -
    ///     https://confy.aristocrat.com/pages/viewpage.action?pageId=159599156
    /// </summary>
    public interface IPlayerInitiatedCashoutProvider
    {
        /// get the cashout amount
        ulong GetCashoutAmount();

        /// clear the cashout amount
        void ClearCashoutAmount();
    }
}