namespace Aristocrat.Monaco.G2S.Common.GAT.CommandHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Kernel.Contracts.Components;
    using Monaco.Common.CommandHandlers;

    /// <summary>
    ///     Get components handler
    /// </summary>
    public class GetComponentsHandler : IFuncHandler<IEnumerable<Component>>
    {
        private readonly IComponentRegistry _componentRegistry;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetComponentsHandler" /> class.
        /// </summary>
        /// <param name="componentRegistry">The component repository.</param>
        public GetComponentsHandler(IComponentRegistry componentRegistry)
        {
            _componentRegistry = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));
        }

        /// <summary>
        ///     Executes this instance.
        /// </summary>
        /// <returns>List of components.</returns>
        public IEnumerable<Component> Execute()
        {
            return _componentRegistry.Components.ToList();
        }
    }
}