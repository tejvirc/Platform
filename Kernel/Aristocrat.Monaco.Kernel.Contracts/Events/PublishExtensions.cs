namespace Aristocrat.Monaco.Kernel
{
    using System;

    /// <summary>
    ///     A set of publish extensions
    /// </summary>
    public static class PublishExtensions
    {
        /// <summary>
        ///     Used to publish events where the type is not known at compile time
        /// </summary>
        /// <param name="this">The event bus instance</param>
        /// <param name="type">The event type</param>
        /// <param name="event">The event to be published</param>
        public static void Publish(this IEventBus @this, Type type, IEvent @event)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var method = @this.GetType().GetMethod("Publish");
            if (method == null)
            {
                throw new ArgumentException(
                    @"Unable to find a Publish method. The interface must have changed.",
                    nameof(@this));
            }

            var genericMethod = method.MakeGenericMethod(type);
            genericMethod.Invoke(@this, new object[] { @event });
        }
    }
}