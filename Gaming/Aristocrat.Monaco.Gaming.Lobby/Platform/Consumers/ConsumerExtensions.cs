using Aristocrat.Monaco.Gaming.Lobby.Platform.Consumers;

namespace Aristocrat.Monaco.Gaming.Lobby.Platform.Consumers;

using System;
using Common.Container;
using Kernel;
using Microsoft.Extensions.DependencyInjection;
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

        container.RegisterManyForOpenGeneric(typeof(IConsumes<>), typeof(IConsumes<>).Assembly);

        return container;
    }
}
