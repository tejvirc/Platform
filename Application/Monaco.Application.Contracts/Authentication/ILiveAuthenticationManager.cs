namespace Aristocrat.Monaco.Application.Contracts.Authentication
{
    using Kernel;

    /// <summary>
    ///     Live Authentication Manager interface
    /// </summary>
    public interface ILiveAuthenticationManager : IService
    {
        /// <summary>
        ///     Gets or sets a value indicating whether or not the service is enabled.
        /// </summary>
        bool Enabled { get; set; }
    }
}
