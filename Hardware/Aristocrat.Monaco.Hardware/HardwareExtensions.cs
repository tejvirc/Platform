namespace Aristocrat.Monaco.Hardware
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Contracts.Communicator;
    using Contracts.NoteAcceptor;
    using Contracts.SharedDevice;
    using Gds.NoteAcceptor;
    using NoteAcceptor;
    using Serial.NoteAcceptor;
    using Services;
    using SimpleInjector;
    using Usb;
#if !(RETAIL)
    using Fake;
    using Virtual;
#endif

    /// <summary>
    ///     Defines the HardwareExtensions class
    /// </summary>
    public static class HardwareExtensions
    {
        /// <summary>
        ///     Finds classes with <see cref="HardwareDeviceAttribute" /> and registers with hardware device factories
        /// </summary>
        /// <param name="container">SimpleInjector container</param>
        /// <param name="assemblies">List of assemblies with hardware device classes</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Container ScanHardwareDeviceTypes(this Container container, IEnumerable<Assembly> assemblies)
        {
            var deviceFactoryNoteAcceptor =
                new DeviceFactory<INoteAcceptor, INoteAcceptorImplementation, NoteAcceptorAdapter>(container);

            var data = assemblies.SelectMany(
                x => x.GetTypes()
                    .Select(
                        t => (Type: t,
                            Attributes: t.GetCustomAttributes(typeof(HardwareDeviceAttribute))
                                .OfType<HardwareDeviceAttribute>())).Where(t => t.Attributes.Any()));
            foreach (var (type, deviceAttributes) in data.SelectMany(x => x.Attributes.Select(a => (x.Type, a))))
            {
                switch (deviceAttributes.DeviceType)
                {
                    case DeviceType.IdReader:
                        // not yet implemented
                        break;
                    case DeviceType.NoteAcceptor:
                        if (typeof(ICommunicator).IsAssignableFrom(type))
                        {
                            deviceFactoryNoteAcceptor.RegisterCommunicator(type, deviceAttributes.Protocol);
                        }
                        else if (typeof(INoteAcceptorImplementation).IsAssignableFrom(type))
                        {
                            deviceFactoryNoteAcceptor.RegisterImplementation(type, deviceAttributes.Protocol);
                        }

                        break;
                    case DeviceType.Printer:
                        // not yet implemented
                        break;
                    case DeviceType.ReelController:
                        // not yet implemented
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            container.RegisterInstance<IDeviceFactory<INoteAcceptor>>(deviceFactoryNoteAcceptor);
            return container;
        }

        /// <summary>
        ///     Searches assemblies for classes with <see cref="HardwareDeviceAttribute" /> and registers in container
        /// </summary>
        /// <param name="container">SimpleInjector container</param>
        /// <param name="assemblies">Additional Assemblies to scan</param>
        public static Container AddScannableHardwareDeviceTypes(this Container container, params Assembly[] assemblies)
        {
            var hardwareAssemblies = new List<Assembly>(assemblies);

            // Load all the Aristocrat.Monaco.Hardware.* assemblies using class types
            hardwareAssemblies.Add(typeof(NoteAcceptorGds).Assembly);
            hardwareAssemblies.Add(typeof(SerialNoteAcceptor).Assembly);
            hardwareAssemblies.Add(typeof(UsbCommunicator).Assembly);
#if !(RETAIL)
            hardwareAssemblies.Add(typeof(FakeNoteAcceptorAdapter).Assembly);
            hardwareAssemblies.Add(typeof(VirtualCommunicator).Assembly);
#endif

            return container.ScanHardwareDeviceTypes(hardwareAssemblies);
        }
    }
}