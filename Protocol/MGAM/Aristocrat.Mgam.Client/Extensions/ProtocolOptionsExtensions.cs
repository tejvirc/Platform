// ReSharper disable once CheckNamespace
namespace Aristocrat.Mgam.Client
{
    using Options;
    using SimpleInjector;
    using System;

    /// <summary>
    ///     Options extension methods.
    /// </summary>
    public static class ProtocolOptionsExtensions
    {
        internal static Container AddOptions(this Container container, Action<ProtocolOptionsBuilder> optionsConfig)
        {
            var builder = new ProtocolOptionsBuilder(container);
            optionsConfig?.Invoke(builder);
            container.RegisterSingleton(() => builder);

            return container;
        }
    }
}
