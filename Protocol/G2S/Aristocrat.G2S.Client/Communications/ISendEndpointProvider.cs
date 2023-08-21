namespace Aristocrat.G2S.Client.Communications
{
    using System;

    /// <summary>
    ///     Provides a mechanism to retrieve and endpoint.
    /// </summary>
    public interface ISendEndpointProvider
    {
        /// <summary>
        ///     Gets an endpoint. The endpoint will be added if it does not exist.
        /// </summary>
        /// <param name="hostId">Host identifier of the endpoint.</param>
        /// <param name="address">The host address.</param>
        /// <returns>Returns an ISendEndpoint instance.</returns>
        ISendEndpoint GetOrAddEndpoint(int hostId, Uri address);

        /// <summary>
        ///     Updates an endpoint. The endpoint will be added if it does not exist.
        /// </summary>
        /// <param name="hostId">Host identifier of the endpoint.</param>
        /// <param name="address">The host address.</param>
        /// <returns>Returns an ISendEndpoint instance.</returns>
        ISendEndpoint GetOrUpdateEndpoint(int hostId, Uri address);

        /// <summary>
        ///     Gets an endpoint.
        /// </summary>
        /// <param name="hostId">Host identifier of the endpoint.</param>
        /// <returns>Returns an ISendEndpoint instance.</returns>
        ISendEndpoint GetEndpoint(int hostId);
    }
}