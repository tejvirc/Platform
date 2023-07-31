namespace Aristocrat.Monaco.Accounting.Contracts.Hopper
{
    using Kernel;
    using TransferOut;

    /// <summary>
    ///     Check coin out amount is successfully transferred.
    /// </summary>
    public interface ICoinOutProvider : ITransferOutProvider, IService
    {
        /// <summary>
        /// Check weather coin out amount is successfully transferred.
        /// </summary>
        /// <returns></returns>
        bool CheckCoinOutException(long transferredAmount, long remainingAmount);
    }
}
