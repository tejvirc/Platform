namespace Aristocrat.Monaco.Common.Container
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using PerformanceCounters;

    /// <summary>
    ///     Defines a performance based interceptor
    /// </summary>
    public class PerformanceInterceptor : IInterceptor
    {
        private readonly ConcurrentDictionary<Type, (string category, string name, PerformanceCounterType? type)> _cache =
            new ConcurrentDictionary<Type, (string category, string name, PerformanceCounterType? type)>();

        /// <inheritdoc />
        public void Intercept(IInvocation invocation)
        {
            var decoratedType = invocation.InvocationTarget.GetType();

            if (!_cache.TryGetValue(decoratedType, out var counter))
            {
                if (!(Attribute.GetCustomAttribute(decoratedType, typeof(CounterDescriptionAttribute)) is
                        CounterDescriptionAttribute description) ||
                    !PerformanceCounterCategory.CounterExists(description.Name, description.Category))
                {
                    counter = (string.Empty, string.Empty, null);
                }
                else
                {
                    counter = (description.Category, description.Name, description.Type);
                }

                _cache.TryAdd(decoratedType, counter);
            }

            if (!counter.type.HasValue)
            {
                invocation.Proceed();
                return;
            }

            switch (counter.type)
            {
                case PerformanceCounterType.NumberOfItems32:
                case PerformanceCounterType.NumberOfItems64:
                    using (var main = new PerformanceCounter(counter.category, counter.name, false))
                    {
                        invocation.Proceed();

                        main.Increment();
                    }
                    break;
                case PerformanceCounterType.AverageTimer32:
                    var watch = Stopwatch.StartNew();

                    invocation.Proceed();

                    watch.Stop();

                    using (var main = new PerformanceCounter(counter.category, counter.name, false))
                    using (var baseCounter = new PerformanceCounter(counter.category, $"{counter.name}Base", false))
                    {
                        main.IncrementBy(watch.ElapsedTicks);
                        baseCounter.Increment();
                    }
                    break;
                default:
                    invocation.Proceed();
                    break;
            }
        }
    }
}