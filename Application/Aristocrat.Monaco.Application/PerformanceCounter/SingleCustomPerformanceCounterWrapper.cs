namespace Aristocrat.Monaco.Application.PerformanceCounter
{
    using System.Diagnostics;

    public class SingleCustomPerformanceCounterWrapper : IPerformanceCounterWrapper
    {
        private readonly PerformanceCounter _counter;

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
    }
}
