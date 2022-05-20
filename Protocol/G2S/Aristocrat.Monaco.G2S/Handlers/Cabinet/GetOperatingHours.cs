namespace Aristocrat.Monaco.G2S.Handlers.Cabinet
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Operations;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Kernel;

    /// <summary>
    ///     An implementation of <see cref="ICommandHandler{TClass,TCommand}" />
    /// </summary>
    public class GetOperatingHours : ICommandHandler<cabinet, getOperatingHours>
    {
        private readonly IG2SEgm _egm;
        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetOperatingHours" /> class using an egm.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="properties">An <see cref="IPropertiesManager" /> instance.</param>
        public GetOperatingHours(IG2SEgm egm, IPropertiesManager properties)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<cabinet, getOperatingHours> command)
        {
            return await Sanction.OwnerAndGuests<ICabinetDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<cabinet, getOperatingHours> command)
        {
            var response = command.GenerateResponse<operatingHoursList>();

            response.Command.operatingHours =
                _properties.GetValues<OperatingHours>(ApplicationConstants.OperatingHours)
                    .Where(h => IsMatch(h.Day, command.Command.weekday))
                    .ToOperatingHours();

            await Task.CompletedTask;
        }

        private static bool IsMatch(DayOfWeek day, t_weekday protocolDay)
        {
            return protocolDay == t_weekday.GTK_all || (int)protocolDay == (int)day;
        }
    }
}