namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    /// <summary>
    ///     Used to get a provider for localization services.
    /// </summary>
    public interface ILocalizerFactory
    {
        /// <summary>
        ///     Get the provider with specified name.
        /// </summary>
        /// <param name="name">Provider name.</param>
        /// <returns>The provider with the specified name.</returns>
        ILocalizer For(string name);
    }
}
