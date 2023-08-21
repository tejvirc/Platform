namespace Aristocrat.Monaco.Application.Contracts
{
    using Kernel;
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     An interface by which the meters contained in a IMeterProvider component can be added, and all meters
    ///     can be accessed for interaction.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         All meters are managed centrally. The meters are distributed into the different meter providers
    ///         implementing <see cref="IMeterProvider" />. The meters are added to the IMeterManager service by calling
    ///         the <c>AddProvider()</c> method of this interface.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     // Below is an illustration for adding a meter provider into the IMeterManager service.
    ///     <code>
    ///      // Create a meter provider
    ///      public class YourMeterProvider : BaseMeterProvider
    ///      {
    ///        private const PersistenceLevel Level = PersistenceLevel.Critical;
    ///        private static readonly string[] YourMeterNames = new[]
    ///        {
    ///          "Meter1",
    ///          "Meter2"
    ///        }
    ///        public YourMetersProvider : base(typeof(YourMetersProvider).ToString()
    ///        {
    ///          // Use the steps described in <c>IMeterProvider</c> to construct the meters.
    ///          // ...
    ///        }
    ///      }
    ///      // In the component that instantiates the meters provider, you can add
    ///      // the provided meters to the IMeterManager service.
    ///      public class OneComponent
    ///      {
    ///        // ...
    ///        private void AddMeters()
    ///        {
    ///          // Get the IMeterManager service
    ///          IMeterManager meterManager = ...
    ///          meterManager.AddProvider(new YourMeterProvider());
    ///        }
    ///      }
    ///    </code>
    /// </example>
    public interface IMeterManager : IService
    {
        /// <summary>
        ///     Gets a list of all meter names attached to the manager.
        /// </summary>
        IEnumerable<string> Meters { get; }

        /// <summary>
        ///     Gets the last time all period meters were cleared.
        /// </summary>
        DateTime LastPeriodClear { get; }

        /// <summary>
        ///     Gets the last time all Master meters were cleared.
        /// </summary>
        DateTime LastMasterClear { get; }

        /// <summary>
        ///     Adds the provider to the list of providers and adds its
        ///     meters to the meter->provider map.
        /// </summary>
        /// <param name="provider">The provider to add</param>
        /// <exception cref="MeterException">
        ///     Thrown if the provider provides meters that are already provided by the MeterManager.
        /// </exception>
        void AddProvider(IMeterProvider provider);

        /// <summary>
        ///     Update the provider allowing for dynamic updates of the existing meters
        /// </summary>
        /// <param name="provider">The provider to add</param>
        /// <exception cref="MeterException">
        ///     Thrown if the provider provides meters that are already provided by the MeterManager.
        /// </exception>
        void InvalidateProvider(IMeterProvider provider);

        /// <summary>
        ///     Gets a list of all meter names provided by the given provider.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns>An IEnumerable of meters provided</returns>
        IEnumerable<string> MetersProvided(string provider);

        /// <summary>
        ///     Returns whether or not a meter with the given name is provided.
        /// </summary>
        /// <param name="meterName">The name of the meter in question</param>
        /// <returns>A value indicating whether or not a meter with the given name is provided</returns>
        bool IsMeterProvided(string meterName);

        /// <summary>
        ///     Gets the meter by the name provided
        /// </summary>
        /// <param name="meterName">The name of the requested meter</param>
        /// <returns>The meter that was requested</returns>
        IMeter GetMeter(string meterName);

        /// <summary>
        ///     Clears the period meters of all meters registered with the manager.
        /// </summary>
        void ClearAllPeriodMeters();

        /// <summary>
        ///     Clears the period meters of meters associated with the provider and registered with the manager.
        ///     Must not affect the period meters owned by other providers. See ExemptProviderFromClearAllPeriodMeters.
        /// </summary>
        void ClearPeriodMeters(string providerName);

        /// <summary>
        ///     Exempts a provider from being systematically cleared by ClearAllPeriodMeters();
        /// </summary>
        void ExemptProviderFromClearAllPeriodMeters(string providerName);

        /// <summary>
        ///     Gets the clearance date of the provider if it is being stored individually for that provider, else it will return
        ///     LastPeriodClear.
        /// </summary>
        DateTime GetPeriodMetersClearanceDate(string providerName);

        /// <summary>
        ///     Creates a snapshot of all meters
        /// </summary>
        /// <returns>The Dictionary of meter string name to MeterSnapshot</returns>
        Dictionary<string, MeterSnapshot> CreateSnapshot();

        /// <summary>
        ///     Creates a snapshot of list of given meters
        /// </summary>
        /// <param name="meters">The list of meters for which snapshot needs to be taken</param>
        /// <returns>The Dictionary of meter string name to MeterSnapshot</returns>
        Dictionary<string, MeterSnapshot> CreateSnapshot(IEnumerable<string> meters);

        /// <summary>
        ///     Creates a snapshot of all meters for a given meter value type
        /// </summary>
        /// <returns>The Dictionary of meter string name to value</returns>
        Dictionary<string, long> CreateSnapshot(MeterValueType valueType);

        /// <summary>
        ///     Creates a snapshot of list of given meters for a given meter type
        /// </summary>
        /// <param name="meters">The list of meters for which snapshot needs to be taken</param>
        /// <param name="valueType">The meter value type for which the snapshot needs to be taken</param>
        /// <returns>The Dictionary of meter string name to value</returns>
        Dictionary<string, long> CreateSnapshot(IEnumerable<string> meters, MeterValueType valueType);

        /// <summary>
        ///     Creates a collection of meters that changed based on the previous snapshot
        /// </summary>
        /// <param name="snapshot">The original snapshot used for comparison</param>
        /// <param name="valueType">The meter value type to compare</param>
        /// <returns>A dictionary of changed meters</returns>
        IDictionary<string, long> GetSnapshotDelta(IDictionary<string, MeterSnapshot> snapshot, MeterValueType valueType);

        /// <summary>
        ///     Creates a collection of meters that changed based on the previous snapshot
        /// </summary>
        /// <param name="snapshot">The original snapshot used for comparison</param>
        /// <param name="valueType">The meter value type to compare</param>
        /// <returns>A dictionary of changed meters</returns>
        IDictionary<string, long> GetSnapshotDelta(IDictionary<string, long> snapshot, MeterValueType valueType);
    }
}
