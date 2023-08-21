namespace Aristocrat.Monaco.G2S.Handlers.CommConfig
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.CommConfig;
    using Constants = G2S.Constants;

    /// <summary>
    ///     The builder for the 'commHostList' command.
    /// </summary>
    public class CommHostListCommandBuilder : ICommHostListCommandBuilder
    {
        private const int All = -1;

        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommHostListCommandBuilder" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        public CommHostListCommandBuilder(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public Task Build(
            ICommConfigDevice device,
            commHostList command,
            CommHostListCommandBuilderParameters parameters)
        {
            command.multicastSupported = t_g2sBoolean.G2S_false;

            var commHostList = new List<commHostItem>();

            foreach (var host in _egm.Hosts.Where(
                h => parameters.HostIndexes.Any(i => i == All) || parameters.HostIndexes.Contains(h.Index)))
            {
                var commsDevice = _egm.GetDevice<ICommunicationsDevice>(host.Id);

                var commHost = new commHostItem
                {
                    hostIndex = host.Index,
                    hostId = host.Id,
                    hostLocation =
                        host.Address == null ? Aristocrat.G2S.Client.Constants.DefaultUrl : host.Address.ToString(),
                    hostRegistered = host.Registered,
                    useDefaultConfig = commsDevice?.UseDefaultConfig ?? false,
                    requiredForPlay = commsDevice?.RequiredForPlay ?? false,
                    timeToLive =
                        commsDevice?.TimeToLive ??
                        (int)Aristocrat.G2S.Client.Constants.DefaultTimeout.TotalMilliseconds,
                    noResponseTimer = (int)(commsDevice?.NoResponseTimer.TotalMilliseconds ??
                                            Aristocrat.G2S.Client.Constants.NoResponseTimer.TotalMilliseconds),
                    allowMulticast = commsDevice?.AllowMulticast ?? false,
                    canModLocal = true,
                    displayCommFault = commsDevice?.DisplayFault ?? false,
                    canModRemote = true
                };

                if (parameters.IncludeOwnerDevices)
                {
                    commHost.ownedDevice1 =
                        _egm.Devices.Where(d => d.IsOwner(host.Id))
                            .Select(
                                owned => new c_commHostConfigItem.ownedDevice
                                {
                                    deviceClass = owned.PrefixedDeviceClass(),
                                    deviceId = owned.Id,
                                    deviceActive = owned.Active
                                }).ToArray();
                }

                if (parameters.IncludeConfigDevices)
                {
                    commHost.configDevice1 =
                        _egm.Devices.Where(d => d.IsConfigurator(host.Id))
                            .Select(
                                config => new c_commHostConfigItem.configDevice
                                {
                                    deviceClass = config.PrefixedDeviceClass(),
                                    deviceId = config.Id,
                                    deviceActive = config.Active
                                }).ToArray();
                }

                if (parameters.IncludeGuestDevices)
                {
                    commHost.guestDevice1 =
                        _egm.Devices.Where(d => d.IsGuest(host.Id))
                            .Select(
                                guest => new c_commHostConfigItem.guestDevice
                                {
                                    deviceClass = guest.PrefixedDeviceClass(),
                                    deviceId = guest.Id,
                                    deviceActive = guest.Active
                                }).ToArray();
                }

                commHostList.Add(commHost);
            }

            // Fill in the gaps
            for (var index = 0; index < Constants.MaxHosts; index++)
            {
                if (_egm.Hosts.All(
                    h => h.Index != index &&
                         (parameters.HostIndexes.Any(i => i == All) || parameters.HostIndexes.Contains(h.Index))))
                {
                    commHostList.Add(
                        new commHostItem
                        {
                            hostIndex = index,
                            hostId = 0,
                            hostLocation = Aristocrat.G2S.Client.Constants.DefaultUrl,
                            hostRegistered = false
                        });
                }
            }

            command.commHostItem = commHostList.OrderBy(h => h.hostIndex).ToArray();

            return Task.CompletedTask;
        }
    }
}