namespace Stubs
{
    using System;
    using System.Reflection;
    using Aristocrat.Monaco.Hardware.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Persistence;
    using Aristocrat.Monaco.Hardware.Contracts.SharedDevice;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Kernel.Contracts;
    using log4net;

    public class SimpleRunnable : BaseRunnable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected override void OnInitialize()
        {
            Logger.Info("Initialized");
        }

        protected override void OnRun()
        {
            Logger.Info("Run started");

            LoadHardware();

            Console.WriteLine("Press <q> to quit");
            while (true)
            {
                // get input
                if (Console.ReadKey().KeyChar == 'q')
                {
                    break;
                }
            }

            Console.WriteLine();
            Logger.Info("Stopped");
        }

        protected override void OnStop()
        {
            Logger.Info("Stopping");
        }

        private void LoadHardware()
        {
            // Hardware config would normally be determined by the config wizard on first boot and
            // passed directly to the HardwareConfiguration service, which would persist it.
            // On subsequent boots, HardwareConfiguration service listens for a PropertyChangedEvent
            // for the "" property.  Simulate that here.

            var storage = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();

            var level = PersistenceLevel.Static;
            var name = "Aristocrat.Monaco.Hardware.HardwareConfiguration";
            var size = Enum.GetValues(typeof(DeviceType)).Length;
            var accessor = storage.GetAccessor(level, name, size);

            using (var transaction = accessor.StartTransaction())
            {
                transaction[(int)DeviceType.NoteAcceptor, "DeviceType"] = DeviceType.NoteAcceptor;
                transaction[(int)DeviceType.NoteAcceptor, "Enabled"] = true;
                transaction[(int)DeviceType.NoteAcceptor, "Make"] = "Fake";
                transaction[(int)DeviceType.NoteAcceptor, "Protocol"] = "Fake";
                transaction[(int)DeviceType.NoteAcceptor, "Port"] = "Fake";

                transaction[(int)DeviceType.Printer, "DeviceType"] = DeviceType.Printer;
                transaction[(int)DeviceType.Printer, "Enabled"] = true;
                transaction[(int)DeviceType.Printer, "Make"] = "Fake";
                transaction[(int)DeviceType.Printer, "Protocol"] = "Fake";
                transaction[(int)DeviceType.Printer, "Port"] = "Fake";

                transaction[(int)DeviceType.IdReader, "DeviceType"] = DeviceType.IdReader;
                transaction[(int)DeviceType.IdReader, "Enabled"] = false;
                transaction[(int)DeviceType.IdReader, "Make"] = "UNIFORM USB GDS";
                transaction[(int)DeviceType.IdReader, "Protocol"] = "GDS";
                transaction[(int)DeviceType.IdReader, "Port"] = "USB";

                //transaction[(int)DeviceType.ReelController, "DeviceType"] = DeviceType.ReelController;
                //transaction[(int)DeviceType.ReelController, "Enabled"] = true;
                //transaction[(int)DeviceType.ReelController, "Make"] = "Fake";
                //transaction[(int)DeviceType.ReelController, "Protocol"] = "Fake";
                //transaction[(int)DeviceType.ReelController, "Port"] = "Fake";

                transaction.Commit();
            }

            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            eventBus.Publish(new PropertyChangedEvent() { PropertyName = "Mono.SelectedAddinConfigurationHashCode" });
        }
    }
}
