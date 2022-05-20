namespace Aristocrat.Monaco.Application.UI.MeterPage
{
    using System;

    /// <summary>
    ///     This is a display meter that acts as a proxy for the underlying object to view its relevant information.
    /// </summary>
    [CLSCompliant(false)]
    public class ProxyDisplayMeter<T> : DisplayMeter
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProxyDisplayMeter{T}"/> class.
        /// </summary>
        /// <param name="meterName">The text to display on the meters page in the Meter column</param>
        /// <param name="order">The placement of the meter on the screen relative to other meters</param>
        /// <param name="underlyingObject">The underlying object whose data is used to return the required information</param>
        /// <param name="valuePredicate">The function that pulls the relevant information from the underlying object and formats it as a string</param> 
        public ProxyDisplayMeter(string meterName, T underlyingObject, Func<T, string> valuePredicate, int order = 0) : base(meterName, null, false, order)
        {
            UnderlyingObject = underlyingObject;
            ValuePredicate = valuePredicate;
        }

        /// <summary>
        ///     The underlying object
        /// </summary>
        private T UnderlyingObject { get; }

        /// <summary>
        ///     The underlying object
        /// </summary>
        private Func<T, string> ValuePredicate { get; }

        /// <summary>
        ///     The formatted value of the meter
        /// </summary>
        public override string Value => ValuePredicate(UnderlyingObject);
    }
}
