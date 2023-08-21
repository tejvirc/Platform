namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Common.Events;
    using Data.CommConfig;
    using Handlers.CommConfig;

    /// <summary>
    ///     Handles the <see cref="CommHostListChangedEvent" />.
    /// </summary>
    public class CommHostListChangedConsumer : Consumes<CommHostListChangedEvent>
    {
        private readonly ICommHostListCommandBuilder _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommHostListChangedConsumer" /> class.
        /// </summary>
        /// <param name="egm">The egm.</param>
        /// <param name="commandBuilder">The command builder.</param>
        public CommHostListChangedConsumer(IG2SEgm egm, ICommHostListCommandBuilder commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public override void Consume(CommHostListChangedEvent theEvent)
        {
            var device = _egm.GetDevice<ICommConfigDevice>();

            var commHostList = new commHostList();

            var commHostListParameters = new CommHostListCommandBuilderParameters
            {
                IncludeOwnerDevices = true,
                IncludeConfigDevices = true,
                IncludeGuestDevices = true,
                HostIndexes = theEvent.HostIndexes
            };

            _commandBuilder.Build(device, commHostList, commHostListParameters);

            device.UpdateHostList(commHostList);
        }
    }
}