namespace Aristocrat.Monaco.Application.Contracts.Metering
{
    using System;
    using System.Globalization;

    /// <summary>
    ///     Definition of the NonPersistentAtomicMeter class.
    /// </summary>
    public class NonPersistentAtomicMeter : IMeter
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NonPersistentAtomicMeter" /> class.
        /// </summary>
        /// <param name="meterName">Name of the meter.</param>
        /// <param name="classification">The classification.</param>
        /// <param name="provider">The provider.</param>
        public NonPersistentAtomicMeter(
            string meterName,
            MeterClassification classification,
            IMeterProvider provider)
        {
            Name = meterName;
            Classification = classification;
            provider.RegisterMeterClearDelegate(ClearPeriod);
        }

        /// <summary>
        ///     The event for the meter value change.
        /// </summary>
        public event EventHandler<MeterChangedEventArgs> MeterChangedEvent;

        /// <summary>
        ///     Gets the class of the meter
        /// </summary>
        public MeterClassification Classification { get; }

        /// <summary>
        ///     Gets the name of the meter
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the Lifetime meter value
        /// </summary>
        public long Lifetime { get; private set; }

        /// <summary>
        ///     Gets the Period meter value
        /// </summary>
        public long Period { get; private set; }

        /// <summary>
        ///     Gets the Session meter value
        /// </summary>
        public long Session { get; private set; }

        /// <summary>
        ///     Increments the Lifetime, Period and Session values for this meter
        /// </summary>
        /// <param name="amount">The amount to increment the meter by</param>
        public void Increment(long amount)
        {
            if (Lifetime + amount <= 0)
            {
                Lifetime = 0;
            }
            else if (Classification.UpperBounds > Lifetime + amount)
            {
                Lifetime += amount;
            }
            else
            {
                var message = string.Format(
                    CultureInfo.InvariantCulture,
                    "UpperBound exceeded for meter {0}\nUpperBound limit: {1}\nLifetime Value: {2}",
                    Name,
                    Classification.UpperBounds,
                    Lifetime + amount);

                throw new MeterException(message);
            }

            if (Period + amount <= 0)
            {
                Period = 0;
            }
            else if (Classification.UpperBounds > Period + amount)
            {
                Period += amount;
            }
            else
            {
                var message = string.Format(
                    CultureInfo.InvariantCulture,
                    "UpperBound exceeded for meter {0}\nUpperBound limit: {1}\nPeriod Value: {2}",
                    Name,
                    Classification.UpperBounds,
                    Period + amount);

                throw new MeterException(message);
            }

            if (Session + amount <= 0)
            {
                Session = 0;
            }
            else if (Classification.UpperBounds > Session + amount)
            {
                Session += amount;
            }
            else
            {
                var message = string.Format(
                    CultureInfo.InvariantCulture,
                    "UpperBound exceeded for meter {0}\nUpperBound limit: {1}\nSession Value: {2}",
                    Name,
                    Classification.UpperBounds,
                    Session + amount);

                throw new MeterException(message);
            }

            MeterChangedEvent?.Invoke(this, new MeterChangedEventArgs(amount));
        }

        /// <summary>
        ///     Used to clear the period meter by a provider through a delegate
        /// </summary>
        private void ClearPeriod()
        {
            Period = 0;
        }
    }
}