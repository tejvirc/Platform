namespace Aristocrat.Monaco.Gaming.Lobby.Services;

using System;
using System.Collections.Concurrent;
using SimpleInjector;

public class ObjectFactory : IObjectFactory
{
    private readonly Container _container;

    private readonly ConcurrentDictionary<Type, InstanceProducer> _producers = new();

    public ObjectFactory(Container container)
    {
        _container = container;
    }

    public T GetObject<T>() where T : class
    {
        var producer = _producers.GetOrAdd(typeof(T), CreateProducer);
        return (T)producer.GetInstance();
    }

    private InstanceProducer CreateProducer(Type implementationType) =>
        Lifestyle.Transient.CreateProducer(implementationType, implementationType, _container);
}
