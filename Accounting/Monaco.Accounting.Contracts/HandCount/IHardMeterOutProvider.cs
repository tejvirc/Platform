namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using Kernel;
    using TransferOut;

    /// <summary>
    ///     Provides a standard API to handpay funds off of the EGM
    /// </summary>
    public interface IHardMeterOutProvider : ITransferOutProvider, IService
    {
    }
}
