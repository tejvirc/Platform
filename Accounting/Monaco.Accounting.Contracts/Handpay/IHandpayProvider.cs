namespace Aristocrat.Monaco.Accounting.Contracts.Handpay
{
    using Kernel;
    using TransferOut;

    /// <summary>
    ///     Provides a standard API to handpay funds off of the EGM
    /// </summary>
    public interface IHandpayProvider : ITransferOutProvider, IService
    {
    }
}
