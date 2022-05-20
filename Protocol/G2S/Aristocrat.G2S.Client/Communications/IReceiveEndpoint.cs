namespace Aristocrat.G2S.Client.Communications
{
    using System;

    /// <summary>
    ///     Provides a mechanism for controlling an endpoint.
    /// </summary>
    public interface IReceiveEndpoint : IEndpoint, IDisposable
    {
        /// <summary>
        ///     Opens the endpoint.
        /// </summary>
        void Open();

        /// <summary>
        ///     Closes the endpoint.
        /// </summary>
        void Close();
    }
}