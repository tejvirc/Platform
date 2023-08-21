namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     An interface to expose information about a component providing meters.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         In XSpin, all atomic meters are provided by components implementing
    ///         this interface. A basic implementation is provided in this assembly to
    ///         enhance the code reusability; check the <seealso cref="BaseMeterProvider" /> to
    ///         see its implementation.
    ///     </para>
    ///     <para>
    ///         The implementation of this interface is responsible for the persistence of meters
    ///         it provides.
    ///     </para>
    ///     <para>
    ///         You can either configure your IMeterProver component as a addin, or add it programmatically by
    ///         calling the AddProvider() method of IMeterManager. The former is illustrated in the
    ///         example section. A composite meter can be provided in an addin configuration file if all
    ///         names of meters in the expression are statically known, or in your IMeterProvider component when
    ///         some names of meters in the expression have to be dynamically determined.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     Create your own meter provider:
    ///     <code>
    ///      public class YourMetersProvider : BaseMeterProvider
    ///      {
    ///        private const PersistenceLevel Level = PersistenceLevel.Critical;
    ///        private static readonly string[] YourMeterNames = new[]
    ///        {
    ///          "Meter1",
    ///          "Meter2"
    ///        }
    ///        public YourMetersProvider : base(typeof(YourMetersProvider).ToString()
    ///        {
    ///           IPersistentStorageAccessor accessor = ...;
    ///           CurrencyMeterClassification currency = new CurrencyMeterClassification();
    ///           for (int i = 0; i &lt; YourMeterNames.Length, ++i)
    ///           {
    ///             IMeter meter = new AtomicMeter(YourMeterName[i], accessor, i, currency, this);
    ///             AddMeter(meter);
    ///           }
    ///        }
    ///      }
    ///    </code>
    ///     The addin configuration describes YourMetersProvider and one example composite
    ///     meter named "CompositeMeter1".
    ///     <code>
    ///      &lt;Addin id="YourMetersProvider" namespace="Client12Addins" version="1.0"&gt;
    ///        &lt;Runtime&gt;
    ///          &lt;Import assembly="YourMetersProvider.dll"/&gt;
    ///        &lt;/Runtime&gt;
    ///        &lt;Dependencies&gt;
    ///          &lt;Addin id="MeterManager" version="1.0" /&gt;
    ///        &lt;/Dependencies&gt;
    ///        &lt;Extension path = "/Application/Metering/Providers"&gt;
    ///          &lt;MeterProvider type="YourMetersProvider" /&gt;
    ///        &lt;/Extension&gt;
    ///        &lt;Extension path = "/Application/Metering/CompositeMeters"&gt;
    ///          &lt;CompositeMeter name="CompositeMeter1" classification="Currency" expression="Meter1 + Meter2"/&gt;
    ///        &lt;/Extension&gt;
    ///      &lt;/Extension&gt;
    ///    </code>
    /// </example>
    public interface IMeterProvider
    {
        /// <summary>
        ///     Gets a list of meters provided by this provider
        /// </summary>
        /// <returns>A collection interface of the meter names</returns>
        ICollection<string> MeterNames { get; }

        /// <summary>
        ///     Gets the name of the meter provider
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Get a meter by name
        /// </summary>
        /// <param name="meterName">The name of the meter being requested</param>
        /// <returns>The meter requested</returns>
        IMeter GetMeter(string meterName);

        /// <summary>
        ///     Used to clear the period meters
        /// </summary>
        void ClearPeriodMeters();

        /// <summary>
        ///     Used by a meter to register its private clear method as a delegate
        /// </summary>
        /// <param name="del">The delegate to use to clear the meter</param>
        void RegisterMeterClearDelegate(ClearPeriodMeter del);

        /// <summary>
        /// The date of when the periodic dates were cleared from this provider.
        /// Used when the meters from a provider are cleared individually as opposed to
        /// a global reset and the date of that individual clear is required.
        /// </summary>
        DateTime LastPeriodClear { get; }
    }
}