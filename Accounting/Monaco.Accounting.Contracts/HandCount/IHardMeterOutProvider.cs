namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using Kernel;
    using TransferOut;

    /// <summary>
    ///     Defines a transfer out provider that will implement transferring funds off the EGM by ticking
    ///     a hard meter that is connected to some external device.
    /// </summary>
    public interface IHardMeterOutProvider : ITransferOutProvider, IService
    {
    }
}