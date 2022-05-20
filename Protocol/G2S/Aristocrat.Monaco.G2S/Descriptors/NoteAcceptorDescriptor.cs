namespace Aristocrat.Monaco.G2S.Descriptors
{
    using System;
    using Aristocrat.G2S.Client.Devices;
    using Hardware.Contracts;
    using Hardware.Contracts.NoteAcceptor;

    public class NoteAcceptorDescriptor : DeviceDescriptorBase, IDeviceDescriptor<INoteAcceptorDevice>
    {
        private readonly IDeviceRegistryService _deviceRegistry;

        public NoteAcceptorDescriptor(IDeviceRegistryService deviceRegistry)
        {
            _deviceRegistry = deviceRegistry ?? throw new ArgumentNullException(nameof(deviceRegistry));
        }

        public Descriptor GetDescriptor(INoteAcceptorDevice device)
        {
            var noteAcceptor = _deviceRegistry.GetDevice<INoteAcceptor>();

            return noteAcceptor == null ? null : base.GetDescriptor(noteAcceptor);
        }
    }
}