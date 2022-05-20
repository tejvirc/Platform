namespace Aristocrat.G2S.Client.Configuration
{
    /// <summary>
    ///     Provides a mechanism to configure the EGM.
    /// </summary>
    public interface IEgmFactoryConfigurator :
        IClientConfigurator
    {
        /// <summary>
        ///     Configures the EGM Id.
        /// </summary>
        /// <param name="egmId">Overrides the EGM Id.</param>
        void WithEgmId(string egmId);

        /// <summary>
        ///     Builds a new instance of the G2S client with configured attributes.
        /// </summary>
        /// <returns>An instance of IClientControl.</returns>
        IClientControl Build();
    }
}