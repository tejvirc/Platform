namespace Aristocrat.Monaco.Accounting.Contracts.Vouchers
{
    using Kernel;
    using TransferOut;

    /// <summary>
    ///     Provides a standard API to handpay funds off of the EGM
    /// </summary>
    public interface IVoucherOutProvider : ITransferOutProvider, IService
    {
    }
}
