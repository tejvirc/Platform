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
    ///     Defines a new instance of an ICommandHandler.
    /// </summary>
    public class SetOperatingHours : ICommandHandler<cabinet, setOperatingHours>
    {
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetOperatingHours" /> class using an egm.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="properties">An <see cref="IPropertiesManager" /> instance.</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance.</param>
        public SetOperatingHours(IG2SEgm egm, IPropertiesManager properties, IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<cabinet, setOperatingHours> command)
        {
            var error = await Sanction.OnlyOwner<ICabinetDevice>(_egm, command);
            if (error != null && error.IsError)
            {
                return error;
            }

            // This will expand the list
            var operatingHours = command.Command.operatingHours
                .ToOperatingHours()
                .ToList();

            // The operating hours cannot contain conflicting states for the same day and time
            if (operatingHours.Any(
                o => operatingHours.Any(
                    o2 => o2.Day == o.Day && o2.Time == o.Time && o2.Enabled != o.Enabled)))
            {
                return new Error(ErrorCode.GTK_CBX002);
            }

            // Time cannot be outside of the valid range of a clock day in milliseconds (86399999)
            return operatingHours.Any(o => o.Time >= TimeSpan.FromDays(1).TotalMilliseconds)
                ? new Error(ErrorCode.GTK_CBX001)
                : null;
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<cabinet, setOperatingHours> command)
        {
            var response = command.GenerateResponse<operatingHoursList>();

            var updatedOperatingHours = command.Command.operatingHours.ToOperatingHours();
            _properties.SetProperty(ApplicationConstants.OperatingHours, updatedOperatingHours);

            var operatingHours = _properties.GetValues<OperatingHours>(ApplicationConstants.OperatingHours);
            response.Command.operatingHours = operatingHours.ToOperatingHours();

            var device = _egm.GetDevice<ICabinetDevice>(command.IClass.deviceId);
            _eventLift.Report(device, EventCode.GTK_CBE001);

            await Task.CompletedTask;
        }
    }
}