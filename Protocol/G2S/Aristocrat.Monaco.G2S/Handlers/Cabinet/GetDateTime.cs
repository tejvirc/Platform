namespace Aristocrat.Monaco.G2S.Handlers.Cabinet
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Defines a new instance of an ICommandHandler.
    /// </summary>
    public class GetDateTime : ICommandHandler<cabinet, getDateTime>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetDateTime" /> class using an EGM.
        /// </summary>
        /// <param name="egm">An instance of an IG2SEgm.</param>
        public GetDateTime(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<cabinet, getDateTime> command)
        {
            return await Sanction.OwnerAndGuests<ICabinetDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<cabinet, getDateTime> command)
        {
            var response = command.GenerateResponse<cabinetDateTime>();

            response.Command.cabinetDateTime = DateTime.UtcNow;

            await Task.CompletedTask;
        }
    }
}