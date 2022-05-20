namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;

    /// <summary>
    ///     A set of subscription extensions
    /// </summary>
    public static class SubscribeExtensions
    {
        /// <summary>
        ///     Used to subscribe to events and specify a delegate that will be called when events
        ///     of the specified type are received
        /// </summary>
        /// <param name="this">The event bus instance</param>
        /// <param name="subscriber">The object reference of the subscriber</param>
        /// <param name="type">The event type</param>
        /// <param name="callback">The delegate to call when this type of event is received</param>
        public static void Subscribe(this IEventBus @this, object subscriber, Type type, Action<IEvent> callback)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var method = @this.GetType().GetMethodEx(nameof(IEventBus.Subscribe), new[] { typeof(object), typeof(Action<>) });

            var genericMethod = method.MakeGenericMethod(type);
            genericMethod.Invoke(@this, new[] { subscriber, callback });
        }

        /// <summary>
        ///     Used to subscribe to events and specify a delegate that will be called when events
        ///     of the specified type are received
        /// </summary>
        /// <param name="this">The event bus instance</param>
        /// <param name="subscriber">The object reference of the subscriber</param>
        /// <param name="type">The event type</param>
        /// <param name="callback">The delegate to call when this type of event is received</param>
        public static void Subscribe(this IEventBus @this, object subscriber, Type type, Func<IEvent, CancellationToken, Task> callback)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var method = @this.GetType().GetMethodEx(nameof(IEventBus.Subscribe), new[] { typeof(object), typeof(Func<,,>) });

            var genericMethod = method.MakeGenericMethod(type);
            genericMethod.Invoke(@this, new[] { subscriber, callback });
        }

        /// <summary>
        /// Used to unsubscribe to events 
        /// </summary>
        /// 
        /// <param name="this"></param>
        /// <param name="subscriber"></param>
        /// <param name="type"></param>
        public static void Unsubscribe(this IEventBus @this, object subscriber, Type type)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var method = @this.GetType().GetMethodEx(nameof(IEventBus.Unsubscribe), new[] { typeof(object) });

            var genericMethod = method.MakeGenericMethod(type);
            genericMethod.Invoke(@this, new[] { subscriber });
        }
    }
}