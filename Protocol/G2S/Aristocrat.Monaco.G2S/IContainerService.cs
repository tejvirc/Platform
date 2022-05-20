namespace Aristocrat.Monaco.G2S
{
    using Kernel;
    using SimpleInjector;

    /// <summary>
    ///     Provides a service to get the dependency injection container for this layer.
    ///     This is used as a workaround for ViewModel classes that are not
    ///     hooked up to the DI framework so that they can get access to the container.
    /// </summary>
    public interface IContainerService : IService
    {
        /// <summary>
        ///     Gets the container.
        /// </summary>
        Container Container { get; }
    }
}