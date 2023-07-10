namespace Aristocrat.Monaco.Application.PerformanceCounter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    /// <summary>
    ///     An implementation of IPerformanceCounterWrapper that adds together numerous underlying
    ///     counters. This is created so we can give a single GPU usage value, and could later be
    ///     used for other similar purposes.
    /// </summary>
    public class AggregatePerformanceCounterWrapper : IPerformanceCounterWrapper
    {
        private readonly PerformanceCounterCategory _category;
        private readonly Dictionary<string, PerformanceCounter> _counters = new();
        private readonly string _counter;
        private float _nextValue;
        private Thread _calculationThread;

        /// <summary>
        ///     Construct an aggregate performance counter given the category and counter type
        ///     we want to aggregate. On each request for a value, we will add together all
        ///     instances of this counter that currently exist in the system.
        /// </summary>
        /// <param name="category">The category of counter we wish to aggregate</param>
        /// <param name="counter">The type of counter we want to sum over all instances</param>
        public AggregatePerformanceCounterWrapper(string category, string counter)
        {
            _category = new PerformanceCounterCategory(category);
            _counter = counter;
            _nextValue = 0.0f;
        }

        /// <inheritdoc />
        public float NextValue()
        {
            // Querying ALL the performance counters for e.g. GPU usage and adding them up can
            // actually take far longer than one second, which is how fast we try to update when
            // in the "live" statistics screen in the menu, so we calculate in the background and
            // return a cached value that is updated as quick as we can. This is very apparent
            // on a developer PC running lots of applications, but should be less of an overhead
            // on an actual EGM, and most importantly shouldn't be an issue during game operation
            // when the query only happens once every 15 seconds.
            TryCalculate();
            return _nextValue;
        }

        private void TryCalculate()
        {
            // If we're already calculating, let that finish.
            if (_calculationThread != null) return;

            // Otherwise, start calculating a new total in the background.
            _calculationThread = new Thread(Calculate)
            {
                Priority = ThreadPriority.Lowest
            };
            _calculationThread.Start();
        }

        private void Calculate()
        {
            // Not only does the querying of all the counters take quite a while, the counters
            // themselves have to be enumerated repeatedly if a process starts or stops. Furthermore,
            // a counter might be invalidated by the time we try to read it, so no fancy Linq
            // chaining for us...
            float newValue = 0.0f;
            try
            {
                // Get the list of counters that are currently being kept for this category.
                var instances = _category.GetInstanceNames();

                // First let's have a stab at removing any counters that don't exist any more. This
                // isn't 100% reliable on a busy PC with apps opening and closing, but as with the
                // overall approach, should work a lot better on an EGM in the field. Note that the
                // whole reason we're doing this is because some counters just read zero on their
                // first value, so you can't just create new counters every time you calculate and
                // take a snapshot, you have to ask the same counter object again later what the
                // value has been doing since you last checked.
                var knownKeys = _counters.Keys.ToArray();
                foreach (string knownKey in knownKeys)
                {
                    if (!instances.Contains(knownKey))
                    {
                        _counters.Remove(knownKey);
                    }
                }

                // Finally, let's try to add up the values for all the counters we have!
                foreach (var instance in instances)
                {
                    if (_counters.ContainsKey(instance))
                    {
                        newValue += _counters[instance].NextValue();
                    }
                    else
                    {
                        var counters = _category.GetCounters(instance);
                        foreach (var counter in counters)
                        {
                            if (counter.CounterName == _counter)
                            {
                                _counters.Add(instance, counter);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // There's a bunch of reasons this can go wrong, which we're not trying super hard
                // to protect against, because we want to do this fast without contending with the
                // rest of the system. Again, this is far less likely to happen on a real EGM as the
                // only process that's starting and stopping regularly is the GDK, who is the one we
                // really want to track aggregate data like GPU usage for anyway. So if we hit a
                // problem on the border between running two games, never mind, we'll get it next time.
            }

            _nextValue = newValue;
            _calculationThread = null;
        }

        public void SetRawValue(uint value)
        {
            throw new System.NotImplementedException();
        }
    }
}