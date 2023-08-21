namespace Aristocrat.Monaco.Hardware.Contracts
{
    using Kernel;
    using Kernel.Contracts;

    /// <summary>
    ///     Provides a mechanism to install a device driver update.
    /// </summary>
    public interface INoteAcceptorFirmwareInstaller : IInstaller, IService
    {
    }
}