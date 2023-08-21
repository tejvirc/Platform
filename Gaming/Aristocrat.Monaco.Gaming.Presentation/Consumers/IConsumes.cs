namespace Aristocrat.Monaco.Gaming.Presentation.Consumers;

using Kernel;

internal interface IConsumes<TEvent>
    where TEvent : IEvent
{
}
