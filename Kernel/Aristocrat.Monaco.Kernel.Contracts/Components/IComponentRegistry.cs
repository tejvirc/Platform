namespace Aristocrat.Monaco.Kernel.Contracts.Components
{
    using System.Collections.Generic;

    /// <summary>
    ///     Provides a mechanism to manage software components.
    /// </summary>
    public interface IComponentRegistry
    {
        /// <summary>
        ///     Gets a list of all registered components
        /// </summary>
        IEnumerable<Component> Components { get; }

        /// <summary>
        ///     Register a component
        /// </summary>
        /// <param name="component">Component to register</param>
        /// <param name="cycling">Component is being cycled</param>
        void Register(Component component, bool cycling = false);

        /// <summary>
        ///     Un-register a component
        /// </summary>
        /// <param name="id">ID of component to un-register</param>
        /// <param name="cycling">Component is being cycled</param>
        /// <returns>True if successful</returns>
        bool UnRegister(string id, bool cycling = false);

        /// <summary>
        ///     Get a component from its ID
        /// </summary>
        /// <param name="id">ID of component to fetch.</param>
        /// <returns>Component with designated ID, or null.</returns>
        Component Get(string id);
    }
}