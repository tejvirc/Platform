using System.Net;

namespace Aristocrat.G2S.Client.Devices
{
    /// <summary>
    ///     Provides a mechanism to control the Multicast capabilities of a Communications device.
    /// </summary>
    public interface IMtpCapableDevice : IMulticastCoordinator
    {
        /// <summary>
        ///     Gets the multicast address.
        /// </summary>
        IPEndPoint MulticastAddress { get; }

        /// <summary>
        ///     Sets the MTP security parameters
        /// </summary>
        void SetMtpSecurityParameters(byte[] key, long messageId, byte[] nextKey, long keyChangeId);

        /// <summary>
        ///     Sets the multicast address.
        /// </summary>
        /// <param name="address">The new keep alive interval.</param>
        void SetMulticastAddress(string address);
    }
}