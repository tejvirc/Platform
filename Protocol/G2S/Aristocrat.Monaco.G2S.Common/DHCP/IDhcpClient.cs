namespace Aristocrat.Monaco.G2S.Common.DHCP
{
    /// <summary>
    ///     Dhcp client
    /// </summary>
    public interface IDhcpClient
    {
        /// <summary>
        ///     Retrieve string of vendor specific info.
        /// </summary>
        /// <returns>String of vendor specific info.</returns>
        string GetVendorSpecificInformation();
    }
}