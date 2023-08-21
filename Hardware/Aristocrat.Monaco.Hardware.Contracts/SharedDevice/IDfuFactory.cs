namespace Aristocrat.Monaco.Hardware.Contracts.SharedDevice
{
    /// <summary>Interface for dfu factory.</summary>
    public interface IDfuFactory
    {
        /// <summary>Creates an adapter.</summary>
        /// <param name="device">   The device.</param>
        /// <returns>The new adapter.</returns>
        IDfuAdapter CreateAdapter(IDfuDevice device);
    }
}
