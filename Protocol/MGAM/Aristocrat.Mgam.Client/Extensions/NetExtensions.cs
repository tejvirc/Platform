// ReSharper disable once CheckNamespace
namespace Aristocrat.Mgam.Client
{
    using System.Net;

    /// <summary>
    ///     Extension methods for network class.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class NetExtensions
    {
        /// <summary>
        ///     Gets the address of the end point.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        /// <returns>The address.</returns>
        public static string GetAddress(this IPEndPoint endPoint)
        {
            return endPoint?.Address.ToString() ?? string.Empty;
        }

        /// <summary>
        ///     Gets the address of the end point.
        /// </summary>
        /// <param name="address">The end point.</param>
        /// <returns>The address.</returns>
        public static string GetAddress(this IPAddress address)
        {
            return address?.ToString() ?? string.Empty;
        }
    }
}
