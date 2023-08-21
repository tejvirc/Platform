namespace Aristocrat.Monaco.Hardware.Contracts
{
    using System.Threading.Tasks;
    using Communicator;

    /// <summary>Interface for Gaming Device Standards (GDS) device.</summary>
    public interface IGdsDevice : IHardwareDevice, IDfuDevice
    {
        /// <summary>
        ///     Initializes this device.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<bool> Initialize(IGdsCommunicator communicator);
    }
}