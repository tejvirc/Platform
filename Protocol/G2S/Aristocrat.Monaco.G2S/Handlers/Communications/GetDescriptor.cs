namespace Aristocrat.Monaco.G2S.Handlers.Communications
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.getDescriptor G2S message
    /// </summary>
    public class GetDescriptor : ICommandHandler<communications, getDescriptor>
    {
        private readonly IG2SEgm _egm;
        private readonly IDeviceDescriptorFactory _descriptorFactory;

        public GetDescriptor(IG2SEgm egm, IDeviceDescriptorFactory descriptorFactory)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _descriptorFactory = descriptorFactory ?? throw new ArgumentNullException(nameof(descriptorFactory));
        }

        public async Task<Error> Verify(ClassCommand<communications, getDescriptor> command)
        {
            var device = _egm.GetDevice<ICommunicationsDevice>(command.IClass.deviceId);

            return await Task.FromResult(command.Validate(device, CommandRestrictions.RestrictedToOwnerAndGuests));
        }

        public async Task Handle(ClassCommand<communications, getDescriptor> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var response = command.GenerateResponse<descriptorList>();

                response.Command.descriptor = _egm.Devices
                    .Where(d => Filter(d, command.Command, command.HostId))
                    .Select(d => new
                    {
                        device = d,
                        descriptor = (_descriptorFactory.Create(d) as dynamic)?.GetDescriptor(d as dynamic)
                    })
                    .Select(
                        d => new descriptor
                        {
                            deviceClass = d.device.PrefixedDeviceClass(),
                            deviceId = d.device.Id,
                            deviceActive = d.device.Active,
                            configurationId = d.device.ConfigurationId,
                            hostEnabled = d.device.HostEnabled,
                            egmEnabled = d.device.Enabled,
                            egmLocked = d.device.Locked,
                            hostLocked = d.device.HostLocked,
                            ownerId = d.device.Owner,
                            configId = d.device.Configurator,
                            deviceOwner = d.device.IsOwner(command.HostId),
                            deviceGuest = d.device.IsGuest(command.HostId),
                            deviceConfig = d.device.IsConfigurator(command.HostId),
                            vendorId = d.descriptor?.VendorId,
                            productId = d.descriptor?.ProductId,
                            releaseNum = d.descriptor?.ReleaseNumber,
                            vendorName = d.descriptor?.VendorName,
                            productName = d.descriptor?.ProductName,
                            serialNum = d.descriptor?.SerialNumber,
                            configDateTime = d.device.ConfigDateTime,
                            configComplete = d.device.ConfigComplete
                        }).ToArray();
            }

            await Task.CompletedTask;
        }

        private static bool Filter(IDevice device, getDescriptor command, int hostId)
        {
            if (!device.IsMatching(command.deviceClass))
            {
                return false;
            }

            if (!device.IsMatching(command.deviceId))
            {
                return false;
            }

            var qualified = command.includeOwners && device.IsOwner(hostId) ||
                            command.includeGuests && device.IsGuest(hostId) ||
                            command.includeConfigs && device.IsConfigurator(hostId) ||
                            command.includeOthers && !device.IsMember(hostId);

            return qualified && (command.includeActive && device.Active || command.includeInactive && !device.Active);
        }
    }
}