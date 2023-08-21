namespace Aristocrat.Monaco.Gaming.Presentation.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Kernel;

/// <summary>
///     A user-friendly event receiver.
/// </summary>
/// <typeparam name="TEvent">The event type.</typeparam>
public abstract class Consumes<TEvent> : Kernel.Consumes<TEvent>, IConsumes<TEvent>
    where TEvent : BaseEvent
{
    protected Consumes()
        : base(
            ServiceManager.GetInstance().GetService<IEventBus>(),
            ServiceManager.GetInstance().GetService<ISharedConsumer>())
    {
    }

    /// <inheritdoc />
    public override void Consume(TEvent theEvent)
    {
        ConsumeAsync(theEvent, CancellationToken.None).Wait();
    }

    /// <summary>
    ///     Consumes an event asynchronously
    /// </summary>
    /// <param name="theEvent">The event to consume</param>
    /// <param name="cancellationToken"></param>
    public abstract Task ConsumeAsync(TEvent theEvent, CancellationToken cancellationToken);
}
