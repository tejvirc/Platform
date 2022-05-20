namespace Aristocrat.Monaco.G2S.Handlers.NoteAcceptor
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Hardware.Contracts;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.NoteAcceptor;

    /// <summary>
    ///     An implementation of <see cref="ICommandBuilder{INoteAcceptorDevice,noteAcceptorStatus}" />
    /// </summary>
    public class NoteAcceptorStatusCommandBuilder : ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>
    {
        private readonly IDeviceRegistryService _deviceRegistry;
        private readonly IDoorService _doorService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorStatusCommandBuilder" /> class.
        /// </summary>
        /// <param name="deviceRegistry">An <see cref="IDeviceRegistryService" /> instance.</param>
        /// <param name="doorService">An <see cref="IDoorService" /> instance.</param>
        public NoteAcceptorStatusCommandBuilder(
            IDeviceRegistryService deviceRegistry,
            IDoorService doorService)
        {
            _deviceRegistry = deviceRegistry ?? throw new ArgumentNullException(nameof(deviceRegistry));
            _doorService = doorService ?? throw new ArgumentNullException(nameof(doorService));
        }

        /// <inheritdoc />
        public async Task Build(INoteAcceptorDevice device, noteAcceptorStatus command)
        {
            var noteAcceptor = _deviceRegistry.GetDevice<INoteAcceptor>();

            command.configurationId = device.ConfigurationId;

            var doorOpen = _doorService.GetDoorOpen((int)DoorLogicalId.CashBox);
            command.doorOpen = doorOpen;

            // This uses the cash door state because may have not disabled the device yet,
            // but according to the G2S 3.0 spec for 13.19.20 G2S_NAE112 Stacker Door Opened
            // When the noteAcceptorStatus.doorOpen = "true" then this must be noteAcceptorStatus.egmEnabled = "false"
            command.egmEnabled = device.Enabled && !doorOpen;

            command.hostEnabled = device.HostEnabled;

            command.disconnected = !noteAcceptor?.Connected ?? false;

            command.stackerRemoved = GetStackerState(NoteAcceptorStackerState.Removed);
            command.stackerFull = GetStackerState(NoteAcceptorStackerState.Full);
            command.stackerJam = GetStackerState(NoteAcceptorStackerState.Jammed);
            command.stackerFault = GetStackerState(NoteAcceptorStackerState.Fault);
            
            command.acceptorFault =
                noteAcceptor != null && noteAcceptor.Faults != NoteAcceptorFaultTypes.None;

            command.acceptorJam = GetFaultFlag(NoteAcceptorFaultTypes.NoteJammed);
            command.firmwareFault = GetFaultFlag(NoteAcceptorFaultTypes.FirmwareFault);
            command.mechanicalFault = GetFaultFlag(NoteAcceptorFaultTypes.MechanicalFault);
            command.opticalFault = GetFaultFlag(NoteAcceptorFaultTypes.OpticalFault);
            command.componentFault = GetFaultFlag(NoteAcceptorFaultTypes.ComponentFault);
            command.nvMemoryFault = GetFaultFlag(NoteAcceptorFaultTypes.NvmFault);

            command.configDateTime = device.ConfigDateTime;
            command.configComplete = device.ConfigComplete;

            await Task.CompletedTask;

            bool GetStackerState(NoteAcceptorStackerState state)
            {
                return noteAcceptor != null && noteAcceptor.StackerState == state;
            }

            bool GetFaultFlag(NoteAcceptorFaultTypes fault)
            {
                return noteAcceptor != null &&
                       (noteAcceptor.Faults & fault) == fault;
            }
        }
    }
}
