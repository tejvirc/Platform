namespace Aristocrat.Monaco.G2S.Descriptors
{
    using System;
    using Aristocrat.G2S.Client.Devices;
    using Hardware.Contracts.IdReader;

    public class IdReaderDescriptor : DeviceDescriptorBase, IDeviceDescriptor<IIdReaderDevice>
    {
        private readonly IIdReaderProvider _provider;

        public IdReaderDescriptor(IIdReaderProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public Descriptor GetDescriptor(IIdReaderDevice device)
        {
            var idReader = _provider[device.Id];

            return idReader == null ? null : base.GetDescriptor(idReader);
        }
    }
}