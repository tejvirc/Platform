namespace Aristocrat.Monaco.G2S.Handlers.Progressive
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <inheritdoc />
    public class SetProgressiveAward : ICommandHandler<progressive, setProgressiveAward>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetProgressiveValue" /> class.
        /// </summary>
        public SetProgressiveAward(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<progressive, setProgressiveAward> command)
        {
            return await Sanction.OnlyOwner<IProgressiveDevice>(_egm, command);
        }

        /// <inheritdoc />
        public Task Handle(ClassCommand<progressive, setProgressiveAward> command)
        {
            var device = _egm.GetDevice<IProgressiveDevice>(command.IClass.deviceId);
            if (device == null)
            {
                return Task.CompletedTask;
            }

            command.GenerateResponse<progressiveAwardAck>();

            return Task.CompletedTask;
        }
    }
}