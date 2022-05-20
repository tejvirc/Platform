namespace Aristocrat.Monaco.Hardware.DFU
{
    using Contracts;
    using Contracts.SharedDevice;

    /// <summary>A dfu factory.</summary>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.SharedDevice.IDfuFactory" />
    public class DfuFactory : IDfuFactory
    {
        /// <inheritdoc />
        public IDfuAdapter CreateAdapter(IDfuDevice device)
        {
            return new DfuAdapter(device);
        }
    }
}