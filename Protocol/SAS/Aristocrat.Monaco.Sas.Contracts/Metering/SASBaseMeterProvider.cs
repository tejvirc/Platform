namespace Aristocrat.Monaco.Sas.Contracts.Metering
{
    using System.Collections.Generic;
    using Application.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;

    /// <summary>
    /// Class that takes care of most of the boilerplate code associated with implementing the
    /// IMeterProvider interface.  Subclasses need only pass meter names into SasBaseMeterProvider's
    /// constructor.
    /// </summary>
    public class SasBaseMeterProvider : BaseMeterProvider
    {
        /// <summary>
        /// Initializes a new instance of the SasBaseMeterProvider class.
        /// </summary>
        /// <param name="name">The name of this provider.</param>
        /// <param name="meterInfoList">
        /// List of MeterInfo objects, specifying information to store about each meter.
        /// </param>
        public SasBaseMeterProvider(string name, IList<MeterInfo> meterInfoList)
            : base(name)
        {
            int numberOfMeters = meterInfoList.Count;
            var storageManager = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
            string storageName = GetType().ToString();
            var storageAccessor = storageManager.BlockExists(storageName)
                ? storageManager.GetBlock(storageName)
                : storageManager.CreateBlock(PersistenceLevel.Critical, storageName, 1);

            for (var i = 0; i < numberOfMeters; i++)
            {
                AddMeter(new AtomicMeter(meterInfoList[i].Name, storageAccessor, meterInfoList[i].MeterClassification, this));
            }
        }
    }
}
