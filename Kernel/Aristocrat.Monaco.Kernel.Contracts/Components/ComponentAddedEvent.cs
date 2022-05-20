namespace Aristocrat.Monaco.Kernel.Contracts.Components
{
    using System;

    /// <summary>
    ///     Published when a <see cref="Component" /> is added
    /// </summary>
    public class ComponentAddedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ComponentAddedEvent" /> class.
        /// </summary>
        /// <param name="component">The added component</param>
        public ComponentAddedEvent(Component component)
        {
            Component = component ?? throw new ArgumentNullException(nameof(component));
        }

        /// <summary>
        ///     Gets the added component
        /// </summary>
        public Component Component { get; }
    }
}