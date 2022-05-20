namespace Aristocrat.Monaco.G2S.Handlers.Printer
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Hardware.Contracts;
    using Hardware.Contracts.Printer;

    /// <summary>
    ///     Implementation of <see cref="getPrinterTemplates" /> for the <see cref="printer" /> class
    /// </summary>
    public class GetPrinterTemplates : ICommandHandler<printer, getPrinterTemplates>
    {
        private readonly IDeviceRegistryService _deviceRegistry;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetPrinterTemplates" /> class.
        /// </summary>
        /// <param name="egm">The G2S egm.</param>
        /// <param name="deviceRegistry">An <see cref="IDeviceRegistryService" /> instance.</param>
        public GetPrinterTemplates(IG2SEgm egm, IDeviceRegistryService deviceRegistry)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _deviceRegistry = deviceRegistry ?? throw new ArgumentNullException(nameof(deviceRegistry));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<printer, getPrinterTemplates> command)
        {
            return await Sanction.OwnerAndGuests<IPrinterDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<printer, getPrinterTemplates> command)
        {
            var response = command.GenerateResponse<printerTemplateList>();

            var printer = _deviceRegistry.GetDevice<IPrinter>();

            response.Command.printerTemplate =
                printer.Templates
                    .Select(t => new printerTemplate { templateIndex = t.Id })
                    .ToArray();

            await Task.CompletedTask;
        }
    }
}