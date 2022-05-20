namespace Aristocrat.G2S.Client
{
    using System;
    using Configuration;

    /// <summary>
    ///     Factory method used to construct a new G2S client.
    /// </summary>
    public static class EgmFactory
    {
        /// <summary>
        ///     Creates a new instance of a G2S client using a delegate.
        /// </summary>
        /// <param name="configure">A delegate that is invoked allowing for configuration of the new client instance.</param>
        /// <returns>Returns a new IClientControl instance.</returns>
        public static IClientControl Create(Action<IEgmFactoryConfigurator> configure)
        {
            var configurator = new EgmFactoryConfigurator();

            configure(configurator);

            return configurator.Build();
        }
    }
}