namespace Aristocrat.Monaco.Application.PerformanceCounter
{
    using System;
    using System.Diagnostics;

    public class SingleCustomPerformanceCounterWrapper : IPerformanceCounterWrapper, IDisposable
    {
        private readonly PerformanceCounter _counter;
        private bool _disposed;

        public SingleCustomPerformanceCounterWrapper(string category, string counter, string categoryHelp, string counterHelp)
        {
            if (!PerformanceCounterCategory.Exists(category))
            {
                PerformanceCounterCategory.Create(category, "A custom category for Monaco", PerformanceCounterCategoryType.SingleInstance, counter, "CPU Temperature Counter");
            }

            _counter = new PerformanceCounter(category, counter, false);
        }

        public float NextValue()
        {
            return _counter.NextValue();
        }

        public void SetRawValue(uint value)
        {
            _counter.RawValue = value;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _counter.Dispose();
            _disposed = true;
        }
    }
}
