namespace Aristocrat.Monaco.Bingo.Commands
{
    using System.Net.NetworkInformation;
    using Application.Contracts;

    public sealed class NetworkInformationProvider : INetworkInformationProvider
    {
        /// <inheritdoc />
        public PhysicalAddress GetPhysicalAddress()
        {
            var networkInterface = NetworkInterfaceInfo.DefaultNetworkInterface;
            return networkInterface?.GetPhysicalAddress() ?? PhysicalAddress.None;
        }
    }
}