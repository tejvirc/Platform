namespace Aristocrat.Extensions.Prism;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

internal class ConcreteAwareOverrideProvider : IServiceProvider
{
    private IServiceProvider _rootProvider { get; }

    private IServiceCollection? _services { get; }

    private (Type type, object instance)[] _overrides { get; }

    public ConcreteAwareOverrideProvider(IServiceProvider serviceProvider, IServiceCollection services, (Type type, object instance)[] overrides)
    {
        _rootProvider = serviceProvider;
        _overrides = overrides;
    }

    public object? GetService(Type serviceType)
    {
        if (!serviceType.IsAbstract && serviceType.IsClass && serviceType != typeof(object))
        {
            return BuildInstance(serviceType);
        }

        var serviceDescriptor = _services?.LastOrDefault((ServiceDescriptor x) => x.ServiceType == serviceType);
        if (serviceDescriptor?.ImplementationType is null)
        {
            return _rootProvider.GetService(serviceType);
        }

        Type implementationType = serviceDescriptor.ImplementationType;
        return BuildInstance(implementationType);
    }

    private object? BuildInstance(Type implType)
    {
        ConstructorInfo[] constructors = implType.GetConstructors();
        if (constructors == null || !constructors.Any())
        {
            return Activator.CreateInstance(implType);
        }

        var constructorInfo = constructors.OrderByDescending((ConstructorInfo x) => x.GetParameters().Length).First();
        var parameters = constructorInfo.GetParameters().Select(delegate (ParameterInfo x)
        {
            var (type, obj) = _overrides.FirstOrDefault(((Type type, object instance) o) => x.ParameterType == o.type);
            return (type != null) ? obj : _rootProvider.GetService(x.ParameterType);
        }).ToArray();

        return constructorInfo.Invoke(parameters);
    }
}
