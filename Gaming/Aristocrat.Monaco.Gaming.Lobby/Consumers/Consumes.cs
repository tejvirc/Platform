namespace Aristocrat.Monaco.Gaming.Lobby.Consumers;

using Aristocrat.Monaco.Kernel;
using System.Threading.Tasks;
using System.Threading;

/// <summary>
///     A user-friendly event receiver.
/// </summary>
/// <typeparam name="TEvent">The event type.</typeparam>
public abstract class Consumes<TEvent> : IConsumer<TEvent>
    where TEvent : IEvent
{
    /// <inheritdoc />
    public void Consume(TEvent theEvent)
    {
        ConsumeAsync(theEvent, CancellationToken.None).Wait();
    }

    /// <summary>
    ///     Consumes an event
    /// </summary>
    /// <param name="theEvent">The event to consume</param>
    /// <param name="cancellationToken"></param>
    public abstract Task ConsumeAsync(TEvent theEvent, CancellationToken cancellationToken);
}