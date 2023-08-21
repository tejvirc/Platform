namespace Aristocrat.Monaco.G2S.Consumers
{
    using Kernel;

    /// <summary>A user-friendly event receiver</summary>
    /// <typeparam name="TEvent">The event type</typeparam>
    public abstract class Consumes<TEvent> : Kernel.Consumes<TEvent>
        where TEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Consumes{TEvent}" /> class.
        /// </summary>
        protected Consumes()
            : base(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().TryGetService<ISharedConsumer>())
        {
        }
    }
}