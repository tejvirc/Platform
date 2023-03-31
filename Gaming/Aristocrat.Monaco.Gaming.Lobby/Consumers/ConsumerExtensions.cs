namespace Aristocrat.Monaco.Gaming.Lobby.Consumers;

using System;
using Common.Container;
using Kernel;
using SimpleInjector;

public static class ConsumerExtensions
{
    public static Container RegisterConsumers(this Container container)
    {
        if (container == null)
        {
            throw new ArgumentNullException(nameof(container));
        }

        // TODO Refactor to use DI container
        ServiceManager.GetInstance().AddService(new SharedConsumerContext());

        container.RegisterManyForOpenGeneric(typeof(IConsumes<>), typeof(Consumers.Consumes<>).Assembly);

        return container;
    }
}
