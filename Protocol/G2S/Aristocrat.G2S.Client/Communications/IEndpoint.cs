namespace Aristocrat.G2S.Client.Communications
{
    using System;

    /// <summary>
    ///     Describes an endpoint
    /// </summary>
    public interface IEndpoint
    {
        /// <summary>
        ///     Gets the Url of the endpoint
        /// </summary>
        Uri Address { get; }
    }
}