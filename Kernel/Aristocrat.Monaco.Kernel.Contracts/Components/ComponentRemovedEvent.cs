namespace Aristocrat.Monaco.Kernel.Contracts.Components
{
    using System;

    /// <summary>
    ///     Published when a <see cref="Component" /> is added
    /// </summary>
    public class ComponentRemovedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ComponentRemovedEvent" /> class.
        /// </summary>
        /// <param name="component">The removed component</param>
        public ComponentRemovedEvent(Component component)
        {
            Component = component ?? throw new ArgumentNullException(nameof(component));
        }

        /// <summary>
        ///     Gets the removed component
        /// </summary>
        public Component Component { get; }
    }
}