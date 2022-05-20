namespace Aristocrat.Monaco.Sas.Contracts.Intercomponent
{
    using Kernel;
    using SimpleInjector;

    /// <summary>
    ///     An extended service interface through which all components in the Sas layer can
    ///     talk to each other. 
    /// </summary>
    public interface ISasContainerService : IService
    {
        /// <summary>
        ///     Gets the container.
        /// </summary>
        Container Container { get; }
    }
}
