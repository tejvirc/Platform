namespace Aristocrat.Monaco.G2S.Handlers.NoteAcceptor
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Hardware.Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Kernel.Contracts;

    /// <summary>
    ///     Implementation of 'getNoteAcceptorProfile' command of 'NoteAcceptor' G2S class.
    /// </summary>
    public class GetNoteAcceptorProfile : ICommandHandler<noteAcceptor, getNoteAcceptorProfile>
    {
        private readonly IDeviceRegistryService _deviceRegistry;
        private readonly IG2SEgm _egm;
        private readonly IPropertiesManager _properties;
        private readonly ITransactionHistory _transactionHistory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetNoteAcceptorProfile" /> class.
        /// </summary>
        /// <param name="egm">The G2S egm.</param>
        /// <param name="deviceRegistry">An <see cref="IDeviceRegistryService" /> instance.</param>
        /// <param name="transactionHistory">An <see cref="ITransactionHistory" /> instance.</param>
        /// <param name="properties">An <see cref="IPropertiesManager" /> instance.</param>
        public GetNoteAcceptorProfile(
            IG2SEgm egm,
            IDeviceRegistryService deviceRegistry,
            ITransactionHistory transactionHistory,
            IPropertiesManager properties)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _deviceRegistry = deviceRegistry ?? throw new ArgumentNullException(nameof(deviceRegistry));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<noteAcceptor, getNoteAcceptorProfile> command)
        {
            return await Sanction.OwnerAndGuests<INoteAcceptorDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<noteAcceptor, getNoteAcceptorProfile> command)
        {
            var device = _egm.GetDevice<INoteAcceptorDevice>(command.IClass.deviceId);
            var response = command.GenerateResponse<noteAcceptorProfile>();

            response.Command.configurationId = device.ConfigurationId;
            response.Command.restartStatus = device.RestartStatus;
            response.Command.useDefaultConfig = device.UseDefaultConfig;
            response.Command.requiredForPlay = device.RequiredForPlay;
            response.Command.minLogEntries = _transactionHistory.GetMaxTransactions<BillTransaction>();

            var noteAcceptor = _deviceRegistry.GetDevice<INoteAcceptor>();
            if (noteAcceptor != null)
            {
                response.Command.voucherEnabled = _properties.GetValue(PropertyKey.VoucherIn, false);
                response.Command.noteEnabled = noteAcceptor.Denominations.Any();

                var currencyId = _properties.GetValue(ApplicationConstants.CurrencyId, ApplicationConstants.DefaultCurrencyId);

                response.Command.noteAcceptorData = noteAcceptor.GetNoteAcceptorData(currencyId).ToArray();
            }
            else
            {
                response.Command.noteEnabled = false;
                response.Command.voucherEnabled = false;
                response.Command.noteAcceptorData = new noteAcceptorData[0];
            }

            response.Command.configDateTime = device.ConfigDateTime;
            response.Command.configComplete = device.ConfigComplete;
            response.Command.promoSupported = t_g2sBoolean.G2S_false;

            await Task.CompletedTask;
        }
    }
}
