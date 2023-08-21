namespace Aristocrat.G2S.Client.Security
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Sock address interop
    /// </summary>
    internal static class SockAddressInterop
    {
        /// <summary>
        ///     Creates an unmanaged sockAddress structure to pass to a WinAPI function.
        /// </summary>
        /// <param name="ipEndPoint">IP address and port number</param>
        /// <returns>a handle for the structure. Use the AddressOfPinnedObject Method to get a stable pointer to the object. </returns>
        /// <remarks>
        ///     When the handle goes out of scope you must explicitly release it by calling the Free method; otherwise, memory
        ///     leaks may occur.
        /// </remarks>
        internal static GCHandle CreateSockAddressStructure(IPEndPoint ipEndPoint)
        {
            var socketAddress = ipEndPoint.Serialize();

            var sockAddressStructureBytes = new byte[socketAddress.Size];
            var handle = GCHandle.Alloc(sockAddressStructureBytes, GCHandleType.Pinned);
            for (var i = 0; i < socketAddress.Size; ++i)
            {
                sockAddressStructureBytes[i] = socketAddress[i];
            }

            return handle;
        }

        /// <summary>
        ///     Reads the unmanaged sockAddress structure returned by a WinAPI function
        /// </summary>
        /// <param name="pSockAddressStructure">pointer to the unmanaged sockAddress structure</param>
        /// <returns>IP address and port number</returns>
        internal static IPEndPoint ReadSockAddressStructure(IntPtr pSockAddressStructure)
        {
            var sAddressFamily = Marshal.ReadInt16(pSockAddressStructure);
            var addressFamily = (AddressFamily)sAddressFamily;

            int sockAddressStructureSize;
            IPEndPoint ipEndPointAny;
            switch (addressFamily)
            {
                case AddressFamily.InterNetwork:
                    // IP v4 address
                    sockAddressStructureSize = 16;
                    ipEndPointAny = new IPEndPoint(IPAddress.Any, 0);
                    break;
                case AddressFamily.InterNetworkV6:
                    // IP v6 address
                    sockAddressStructureSize = 28;
                    ipEndPointAny = new IPEndPoint(IPAddress.IPv6Any, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pSockAddressStructure), "Unknown address family");
            }

            // get bytes of the sockAddress structure
            var sockAddressStructureBytes = new byte[sockAddressStructureSize];
            Marshal.Copy(pSockAddressStructure, sockAddressStructureBytes, 0, sockAddressStructureSize);

            // create SocketAddress from bytes
            var socketAddress = new SocketAddress(AddressFamily.Unspecified, sockAddressStructureSize);
            for (var i = 0; i < sockAddressStructureSize; i++)
            {
                socketAddress[i] = sockAddressStructureBytes[i];
            }

            // create IPEndPoint from SocketAddress
            var result = (IPEndPoint)ipEndPointAny.Create(socketAddress);

            return result;
        }
    }
}