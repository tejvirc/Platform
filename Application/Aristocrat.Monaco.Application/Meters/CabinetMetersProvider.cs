namespace Aristocrat.Monaco.Application.Meters
{
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Metering;
    using Hardware.Contracts.Persistence;
    using Kernel;

    /// <summary>
    ///     Provide meters for cabinet level events (like door opens)
    /// </summary>
    public class CabinetMetersProvider : BaseMeterProvider
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;

        private readonly List<(string name, MeterClassification classification)> _meters = new List<(string, MeterClassification)>
        {
            (ApplicationMeters.BellyDoorOpenCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.BellyDoorOpenPowerOffCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.CashDoorOpenCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.CashDoorOpenPowerOffCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.LogicDoorOpenCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.LogicDoorOpenPowerOffCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.MainDoorOpenCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.MainDoorOpenPowerOffCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.PowerResetCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.SecondaryCashDoorOpenCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.SecondaryCashDoorOpenPowerOffCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.TopBoxDoorOpenCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.TopBoxDoorOpenPowerOffCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.UniversalInterfaceBoxDoorOpenCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.UniversalInterfaceBoxDoorOpenPowerOffCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.DropDoorOpenCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.DropDoorOpenPowerOffCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.AdministratorAccessCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.TechnicianAccessCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.BatteryLowCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.MemoryErrorCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.MechanicalMeterDisconnectCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.MechanicalMeterErrorCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.MechanicalMeterDoorOpenCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.MechanicalMeterDoorOpenPowerOffCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.PrinterDisconnectCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.PrinterErrorCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.PlayerButtonErrorCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.SmartCardErrorCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.TopTouchScreenDisconnectCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.MainTouchScreenDisconnectCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.VbdTouchScreenDisconnectCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.TopperVideoDisplayDisconnectCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.TopVideoDisplayDisconnectCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.MainVideoDisplayDisconnectCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.VbdVideoDisplayDisconnectCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.MainOpticDoorOpenCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.MainOpticDoorOpenPowerOffCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.TopBoxOpticDoorOpenCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.TopBoxOpticDoorOpenPowerOffCount, new OccurrenceMeterClassification()),
            (ApplicationMeters.ProgressiveDisconnectCount, new OccurrenceMeterClassification())
        };

        /// <summary>
        ///     Initializes a new instance of the <see cref="CabinetMetersProvider" /> class.
        /// </summary>
        public CabinetMetersProvider()
            : base(typeof(CabinetMetersProvider).ToString())
        {
            var persistentStorage = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();

            var blockName = GetType().ToString();

            var accessor = persistentStorage.BlockExists(blockName)
                ? persistentStorage.GetBlock(blockName)
                : persistentStorage.CreateBlock(Level, blockName, 1);

            AddMeters(accessor);
            AddCompositeMeters();
        }

        private void AddMeters(IPersistentStorageAccessor accessor)
        {
            foreach (var (name, classification) in _meters)
            {
                AddMeter(new AtomicMeter(name, accessor, classification, this));
            }
        }

        private void AddCompositeMeters()
        {
            AddMeter(
                new CompositeMeter(
                    ApplicationMeters.BellyDoorOpenTotalCount,
                    (timeFrame) => GetMeter(ApplicationMeters.BellyDoorOpenCount).GetValue(timeFrame) +
                                   GetMeter(ApplicationMeters.BellyDoorOpenPowerOffCount).GetValue(timeFrame),
                    new List<string>
                    {
                        ApplicationMeters.BellyDoorOpenCount,
                        ApplicationMeters.BellyDoorOpenPowerOffCount
                    },
                    new OccurrenceMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    ApplicationMeters.CashDoorOpenTotalCount,
                    (timeFrame) => GetMeter(ApplicationMeters.CashDoorOpenCount).GetValue(timeFrame) +
                                   GetMeter(ApplicationMeters.CashDoorOpenPowerOffCount).GetValue(timeFrame),
                    new List<string>
                    {
                        ApplicationMeters.CashDoorOpenCount,
                        ApplicationMeters.CashDoorOpenPowerOffCount
                    },
                    new OccurrenceMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    ApplicationMeters.LogicDoorOpenTotalCount,
                    (timeFrame) => GetMeter(ApplicationMeters.LogicDoorOpenCount).GetValue(timeFrame) +
                                   GetMeter(ApplicationMeters.LogicDoorOpenPowerOffCount).GetValue(timeFrame),
                    new List<string>
                    {
                        ApplicationMeters.LogicDoorOpenCount,
                        ApplicationMeters.LogicDoorOpenPowerOffCount
                    },
                    new OccurrenceMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    ApplicationMeters.MainDoorOpenTotalCount,
                    (timeFrame) => GetMeter(ApplicationMeters.MainDoorOpenCount).GetValue(timeFrame) +
                                   GetMeter(ApplicationMeters.MainDoorOpenPowerOffCount).GetValue(timeFrame),
                    new List<string>
                    {
                        ApplicationMeters.MainDoorOpenCount,
                        ApplicationMeters.MainDoorOpenPowerOffCount
                    },
                    new OccurrenceMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    ApplicationMeters.SecondaryCashDoorOpenCount,
                    (timeFrame) => GetMeter(ApplicationMeters.SecondaryCashDoorOpenCount).GetValue(timeFrame) +
                                   GetMeter(ApplicationMeters.SecondaryCashDoorOpenPowerOffCount).GetValue(timeFrame),
                    new List<string>
                    {
                        ApplicationMeters.SecondaryCashDoorOpenCount,
                        ApplicationMeters.SecondaryCashDoorOpenPowerOffCount
                    },
                    new OccurrenceMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    ApplicationMeters.TopBoxDoorOpenTotalCount,
                    (timeFrame) => GetMeter(ApplicationMeters.TopBoxDoorOpenCount).GetValue(timeFrame) +
                                   GetMeter(ApplicationMeters.TopBoxDoorOpenPowerOffCount).GetValue(timeFrame),
                    new List<string>
                    {
                        ApplicationMeters.TopBoxDoorOpenCount,
                        ApplicationMeters.TopBoxDoorOpenPowerOffCount
                    },
                    new OccurrenceMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    ApplicationMeters.UniversalInterfaceBoxDoorOpenTotalCount,
                    (timeFrame) => GetMeter(ApplicationMeters.UniversalInterfaceBoxDoorOpenCount).GetValue(timeFrame) +
                                   GetMeter(ApplicationMeters.UniversalInterfaceBoxDoorOpenPowerOffCount).GetValue(timeFrame),
                    new List<string>
                    {
                        ApplicationMeters.UniversalInterfaceBoxDoorOpenCount,
                        ApplicationMeters.UniversalInterfaceBoxDoorOpenPowerOffCount
                    },
                    new OccurrenceMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    ApplicationMeters.DropDoorOpenTotalCount,
                    (timeFrame) => GetMeter(ApplicationMeters.DropDoorOpenCount).GetValue(timeFrame) +
                                   GetMeter(ApplicationMeters.DropDoorOpenPowerOffCount).GetValue(timeFrame),
                    new List<string>
                    {
                        ApplicationMeters.DropDoorOpenCount,
                        ApplicationMeters.DropDoorOpenPowerOffCount
                    },
                    new OccurrenceMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    ApplicationMeters.MechanicalMeterDoorOpenTotalCount,
                    (timeFrame) => GetMeter(ApplicationMeters.MechanicalMeterDoorOpenCount).GetValue(timeFrame) +
                                   GetMeter(ApplicationMeters.MechanicalMeterDoorOpenPowerOffCount).GetValue(timeFrame),
                    new List<string>
                    {
                        ApplicationMeters.MechanicalMeterDoorOpenCount,
                        ApplicationMeters.MechanicalMeterDoorOpenPowerOffCount
                    },
                    new OccurrenceMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    ApplicationMeters.MainOpticDoorOpenTotalCount,
                    (timeFrame) => GetMeter(ApplicationMeters.MainOpticDoorOpenCount).GetValue(timeFrame) +
                                   GetMeter(ApplicationMeters.MainOpticDoorOpenPowerOffCount).GetValue(timeFrame),
                    new List<string>
                    {
                        ApplicationMeters.MainOpticDoorOpenCount,
                        ApplicationMeters.MainOpticDoorOpenPowerOffCount
                    },
                    new OccurrenceMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    ApplicationMeters.TopBoxOpticDoorOpenTotalCount,
                    (timeFrame) => GetMeter(ApplicationMeters.TopBoxOpticDoorOpenCount).GetValue(timeFrame) +
                                   GetMeter(ApplicationMeters.TopBoxOpticDoorOpenPowerOffCount).GetValue(timeFrame),
                    new List<string>
                    {
                        ApplicationMeters.TopBoxOpticDoorOpenCount,
                        ApplicationMeters.TopBoxOpticDoorOpenPowerOffCount
                    },
                    new OccurrenceMeterClassification()));

        }
    }
}