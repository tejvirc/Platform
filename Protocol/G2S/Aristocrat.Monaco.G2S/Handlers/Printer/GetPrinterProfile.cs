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
    using Services;

    /// <summary>
    ///     Implementation of <see cref="getPrinterProfile" /> for the <see cref="printer" /> class
    /// </summary>
    public class GetPrinterProfile : ICommandHandler<printer, getPrinterProfile>
    {
        private readonly IDeviceRegistryService _deviceRegistry;
        private readonly IG2SEgm _egm;
        private readonly IPrintLog _printLog;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetPrinterProfile" /> class.
        /// </summary>
        /// <param name="egm">The G2S egm.</param>
        /// <param name="deviceRegistry">An <see cref="IDeviceRegistryService" /> instance.</param>
        /// <param name="printLog">An <see cref="IPrintLog" /> instance.</param>
        public GetPrinterProfile(IG2SEgm egm, IDeviceRegistryService deviceRegistry, IPrintLog printLog)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _deviceRegistry = deviceRegistry ?? throw new ArgumentNullException(nameof(deviceRegistry));
            _printLog = printLog ?? throw new ArgumentNullException(nameof(printLog));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<printer, getPrinterProfile> command)
        {
            return await Sanction.OwnerAndGuests<IPrinterDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<printer, getPrinterProfile> command)
        {
            var device = _egm.GetDevice<IPrinterDevice>(command.IClass.deviceId);
            var response = command.GenerateResponse<printerProfile>();

            response.Command.configurationId = device.ConfigurationId;
            response.Command.restartStatus = device.RestartStatus;
            response.Command.useDefaultConfig = device.UseDefaultConfig;
            response.Command.requiredForPlay = device.RequiredForPlay;
            response.Command.minLogEntries = _printLog.MinimumLogEntries;

            response.Command.configDateTime = device.ConfigDateTime;
            response.Command.configComplete = device.ConfigComplete;

            response.Command.hostInitiated = t_g2sBoolean.G2S_false;
            response.Command.customTemplates = t_g2sBoolean.G2S_false;

            var printer = _deviceRegistry.GetDevice<IPrinter>();

            response.Command.printerRegionProfile =
                printer.Regions
                    .Select(r => new printerRegionProfile { regionIndex = r.Id, regionConfig = r.ToPDL(printer.UseLargeFont) })
                    .ToArray();

            response.Command.printerTemplateProfile =
                printer.Templates
                    .Select(t => new printerTemplateProfile { templateIndex = t.Id, templateConfig = t.ToPDL() })
                    .ToArray();

            await Task.CompletedTask;
        }
    }
}