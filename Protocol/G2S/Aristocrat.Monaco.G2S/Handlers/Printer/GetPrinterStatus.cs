namespace Aristocrat.Monaco.G2S.Handlers.Printer
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Implementation of <see cref="getPrinterStatus" /> for the <see cref="printer" /> class
    /// </summary>
    public class GetPrinterStatus : ICommandHandler<printer, getPrinterStatus>
    {
        private readonly ICommandBuilder<IPrinterDevice, printerStatus> _command;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetPrinterStatus" /> class.
        /// </summary>
        /// <param name="egm">The G2S egm.</param>
        /// <param name="command">An <see cref="ICommandBuilder{IPrinterDevice, printerStatus}" /> instance</param>
        public GetPrinterStatus(IG2SEgm egm, ICommandBuilder<IPrinterDevice, printerStatus> command)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _command = command ?? throw new ArgumentNullException(nameof(command));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<printer, getPrinterStatus> command)
        {
            return await Sanction.OwnerAndGuests<IPrinterDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<printer, getPrinterStatus> command)
        {
            var device = _egm.GetDevice<IPrinterDevice>(command.IClass.deviceId);
            var response = command.GenerateResponse<printerStatus>();

            await _command.Build(device, response.Command);
        }
    }
}