namespace Aristocrat.Monaco.Application.PerformanceCounter
{
    using System.Diagnostics;

    /// <summary>
    ///     An implementation of IPerformanceCounterWrapper that is simply a single counter.
    /// </summary>
    public class SinglePerformanceCounterWrapper : IPerformanceCounterWrapper
    {
        private readonly PerformanceCounter _counter;

        /// <summary>
        ///     Construct a performance counter given the category, counter type and instance.
        ///     Usually this means a process specific performance counter.
        /// </summary>
        /// <param name="category">The category of the counter we wish to read</param>
        /// <param name="counter">The type of the counter we wish to read</param>
        /// <param name="instance">The instance name of the counter we wish to read</param>
        public SinglePerformanceCounterWrapper(string category, string counter, string instance)
        {
            _counter = new PerformanceCounter(category, counter, instance);   
        }

        /// <summary>
        ///     Construct a performance counter given the category and counter type. Usually this
        ///     means a system-wide performance counter.
        /// </summary>
        /// <param name="category">The category of the counter we wish to read</param>
        /// <param name="counter">The type of the counter we wish to read</param>
        public SinglePerformanceCounterWrapper(string category, string counter)
        {
            _counter = new PerformanceCounter(category, counter);
        }

        public void SetRawValue(uint value)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public float NextValue()
        {
            return _counter.NextValue();
        }
    }
}
