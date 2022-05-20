namespace Aristocrat.Bingo.Client.Messages
{
    using Grpc.Core;

    /// <summary>
    ///     The provider for authorization within the platform
    /// </summary>
    public interface IAuthorizationProvider
    {
        /// <summary>
        ///     Gets the authorization data to use for communication
        /// </summary>
        Metadata AuthorizationData { get; set; }
    }
}