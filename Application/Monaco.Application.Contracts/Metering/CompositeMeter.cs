namespace Aristocrat.Monaco.Application.Contracts
{
    using Flee.PublicTypes;
    using log4net;
    using Metering;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     A meter implementation for meters in which all values in the different time frames
    ///     are calculated from a formula.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         All values in three time frames are not persisted in the storage because they are calculated
    ///         at the runtime.
    ///     </para>
    ///     <para>
    ///         The Fast Lightweight Expression Evaluator (Flee) is used to evaluate an expression.
    ///         You can download it from http://flee.codeplex.com/releases/view/7153 to learn more.
    ///     </para>
    /// </remarks>
    public class CompositeMeter : IMeter, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly object _lockObject = new object();

        private readonly Type _classificationType;

        private readonly List<string> _meterNames;
        private List<IMeter> _meters;
        private bool _initialized;

        private ExpressionContext _expressionContext;
        private IGenericExpression<long> _occurrenceExpression;
        private IGenericExpression<double> _percentageExpression;
        private IGenericExpression<decimal> _currencyExpression;

        private readonly Func<MeterTimeframe, long> _expression;

        private long _currentLifeTimeValue;

        private event EventHandler<MeterChangedEventArgs> CompositeMeterChangedEvent;

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompositeMeter" /> class.
        /// </summary>
        /// <param name="name">Name of the composite meter.</param>
        /// <param name="expression">Expression that defines the meter.</param>
        /// <param name="meterNameList">List of the names of the meters used in the formula.</param>
        /// <param name="classificationName">Name of the classification of the meter.</param>
        public CompositeMeter(string name, Func<MeterTimeframe, long> expression, IEnumerable<string> meterNameList, string classificationName)
            : this(name, expression, meterNameList, MeterUtilities.ParseClassification(classificationName))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompositeMeter" /> class.
        /// </summary>
        /// <param name="name">Name of the composite meter.</param>
        /// <param name="expression">Expression that defines the meter.</param>
        /// <param name="meterNameList">List of the names of the meters used in the formula.</param>
        /// <param name="classification">Classification of the meter.</param>
        public CompositeMeter(string name, Func<MeterTimeframe, long> expression, IEnumerable<string> meterNameList, MeterClassification classification)
        {
            Name = name;

            _expression = expression ?? throw new ArgumentNullException(nameof(expression));

            _meterNames = meterNameList.ToList();

            Classification = classification;

            _classificationType = Classification.GetType();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompositeMeter" /> class.
        /// </summary>
        /// <param name="name">Name of the composite meter.</param>
        /// <param name="formula">Formula of the composite meter.</param>
        /// <param name="meterNameList">List of the names of the meters used in the formula.</param>
        /// <param name="classificationName">Name of the classification of the meter.</param>
        /// <exception cref="MeterException">
        ///     Thrown when any of the atomic meters do not have the same classification as the
        ///     one for the composite meter.
        /// </exception>
        public CompositeMeter(string name, string formula, IEnumerable<string> meterNameList, string classificationName)
        {
            Name = name;
            Formula = formula;

            //Logger.Debug($"Constructing \"{name}\", formula = \"{formula}\"");

            _meterNames = meterNameList.ToList();

            Classification = MeterUtilities.ParseClassification(classificationName);
            if (Classification == null)
            {
                var message = $"Meter \"{Name}\" needs unknown classification\"{classificationName}\"";
                Logger.Fatal(message);
                throw new MeterException(message);
            }

            _classificationType = Classification.GetType();
        }

        /// <summary>
        ///     Gets the formula for the composite meter.
        ///     The formula expression looks like: MeterA + MeterB + MeterC. The meter
        ///     in the expression can be either of a atomic or composite meter.
        /// </summary>
        public string Formula { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public event EventHandler<MeterChangedEventArgs> MeterChangedEvent
        {
            add
            {
                if (_currentLifeTimeValue < 0)
                {
                    SubscribeDependencies();
                }

                CompositeMeterChangedEvent += value;
            }
            remove => CompositeMeterChangedEvent -= value;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public MeterClassification Classification { get; }

        /// <inheritdoc />
        public long Lifetime => GetValue(MeterTimeframe.Lifetime);

        /// <inheritdoc />
        public long Period => GetValue(MeterTimeframe.Period);

        /// <inheritdoc />
        public long Session => GetValue(MeterTimeframe.Session);

        /// <inheritdoc />
        public void Increment(long amount)
        {
            lock (_lockObject)
            {
                Logger.Fatal(
                    $"This method {MethodBase.GetCurrentMethod()!.Name} can never be called because all values are calculated from an express provided at runtime");
            }

            throw new NotImplementedException();
        }

        /// <summary>
        ///     Initializes this meter currently mainly for the Event Registration.
        /// </summary>
        /// <param name="meterManager">The instance of meter manager.</param>
        /// <exception cref="ArgumentNullException">Thrown when the meter manager provided is null</exception>
        /// <remarks>
        ///     The MeterChangedEvent in each meter contained in the formula expression is hooked up
        ///     in this initialization.
        /// </remarks>
        public void Initialize(IMeterManager meterManager)
        {
            if (meterManager == null)
            {
                throw new ArgumentNullException(nameof(meterManager));
            }

            if (_initialized)
            {
                return;
            }

            GetMeters(meterManager);

            _currentLifeTimeValue = -1;

            _initialized = true;
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_meters != null)
                {
                    foreach (var meter in _meters)
                    {
                        meter.MeterChangedEvent -= OnMeterValueChanges;
                    }

                    _meters.Clear();
                }
            }

            _disposed = true;
        }

        private long GetValue(MeterTimeframe timeFrame)
        {
            lock (_lockObject)
            {
                return _expression?.Invoke(timeFrame) % Classification.UpperBounds ?? EvaluateFormula(timeFrame);
            }
        }

        private void GetMeters(IMeterManager meterManager)
        {
            lock (_lockObject)
            {
                if (_meters != null)
                {
                    return;
                }

                _meters = new List<IMeter>(_meterNames.Count);
                var item = 0;

                foreach (var meterName in _meterNames)
                {
                    var meter = meterManager.GetMeter(meterName);

                    if (meter is CompositeMeter compositeMeter)
                    {
                        compositeMeter.Initialize(meterManager);
                    }

                    _meters.Insert(item++, meter);
                }
            }
        }

        private long EvaluateFormula(MeterTimeframe timeFrame)
        {
            if (_meters == null)
            {
                return 0;
            }

            if (_expressionContext == null)
            {
                _expressionContext = new ExpressionContext();
            }

            foreach (var meter in _meters)
            {
                if (_classificationType == typeof(PercentageMeterClassification))
                {
                    _expressionContext.Variables[meter.Name] = (double)meter.GetValue(timeFrame);
                }
                else if (_classificationType == typeof(OccurrenceMeterClassification))
                {
                    _expressionContext.Variables[meter.Name] = meter.GetValue(timeFrame);
                }
                else
                {
                    _expressionContext.Variables[meter.Name] = (decimal)meter.GetValue(timeFrame);
                }
            }

            if (_classificationType == typeof(PercentageMeterClassification))
            {
                if (_percentageExpression == null)
                {
                    _percentageExpression = _expressionContext.CompileGeneric<double>(Formula);
                }

                var percentage = _percentageExpression.Evaluate();

                if (double.IsNaN(percentage) || double.IsInfinity(percentage))
                {
                    return 0;
                }

                try
                {
                    return (long)Math.Round(percentage * PercentageMeterClassification.Multiplier);
                }
                catch (OverflowException)
                {
                    return long.MaxValue;
                }
            }

            if (_classificationType == typeof(OccurrenceMeterClassification))
            {
                if (_occurrenceExpression == null)
                {
                    _occurrenceExpression = _expressionContext.CompileGeneric<long>(Formula);
                }

                return _occurrenceExpression.Evaluate() % Classification.UpperBounds;
            }

            if (_currencyExpression == null)
            {
                _currencyExpression = _expressionContext.CompileGeneric<decimal>(Formula);
            }

            return (long)(_currencyExpression.Evaluate() % Classification.UpperBounds);
        }

        private void OnMeterValueChanges(object sender, EventArgs e)
        {
            long delta;

            lock (_lockObject)
            {
                var newLifeTimeValue = Lifetime;
                delta = newLifeTimeValue - _currentLifeTimeValue;
                _currentLifeTimeValue = newLifeTimeValue;
            }

            // Don't put this inside the lock.  This meter change can be other higher level meters than have
            // their own locks that can cause deadlocks if we handle this inside the lock
            CompositeMeterChangedEvent?.Invoke(this, new MeterChangedEventArgs(delta));
        }

        private void SubscribeDependencies()
        {
            if (_meters == null)
            {
                return;
            }

            _currentLifeTimeValue = Lifetime;

            foreach (var meter in _meters)
            {
                meter.MeterChangedEvent += OnMeterValueChanges;
            }
        }
    }
}
