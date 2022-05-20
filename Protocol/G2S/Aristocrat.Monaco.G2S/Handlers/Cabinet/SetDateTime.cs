namespace Aristocrat.Monaco.G2S.Handlers.Cabinet
{
    using System;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Defines a new instance of an ICommandHandler.
    /// </summary>
    public class SetDateTime : ICommandHandler<cabinet, setDateTime>
    {
        private readonly IG2SEgm _egm;
        private readonly ITime _time;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetDateTime" /> class using an egm an instance of ITime.
        /// </summary>
        /// <param name="egm">An instance of an IG2SEgm.</param>
        /// <param name="time">An instance of an ITime service.</param>
        public SetDateTime(IG2SEgm egm, ITime time)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _time = time ?? throw new ArgumentNullException(nameof(time));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<cabinet, setDateTime> command)
        {
            return await Sanction.OnlyOwner<ICabinetDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<cabinet, setDateTime> command)
        {
            _time.Update(new DateTimeOffset(command.Command.cabinetDateTime));

            var response = command.GenerateResponse<cabinetDateTime>();

            response.Command.cabinetDateTime = DateTime.UtcNow;

            await Task.CompletedTask;
        }
    }
}