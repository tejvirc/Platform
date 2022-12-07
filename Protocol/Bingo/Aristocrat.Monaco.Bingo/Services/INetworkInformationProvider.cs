namespace Aristocrat.Monaco.Bingo.Commands
{
    using System.Net.NetworkInformation;

    /// <summary>
    ///     Provider for getting network information
    /// </summary>
    public interface INetworkInformationProvider
    {
        /// <summary>
        ///     Gets the physical address for the device
        /// </summary>
        /// <returns>The physical address or no address if one is not found</returns>
        PhysicalAddress GetPhysicalAddress();
    }
}